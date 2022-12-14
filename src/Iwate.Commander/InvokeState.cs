namespace Iwate.Commander
{
    public interface IInvokeState
    {
        string Id { get; set; }
        
        string Partition { get; set; }

        string Command {  get; set; }

        string InvokedBy {  get; set; }
        
        InvokeStatus Status { get; set; }
    }

    public class InvokeState : IInvokeState
    {
        public string Id { get; set; }

        public string Partition { get; set; }

        public string Command { get; set; }

        public string InvokedBy {  get; set; }
        
        public InvokeStatus Status { get; set; }
    }
}