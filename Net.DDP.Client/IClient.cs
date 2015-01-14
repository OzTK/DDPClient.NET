namespace Net.DDP.Client
{
    public interface IClient
    {
        void AddItem(string jsonItem);
        void Connect(string url);
        void Connect(string url, bool useSSL);
        void Call(string methodName, params string[] args);
        int Subscribe(string methodName, params string[] args);
        int GetCurrentRequestId();
    }
}
