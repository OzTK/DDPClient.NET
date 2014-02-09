using System.Text;

namespace Net.DDP.Client
{
    public class DDPClient : IClient
    {
        public const string DDP_TYPE_CONNECTED = "connected";
        public const string DDP_TYPE_READY = "ready";
        public const string DDP_TYPE_ADDED = "added";

        public const string DDP_PROPS_MESSAGE = "msg";
        public const string DDP_PROPS_ID = "id";
        public const string DDP_PROPS_COLLECTION = "collection";
        public const string DDP_PROPS_FIELDS = "fields";
        public const string DDP_PROPS_SESSION = "session";

        private readonly DDPConnector _connector;
        private int _uniqueId;
        private readonly ResultQueue _queueHandler;

        public DDPClient(IDataSubscriber subscriber)
        {
            _connector = new DDPConnector(this);
            _queueHandler = new ResultQueue(subscriber);
            _uniqueId = 1;
        }

        public void AddItem(string jsonItem)
        {
            _queueHandler.AddItem(jsonItem);
        }

        public void Connect(string url)
        {
            _connector.Connect(url);
        }

        public void Call(string methodName, params string[] args)
        {
            string message = string.Format("\"msg\": \"method\",\"method\": \"{0}\",\"params\": [{1}],\"id\": \"{2}\"", methodName, CreateJSonArray(args), NextId());
            message = "{" + message+ "}";
            _connector.Send(message);
        }

        public int Subscribe(string subscribeTo, params string[] args)
        {
            string message = string.Format("\"msg\": \"sub\",\"name\": \"{0}\",\"params\": [{1}],\"id\": \"{2}\"", subscribeTo,CreateJSonArray(args), NextId());
            message = "{" + message + "}";
            _connector.Send(message);
            return GetCurrentRequestId();
        }

        private string CreateJSonArray(params string[] args)
        {
            if (args == null)
                return string.Empty;

            StringBuilder argumentBuilder = new StringBuilder();
            string delimiter=string.Empty;
            for (int i = 0; i < args.Length; i++)
            {
                argumentBuilder.Append(delimiter);
                argumentBuilder.Append(args[i]);
                delimiter = ",";
            }
            
            return argumentBuilder.ToString();
        }
        private int NextId()
        {
            return _uniqueId++;
        }

        public int GetCurrentRequestId()
        {
            return _uniqueId;
        }

    }
}
