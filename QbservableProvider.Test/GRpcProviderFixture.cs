using System;
using System.Net.Http;
using System.Reactive.Subjects;
using Moq;
using NUnit.Framework;
using QbservableProvider.Core;
using QbservableProvider.Core.GRpcProvider;
using RichardSzalay.MockHttp;

namespace Tests
{
    public class GRpcProviderFixture
    {
        private StreamDbContext<string> _sut;
        private Subject<string> _subject;
        private MockHttpMessageHandler _mockHttp;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private IDisposable _sub;

        [SetUp]
        public void Setup()
        {
            _subject = new Subject<string>();

            _mockHttp = new MockHttpMessageHandler();
            var client = new HttpClient(_mockHttp);

            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(client);

            var options = new StreamDbContextOptionsBuilder()
                .UseGRpcStream("http://some-host")
                .UseHttpClientFactory(_httpClientFactoryMock.Object)
                .Options;

            _sut = new StreamDbContext<string>(options);
        }

        [TearDown]
        public void TearDown()
        {
            _sub?.Dispose();
        }

        // TODO....
    }
}
