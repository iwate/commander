using System;
using System.Collections.Generic;

namespace Iwate.Commander
{
    internal class CommandRegistry : ICommandRegistry
    {
        private readonly IDictionary<string, Type> _registry = new Dictionary<string, Type>();

        public IEnumerable<Type> Values => _registry.Values;

        public void Append<TCommand>() where TCommand : ICommand
        {
            var type = typeof(TCommand);
            _registry.Add(type.Name, type);
        }

        public bool TryLookup(string name, out Type type)
        {
            return _registry.TryGetValue(name, out type);
        }
    }
}
