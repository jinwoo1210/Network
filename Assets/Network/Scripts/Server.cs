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

    private TcpListener listener;       // ������(���Թ�) : ����ϰ� �ִ� Ŭ���̾�Ʈ��
    private List<TcpClient> clients = new List<TcpClient>();    // ���� Ŭ���̾�Ʈ���� ����ؾ��ؼ� List �� ����

    private List<TcpClient> disconnects = new List<TcpClient>();     // ������ ���� Ŭ���̾�Ʈ��

    private IPAddress ip;
    private int port;

    private bool isOpened;  // ������ �����ִ��� Ȯ���ϴ� bool ��
    public bool IsOpened { get { return isOpened; } }

    private void Start()
    {   // ȣ��Ʈ�ʿ��� �����Ǹ� Ȯ��
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
        // ������ ���� �޼ҵ�
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

            // 1. ��Ʈ��ũ ��Ʈ�� Ȯ���غ���
            NetworkStream stream = client.GetStream();
            if (stream.DataAvailable)
            {   // ���� ������ �ִٸ�?
                StreamReader reader = new StreamReader(stream);
                string text = reader.ReadLine();
                AddLog(text);
                Debug.Log($"Server send message {text}");
                // �ؽ�Ʈ ������ �ٸ� Ŭ���̾�Ʈ ��ο��� �����Ѵ�.
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
    {   // ������ ���� �޼ҵ�
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
        //127.0.0.1 ������ ip : ���� ������ ���� �޴� �ڰ�ȸ�� ip
    }

    public void Close()
    {   // ������ �ݴ� �޼ҵ�
        listener?.Stop();
        listener = null;
        isOpened = false;
    }

    public void SendAll(string chat)
    {   // Ư�� Ŭ���̾�Ʈ�� �޼����� �ް�, ��� Ŭ���̾�Ʈ �޼����� ���� ������
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
    {// Ŭ���̾�Ʈ�� �����ؾ� �� ���� �� �ݹ��Լ�
        if (isOpened == false)
            return;
        // ���� �������� ��û�� Ŭ���̾�Ʈ �������� (EndAcceptTcpClient : ��� Ŭ���̾�Ʈ �޴� ���� ���߰�)
        TcpClient client = listener.EndAcceptTcpClient(ar);
        // Ŭ���̾�Ʈ ����Ʈ�� �߰�
        clients.Add(client);
        // �ٽ� Ŭ���̾�Ʈ�� ���� �غ�
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
