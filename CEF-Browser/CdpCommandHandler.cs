using System;
using System.IO;
using System.Text;
using System.Linq;
using CefSharp;
using CefSharp.Callback;

namespace CEF_Browser
{
    /// <summary>
    /// Interface for logging CDP messages
    /// </summary>
    public interface ICdpMessageLogger
    {
        /// <summary>
        /// Logs a CDP message
        /// </summary>
        void Log(string message);
    }

    /// <summary>
    /// File-based implementation for logging CDP messages
    /// </summary>
    public class FileCdpMessageLogger : ICdpMessageLogger
    {
        private readonly string _logFilePath;

        public FileCdpMessageLogger()
        {
            var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CEF-Browser", "Logs");
            try
            {
                Directory.CreateDirectory(logDir);
                _logFilePath = Path.Combine(logDir, $"cdp-{DateTime.Now:yyyyMMdd-HHmmss}.log");
                
                // Write initial log entry to verify file creation
                File.WriteAllText(_logFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] CDP Logger initialized. Log file: {_logFilePath}{Environment.NewLine}");
                System.Diagnostics.Debug.WriteLine($"[CDP LOG] Logger initialized. Log file: {_logFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CDP LOG ERROR] Failed to initialize logger: {ex.Message}");
                // Fallback to temp directory
                _logFilePath = Path.Combine(Path.GetTempPath(), $"cef-cdp-{DateTime.Now:yyyyMMdd-HHmmss}.log");
            }
        }

        public void Log(string message)
        {
            try
            {
                // Ensure directory exists
                var logDir = Path.GetDirectoryName(_logFilePath);
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                
                // Also write to debug output for immediate visibility
                System.Diagnostics.Debug.WriteLine($"[CDP LOG] {message}");
                
                File.AppendAllText(_logFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                // Log error to debug output
                System.Diagnostics.Debug.WriteLine($"[CDP LOG ERROR] Failed to write log: {ex.Message}, Path: {_logFilePath}");
            }
        }
    }

    /// <summary>
    /// Interface for extracting request ID from CDP messages
    /// </summary>
    public interface IRequestIdExtractor
    {
        /// <summary>
        /// Extracts request ID from CDP message
        /// </summary>
        string Extract(string message);
    }

    /// <summary>
    /// Default implementation for extracting request ID from CDP messages
    /// </summary>
    public class RequestIdExtractor : IRequestIdExtractor
    {
        /// <summary>
        /// Extracts request ID from CDP message
        /// </summary>
        public string Extract(string message)
        {
            try
            {
                var idIndex = message.IndexOf("\"id\":");
                if (idIndex < 0)
                {
                    return null;
                }

                var startIndex = idIndex + 5;
                // Skip whitespace
                while (startIndex < message.Length && char.IsWhiteSpace(message[startIndex]))
                {
                    startIndex++;
                }
                
                var endIndex = startIndex;
                // Find the end of the number
                while (endIndex < message.Length && 
                       (char.IsDigit(message[endIndex]) || message[endIndex] == '.' || message[endIndex] == '-'))
                {
                    endIndex++;
                }
                
                if (endIndex > startIndex)
                {
                    return message.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }
            catch
            {
                // Ignore parsing errors
            }
            return null;
        }
    }

    /// <summary>
    /// Handles CDP commands that are not natively supported by CefSharp
    /// </summary>
    public class CdpCommandHandler : IDevToolsMessageObserver, IDisposable
    {
        private readonly IRequestIdExtractor _requestIdExtractor;
        private readonly ICdpMessageLogger _logger;
        private const string SetDownloadBehaviorMethod = "Browser.setDownloadBehavior";
        
        /// <summary>
        /// Tracks pending requests that we need to handle
        /// </summary>
        private readonly System.Collections.Generic.Dictionary<int, string> _pendingRequests = 
            new System.Collections.Generic.Dictionary<int, string>();

        /// <summary>
        /// Initializes a new instance of CdpCommandHandler
        /// </summary>
        public CdpCommandHandler() : this(new RequestIdExtractor(), new FileCdpMessageLogger())
        {
        }

        /// <summary>
        /// Initializes a new instance of CdpCommandHandler with dependency injection
        /// </summary>
        public CdpCommandHandler(IRequestIdExtractor requestIdExtractor, ICdpMessageLogger logger)
        {
            _requestIdExtractor = requestIdExtractor ?? throw new ArgumentNullException(nameof(requestIdExtractor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.Log("CdpCommandHandler instance created");
            System.Diagnostics.Debug.WriteLine("[CDP] CdpCommandHandler instance created");
        }

        /// <summary>
        /// Called on receipt of a DevTools protocol message
        /// Returns true if the message was handled, false otherwise
        /// </summary>
        public bool OnDevToolsMessage(IBrowser browser, Stream message)
        {
            try
            {
                var browserInfo = browser != null ? $"Browser ID: {browser.Identifier}" : "Browser is null";
                _logger.Log($"OnDevToolsMessage called - {browserInfo}");
                System.Diagnostics.Debug.WriteLine($"[CDP] OnDevToolsMessage called - {browserInfo}");
                
                if (message == null || message.Length == 0)
                {
                    _logger.Log("OnDevToolsMessage: message is null or empty");
                    System.Diagnostics.Debug.WriteLine("[CDP] OnDevToolsMessage: message is null or empty");
                    return false;
                }

                _logger.Log($"OnDevToolsMessage: message length = {message.Length}, Position = {message.Position}");
                System.Diagnostics.Debug.WriteLine($"[CDP] OnDevToolsMessage: message length = {message.Length}, Position = {message.Position}");
                
                var messageString = ReadStreamToString(message);
                
                // Log all messages for debugging (limit to first 1000 chars to see more context)
                if (!string.IsNullOrEmpty(messageString))
                {
                    var preview = messageString.Length > 1000 ? messageString.Substring(0, 1000) + "..." : messageString;
                    _logger.Log($"CDP Message (length: {messageString.Length}): {preview}");
                    System.Diagnostics.Debug.WriteLine($"[CDP] CDP Message (length: {messageString.Length}): {preview}");
                }
                else
                {
                    _logger.Log("OnDevToolsMessage: messageString is null or empty after reading");
                    System.Diagnostics.Debug.WriteLine("[CDP] OnDevToolsMessage: messageString is null or empty after reading");
                }
            
            // Check if this is a request (has "method" field) or response (has "result" or "error" field)
            var isRequest = messageString.Contains("\"method\":") && !messageString.Contains("\"result\":") && !messageString.Contains("\"error\":");
            var isResponse = messageString.Contains("\"result\":") || messageString.Contains("\"error\":");
            
            // Handle Browser.setDownloadBehavior REQUEST
            if (isRequest && IsSetDownloadBehaviorCommand(messageString))
            {
                _logger.Log("Browser.setDownloadBehavior REQUEST detected in OnDevToolsMessage!");
                System.Diagnostics.Debug.WriteLine("Browser.setDownloadBehavior REQUEST detected!");
                
                // Extract request ID
                var requestIdStr = _requestIdExtractor.Extract(messageString);
                if (!string.IsNullOrEmpty(requestIdStr) && int.TryParse(requestIdStr, out int requestId))
                {
                    _pendingRequests[requestId] = SetDownloadBehaviorMethod;
                    _logger.Log($"Stored pending request ID: {requestId}");
                    
                    // Send success response immediately
                    SendSuccessResponse(browser, requestIdStr);
                    LogHandledCommand(requestIdStr);
                    
                    // Return true to indicate we handled it, preventing default processing
                    return true; // Message handled, don't process further
                }
                else
                {
                    _logger.Log($"Warning: Could not extract valid request ID from: {messageString}");
                }
            }
            
            // Handle error responses for Browser.setDownloadBehavior
            if (isResponse && messageString.Contains("\"error\":"))
            {
                var requestIdStr = _requestIdExtractor.Extract(messageString);
                _logger.Log($"Error response detected, extracted request ID: {requestIdStr ?? "null"}");
                if (!string.IsNullOrEmpty(requestIdStr) && int.TryParse(requestIdStr, out int requestId))
                {
                    if (_pendingRequests.ContainsKey(requestId) && 
                        _pendingRequests[requestId] == SetDownloadBehaviorMethod)
                    {
                        _logger.Log($"Browser.setDownloadBehavior request {requestId} failed, sending success response");
                        _pendingRequests.Remove(requestId);
                        
                        // Send success response to override the error
                        SendSuccessResponse(browser, requestIdStr);
                        return true; // Message handled
                    }
                    else
                    {
                        _logger.Log($"Error response for request {requestId}, but not in pending requests or different method");
                    }
                }
            }
            
            // Log if message was not handled
            if (isRequest)
            {
                _logger.Log($"Unhandled CDP request: {messageString.Substring(0, Math.Min(200, messageString.Length))}");
            }
            else if (isResponse)
            {
                _logger.Log($"Unhandled CDP response: {messageString.Substring(0, Math.Min(200, messageString.Length))}");
            }
            
            return false;
            }
            catch (Exception ex)
            {
                _logger.Log($"Exception in OnDevToolsMessage: {ex.Message}, StackTrace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"[CDP ERROR] Exception in OnDevToolsMessage: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Reads stream content to string
        /// </summary>
        private string ReadStreamToString(Stream stream)
        {
            try
            {
                var originalPosition = stream.Position;
                stream.Position = 0;
                using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
                {
                    var result = reader.ReadToEnd();
                    stream.Position = originalPosition;
                    return result;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Checks if message is Browser.setDownloadBehavior command
        /// </summary>
        private bool IsSetDownloadBehaviorCommand(string message)
        {
            if (message == null)
            {
                return false;
            }
            
            // Check for the method name in the message
            return message.Contains($"\"method\":\"{SetDownloadBehaviorMethod}\"") ||
                   message.Contains($"\"method\":\"{SetDownloadBehaviorMethod}\"");
        }

        /// <summary>
        /// Handles Browser.setDownloadBehavior CDP command
        /// </summary>
        private bool HandleSetDownloadBehavior(IBrowser browser, string message)
        {
            try
            {
                var requestId = _requestIdExtractor.Extract(message);
                if (string.IsNullOrEmpty(requestId))
                {
                    return false;
                }

                SendSuccessResponse(browser, requestId);
                LogHandledCommand(requestId);
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }
        }

        /// <summary>
        /// Sends success response for Browser.setDownloadBehavior command
        /// </summary>
        private void SendSuccessResponse(IBrowser browser, string requestId)
        {
            try
            {
                // Parse requestId as integer if possible
                int id;
                if (!int.TryParse(requestId, out id))
                {
                    _logger.Log($"Warning: Could not parse requestId as integer: {requestId}");
                    return;
                }
                
                var response = $"{{\"id\":{id},\"result\":{{}}}}";
                _logger.Log($"Sending response: {response}");
                System.Diagnostics.Debug.WriteLine($"Sending CDP response: {response}");
                
                browser.GetHost().SendDevToolsMessage(response);
                _logger.Log("Response sent successfully");
            }
            catch (Exception ex)
            {
                _logger.Log($"Error sending response: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error sending response: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs that command was handled
        /// </summary>
        private void LogHandledCommand(string requestId)
        {
            var message = $"Browser.setDownloadBehavior handled with id: {requestId}";
            _logger.Log(message);
            System.Diagnostics.Debug.WriteLine(message);
        }

        /// <summary>
        /// Logs error without throwing to prevent breaking CDP connection
        /// </summary>
        private void LogError(Exception ex)
        {
            var message = $"Error handling Browser.setDownloadBehavior: {ex.Message}";
            _logger.Log(message);
            System.Diagnostics.Debug.WriteLine(message);
        }

        /// <summary>
        /// Called when the DevTools agent attaches
        /// </summary>
        public void OnDevToolsAgentAttached(IBrowser browser)
        {
            try
            {
                var browserInfo = browser != null ? $"Browser ID: {browser.Identifier}, CanGoBack: {browser.CanGoBack}" : "Browser is null";
                _logger.Log($"DevTools agent attached - {browserInfo}");
                System.Diagnostics.Debug.WriteLine($"[CDP] DevTools agent attached - {browserInfo}");
            }
            catch (Exception ex)
            {
                _logger.Log($"Error in OnDevToolsAgentAttached: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[CDP ERROR] OnDevToolsAgentAttached error: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when the DevTools agent detaches
        /// </summary>
        public void OnDevToolsAgentDetached(IBrowser browser)
        {
            try
            {
                var browserInfo = browser != null ? $"Browser ID: {browser.Identifier}" : "Browser is null";
                _logger.Log($"DevTools agent detached - {browserInfo}");
                System.Diagnostics.Debug.WriteLine($"[CDP] DevTools agent detached - {browserInfo}");
            }
            catch (Exception ex)
            {
                _logger.Log($"Error in OnDevToolsAgentDetached: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[CDP ERROR] OnDevToolsAgentDetached error: {ex.Message}");
            }
        }

        /// <summary>
        /// Called on receipt of a DevTools protocol event
        /// </summary>
        public void OnDevToolsEvent(IBrowser browser, string method, Stream parameters)
        {
            try
            {
                var parametersString = parameters != null ? ReadStreamToString(parameters) : "null";
                var preview = parametersString.Length > 500 ? parametersString.Substring(0, 500) + "..." : parametersString;
                _logger.Log($"OnDevToolsEvent called - Method: {method ?? "null"}, Parameters length: {parametersString?.Length ?? 0}, Preview: {preview}");
                System.Diagnostics.Debug.WriteLine($"[CDP] OnDevToolsEvent - Method: {method}, Parameters: {preview}");
            }
            catch (Exception ex)
            {
                _logger.Log($"Error in OnDevToolsEvent: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[CDP ERROR] OnDevToolsEvent error: {ex.Message}");
            }
        }

        /// <summary>
        /// Called on receipt of a DevTools method result
        /// </summary>
        public void OnDevToolsMethodResult(IBrowser browser, int messageId, bool success, Stream result)
        {
            try
            {
                var resultString = result != null ? ReadStreamToString(result) : "null";
                var preview = resultString.Length > 500 ? resultString.Substring(0, 500) + "..." : resultString;
                _logger.Log($"OnDevToolsMethodResult called - ID: {messageId}, Success: {success}, Result length: {resultString?.Length ?? 0}, Preview: {preview}");
                System.Diagnostics.Debug.WriteLine($"[CDP] OnDevToolsMethodResult - ID: {messageId}, Success: {success}, Result: {preview}");
                
                // Check if this is a failed Browser.setDownloadBehavior request
                if (!success && _pendingRequests.ContainsKey(messageId))
                {
                    var method = _pendingRequests[messageId];
                    _logger.Log($"Pending request {messageId} ({method}) failed, attempting to handle");
                    if (method == SetDownloadBehaviorMethod)
                    {
                        _logger.Log($"Browser.setDownloadBehavior request {messageId} failed, sending success response");
                        _pendingRequests.Remove(messageId);
                        
                        // Send success response to override the error
                        SendSuccessResponse(browser, messageId.ToString());
                    }
                }
                else if (success && _pendingRequests.ContainsKey(messageId))
                {
                    // Request succeeded, remove from pending
                    var method = _pendingRequests[messageId];
                    _logger.Log($"Pending request {messageId} ({method}) succeeded, removing from pending");
                    _pendingRequests.Remove(messageId);
                }
                else if (!success)
                {
                    // Log all failed requests to see what's happening
                    _logger.Log($"Unhandled failed request - ID: {messageId}, Result: {preview}");
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Error in OnDevToolsMethodResult: {ex.Message}, StackTrace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"[CDP ERROR] OnDevToolsMethodResult error: {ex.Message}");
            }
        }

        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
