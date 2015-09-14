using System;
using WebSocket4Net;

namespace Net.DDP.Client
{
    internal class DDPConnector
    {
        private WebSocket _socket;
        private string _url = string.Empty;
        private int _isWait;
        private readonly IClient _client;

        public DDPConnector(IClient client)
        {
            this._client = client;
        }

        public void Connect(string url, bool useSSL = true)
        {
            _url = string.Format("{0}://{1}/websocket", useSSL ? "wss" : "ws", url);
            _socket = new WebSocket(_url);
            _socket.MessageReceived += socket_MessageReceived;
            _socket.Opened += _socket_Opened;
            _socket.Open();
            _isWait = 1;
            this.Wait();
            if (_socket.State != WebSocketState.Open)
            {
                throw new SocketException();
            }
        }

        public void Close()
        {
            _socket.Close();
        }

        public void Send(string message)
        {
            _socket.Send(message);
        }

        void _socket_Opened(object sender, EventArgs e)
        {
            this.Send("{\"msg\":\"connect\",\"version\":\"pre1\",\"support\":[\"pre1\"]}");
            _isWait = 0;
        }

        void socket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            this._client.AddItem(e.Message);
        }

        private void Wait()
        {
            while (_isWait != 0 && _socket.State == WebSocketState.Connecting)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

    }
}
