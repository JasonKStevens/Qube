namespace Qube.Core
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