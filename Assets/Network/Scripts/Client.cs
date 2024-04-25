using System;
using System.IO;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class Client : MonoBehaviour
{
    [SerializeField] Chat chat;

    [SerializeField] TMP_InputField nameField;
    [SerializeField] TMP_InputField ipField;
    [SerializeField] TMP_InputField portField;

    private TcpClient client;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    private string clientName;
    private string ip;
    private int port;

    private bool isConnected;
    public bool IsConnected { get { return isConnected; } }

    private void Update()
    {
        if (IsConnected == false)
            return;

        if (stream.DataAvailable == false)
            return;

        string text = reader.ReadLine();
        ReceiveChat(text);
    }


    public void Connect()
    {
        if (isConnected)        // 이미 접속된 상태라면 진행하지 않음.
            return;

        // 1. 접속을 위한 clientName, ip, port 저장
        clientName = nameField.text;
        ip = ipField.text;
        port = int.Parse(portField.text);

        // 2. Ip, Port로 접속
        try // 예외처리
        {
            client = new TcpClient(ip, port);
            stream = client.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            Debug.Log("Connect success");
            isConnected = true;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    public void DisConnect()
    {
        // 접속을 끊기 위한 함수.
        writer?.Close();
        writer = null;
        reader?.Close();
        reader = null;
        stream?.Close();
        stream = null;
        client?.Close();
        client = null;

        isConnected = false;
    }

    public void SendChat(string chatText)
    {
        if (IsConnected == false)
            return;
        Debug.Log($"Client send message : {chatText}");

        try
        {
            writer.WriteLine($"{clientName} : {chatText}");
            writer.Flush();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    public void ReceiveChat(string chatText)
    {
        Debug.Log($"Client receive message : {chatText}");
        chat.AddMessage(chatText);
    }

    private void AddMessage(string message)
    {
        Debug.Log($"[Client] {message}");
        chat.AddMessage($"[Client] {message}");
    }
}
