using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_UWP
#elif UNITY_EDITOR || UNITY_STANDALONE
using WebSocketSharp;
using WebSocketSharp.Server;
#endif

namespace WebSocketConteol
{
    public class WebSocketServerManager
    {
#if UNITY_UWP
#elif UNITY_EDITOR || UNITY_STANDALONE
        private WebSocketServer server;
#endif

        public WebSocketServerManager(int port)
        {
#if UNITY_UWP
#elif UNITY_EDITOR || UNITY_STANDALONE
            server = new WebSocketServer(port);
            server.AddWebSocketService<WebSocketMessage>("/");
            server.Start();
#endif
        }

        public void DisconnectClient()
        {
#if UNITY_UWP
#elif UNITY_EDITOR || UNITY_STANDALONE
            server.Stop();
            server = null;
#endif
        }
    }

#if UNITY_UWP
#elif UNITY_EDITOR || UNITY_STANDALONE
    public class WebSocketMessage : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Debug.Log(e.IsText);
            if (e.IsText == true)
            {
                Sessions.Broadcast(e.Data);
            }
            else
            {
                Sessions.Broadcast(e.RawData);
            }
        }
    }
#endif
}
