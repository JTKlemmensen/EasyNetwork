using ObjectNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
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

        private string protocol = null;
        private void Connection_OnMessageReceived(byte[] data)
        {
            if (protocol == null)
                protocol = Serializer.Deserialize<string>(data);
            else
            {
                Manager.CallCommand(protocol, data, this);
                protocol = null;
            }
        }

        public void SendObject<T>(T t)
        {
            connection.SendData(Serializer.Serialize(t.GetType().FullName));
            connection.SendData(Serializer.Serialize(t));
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