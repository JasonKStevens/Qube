﻿using Qube.Core;
using System;

namespace Qube.Grpc
{
    public static class OptionsBuilderExtensions
    {
        public static StreamDbContextOptionsBuilder UseGrpcStream(this StreamDbContextOptionsBuilder builder, string url)
        {
            builder.Options.SetConnectionString(url);
            builder.Options.SetStreamFactory(StreamFactory);
            return builder;
        }

        private static object StreamFactory(StreamDbContextOptions options, Type genericParam, string[] streamPatterns)
        {
            var stream = typeof(Stream<>)
                .MakeGenericType(new[] { genericParam })
                .GetConstructor(new[] { typeof(StreamDbContextOptions), typeof(string[]) })
                .Invoke(new object[] { options, streamPatterns });
            return stream;
        }
    }
}