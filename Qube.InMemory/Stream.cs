﻿using Qube.Core;
using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Qube.InMemory
{
    internal class Stream<T> : IQbservable<T>
    {
        private readonly StreamDbContextOptions _options;
        private readonly string[] _streamPatterns;
        private readonly IObservable<T> _observable;

        public Type ElementType { get; private set; }
        public Expression Expression { get; private set; }
        public IQbservableProvider Provider { get; private set; }

        public Stream(IObservable<T> observable, StreamDbContextOptions options, string[] streamPatterns)
        {
            _observable = observable;
            _options = options;
            _streamPatterns = streamPatterns;

            ElementType = typeof(T);
            Provider = new StreamDbProvider<T>(observable, options, streamPatterns);
            Expression = Expression.Parameter(typeof(IQbservable<T>), "o");
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var sub = new Subscription<T, T>(Expression, _observable, _options, _streamPatterns);
            sub.Attach(observer);
            return sub;
        }
    }
}
