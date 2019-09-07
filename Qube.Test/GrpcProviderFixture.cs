using System;
using System.Net.Http;
using System.Reactive.Subjects;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using Qube.Core;
using Qube.Grpc;

namespace Qube.Test
{
    public class GrpcProviderFixture
    {
        private StreamDbContext _sut;
        private Subject<string> _subject;
        private MockHttpMessageHandler _mockHttp;
        private IDisposable _sub;

        [SetUp]
        public void Setup()
        {
            _subject = new Subject<string>();

            _mockHttp = new MockHttpMessageHandler();
            var client = new HttpClient(_mockHttp);

            var options = new StreamDbContextOptionsBuilder()
                .UseGrpcStream("http://some-host")
                .Options;

            _sut = new StreamDbContext(options);
        }

        [TearDown]
        public void TearDown()
        {
            _sub?.Dispose();
        }

        // TODO....
    }
}
