using System.Net;
using System.Net.Sockets;

namespace xe9c_gateway;

internal class Program
{
    static void Main(string[] args)
    {
        Xe9c_gateway gateway = new(args[0], args[1], int.Parse(args[2]));
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
                Console.WriteLine($"[{DateTime.Now}] [!] Была попытка подключения по http (полученное имя: {getClientName}). Клиент ({gateway.HideIP(clientSocketAddress)}***) забанен на 1 мин.");
                gateway._bannedIPs.Add(clientSocketAddress);
                // в бан на 1 минуту
                Task.Run(() =>
                {
                    Thread.Sleep(60000);
                    gateway._bannedIPs.Remove(clientSocketAddress);
                    Console.WriteLine($"[{DateTime.Now}] [i] Клиент разбанен: {gateway.HideIP(clientSocketAddress)}***");
                });
                clientSocket.Close();
                clientSocket.Dispose();
                continue;
            }
            gateway.SendGatewayName(clientSocket);
            Console.WriteLine($"[{DateTime.Now}] [+] Подключён клиент: {getClientName}");
            gateway.AddClient(getClientName, clientSocket);
            Task.Run(() => { gateway.HandleClient(clientSocket); });
        }
    }
}
