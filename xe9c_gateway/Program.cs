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
            string getClientName = gateway.GetClientName(clientSocket);
            gateway.SendGatewayName(clientSocket);
            Console.WriteLine($"[{DateTime.Now}] [+] Подключён клиент: {getClientName}");
            gateway.AddClient(getClientName, clientSocket);
            Task.Run(() => { gateway.HandleClient(clientSocket); });
        }
    }
}
