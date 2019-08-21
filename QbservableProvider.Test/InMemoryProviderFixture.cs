using Microsoft.Reactive.Testing;
using NUnit.Framework;
using QbservableProvider.Core;
using QbservableProvider.Core.InMemoryProvider;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Tests
{
    public class InMemoryProviderFixture
    {
        private StreamDbContext<string> _sut;
        private IDisposable _sub;
        private TestScheduler _scheduler;
        private Subject<string> _subject;

        [SetUp]
        public void Setup()
        {
            _subject = new Subject<string>();

            var options = new StreamDbContextOptionsBuilder()
                .UseInMemoryStream(_subject)
                .Options;

            _sut = new StreamDbContext<string>(options);
        }

        [TearDown]
        public void TearDown()
        {
            _sub?.Dispose();
        }

        [Test]
        public void should_create_stream()
        {
            // Act
            var stream = _sut.FromAll();

            // Assert
            Assert.That(stream, Is.Not.Null);
        }

        [Test]
        public void should_receive_events_from_stream_without_linq_query()
        {
            // Arrange
            string value = null;

            _sub = _sut.FromAll()
                .Subscribe(s => value = s);

            // Act
            _subject.OnNext("some-value");

            // Assert
            Assert.That(value, Is.EqualTo("some-value"));
        }

        [Test]
        public void should_receive_events_from_stream_with_linq_query()
        {
            // Arrange
            string value = null;

            _sub = _sut.FromAll()
                .Where(x => x != null)
                .Subscribe(s => value = s);

            // Act
            _subject.OnNext("some-value");

            // Assert
            Assert.That(value, Is.EqualTo("some-value"));
        }

        [Test]
        public void should_pass_through_linq_expression()
        {
            // Arrange
            int eventCount = 0;

            _sub = _sut.FromAll()
                .Where(x => x != null)
                .Subscribe(x => eventCount++);

            // Act
            _subject.OnNext(null);
            _subject.OnNext("some-value");

            // Assert
            Assert.That(eventCount, Is.EqualTo(1));
        }
    }
}