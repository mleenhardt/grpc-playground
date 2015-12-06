using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Playground.Common.ServiceDefinition;

namespace Playground.Server
{
    public class PersonRepository
    {
        private readonly ConcurrentDictionary<int, Person> _people = new ConcurrentDictionary<int, Person>();

        public class PersonCreatedEventArgs : EventArgs
        {
            public Person Person { get; private set; }

            public PersonCreatedEventArgs(Person person)
            {
                Person = person;
            }
        }

        public EventHandler<PersonCreatedEventArgs> PersonCreated;

        private void OnPersonCreated(Person person)
        {
            if (PersonCreated != null)
            {
                PersonCreated(this, new PersonCreatedEventArgs(person));
            }
        }

        public Person GetById(int id)
        {
            Person person;
            _people.TryGetValue(id, out person);
            return person;
        }

        public ICollection<Person> GetAllPeople()
        {
            return _people.Values;
        }

        public bool TryCreate(Person person)
        {
            if (_people.TryAdd(person.Id, person))
            {
                OnPersonCreated(person);
                return true;
            }
            return false;
        }
    }
}