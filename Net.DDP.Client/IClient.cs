namespace Net.DDP.Client
{
    public interface IClient
    {
        void AddItem(string jsonItem);
        void Connect(string url);
        void Call(string methodName, params object[] args);
        int Subscribe(string methodName, params string[] args);
        int GetCurrentRequestId();
    }
}
