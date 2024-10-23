using System.Net.Sockets;

namespace xe9c_gateway;

internal class Program
{
    static void Main(string[] args)
    {
        Xe9c_gateway gateway = new(args[1], int.Parse(args[2]));
        Console.WriteLine(gateway.GatewayInfo());
        Socket s = gateway.CreateGateway();

        while (true)
        {
            Socket clientSocket = s.Accept();
            string getClientName = gateway.GetClientName(clientSocket);
            Console.WriteLine($"[{DateTime.Now}] [+] Connected client: {getClientName}");
            gateway.AddClient(getClientName, clientSocket);
            Task.Run(() => { gateway.HandleClient(clientSocket); });
        }
    }
}
