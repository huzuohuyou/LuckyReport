using WebSocketSharp;
using WebSocketSharp.Server;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;

namespace LuckyReport.Server.Services
{
    public class Chat : WebSocketBehavior
    {
        private Dictionary<IWebSocketSession,string> conns
        = new Dictionary<IWebSocketSession,string>();
        private string _suffix;

        public Chat()
        {
            _suffix = String.Empty;
        }

        public string Suffix
        {
            get
            {
                return _suffix;
            }

            set
            {
                _suffix = value ?? String.Empty;
            }
        }

        protected override void OnOpen()
        {
            Console.WriteLine();
        }

        protected override void OnError(ErrorEventArgs e)
        {
            Console.WriteLine();
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Sessions.Broadcast(e.Data + _suffix);
        }
    }
}
