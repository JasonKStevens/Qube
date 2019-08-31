using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using NUnit.Framework;
using Qube.Core;
using Qube.InMemory;

namespace QbservableProvider.Test
{
    public class InMemoryProviderFixture
    {
        private StreamDbContext _sut;
        private Subject<string> _subject;
        private IDisposable _sub;

        [SetUp]
        public void Setup()
        {
            _subject = new Subject<string>();

            var options = new StreamDbContextOptionsBuilder()
                .UseInMemoryStream(_subject.AsQbservable())
                .Options;

            _sut = new StreamDbContext(options);
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
            var stream = _sut.FromAll<string>();

            // Assert
            Assert.That(stream, Is.Not.Null);
        }

        [Test]
        public void should_receive_events_from_stream_without_linq_query()
        {
            // Arrange
            string value = null;

            _sub = _sut.FromAll<string>()
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

            _sub = _sut.FromAll<string>()
                .Where(x => x != null)
                .Subscribe(s => value = s);

            // Act
            _subject.OnNext("some-value");

            // Assert
            Assert.That(value, Is.EqualTo("some-value"));
        }

        [Test]
        public void should_pass_apply_linq_expression()
        {
            // Arrange
            int eventCount = 0;

            _sub = _sut.FromAll<string>()
                .Where(x => x != null)
                .Subscribe(x => eventCount++);

            // Act
            _subject.OnNext(null);
            _subject.OnNext("some-value");

            // Assert
            Assert.That(eventCount, Is.EqualTo(1));
        }

        [Test]
        public void should_convert_to_different_out_type()
        {
            // Arrange
            int result = 0;

            _sub = _sut.FromAll<string>()
                .Select(x => int.Parse(x))
                .Subscribe(x => result = x);

            // Act
            _subject.OnNext("123");

            // Assert
            Assert.That(result, Is.EqualTo(123));
        }

        [Test]
        public void should_complete()
        {
            // Arrange
            var completed = false;

            _sub = _sut.FromAll<string>()
                .Select(x => int.Parse(x))
                .Subscribe(_ => {}, () => completed = true);

            // Act
            _subject.OnCompleted();

            // Assert
            Assert.That(completed, Is.True);
        }

        [Test]
        public void should_error()
        {
            // Arrange
            Exception actualError = null;
            var expectedError = new Exception();

            _sub = _sut.FromAll<string>()
                .Select(x => int.Parse(x))
                .Subscribe(_ => {}, ex => actualError = ex);

            // Act
            _subject.OnError(expectedError);

            // Assert
            Assert.That(actualError, Is.EqualTo(expectedError));
        }
    }
}
