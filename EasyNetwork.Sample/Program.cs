using EasyNetwork.Network;
using EasyNetwork.Network.Abstract;
using EasyNetwork.Network.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyNetwork.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            RunTestChat();
        }

        static void RunTestChat()
        {
            Task.Run(RunServer);
            Thread.Sleep(1000);
            Task.Run(RunClient);

            Thread.Sleep(25000);
        }

        static void RunClient()
        {
            Console.WriteLine("RunClient");
            var client = new ConnectionBuilder()
                .CreateClient("127.0.0.1", 25000);

            client.AddEventHandler(new IdleChecker());
            client.OnConnect(c =>
            {
                Console.WriteLine("Lambda client connected");
            });

            
            client.OnDisconnect(c => 
            {
                Console.WriteLine("Lambda Client Disconnected");
            });

            object creator = new object();
            client.OnCommand<string>((c, s) => 
            {
                Console.WriteLine("Lambda Client received: " + s);
                client.RemoveOnCommand(creator);
                client.SendObject("I just removed command eventhandler!");
            }, creator);
            
            client.AddEventHandler(new ClientHandler());
            client.Start();
        }

        static void RunServer()
        {
            var listener = new ConnectionBuilder()
                .CreateServer(25000);

            listener.OnInboundConnection += (c) =>
            {
                c.OnConnect(c2 => 
                {
                    Console.WriteLine("Server Lambda: a client connected");
                    c2.AddEventHandler(new ServerHandler());
                    c2.AddEventHandler(new IdleChecker());
                    c2.SendObject("hello");
                });
            };

            listener.Start();
        }
    }

    public class ClientHandler
    {
        [Connect]
        public void OnConnect(IObjectConnection connection)
        {
            Console.WriteLine("[Client] OnConnect SDAConnected");
        }

        [Command]
        public void OnMessage(IObjectConnection connection, string message)
        {
            Console.WriteLine("[Client] Received message: " + message);
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
            connection.Stop();
        }

        [Disconnect]
        public void OnDisconnect(IObjectConnection connection)
        {
            Console.WriteLine("[Server] Disconnected");
        }
    }
}