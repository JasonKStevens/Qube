﻿using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace QbservableProvider.Core.InMemoryProvider
{
    internal class QueryStream<TIn, TOut> : IQbservable<TOut>
    {
        private readonly StreamDbContextOptions _options;
        private readonly IObservable<TIn> _observable;

        public Type ElementType { get; private set; }
        public Expression Expression { get; private set; }
        public IQbservableProvider Provider { get; private set; }

        public QueryStream(
            IQbservableProvider provider,
            Expression expression,
            IObservable<TIn> observable,
            StreamDbContextOptions options)
        {
            _observable = observable;
            _options = options;

            ElementType = typeof(TIn);
            Provider = provider;
            Expression = expression;
        }

        public IDisposable Subscribe(IObserver<TOut> observer)
        {
            var sub = new Subscription<TIn, TOut>(Expression, _observable, _options);
            sub.Attach(observer);
            return sub;
        }
    }
}