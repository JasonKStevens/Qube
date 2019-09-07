using NUnit.Framework;
using Qube.Core;
using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Qube.Test
{
    public class ServerQueryObservableFixture
    {
        private Subject<string> _subject;
        private Expression<Func<IQbservable<string>, IQbservable<string>>> _queryExpression;
        private ServerQueryObservable<string> _sut;
        private IDisposable _sub;

        [SetUp]
        public void Setup()
        {
            _subject = new Subject<string>();
            _queryExpression = (IQbservable<string> o) => o.Where(x => x == "include");
            _sut = new ServerQueryObservable<string>(typeof(string), _subject.AsQbservable(), _queryExpression);
        }

        [TearDown]
        public void TearDown()
        {
            _sub?.Dispose();
        }

        [Test]
        public void should_emit_next()
        {
            // Arrange 
            string next = null;
            bool completed = false;
            bool errored = false;

            _sub = _sut.Subscribe(
                s => next = s,
                e => errored = true,
                () => completed = true
            );

            // Act 
            _subject.OnNext("include");

            // Assert
            Assert.That(next, Is.EqualTo("include"));
            Assert.That(completed, Is.False);
            Assert.That(errored, Is.False);
        }

        [Test]
        public void should_not_emit_next()
        {
            // Arrange 
            string next = null;
            bool completed = false;
            bool errored = false;

            _sub = _sut.Subscribe(
                s => next = s,
                e => errored = true,
                () => completed = true
            );

            // Act 
            _subject.OnNext("do-not-include");

            // Assert
            Assert.That(next, Is.Null);
            Assert.That(completed, Is.False);
            Assert.That(errored, Is.False);
        }

        [Test]
        public void should_emit_completed()
        {
            // Arrange 
            string next = null;
            bool completed = false;
            bool errored = false;

            _sub = _sut.Subscribe(
                s => next = s,
                e => errored = true,
                () => completed = true
            );

            // Act 
            _subject.OnCompleted();

            // Assert
            Assert.That(next, Is.Null);
            Assert.That(completed, Is.True);
            Assert.That(errored, Is.False);
        }

        [Test]
        public void should_emit_error()
        {
            // Arrange 
            string next = null;
            bool completed = false;
            bool errored = false;

            _sub = _sut.Subscribe(
                s => next = s,
                e => errored = true,
                () => completed = true
            );

            // Act 
            _subject.OnError(new Exception());

            // Assert
            Assert.That(next, Is.Null);
            Assert.That(completed, Is.False);
            Assert.That(errored, Is.True);
        }
    }
}
