using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace HW_UDP
{
     class Program
    {
        private const int PORT = 8888;
        static void Main()
        {
            UdpClient udpClient = new UdpClient();
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, PORT);

            Console.WriteLine("enter the name component or for out 'exit'");

            while (true)
            {
                Console.WriteLine("request: ");
                string request = Console.ReadLine();
                if(request?.ToLower() == "exit")
                {
                    break;
                }
                byte[] data = Encoding.UTF8.GetBytes(request);
                udpClient.Send(data,0,serverEndPoint);

                IPEndPoint remoteEndPoint = null;
                byte[] response = udpClient.Receive(ref remoteEndPoint);
                string responseText = Encoding.UTF8.GetString(response);
                Console.WriteLine($"response from server {responseText}");
            }
            udpClient.Close();
        }
    }
    
}
