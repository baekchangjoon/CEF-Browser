using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace CEF_Browser
{
    /// <summary>
    /// Proxies CDP traffic to handle unsupported commands
    /// </summary>
    public class CdpProxyService : IDisposable
    {
        private HttpListener _listener;
        private readonly int _listenPort;
        private readonly int _targetPort;
        private CancellationTokenSource _cts;
        private Task _proxyTask;

        public CdpProxyService(int listenPort = 9222, int targetPort = 19222)
        {
            _listenPort = listenPort;
            _targetPort = targetPort;
        }

        public void Start()
        {
            if (_listener != null && _listener.IsListening) return;

            _cts = new CancellationTokenSource();
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{_listenPort}/");
            _listener.Start();

            Console.WriteLine($"[CDP Proxy] Started on port {_listenPort}, forwarding to {_targetPort}");
            _proxyTask = Task.Run(() => AcceptConnections(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
            _proxyTask?.Wait(1000);
            _listener?.Close();
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }

        private async Task AcceptConnections(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = HandleContext(context, token);
                }
                catch (HttpListenerException) when (token.IsCancellationRequested)
                {
                    // Listener stopped
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CDP Proxy] Accept error: {ex.Message}");
                }
            }
        }

        private async Task HandleContext(HttpListenerContext context, CancellationToken token)
        {
            if (context.Request.IsWebSocketRequest)
            {
                await HandleWebSocket(context, token);
            }
            else
            {
                await HandleHttpRequest(context);
            }
        }

        private async Task HandleHttpRequest(HttpListenerContext context)
        {
            try
            {
                var targetUrl = $"http://localhost:{_targetPort}{context.Request.Url.PathAndQuery}";
                var request = WebRequest.CreateHttp(targetUrl);
                request.Method = context.Request.HttpMethod;

                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    context.Response.StatusCode = (int)response.StatusCode;
                    context.Response.ContentType = response.ContentType;

                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var content = await reader.ReadToEndAsync();
                        // Rewrite the port in the response (e.g., for /json/version or /json/list)
                        content = content.Replace($":{_targetPort}", $":{_listenPort}");
                        
                        var bytes = Encoding.UTF8.GetBytes(content);
                        context.Response.ContentLength64 = bytes.Length;
                        await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CDP Proxy] HTTP Forward error: {ex.Message}");
                context.Response.StatusCode = 500;
            }
            finally
            {
                context.Response.Close();
            }
        }

        private async Task HandleWebSocket(HttpListenerContext context, CancellationToken token)
        {
            HttpListenerWebSocketContext wsContext = null;
            try
            {
                wsContext = await context.AcceptWebSocketAsync(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CDP Proxy] WS Accept error: {ex.Message}");
                context.Response.StatusCode = 500;
                context.Response.Close();
                return;
            }

            ClientWebSocket targetWs = new ClientWebSocket();
            var clientSendLock = new SemaphoreSlim(1, 1);

            try
            {
                var targetUri = new Uri($"ws://localhost:{_targetPort}{context.Request.Url.PathAndQuery}");
                await targetWs.ConnectAsync(targetUri, token);

                await Task.WhenAll(
                    ForwardClientToTarget(wsContext.WebSocket, targetWs, token, clientSendLock),
                    ForwardTargetToClient(targetWs, wsContext.WebSocket, token, clientSendLock)
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CDP Proxy] WS Session error: {ex.Message}");
            }
            finally
            {
                if (wsContext.WebSocket.State == WebSocketState.Open)
                    await wsContext.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                
                targetWs.Dispose();
                clientSendLock.Dispose();
            }
        }

        private async Task ForwardClientToTarget(WebSocket client, ClientWebSocket target, CancellationToken token, SemaphoreSlim clientSendLock)
        {
            var buffer = new byte[8192];
            while (client.State == WebSocketState.Open && target.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                WebSocketReceiveResult result;
                try 
                {
                    result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                } 
                catch 
                { 
                    break; 
                }

                if (result.MessageType == WebSocketMessageType.Close) break;

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                
                // Check if this is the problematic command
                if (message.Contains("\"method\":\"Browser.setDownloadBehavior\""))
                {
                    Console.WriteLine("[CDP Proxy] Intercepted Browser.setDownloadBehavior");
                    // Extract ID
                    var id = ExtractId(message);
                    if (id != null)
                    {
                        var response = $"{{\"id\":{id},\"result\":{{}}}}";
                        var respBytes = Encoding.UTF8.GetBytes(response);
                        
                        await clientSendLock.WaitAsync(token);
                        try
                        {
                            await client.SendAsync(new ArraySegment<byte>(respBytes), WebSocketMessageType.Text, true, token);
                        }
                        finally
                        {
                            clientSendLock.Release();
                        }
                        continue;
                    }
                }

                await target.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, token);
            }
        }

        private async Task ForwardTargetToClient(ClientWebSocket target, WebSocket client, CancellationToken token, SemaphoreSlim clientSendLock)
        {
            var buffer = new byte[8192];
            while (client.State == WebSocketState.Open && target.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await target.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                }
                catch
                {
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Close) break;

                await clientSendLock.WaitAsync(token);
                try
                {
                    await client.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, token);
                }
                finally
                {
                    clientSendLock.Release();
                }
            }
        }

        private string ExtractId(string json)
        {
            try 
            {
                var marker = "\"id\":";
                var index = json.IndexOf(marker);
                if (index == -1) return null;
                
                var start = index + marker.Length;
                var end = start;
                while (end < json.Length && char.IsDigit(json[end])) end++;
                
                return json.Substring(start, end - start);
            }
            catch { return null; }
        }
    }
}
