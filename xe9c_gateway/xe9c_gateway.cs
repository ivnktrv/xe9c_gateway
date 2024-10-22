using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace xe9c_gateway;

public class Xe9c_gateway
{
    private List<Socket> _connectedClients = new();
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

    public string GatewayInfo()
    {
        return $"### GATEWAY INFO ###\n\nIP: {_ip}\nPort: {_port}";
    }

    public void AddClient(Socket client)
    {
        _connectedClients.Add(client);
    }
    public void RemoveClient(Socket client)
    {
        _connectedClients.Remove(client);
    }

    public Socket CreateGateway()
    {
        IPEndPoint ipEndPoint = new(IPAddress.Parse(_ip), _port);
        Socket __socket = new(
            AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        __socket.Bind(ipEndPoint);
        __socket.Listen();

        return __socket;
    }

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
            return Encoding.UTF8.GetBytes($"[INFO] Client disconnected");
        }
        catch (Exception ex)
        {
            return Encoding.UTF8.GetBytes($"[ERROR] {ex.Message}");
        }
    }

    public void BroadcastMsg(Socket __sender, byte[] msg)
    {
        foreach (Socket client in _connectedClients)
        {
            if (client == __sender) continue;
            client.Send(msg);
        }
    }

    public void HandleClient(Socket __socket)
    {
        while (__socket.Connected)
        {
            byte[] buffer = ReceiveMsg(__socket);
            BroadcastMsg(__socket, buffer);
        }
        RemoveClient(__socket);
        __socket.Close();
    }
}
