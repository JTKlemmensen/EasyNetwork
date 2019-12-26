using EasyNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EasyNetwork.Network
{
    public class SecureTcpConnectionProvider : IConnectionProvider
    {
        public IEventManager Manager { get; set; }
        public IDataFormatters DataFormatters { get; set; }

        public IObjectConnection Create(string ip, int port)
        {
            ValidateProperties();

            GetSocket getSocket = () =>
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveBufferSize = 2048;
                socket.SendBufferSize = 2048;
                socket.NoDelay = true;
                socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));

                return socket;
            };

            var secureConnection = new SecureClientConnection(new TcpConnection(getSocket), DataFormatters.AsymmetricCipher, DataFormatters.SymmetricCipher);
            var objectConnection = new DefaultObjectConnection(secureConnection) {Manager = Manager, Serializer = DataFormatters.Serializer };

            return objectConnection;
        }

        private void ValidateProperties()
        {
            if (Manager == null)
                throw new Exception("The EventManager cannot be null");

            if (DataFormatters == null)
                throw new Exception("The DataFormatters cannot be null");

            if (DataFormatters.AsymmetricCipher == null)
                throw new Exception("The AsymmetricCipher cannot be null");

            if (DataFormatters.SymmetricCipher == null)
                throw new Exception("The SymmetricCipher cannot be null");

            if (DataFormatters.Serializer == null)
                throw new Exception("The Serializer cannot be null");
        }
    }
}