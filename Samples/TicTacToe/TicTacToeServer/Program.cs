using EasyNetwork.Network;
using System;
using System.Threading.Tasks;

namespace TicTacToe
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(RunServer);
            Console.WriteLine("Stop the server by pressing Enter");
            Console.ReadLine();
        }

        static void RunServer()
        {
            var server = new ConnectionBuilder().CreateServer(25000);

            var manager = new ServerManager();
            var idleChecker = new IdleChecker(1);
            server.OnInboundConnection += (c) => 
            {
                c.AddEventHandler(idleChecker);
                c.AddEventHandler(manager); 
            };
            server.Start();
        }
    }
}