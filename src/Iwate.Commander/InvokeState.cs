namespace Iwate.Commander
{
    public interface IInvokeState
    {
        InvokeId Id { get; set; }
        
        InvokeStatus Status { get; set; }
    }

    public class InvokeState : IInvokeState
    {
        public InvokeId Id { get; set; }

        public InvokeStatus Status { get; set; }
    }
}