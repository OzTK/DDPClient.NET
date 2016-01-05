using Newtonsoft.Json;

namespace Net.DDP.Client
{
    public class DDPClient : IClient
    {
        public const string DDP_MESSAGE_TYPE_READY = "ready";
        public const string DDP_MESSAGE_TYPE_ADDED = "added";
        public const string DDP_MESSAGE_TYPE_CHANGED = "changed";
        public const string DDP_MESSAGE_TYPE_NOSUB = "nosub";
        public const string DDP_MESSAGE_TYPE_REMOVED = "removed";
		public const string DDP_MESSAGE_TYPE_RESULT = "result";
		public const string DDP_MESSAGE_TYPE_UPDATED = "updated";
		public const string DDP_MESSAGE_TYPE_CONNECTED = "connected";
		public const string DDP_MESSAGE_TYPE_FAILED = "failed";

        public const string DDP_PROPS_MESSAGE = "msg";
        public const string DDP_PROPS_ID = "id";
        public const string DDP_PROPS_COLLECTION = "collection";
        public const string DDP_PROPS_FIELDS = "fields";
        public const string DDP_PROPS_SESSION = "session";
        public const string DDP_PROPS_RESULT = "result";
        public const string DDP_PROPS_ERROR = "error";
        public const string DDP_PROPS_SUBS = "subs";
		public const string DDP_PROPS_METHODS = "methods";
		public const string DDP_PROPS_VERSION = "version";

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

        public void Connect(string url, bool useSSL)
        {
            _connector.Connect(url, useSSL);
        }

        public void Call(string methodName, params object[] args)
        {
            string message = string.Format("\"msg\": \"method\",\"method\": \"{0}\",\"params\": {1},\"id\": \"{2}\"", methodName, CreateJSonArray(args), NextId());
            message = "{" + message + "}";
            _connector.Send(message);
        }

        public int Subscribe(string subscribeTo, params string[] args)
        {
            string message = string.Format("\"msg\": \"sub\",\"name\": \"{0}\",\"params\": [{1}],\"id\": \"{2}\"", subscribeTo, CreateJSonArray(args), NextId());
            message = "{" + message + "}";
            _connector.Send(message);
            return GetCurrentRequestId();
        }

        private string CreateJSonArray(params object[] args)
        {
            if (args == null)
                return "[]";
            
            return JsonConvert.SerializeObject(args);
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
