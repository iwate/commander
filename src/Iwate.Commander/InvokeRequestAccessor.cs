using System;

namespace Iwate.Commander
{
    internal class InvokeRequestAccessor
    {
        private InvokeRequest _value = null;
        public void Set(InvokeRequest value) => _value = value;
        public InvokeRequest Get() => _value ?? throw new InvalidOperationException("Not initialized");
    }
}
