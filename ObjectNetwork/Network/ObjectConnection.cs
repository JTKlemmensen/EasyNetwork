using ObjectNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Network
{
    public class ObjectConnection
    {
        private IConnection connection;
        private IEventManager manager;
        private ISerializer serializer;
        private bool hasBeenStarted;
        private bool hasBeenStopped;
        private static int idGenerator = 0;
        public int Id { get; } = idGenerator++;

        public ObjectConnection(IConnection connection, IEventManager manager, ISerializer serializer)
        {
            this.connection = connection;
            this.manager = manager;
            this.serializer = serializer;
            connection.OnDataReceived += Connection_OnMessageReceived;
        }

        private string protocol = null;
        private void Connection_OnMessageReceived(byte[] data)
        {
            if (protocol == null)
                protocol = serializer.Deserialize<string>(data);
            else
            {
                manager.CallCommand(protocol, data, this);
                protocol = null;
            }
        }

        public void SendObject<T>(T t)
        {
            connection.SendData(serializer.Serialize(t.GetType().FullName));
            connection.SendData(serializer.Serialize(t));
        }

        public void Stop()
        {
            if (hasBeenStopped)
                return;

            hasBeenStopped = true;
            hasBeenStarted = true;
            connection.OnDisconnected += () => manager.CallDisconnect(this);
            connection.Stop();
        }

        public void Start()
        {
            if (hasBeenStarted)
                return;

            hasBeenStarted = true;
            connection.OnConnected += () => manager.CallConnect(this);
            connection.Start();
        }
    }
}