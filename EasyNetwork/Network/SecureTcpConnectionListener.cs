using ObjectNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ObjectNetwork.Network
{
    public class SecureTcpConnectionListener : IConnectionListener
    {
        private int port;
        public IEventManager Manager { get; set; }
        public IDataFormatters DataFormatters { get; set; }
        private bool run;

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

        private void Run()
        {
            ValidateProperties();

            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            while (run)
                if (listener.Pending())
                {
                    var socket = listener.AcceptSocket();
                    GetSocket getSocket = () => socket;
                    var secureConnection = new SecureServerConnection(new TcpConnection(getSocket), DataFormatters.AsymmetricCipher, DataFormatters.SymmetricCipher);
                    var objectConnection = new DefaultObjectConnection(secureConnection) { Manager = Manager, Serializer = DataFormatters.Serializer };
                    objectConnection.Start();
                }
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