using System;
using System.Collections.Generic;

namespace Qube.Core
{
    public class StreamDbContextOptionsBuilder
    {
        public StreamDbContextOptions Options { get; private set; }

        public StreamDbContextOptionsBuilder()
        {
            Options = new StreamDbContextOptions();
        }

        public StreamDbContextOptionsBuilder RegisterTypes(Func<IEnumerable<Type>> getTypes)
        {
            Options.RegisterTypes(getTypes());
            return this;
        }
    }
}