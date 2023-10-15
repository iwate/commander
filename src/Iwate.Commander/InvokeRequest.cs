using System.IO;

namespace Iwate.Commander
{
    public class InvokeRequestBase
    {
        public string Id { get; set; }

        public string Partition { get; set; }

        public string Command { get; set; }

        public string InvokedBy { get; set; }
    }

    public class InvokeRequest : InvokeRequestBase
    {
        public Stream Payload { get; set; }
    }
}