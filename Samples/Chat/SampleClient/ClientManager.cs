using EasyNetwork.Network.Abstract;
using EasyNetwork.Network.Attributes;
using NetworkEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleClient
{
    public class ClientManager
    {
        [Command]
        public void OnClientConnected(IObjectConnection connection, ClientConnected info)
        {
            Console.WriteLine(info.Name + " has connected");
        }

        [Command]
        public void OnChatMessageReceived(IObjectConnection connection, Message message)
        {
            Console.WriteLine("[" + message.SenderName + "]: " + message.Content);
        }

        [Command]
        public void OnClientDisconnected(IObjectConnection connection, ClientDisconnected info)
        {
            Console.WriteLine(info.Name + " has disconnected");
        }

        [Disconnect]
        public void OnDisconnect(IObjectConnection connection)
        {
            Console.WriteLine("You have been disconnected from the server");
        }
    }
}