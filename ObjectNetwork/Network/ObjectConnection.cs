using ObjectNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ObjectNetwork.Network
{
    public class ObjectConnection
    {
        private IConnection connection;
        public IEventManager Manager { get; set; }
        public ISerializer Serializer { get; set; }
        private bool hasBeenStarted;
        private bool hasBeenStopped;
        private static int idGenerator = 0;
        public int Id { get; } = idGenerator++;

        public ObjectConnection(IConnection connection)
        {
            this.connection = connection;
            connection.OnDataReceived += Connection_OnMessageReceived;
        }

        private void Connection_OnMessageReceived(byte[] data)
        {
            var message = Serializer.Deserialize<NetworkMessage>(data);

            Manager.CallCommand(message.Name, message.Data, this);
        }

        public void SendObject<T>(T t)
        {
            var message = new NetworkMessage { Name= t.GetType().FullName, Data = Serializer.Serialize(t) };
            connection.SendData(Serializer.Serialize(message));
        }

        public void Stop()
        {
            if (hasBeenStopped)
                return;

            hasBeenStopped = true;
            hasBeenStarted = true;
            connection.OnDisconnected += () => Manager.CallDisconnect(this);
            connection.Stop();
        }

        public void Start()
        {
            if (hasBeenStarted)
                return;

            hasBeenStarted = true;
            connection.OnConnected += () => Manager.CallConnect(this);
            connection.Start();
        }
    }
}