using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#if UNITY_UWP
using Windows.Networking.Sockets;
using System.Threading.Tasks;
using Windows.Storage.Streams;
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
        private MessageWebSocket ws;
        private Task task = null;
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

        public void CoonetServer(string url,bool stringflag=false)
        {
#if UNITY_UWP
            task = Task.Run(async () =>
            {
                ws = new MessageWebSocket();
                ws.Control.MessageType = SocketMessageType.Binary;
                if (stringflag == true) ws.Control.MessageType = SocketMessageType.Utf8;
                ws.MessageReceived += MessageReceived;
                ws.Closed += (sender, e) => { if (WebSocketCloseEvent != null) WebSocketCloseEvent("Close"); };
                try
                {
                    await ws.ConnectAsync(new Uri(url));
                }
                catch (Exception e)
                {
                    if (WebSocketErrorEvent != null) WebSocketErrorEvent(e.ToString());
                }
            });
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
            ws.MessageReceived -= MessageReceived;
            ws.Dispose();
            ws = null;
#elif UNITY_EDITOR || UNITY_STANDALONE
            ws.OnMessage -= OnMessage;
            ws.Close();
            ws = null;
#endif
        }

        public bool SendMessage(string data)
        {
#if UNITY_UWP
            if (ws != null && task.IsCompleted==true)
            {
                task = Task.Run(async () =>
                {
                    using (var datawriter=new DataWriter(ws.OutputStream))
                    {
                        datawriter.WriteString(data);
                        await datawriter.StoreAsync();
                        datawriter.DetachStream();
                    }
                });
                return true;
            }
#elif UNITY_EDITOR || UNITY_STANDALONE
            if (ws != null)
            {
                ws.Send(data);
                return true;
            }
#endif
            return false;
        }

        public bool SendMessage(byte[] data)
        {
#if UNITY_UWP
            if (ws != null && task.IsCompleted==true)
            {
                task = Task.Run(async () =>
                {
                    using (var datawriter=new DataWriter(ws.OutputStream))
                    {
                        datawriter.WriteBytes(data);
                        await datawriter.StoreAsync();
                        datawriter.DetachStream();
                    }
                });
                return true;
            }
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
        private void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            try
            {
                using (DataReader datareader = args.GetDataReader())
                {
                    var buffer = datareader.UnconsumedBufferLength;
                    byte[] buf = new byte[buffer];
                    datareader.ReadBytes(buf);
                    if (WebSocketByteEvent != null) WebSocketByteEvent(buf);

                    datareader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    string data = datareader.ReadString(datareader.UnconsumedBufferLength);
                    if (WebSocketMessageEvent != null) WebSocketMessageEvent(data);
                }
            }
            catch (Exception e)
            {
                if (WebSocketErrorEvent != null) WebSocketErrorEvent(e.ToString());
            }
        }
#elif UNITY_EDITOR || UNITY_STANDALONE
        private void OnMessage(object sender, MessageEventArgs e)
        {
            if (WebSocketByteEvent != null) WebSocketByteEvent(e.RawData);
            if (WebSocketMessageEvent != null) WebSocketMessageEvent(e.Data);
        }
#endif
    }
}
