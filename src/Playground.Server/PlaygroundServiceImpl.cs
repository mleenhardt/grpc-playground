using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Playground.Common.ServiceDefinition;

namespace Playground.Server
{
    public class PlaygroundServiceImpl : PlaygroundService.IPlaygroundService
    {
        private readonly PersonRepository _personRepository;
        private readonly ManualResetEventSlim _manualResetEvent = new ManualResetEventSlim(false);
        
        public PlaygroundServiceImpl(PersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        private void LogRpc(ServerCallContext context)
        {
            Console.WriteLine("{0} -- Receiving RPC call, method={1}, host={2}, peer={3}",
                DateTime.Now, context.Method, context.Host, context.Peer);
        }

        public Task<Person> GetPersonById(PersonId request, ServerCallContext context)
        {
            LogRpc(context);
            Person person = _personRepository.GetById(request.Id);
            return Task.FromResult(person);
        }

        public async Task GetPersonList(PersonListRequest request, IServerStreamWriter<Person> responseStream, ServerCallContext context)
        {
            LogRpc(context);
            foreach (Person person in _personRepository.GetAllPeople())
            {
                await responseStream.WriteAsync(person);
            }
        }

        public async Task<PersonListResponse> CreatePeople(IAsyncStreamReader<Person> requestStream, ServerCallContext context)
        {
            LogRpc(context);
            while (await requestStream.MoveNext(CancellationToken.None))
            {
                var person = requestStream.Current;
                _personRepository.TryCreate(person);
            }
            return new PersonListResponse { People = { _personRepository.GetAllPeople() } };
        }

        public Task ListenForNewPeople(ListenForNewPeopleRequest request, IServerStreamWriter<Person> responseStream, ServerCallContext context)
        {
            LogRpc(context);
            _personRepository.PersonCreated += async (sender, arg) => await responseStream.WriteAsync(arg.Person);
            _manualResetEvent.Wait();
            return Task.FromResult(0);
        }

        public void Shutdown()
        {
            _manualResetEvent.Set();
        }
    }
}