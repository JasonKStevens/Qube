using System;
using System.Reactive.Linq;

namespace QbservableProvider.Core
{
    public class StreamDbContextOptionsBuilder
    {
        public StreamDbContextOptions Options { get; private set; }

        public StreamDbContextOptionsBuilder()
        {
            Options = new StreamDbContextOptions();
        }
    }
}