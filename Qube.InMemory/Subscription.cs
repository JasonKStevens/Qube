﻿using Qube.Core;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;

namespace Qube.InMemory
{
    internal class Subscription<TIn, TOut> : IDisposable
    {
        private readonly bool _isLinqQuery;
        private readonly Expression _expression;
        private readonly IObservable<TIn> _observable;
        private readonly StreamDbContextOptions _options;
        private readonly string[] _streamPatterns;
        private IDisposable _sub;

        internal Subscription(Expression expression, IObservable<TIn> observable, StreamDbContextOptions options, string[] streamPatterns)
        {
            _isLinqQuery = expression is MethodCallExpression;
            _expression = expression;
            _observable = observable;
            _options = options;
            _streamPatterns = streamPatterns;
        }

        internal void Attach(IObserver<TOut> observer)
        {
            var qbservable = _isLinqQuery ? BuildQbservable() : _observable.Select(x => (TOut)(object) x);
            _sub = qbservable.Subscribe(observer);
        }

        private IQbservable<TOut> BuildQbservable()
        {
            // IQbservable<T> parameter "o"
            var parameter = (ParameterExpression) ((MethodCallExpression)_expression).Arguments[0];

            // TIn & TOut
            var inType = parameter.Type.GetGenericArguments()[0];
            var iQbservableIn = typeof(IQbservable<>).MakeGenericType(new[] { inType });
            var iQbservableOut = typeof(IQbservable<TOut>);

            // Func<IQbservable<TIn>, IQbservable<TOut>>
            var funcInOut = typeof(Func<,>).MakeGenericType(new[] { iQbservableIn, iQbservableOut });

            // Expression.Lambda<Func<IQbservable<TIn>, IQbservable<TOut>>>(_expression, parameter)
            var genericMethodInfo = typeof(Expression)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == "Lambda" && m.IsGenericMethod)
                .Where(m =>
                {
                    return m.GetParameters()
                        .Select(p => p.ParameterType)
                        .SequenceEqual(new []{ typeof(Expression), typeof(ParameterExpression[]) });
                })
                .Single()
                .MakeGenericMethod(new[] { funcInOut });

            var lambdaExpression = (LambdaExpression) genericMethodInfo.Invoke(null, new object[] { _expression, new[] { parameter } });
            var lambda = lambdaExpression.Compile();

            var qbservable = (IQbservable<TOut>) lambda.DynamicInvoke(_observable);
            return qbservable;
        }

        public void Dispose()
        {
            _sub?.Dispose();
        }
    }
}
