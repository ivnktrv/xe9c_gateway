using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace xe9c_gateway;

public class Xe9c_gateway
{   /// <summary>
    /// список подключённых клиентов (вид: { сокет, имя_клиента })
    /// </summary>
    private readonly Dictionary<Socket, string> _connectedClients = [];
    public List<IPAddress> _bannedIPs = [];
    private string _gatewayName = "None";
    private string _ip = "127.0.0.1";
    private int _port = 32768;

    public string GatewayName
    {
        get => _gatewayName;
        set { if (value != null) _gatewayName = value; }
    }
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

    public Xe9c_gateway(string gatewayName, string ip, int port)
    {
        _gatewayName = gatewayName;
        _ip = ip;
        _port = port;
    }

    /// <summary>
    /// Информация о шлюзе
    /// </summary>
    public virtual string GatewayInfo()
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
                     ┌─────┐      GATEWAY (v2.0)
                     │     │
                     └──┬──┘
                       ─┴─
                           
         ИНФА О ШЛЮЗЕ
        ┌────────────────
        │
        ├─ Имя: {_gatewayName}
        ├─ IP: {_ip}
        ├─ Порт: {_port}
        
        [{DateTime.Now}] [...] Ожидаю подключений
        """;
    }

    /// <summary>
    /// Добавить клиента в список подключённых клиентов
    /// </summary>
    /// <param name="clientName"></param>
    /// <param name="client"></param>
    public void AddClient(string clientName, Socket client)
    {
        _connectedClients.Add(client, clientName);
    }

    /// <summary>
    /// Удалить клиента из списка подключённых клиентов
    /// </summary>
    /// <param name="client"></param>
    public void RemoveClient(Socket client)
    {
        _connectedClients.Remove(client);
    }

    /// <summary>
    /// Создание сокета
    /// </summary>
    /// <returns>Созданный сокет шлюза</returns>
    public Socket CreateGateway()
    {
        IPEndPoint ipEndPoint = new(IPAddress.Parse(_ip), _port);
        Socket __socket = new(
            AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        __socket.Bind(ipEndPoint);
        __socket.Listen();

        return __socket;
    }

    /// <summary>
    /// Принятие сообщения от клиента
    /// </summary>
    /// <param name="__socket"></param>
    /// <returns>Полученное сообщение в виде массива байтов</returns>
    private protected virtual byte[] ReceiveMsg(Socket __socket)
    {
        try
        {
            byte[] getMsg = new byte[2048];
            __socket.Receive(getMsg);

            return getMsg;
        }
        catch (SocketException)
        {
            return Encoding.UTF8.GetBytes($"[INFO] Клиент отключился: {_connectedClients[__socket]}");
        }
        catch (Exception ex)
        {
            return Encoding.UTF8.GetBytes($"[ERROR] {ex.Message}");
        }
    }

    /// <summary>
    /// Отправка сообщения подключённым клиентам
    /// </summary>
    /// <param name="__socket"></param>
    /// <param name="msg"></param>
    private protected virtual void BroadcastMsg(Socket __socket, byte[] msg)
    {
        foreach (Socket client in _connectedClients.Keys.ToList())
        {
            if (client == __socket) continue;
            try
            {
                client.Send(msg);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"[{DateTime.Now}] [i] Была попытка отправить сообщение несуществующему клиенту. Удаляю клиента из списка (Подробнее: {ex.Message})");
                RemoveClient(client);
            }
        }
    }

    /// <summary>
    /// Обслуживание клиента. То есть ожидаем сообщение от него и это сообщение
    /// отправляем всем подключённым клиентам
    /// </summary>
    public virtual void HandleClient(Socket __socket)
    {
        try
        {
            BroadcastMsg(
                __socket,
                Encoding.UTF8.GetBytes(
                    $"[INFO] Подключён клиент: {_connectedClients[__socket]}"
                    )
                );
            while (__socket.Connected)
            {
                byte[] buffer = ReceiveMsg(__socket);
                BroadcastMsg(__socket, buffer);
            }
            Console.WriteLine($"[{DateTime.Now}] [i] Клиент отключился: {_connectedClients[__socket]}");
            RemoveClient(__socket);
            __socket.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now}] [ERROR] {ex}");
        }
    }

    /// <summary>
    /// Получить имя клиента
    /// </summary>
    /// <returns>Имя клиента в виде строки (string)</returns>
    public virtual string GetClientName(Socket __socket)
    {
        byte[] getName = new byte[32]; 
        __socket.Receive(getName);

        return Encoding.UTF8.GetString(getName);
    }

    /// <summary>
    /// Отправить имя шлюза
    /// </summary>
    public virtual void SendGatewayName(Socket __socket)
    {
        byte[] sendName = Encoding.UTF8.GetBytes(_gatewayName);
        __socket.Send(sendName);
    }
}
