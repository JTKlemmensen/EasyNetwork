using EasyNetwork.Network;
using EasyNetwork.Network.Abstract;
using EasyNetwork.Network.Attributes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyNetwork.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(RunServer);
            Thread.Sleep(500);
            Task.Run(RunClient);

            Thread.Sleep(25000);
        }

        static void RunClient()
        {
            new ConnectionBuilder()
                .AddEventHandler(new ClientHandler())
                .AddEventHandler(new IdleChecker())
                .CreateClient("127.0.0.1", 25000);
        }

        static void RunServer()
        {
            new ConnectionBuilder()
                .AddEventHandler(new ServerHandler())
                .AddEventHandler(new IdleChecker())
                .CreateServer(25000);
        }
    }

    public class ClientHandler
    {
        
        [Connect]
        public void OnConnect(IObjectConnection connection)
        {
            Console.WriteLine("[Client] Connected");
        }
        
        [Command]
        public void OnMessage(IObjectConnection connection, string message)
        {
            Console.WriteLine("[Client] Received message: " + message);
            connection.SendObject("Hello server");
            connection.Stop();
        }
        
        [Disconnect]
        public void OnDisconnect(IObjectConnection connection)
        {
            Console.WriteLine("[Client] Disconnected");
        }
    }

    public class ServerHandler
    {
        [Connect]
        public void OnConnect(IObjectConnection connection)
        {
            Console.WriteLine("[Server] A Client connected");
            connection.SendObject("Hello and welcome client!");
        }

        [Command]
        public void OnMessage(IObjectConnection connection, string message)
        {
            Console.WriteLine("[Server] Received message: " + message);
        }

        [Disconnect]
        public void OnDisconnect(IObjectConnection connection)
        {
            Console.WriteLine("[Server] A Client disconnected");
        }
    }
}
