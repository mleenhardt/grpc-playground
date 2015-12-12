using System.Collections.Generic;
using System.IO;
using Grpc.Core;
using Playground.Common.ServiceDefinition;

namespace Playground.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var channel = new Channel("localhost", 1337, new SslCredentials(File.ReadAllText("certificates\\ca.crt")));
            var client = new ServiceClient(PlaygroundService.NewClient(channel));

            var listenTask = client.ListenForNewPeopleAsync();

            client.CreatePersonAsync(new List<Person>
            {
                new Person { Id = 1, Name = "John Doe" },
                new Person { Id = 2, Name = "Lisa Simpson" },
                new Person { Id = 3, Name = "Michael Jackson" },
                new Person { Id = 4, Name = "Mike Bully" },
                new Person { Id = 5, Name = "Mark Commet" },
                new Person { Id = 6, Name = "Alfred Punstar" },
            }).Wait();

            var person = client.GetPersonByIdAsync(5).Result;

            var personList = client.GetPersonListAsync().Result;

            channel.ShutdownAsync().Wait();
        }
    }
}
