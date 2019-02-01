using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketConteol;

/// <summary>
/// WebSocket送受信サンプル
/// </summary>
public class WebSocketControlSample : MonoBehaviour
{
    /// <summary>
    /// 受信文字列表示用
    /// </summary>
    [SerializeField] private Text text;
    /// <summary>
    /// サーバーアドレス
    /// </summary>
    [SerializeField] private string ipAddress = "192.168.1.7";
    /// <summary>
    /// 接続ポート
    /// </summary>
    [SerializeField] private int port = 3000;

    private WebSocketClientManager client;
    private WebSocketServerManager server;

    private string localSting;
    // Use this for initialization
    void Start () {
        server = new WebSocketServerManager(port);
        client = new WebSocketClientManager("ws://" + ipAddress + ":" + port + "/",
            WebSocketClientManager.DataType.Text);
        client.OnWebSocketError += (ms) => localSting = ms;
        client.ReceiveTextMessage += (ms) => localSting = ms;
    }

    void OnDestroy()
    {
        client.DisconnectServer();
        server.DisconnectClient();
    }

    // Update is called once per frame
    void Update ()
    {
        if (Input.GetKeyUp(KeyCode.Space)) client.SendMessage("Hello World");
        if (localSting != null) text.text += localSting + Environment.NewLine;
        localSting = null;
    }
}
