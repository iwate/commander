using System;

namespace Iwate.Commander
{
    public interface ICommandRegistry
    {
        bool TryLookup(string name, out Type type);
    }
}