using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketConteol;

/// <summary>
/// WebSocket送受信サンプル
/// </summary>
public class WebSocketControlSample : MonoBehaviour {

    private WebSocketClientManager client;
    private WebSocketServerManager server;
    // Use this for initialization
    void Start () {
        server = new WebSocketServerManager(3000);
        client = new WebSocketClientManager("ws://192.168.1.7:3000/", WebSocketClientManager.DataType.Text);
        client.ReceiveTextMessage += (ms) => Debug.Log(ms);
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
    }
}
