using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Qube.Core;
using Newtonsoft.Json;

namespace Qube.Grpc.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var types = new[] { typeof(TestEvent) };

            var options = new StreamDbContextOptionsBuilder()
                .UseGrpcStream("127.0.0.1:5001")
                .RegisterTypes(() => types)
                .Options;

            var es = new StreamDbContext(options);

            var orderEvents = es.FromAll<TestEvent>()
                .Subscribe(
                    onNext: s => Console.WriteLine(JsonConvert.SerializeObject(s)),
                    onError: e => Console.WriteLine("ERROR: " + e),
                    onCompleted: () => Console.WriteLine("DONE")
                );

            while (!Console.KeyAvailable && Console.ReadKey(true).Key != ConsoleKey.Escape)
            {
                await Task.Delay(50);
            }
        }
    }
}
