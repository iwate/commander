using System.IO;

namespace Iwate.Commander
{
    public class InvokeRequest
    {
        public InvokeId Id { get; set; }

        public string Partition { get; set; }

        public string Command { get; set; }

        public Stream Payload { get; set; }
    }
}