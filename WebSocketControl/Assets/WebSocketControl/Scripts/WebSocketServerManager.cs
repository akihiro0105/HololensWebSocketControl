using System.Collections;
using System.Collections.Generic;
#if UNITY_UWP
#elif UNITY_EDITOR || UNITY_STANDALONE
using WebSocketSharp;
using WebSocketSharp.Server;
#endif

namespace WebSocketConteol
{
    /// <summary>
    /// WebSocketサーバー側機能(UWPは未対応)
    /// </summary>
    public class WebSocketServerManager
    {
#if UNITY_UWP
#elif UNITY_EDITOR || UNITY_STANDALONE
        private WebSocketServer server;
#endif

        /// <summary>
        /// サーバー起動
        /// </summary>
        /// <param name="port"></param>
        public WebSocketServerManager(int port)
        {
#if UNITY_UWP
#elif UNITY_EDITOR || UNITY_STANDALONE
            server = new WebSocketServer(port);
            server.AddWebSocketService<WebSocketMessage>("/");
            server.Start();
#endif
        }

        /// <summary>
        /// サーバー停止
        /// </summary>
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
    /// <summary>
    /// 受信データに対する動作を定義
    /// </summary>
    public class WebSocketMessage : WebSocketBehavior
    {
        /// <summary>
        /// 受信データを接続されているデバイス全体に送信
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMessage(MessageEventArgs e)
        {
            // 受信データ内容が文字かbyteデータか判断して切り分け
            if (e.IsText == true)Sessions.Broadcast(e.Data);
            else Sessions.Broadcast(e.RawData);
        }
    }
#endif
}
