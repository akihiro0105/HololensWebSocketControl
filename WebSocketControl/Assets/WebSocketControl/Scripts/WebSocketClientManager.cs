using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#if UNITY_UWP
#elif UNITY_EDITOR || UNITY_STANDALONE
using WebSocketSharp;
#endif

namespace WebSocketConteol
{
    public class WebSocketClientManager
    {
        public delegate void WebSocketMessageEventHandler(string ms);
        public WebSocketMessageEventHandler WebSocketMessageEvent;

        public delegate void WebSocketByteEventHandler(byte[] data);
        public WebSocketByteEventHandler WebSocketByteEvent;

        public delegate void WebSocketLogEventHandler(string ms);
        public WebSocketLogEventHandler WebSocketOpenEvent;
        public WebSocketLogEventHandler WebSocketErrorEvent;
        public WebSocketLogEventHandler WebSocketCloseEvent;

#if UNITY_UWP
#elif UNITY_EDITOR || UNITY_STANDALONE
        private WebSocket ws;
#endif
        public WebSocketClientManager()
        {

        }

        public WebSocketClientManager(string url)
        {
            CoonetServer(url);
        }

        public void CoonetServer(string url)
        {
#if UNITY_UWP
#elif UNITY_EDITOR || UNITY_STANDALONE
            ws = new WebSocket(url);
            ws.OnMessage += OnMessage;
            ws.OnOpen += (sender, e) => { if (WebSocketOpenEvent != null) WebSocketOpenEvent("Open"); };
            ws.OnError += (sender, e) => { if (WebSocketErrorEvent != null) WebSocketErrorEvent(e.Message); };
            ws.OnClose += (sender, e) => { if (WebSocketCloseEvent != null) WebSocketCloseEvent("Close"); };
            ws.ConnectAsync();
#endif
        }

        public void DisconnectServer()
        {
#if UNITY_UWP
#elif UNITY_EDITOR || UNITY_STANDALONE
            ws.OnMessage -= OnMessage;
            ws.Close();
#endif
        }

        public bool SendMessage(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            return SendMessage(bytes);
        }

        public bool SendMessage(byte[] data)
        {
#if UNITY_UWP
#elif UNITY_EDITOR || UNITY_STANDALONE
            if (ws != null)
            {
                ws.Send(data);
                return true;
            }
#endif
            return false;
        }

#if UNITY_UWP
#elif UNITY_EDITOR || UNITY_STANDALONE
        private void OnMessage(object sender, MessageEventArgs e)
        {
            if (WebSocketByteEvent != null) WebSocketByteEvent(e.RawData);
            string data = Encoding.UTF8.GetString(e.RawData);
            if (WebSocketMessageEvent != null) WebSocketMessageEvent(data);
        }
#endif
    }
}
