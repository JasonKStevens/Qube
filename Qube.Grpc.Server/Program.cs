using Grpc.Core;
using System;
using GrpcServer = Grpc.Core.Server;

namespace Qube.Grpc.Server
{
    public class Program
    {
        private const int Port = 5001;

        public static void Main(string[] args)
        {
            var server = new GrpcServer
            {
                Services = { StreamService.BindService(new QueryService()) },
                Ports = { new ServerPort("127.0.0.1", Port, ServerCredentials.Insecure) }
            };

            server.Start();

            Console.WriteLine("Grpc server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
