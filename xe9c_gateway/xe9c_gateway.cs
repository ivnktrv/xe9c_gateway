using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace xe9c_gateway;

public class Xe9c_gateway
{       // список подключённых клиентов (вид: { сокет, имя_клиента })
    private Dictionary<Socket, string> _connectedClients = new();
    private string _ip = "127.0.0.1";
    private int _port = 32768;

    public string IP
    {
        get => _ip;
        private set { if (value != null) _ip = value; }
    }
    public int Port
    {
        get => _port;
        private set { if (value != null) _port = value; }
    }

    public Xe9c_gateway() { }

    public Xe9c_gateway(string ip, int port)
    {
        _ip = ip;
        _port = port;
    }

    public string GatewayInfo() // информация о шлюзе
    {
        return $"""
         ┌─────┐             ___      ___           _____
         │     │             \  \    /  /          /     \
         └──┬──┘              \  \  /  /   _____   |  O  |   ____
           ─┴─ \__ ┌──────┐    \  \/  /   /  _  \  \___  |  /  __|
                   |-   "'|    /  /\  \   |  ___/      | |  | /
                   └──────┘   /  /  \  \  |  \__   ____/ /  | \__
                     |       /__/    \__\  \____|  |____/   \____|
                      \
                     ┌─────┐      GATEWAY (v1.0)
                     │     │
                     └──┬──┘
                       ─┴─
                           
         GATEWAY INFO
        ┌────────────────
        |
        ├─ IP: {_ip}
        ├─ Port: {_port}
        
        [{DateTime.Now}] [...] Waiting for connection
        """;
    }
    public void AddClient(string clientName, Socket client)
    {
        _connectedClients.Add(client, clientName);
    }
    public void RemoveClient(Socket client)
    {
        _connectedClients.Remove(client);
    }

    public Socket CreateGateway()
    {
        IPEndPoint ipEndPoint = new(IPAddress.Any, _port);
        Socket __socket = new(
            AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        __socket.Bind(ipEndPoint);
        __socket.Listen();

        return __socket;
    }
    // ожидание сообщения
    public byte[] ReceiveMsg(Socket __socket)
    {
        try
        {
            byte[] getMsg = new byte[2048];
            __socket.Receive(getMsg);

            return getMsg;
        }
        catch (SocketException)
        {
            return Encoding.UTF8.GetBytes($"[INFO] Client disconnected: {_connectedClients[__socket]}");
        }
        catch (Exception ex)
        {
            return Encoding.UTF8.GetBytes($"[ERROR] {ex.Message}");
        }
    }
    // отправка сообщения подключённым клиентам
    public void BroadcastMsg(Socket __sender, byte[] msg)
    {
        foreach (var client in _connectedClients)
        {
            if (client.Key == __sender) continue;
            client.Key.Send(msg);
        }
    }
    // обслуживание клиента
    public void HandleClient(Socket __socket)
    {
        BroadcastMsg(
            __socket, 
            Encoding.UTF8.GetBytes(
                $"[INFO] Client connected: {_connectedClients[__socket]}"
                )
            );
        while (__socket.Connected)
        {
            byte[] buffer = ReceiveMsg(__socket);
            BroadcastMsg(__socket, buffer);
        }
        Console.WriteLine($"[{DateTime.Now}] [i] Client disconnected: {_connectedClients[__socket]}");
        RemoveClient(__socket);
        __socket.Close();
    }

    public string GetClientName(Socket __socket)
    {
        byte[] getName = new byte[32]; 
        __socket.Receive(getName);

        return Encoding.UTF8.GetString(getName);
    }
}
