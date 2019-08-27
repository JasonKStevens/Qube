using Microsoft.Reactive.Testing;
using NUnit.Framework;
using Qube.Core;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Qube.Core;
using Qube.InMemory;

namespace QbservableProvider.Test
{
    public class QueryFixture : ReactiveTest
    {
        private StreamDbContext<EventFake> _sut;
        private IDisposable _subscription;
        private TestScheduler _scheduler;
        private Subject<EventFake> _subject;

        [SetUp]
        public void Setup()
        {
            // http://introtorx.com/Content/v1.0.10621.0/16_TestingRx.html
            _scheduler = new TestScheduler();
            _subject = new Subject<EventFake>();

            var options = new StreamDbContextOptionsBuilder()
                .UseInMemoryStream(_subject)
                .Options;

            _sut = new StreamDbContext<EventFake>(options);
        }

        [TearDown]
        public void TearDown()
        {
            _subscription?.Dispose();
        }

        [Test]
        public void Should_query()
        {
            var input = _scheduler.CreateHotObservable(
                OnNext(100, "abc"),
                OnNext(200, "def"),
                OnNext(250, "ghi"),
                OnNext(300, "pqr"),
                OnNext(450, "xyz"),
                OnCompleted<string>(500)
                );

            var results = _scheduler.Start(
                () => input.Buffer(() => input.Throttle(TimeSpan.FromTicks(100), _scheduler))
                           .Select(b => string.Join(",", b)),
                created: 50,
                subscribed: 150,
                disposed: 600);

            ReactiveAssert.AreElementsEqual(results.Messages, new Recorded<Notification<string>>[] {
                OnNext(400, "def,ghi,pqr"),
                OnNext(500, "xyz"),
                OnCompleted<string>(500)
            });

            ReactiveAssert.AreElementsEqual(input.Subscriptions, new Subscription[] {
                Subscribe(150, 500),
                Subscribe(150, 400),
                Subscribe(400, 500)
            });


            _subscription = _sut.FromAll()
                .Subscribe(
                    e => Assert.Pass(),
                    () => { }
                );
        }
    }
}