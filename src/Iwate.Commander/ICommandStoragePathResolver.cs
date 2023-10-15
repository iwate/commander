namespace Iwate.Commander
{
    public interface ICommandStoragePathResolver
    {
        string GetStatePath(string id);
        string GetQueueDirPath(string partition);
        string GetQueuePath(InvokeRequestBase request);
        bool TryParseQueue(string path, out InvokeRequestBase request);
    }
}