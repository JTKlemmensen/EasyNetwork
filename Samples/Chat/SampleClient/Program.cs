using EasyNetwork.Network;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(RunClient);
            while (true) 
                Thread.Sleep(50);
        }

        static void RunClient()
        {
            var client = new ConnectionBuilder().CreateClient("127.0.0.1", 25000);

            client.AddEventHandler(new IdleChecker(1));
            client.AddEventHandler(new ClientManager());
            client.Start();

            while (true)
            {
                string message = Console.ReadLine();
                client.SendObject(message);
            }
        }
    }
}