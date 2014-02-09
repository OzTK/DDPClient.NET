namespace Net.DDP.Client
{
    public interface IDataSubscriber
    {
        void DataReceived(dynamic data);
        string Session { get; set; }
    }
}
