using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Timers;

namespace Server
{
    class Program
    {
        private const int Port = 8888;
        private static UdpClient udpServer;
        private static Dictionary<string, string> components = new Dictionary<string, string>()
        {
            {"Processor", "480$" },
            {"RAM", "120$" },
            {"GPU", "680$" }
        };

        private static Dictionary<IPEndPoint, (int запросы, DateTime ПоследнийЗапрос)> clientRequest = new();
        private static int MaxRequestPerHour = 10;
        private static int maxClient = 100;
        private static TimeSpan inactiveTimeOut = TimeSpan.FromMinutes(10);
        private static System.Timers.Timer cleanUpTimer;
        static void Main(string[] args)
        {
            udpServer = new UdpClient(Port);
            Console.WriteLine($"The server is running on port {Port}");

            cleanUpTimer = new System.Timers.Timer(60000);
            cleanUpTimer.Elapsed += CleanupInactiveClients;
            cleanUpTimer.Start();

            Listen();
        }
        private static void Listen()
        {
            while (true)
            {
                IPEndPoint remoteEndPoint = null;
                byte[] data = udpServer.Receive(ref remoteEndPoint);
                string request = Encoding.UTF8.GetString(data);

                Log($"Receiver a request from {remoteEndPoint} : {request}");
                if(!IsClientAllowed(remoteEndPoint))
                {
                    string response = "Request likmit has been reached. try again later";
                    SendResponse(remoteEndPoint, response);
                    continue;
                }
                string price = GetPrice(request);
                SendResponse(remoteEndPoint, price);
            }
        }

        private static bool IsClientAllowed(IPEndPoint client)
        { 
            if(!clientRequest.ContainsKey(client))
            {
                if(clientRequest.Count >= maxClient)
                {
                    return false;
                }
                clientRequest[client] = (0, DateTime.Now);

            }
            var (count, lastRequest) = clientRequest[client];
            if((DateTime.Now - lastRequest).TotalHours >=1)
            {
                count = 0;
            }    
            if(count >= MaxRequestPerHour)
            {
                return false;
            }
            clientRequest[client] = (count + 1, DateTime.Now);
            return true;

        }

        private static void SendResponse(IPEndPoint client, string response)
        {
            byte[] data = Encoding.UTF8.GetBytes(response);
            udpServer.Send(data, 0, client);
            Log($"sent a reply to the client {client} : {response}");
        }

        private static string GetPrice(string component)
        {
            return components.ContainsKey(component) ?
                components[component] :
                "components not foud";
        }
        public static void CleanupInactiveClients(object sender, ElapsedEventArgs e)
        {
            var invactiveClients =new List<IPEndPoint>();

            foreach (var client in clientRequest)
            {
                if((DateTime.Now - client.Value.ПоследнийЗапрос) > inactiveTimeOut)
                {
                    invactiveClients.Add(client.Key);
                }
            }
            foreach (var client in invactiveClients)
            {
                clientRequest.Remove(client);
                Log($"client: {client} was shutDown due to inactivity");
            }
        }
        private static void Log(string Message)
        {
            Console.WriteLine(Message);
            File.AppendAllText("server_log.txt", $"{DateTime.Now}: {Message}\n");
        }
    }
}
