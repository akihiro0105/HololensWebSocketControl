using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#if UNITY_UWP
using Windows.Networking.Sockets;
using System.Threading.Tasks;
using Windows.Storage.Streams;
#elif UNITY_EDITOR || UNITY_STANDALONE
using WebSocketSharp;
#endif

namespace WebSocketConteol
{
    /// <summary>
    /// WebSocketクライアント側機能
    /// </summary>
    public class WebSocketClientManager
    {
        /// <summary>
        /// データタイプ
        /// </summary>
        public enum DataType
        {
            Byte,
            Text
        }
        private DataType dataType = DataType.Byte;

        /// <summary>
        /// 受信データイベント
        /// </summary>
        public event Action<string> ReceiveTextMessage;

        public event Action<byte[]> ReceiveByteMessage;

        /// <summary>
        /// WebSocketの接続，エラー，切断状態イベント
        /// </summary>
        public event Action OnWebSocketOpen;
        public event Action<string> OnWebSocketError;
        public event Action OnWebSocketClose;

#if UNITY_UWP
        private MessageWebSocket ws;
        private Task task = null;
#elif UNITY_EDITOR || UNITY_STANDALONE
        private WebSocket ws;
#endif

        /// <summary>
        /// クライアント起動
        /// </summary>
        /// <param name="url"></param>
        /// <param name="type"></param>
        public WebSocketClientManager(string url, DataType type)
        {
            dataType = type;
#if UNITY_UWP
            task = Task.Run(async () =>
            {
                ws = new MessageWebSocket();
                ws.Control.MessageType = (dataType == DataType.Byte) ? SocketMessageType.Binary : SocketMessageType.Utf8;
                ws.MessageReceived += MessageReceived;
                ws.Closed += (sender, e) => { if (OnWebSocketClose != null) OnWebSocketClose.Invoke(); };
                try
                {
                    await ws.ConnectAsync(new Uri(url));
                }
                catch (Exception e)
                {
                    if (OnWebSocketError != null) OnWebSocketError.Invoke(e.ToString());
                }
            });
#elif UNITY_EDITOR || UNITY_STANDALONE
            ws = new WebSocket(url);
            ws.OnMessage += OnMessage;
            ws.OnOpen += (sender, e) =>
            {
                if (OnWebSocketOpen != null) OnWebSocketOpen.Invoke();
            };
            ws.OnError += (sender, e) =>
            {
                if (OnWebSocketError != null) OnWebSocketError.Invoke(e.Message);
            };
            ws.OnClose += (sender, e) =>
            {
                if (OnWebSocketClose != null) OnWebSocketClose.Invoke();
            };
            ws.ConnectAsync();
#endif
        }

        /// <summary>
        /// クライアント機能を停止
        /// </summary>
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

        /// <summary>
        /// テキストデータ送信
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void SendMessage(string data)
        {
            if (dataType == DataType.Byte)
            {
                SendMessage(Encoding.UTF8.GetBytes(data));
            }
            else
            {
#if UNITY_UWP
                if (ws != null && task.IsCompleted == true)
                {
                    task = Task.Run(async () =>
                    {
                        using (var writer = new DataWriter(ws.OutputStream))
                        {
                            writer.WriteString(data);
                            await writer.StoreAsync();
                            writer.DetachStream();
                        }
                    });
                }
#elif UNITY_EDITOR || UNITY_STANDALONE
                if (ws != null) ws.Send(data);
#endif
            }
        }

        /// <summary>
        /// byteデータ送信
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void SendMessage(byte[] data)
        {
            if (dataType == DataType.Byte)
            {
#if UNITY_UWP
                if (ws != null && task.IsCompleted == true)
                {
                    task = Task.Run(async () =>
                    {
                        using (var writer = new DataWriter(ws.OutputStream))
                        {
                            writer.WriteBytes(data);
                            await writer.StoreAsync();
                            writer.DetachStream();
                        }
                    });
                }
#elif UNITY_EDITOR || UNITY_STANDALONE
                if (ws != null) ws.Send(data);
#endif
            }
            else
            {
                SendMessage(Encoding.UTF8.GetString(data));
            }
        }

        /// <summary>
        /// 受信データイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
#if UNITY_UWP
        private void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            try
            {
                using (DataReader datareader = args.GetDataReader())
                {
                    byte[] data = null;
                    string text = null;
                    if (args.MessageType==SocketMessageType.Binary)
                    {
                        data = new byte[datareader.UnconsumedBufferLength];
                        datareader.ReadBytes(data);
                        text = Encoding.UTF8.GetString(data);
                    }
                    else
                    {
                        datareader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                        text = datareader.ReadString(datareader.UnconsumedBufferLength);
                        data = Encoding.UTF8.GetBytes(text);
                    }
                    if (ReceiveByteMessage != null) ReceiveByteMessage(data);
                    if (ReceiveTextMessage != null) ReceiveTextMessage(text);
                }
            }
            catch (Exception e)
            {
                if (OnWebSocketError != null) OnWebSocketError.Invoke(e.ToString());
            }
        }
#elif UNITY_EDITOR || UNITY_STANDALONE
        private void OnMessage(object sender, MessageEventArgs e)
        {
            var data = (e.IsBinary) ? e.RawData : Encoding.UTF8.GetBytes(e.Data);
            var text = (e.IsBinary) ? Encoding.UTF8.GetString(e.RawData) : e.Data;
            if (ReceiveByteMessage != null) ReceiveByteMessage(data);
            if (ReceiveTextMessage != null) ReceiveTextMessage(text);
        }
#endif
    }
}
