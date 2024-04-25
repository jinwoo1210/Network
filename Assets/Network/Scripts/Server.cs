using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class Server : MonoBehaviour
{
    [SerializeField] RectTransform logContent;
    [SerializeField] TMP_Text logTextPrefab;
    [SerializeField] TMP_InputField ipField;
    [SerializeField] TMP_InputField portField;

    private TcpListener listener;       // 리스너(출입문) : 통신하고 있는 클라이언트를
    private List<TcpClient> clients = new List<TcpClient>();    // 여러 클라이언트들을 통신해야해서 List 로 보관

    private List<TcpClient> disconnects = new List<TcpClient>();     // 접속이 끊긴 클라이언트들

    private IPAddress ip;
    private int port;

    private bool isOpened;  // 서버가 열려있는지 확인하는 bool 값
    public bool IsOpened { get { return isOpened; } }

    private void Start()
    {   // 호스트쪽에서 아이피를 확인
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        ip = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        ipField.text = ip.ToString();
    }

    private void OnDestroy()
    {
        if (isOpened)
        {
            Close();
        }
    }

    private void Update()
    {
        // 서버를 여는 메소드
        if (isOpened == false)
            return;

        foreach (TcpClient client in clients)
        {
            Debug.Log("for");

            if (CheckClient(client) == false)
            {
                client.Close();
                disconnects.Add(client);
                continue;
            }

            // 1. 네트워크 스트림 확인해보기
            NetworkStream stream = client.GetStream();
            if (stream.DataAvailable)
            {   // 받을 내용이 있다면?
                StreamReader reader = new StreamReader(stream);
                string text = reader.ReadLine();
                AddLog(text);
                Debug.Log($"Server send message {text}");
                // 텍스트 내용을 다른 클라이언트 모두에게 전달한다.
                SendAll(text);
            }
        }

        foreach (TcpClient client in disconnects)
        {
            clients.Remove(client);
        }
        disconnects.Clear();
    }

    public void Open()
    {   // 서버를 여는 메소드
        if (isOpened)
            return;

        AddLog("Try to Open");

        port = int.Parse(portField.text);
        try
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            isOpened = true;
            listener.BeginAcceptTcpClient(AcceptCallback, listener);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        //127.0.0.1 루프백 ip : 내가 보내고 내가 받는 자가회신 ip
    }

    public void Close()
    {   // 서버를 닫는 메소드
        listener?.Stop();
        listener = null;
        isOpened = false;
    }

    public void SendAll(string chat)
    {   // 특정 클라이언트가 메세지를 받고, 모든 클라이언트 메세지를 전부 보내기
        foreach (TcpClient client in clients)
        {
            NetworkStream stream = client.GetStream();
            StreamWriter writer = new StreamWriter(stream);

            try
            {
                writer.WriteLine(chat);
                writer.Flush();
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
    }

    private void AcceptCallback(IAsyncResult ar)
    {// 클라이언트가 접속해쓸 때 실행 할 콜백함수
        if (isOpened == false)
            return;
        // 가장 마지막에 요청한 클라이언트 가져오기 (EndAcceptTcpClient : 잠시 클라이언트 받는 것을 멈추고)
        TcpClient client = listener.EndAcceptTcpClient(ar);
        // 클라이언트 리스트에 추가
        clients.Add(client);
        // 다시 클라이언트를 받을 준비
        listener.BeginAcceptTcpClient(AcceptCallback, listener);
    }

    private void AddLog(string message)
    {
        Debug.Log($"[Server] {message}");
        TMP_Text newLog = Instantiate(logTextPrefab, logContent);
        newLog.text = message;
    }

    private bool CheckClient(TcpClient client)
    {
        try
        {
            if (client == null)
                return false;

            if (client.Connected == false)
                return false;

            bool check = client.Client.Poll(0, SelectMode.SelectRead);
            if (check)
            {
                int size = client.Client.Receive(new byte[1], SocketFlags.Peek);
                if (size == 0)
                {
                    return false;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            return false;
        }
    }
}
