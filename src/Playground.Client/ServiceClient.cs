using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Playground.Common.ServiceDefinition;

namespace Playground.Client
{
    public class ServiceClient
    {
        private readonly PlaygroundService.IPlaygroundServiceClient _grpcClient;

        public ServiceClient(PlaygroundService.IPlaygroundServiceClient grpcClient)
        {
            _grpcClient = grpcClient;
        }

        private void LogRpc(string methodName)
        {
            Console.WriteLine("{0} -- Sending RPC call, method={1}", DateTime.Now, methodName);
        }

        public async Task<Person> GetPersonByIdAsync(int id)
        {
            LogRpc("GetPersonByIdAsync");
            var person = await _grpcClient.GetPersonByIdAsync(new PersonId { Id = id });
            return person;
        }

        public async Task<ICollection<Person>> GetPersonListAsync()
        {
            LogRpc("GetPersonListAsync");
            var list = new List<Person>();
            using (var call = _grpcClient.GetPersonList(new PersonListRequest()))
            {
                while (await call.ResponseStream.MoveNext())
                {
                    var person = call.ResponseStream.Current;
                    list.Add(person);
                }
                return list;
            }
        }

        public async Task<ICollection<Person>> CreatePersonAsync(IEnumerable<Person> people)
        {
            LogRpc("CreatePersonAsync");
            using (var call = _grpcClient.CreatePeople())
            {
                foreach (var person in people)
                {
                    await call.RequestStream.WriteAsync(person);
                }
                await call.RequestStream.CompleteAsync();
                var response = await call.ResponseAsync;
                return response.People;
            }
        }

        public async Task ListenForNewPeopleAsync()
        {
            LogRpc("ListenForNewPeopleAsync");
            using (var call = _grpcClient.ListenForNewPeople(new ListenForNewPeopleRequest()))
            {
                while (await call.ResponseStream.MoveNext())
                {
                    var person = call.ResponseStream.Current;
                    Console.WriteLine("A new person was added on the server, id={0}, name={1}", person.Id, person.Name);
                }
            }
        }
    }
}