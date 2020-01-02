using EasyNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EasyNetwork.Network
{
    public class SecureTcpConnectionListener : IConnectionListener
    {
        private int port;
        public IDataFormatters DataFormatters { get; set; }
        private bool run;

        public event InboundConnection OnInboundConnection;

        public SecureTcpConnectionListener(int port)
        {
            this.port = port;
        }

        public void Start()
        {
            run = true;
            Task.Run(Run);
        }

        public void Stop()
        {
            run = false;
        }

        private async Task Run()
        {
            ValidateProperties();

            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            while (run)
                if (listener.Pending())
                {
                    GetSocket getSocket = () => listener.AcceptSocket();
                    var secureConnection = new SecureServerConnection(new TcpConnection(getSocket), DataFormatters.AsymmetricCipher, DataFormatters.SymmetricCipher);
                    var objectConnection = new DefaultObjectConnection(secureConnection) { Serializer = DataFormatters.Serializer };
                    OnInboundConnection?.Invoke(objectConnection);
                    await objectConnection.Start();
                }
        }

        private void ValidateProperties()
        {
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