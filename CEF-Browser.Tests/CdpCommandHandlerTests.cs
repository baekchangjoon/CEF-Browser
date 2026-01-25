using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using CEF_Browser;
using CefSharp;
using Moq;

namespace CEF_Browser.Tests
{
    /// <summary>
    /// Tests for CdpCommandHandler
    /// </summary>
    [TestFixture]
    public class CdpCommandHandlerTests
    {
        private Mock<IRequestIdExtractor> mockExtractor;
        private Mock<ICdpMessageLogger> mockLogger;
        private CdpCommandHandler handler;

        [SetUp]
        public void Setup()
        {
            mockExtractor = new Mock<IRequestIdExtractor>();
            mockLogger = new Mock<ICdpMessageLogger>();
            handler = new CdpCommandHandler(mockExtractor.Object, mockLogger.Object);
        }

        [Test]
        public void Constructor_WithNullExtractor_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CdpCommandHandler(null, mockLogger.Object));
        }

        [Test]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CdpCommandHandler(mockExtractor.Object, null));
        }

        [Test]
        public void Constructor_WithDefault_InitializesSuccessfully()
        {
            var defaultHandler = new CdpCommandHandler();
            Assert.IsNotNull(defaultHandler);
        }

        [Test]
        public void OnDevToolsMessage_WithSetDownloadBehaviorCommand_CallsExtractor()
        {
            var message = "{\"id\":123,\"method\":\"Browser.setDownloadBehavior\",\"params\":{}}";
            var stream = CreateStreamFromString(message);
            var mockBrowser = new Mock<IBrowser>();
            var mockHost = new Mock<IBrowserHost>();
            mockBrowser.Setup(b => b.GetHost()).Returns(mockHost.Object);
            
            mockExtractor.Setup(x => x.Extract(message)).Returns("123");

            var result = handler.OnDevToolsMessage(mockBrowser.Object, stream);

            Assert.IsTrue(result);
            mockExtractor.Verify(x => x.Extract(message), Times.Once);
        }

        [Test]
        public void OnDevToolsMessage_WithOtherCommand_DoesNotCallExtractor()
        {
            var message = "{\"id\":123,\"method\":\"Page.navigate\",\"params\":{}}";
            var stream = CreateStreamFromString(message);
            var mockBrowser = new Mock<IBrowser>();

            var result = handler.OnDevToolsMessage(mockBrowser.Object, stream);

            Assert.IsFalse(result);
            mockExtractor.Verify(x => x.Extract(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void OnDevToolsMessage_WithNullStream_ReturnsFalse()
        {
            var mockBrowser = new Mock<IBrowser>();

            var result = handler.OnDevToolsMessage(mockBrowser.Object, null);

            Assert.IsFalse(result);
        }

        [Test]
        public void OnDevToolsMessage_WithEmptyRequestId_ReturnsFalse()
        {
            var message = "{\"id\":,\"method\":\"Browser.setDownloadBehavior\",\"params\":{}}";
            var stream = CreateStreamFromString(message);
            var mockBrowser = new Mock<IBrowser>();

            mockExtractor.Setup(x => x.Extract(message)).Returns(string.Empty);

            var result = handler.OnDevToolsMessage(mockBrowser.Object, stream);

            Assert.IsFalse(result);
            mockExtractor.Verify(x => x.Extract(message), Times.Once);
        }

        [Test]
        public void OnDevToolsMessage_WhenExtractorThrows_ReturnsFalse()
        {
            var message = "{\"id\":123,\"method\":\"Browser.setDownloadBehavior\",\"params\":{}}";
            var stream = CreateStreamFromString(message);
            var mockBrowser = new Mock<IBrowser>();

            mockExtractor.Setup(x => x.Extract(message)).Throws<Exception>();

            var result = handler.OnDevToolsMessage(mockBrowser.Object, stream);

            Assert.IsFalse(result);
        }

        private Stream CreateStreamFromString(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            return new MemoryStream(bytes);
        }
    }

    /// <summary>
    /// Tests for RequestIdExtractor
    /// </summary>
    [TestFixture]
    public class RequestIdExtractorTests
    {
        private RequestIdExtractor extractor;

        [SetUp]
        public void Setup()
        {
            extractor = new RequestIdExtractor();
        }

        [Test]
        public void Extract_WithValidId_ReturnsId()
        {
            var message = "{\"id\":123,\"method\":\"test\"}";
            var result = extractor.Extract(message);

            Assert.AreEqual("123", result);
        }

        [Test]
        public void Extract_WithIdAtEnd_ReturnsId()
        {
            var message = "{\"method\":\"test\",\"id\":456}";
            var result = extractor.Extract(message);

            Assert.AreEqual("456", result);
        }

        [Test]
        public void Extract_WithNoId_ReturnsNull()
        {
            var message = "{\"method\":\"test\"}";
            var result = extractor.Extract(message);

            Assert.IsNull(result);
        }

        [Test]
        public void Extract_WithNullMessage_ReturnsNull()
        {
            var result = extractor.Extract(null);

            Assert.IsNull(result);
        }

        [Test]
        public void Extract_WithEmptyMessage_ReturnsNull()
        {
            var result = extractor.Extract(string.Empty);

            Assert.IsNull(result);
        }

        [Test]
        public void Extract_WithIdAndWhitespace_ReturnsTrimmedId()
        {
            var message = "{\"id\":   789   ,\"method\":\"test\"}";
            var result = extractor.Extract(message);

            Assert.AreEqual("789", result);
        }
    }
}
