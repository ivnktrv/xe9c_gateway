using System.Net;
using System.Net.Sockets;

namespace xe9c_gateway;

internal class Program
{
    static void Main(string[] args)
    {
        Xe9cLog log = new();
        Xe9c_gateway gateway = new(args[0], args[1], int.Parse(args[2]), log);

        if (args[1].Length > 32)
        {
            Console.WriteLine("[-] Длина имени не должна превышать 32 символа");
            return;
        }
        Console.WriteLine(gateway.GatewayInfo());
        Socket s = gateway.CreateGateway();

        while (true)
        {
            Socket clientSocket = s.Accept();
            IPAddress clientSocketAddress = ((IPEndPoint)clientSocket.RemoteEndPoint).Address;
            // если клиент в списке бана, отключаем его
            if (gateway._bannedIPs.Contains(clientSocketAddress))
            {
                clientSocket.Close();
                clientSocket.Dispose();
                continue;
            }
            string getClientName = gateway.GetClientName(clientSocket);
            // проверяем, является ли это http подключением
            if (getClientName.Contains("HTTP"))
            {
                log.Logging($"Была попытка подключения по http (полученное имя: {getClientName}). Клиент ({gateway.HideIP(clientSocketAddress)}***) забанен на 1 мин.", Xe9cLog.LoggingLevel.Warning);
                gateway._bannedIPs.Add(clientSocketAddress);
                // в бан на 1 минуту
                Task.Run(() =>
                {
                    Thread.Sleep(60000);
                    gateway._bannedIPs.Remove(clientSocketAddress);
                    log.Logging($"Клиент разбанен: {gateway.HideIP(clientSocketAddress)}***", Xe9cLog.LoggingLevel.Info);
                });
                clientSocket.Close();
                clientSocket.Dispose();
                continue;
            }
            gateway.SendGatewayName(clientSocket);
            log.Logging($"Подключён клиент: {getClientName}", Xe9cLog.LoggingLevel.Info);
            gateway.AddClient(getClientName, clientSocket);
            Task.Run(() => { gateway.HandleClient(clientSocket); });
        }
    }
}
