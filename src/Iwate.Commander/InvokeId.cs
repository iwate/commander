using System;

namespace Iwate.Commander
{
    public struct InvokeId
    {
        private readonly string _value;
        private InvokeId(string value)
        {
            _value = value;
        }
        public static InvokeId NewInvokeId() => new InvokeId(Ulid.NewUlid().ToString());
        public static InvokeId Parse(string value) => new InvokeId(Ulid.Parse(value).ToString());
        public override string ToString() => _value;
        public override int GetHashCode() => _value.GetHashCode();
    }
}