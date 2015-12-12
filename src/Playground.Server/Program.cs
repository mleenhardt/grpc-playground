using System;
using System.IO;
using Grpc.Core;
using Playground.Common.ServiceDefinition;

namespace Playground.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            const int port = 1337;

            var serviceImpl = new PlaygroundServiceImpl(new PersonRepository());
            var server = new Grpc.Core.Server
            {
                Services = { PlaygroundService.BindService(serviceImpl) },
                Ports =
                {
                    new ServerPort("0.0.0.0", port, new SslServerCredentials(
                        new[]
                        {
                            new KeyCertificatePair(
                                File.ReadAllText("certificates\\server.crt"),
                                File.ReadAllText("certificates\\server.key"))
                        }))
                }
            };
            server.Start();

            Console.WriteLine("RPC server listening on port " + port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            serviceImpl.Shutdown();
            server.ShutdownAsync().Wait();
        }
    }
}
