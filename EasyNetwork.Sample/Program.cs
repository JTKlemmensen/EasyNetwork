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
            var client = new ConnectionBuilder()
                .AddEventHandler(new ClientHandler())
                .AddEventHandler(new IdleChecker())
                .CreateClient("127.0.0.1", 25000);

            client.OnConnect(async c => 
            {
                var result = await c.SendObject<string>("1 2 3");
                Console.WriteLine("Test: "+ result); 
            });

            client.OnDisconnect(c => { Console.WriteLine("Lambda Client Disconnected"); });
            client.OnCommand<string>((c, s) => Console.WriteLine("Lambda Client received: "+s));
            
            client.Start();
            Thread.Sleep(1000);
            //client.SendObject("Hello and welcome client!");
        }

        static async Task<string> Send(string s)
        {
            return await Task<string>.Run(async () => { await Task.Delay(5000); return "response"; });
        }

        static void RunServer()
        {
            var listener = new ConnectionBuilder()
                .AddEventHandler(new ServerHandler())
                .AddEventHandler(new IdleChecker())
                .CreateServer(25000);

            listener.OnInboundConnection += (c) =>
            {
                c.OnConnect(c2 => Console.WriteLine("Server Lambda: a client connected"));
            };

            listener.Start();
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
            //connection.SendObject("Hello server");
            //connection.Stop();
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
        }

        [Command]
        public void OnMessage(IObjectConnection connection, string message)
        {
            Console.WriteLine("[Server] Received message: " + message);
            connection.SendObject("Hello client");
        }

        [Disconnect]
        public void OnDisconnect(IObjectConnection connection)
        {
            Console.WriteLine("[Server] A Client disconnected");
        }
    }
}