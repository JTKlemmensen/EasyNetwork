using NUnit.Framework;
using EasyNetwork.Network;
using EasyNetwork.Network.Abstract;
using EasyNetwork.Network.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyNetwork.Test
{
    public class TestBuilder
    {
        [Test]
        public void Test()
        {
            Task.Run(RunServer);
            Thread.Sleep(500);
            Task.Run(RunClient);

            Thread.Sleep(25000);
        }

        private void RunClient()
        {
            new ConnectionBuilder()
                .AddEventHandler(new TestBuilder())
                .AddEventHandler(new IdleChecker())
                .CreateClient("127.0.0.1", 25000).Start();
        }

        private void RunServer()
        {
            var listener = new ConnectionBuilder()
                .AddEventHandler(new TestBuilder())
                .AddEventHandler(new IdleChecker())
                .CreateServer(25000);
            listener.Start();
        }

        [Connect]
        public void OnConnect(IObjectConnection connection)
        {
            Debug.WriteLine("Ip: " + connection.Ip);
            Task.Run(Slow);
            //connection.SendObject("Hello world!");
        }

        private async Task Slow()
        {
            await Task.Delay(500);
            Debug.WriteLine("Connected0000000");
        }

        [Disconnect]
        public void OnConnect2(DefaultObjectConnection connection)
        {
            Debug.WriteLine("DISCONNECTET"); 
        }

        [Command]
        public void OnMessage(DefaultObjectConnection connection, PingObject message)
        {
            Debug.WriteLine("Received Message: ");
        }
    }
}