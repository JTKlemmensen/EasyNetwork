using ObjectNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ObjectNetwork.Network
{
    public class TcpConnection : IConnection
    {
        protected static int DefaultPort = 25000;
        private GetSocket getSocket;
        private Socket socket;
        private BinaryWriter writer;
        private bool stop;

        public event Disconnected OnDisconnected;
        public event Connected OnConnected;
        public event DataReceived OnDataReceived;

        public TcpConnection(GetSocket getSocket)
        {
            this.getSocket = getSocket;
        }

        public void SendData(byte[] data)
        {
            if (writer != null && stop != true)
            {
                writer.Write(data.Length);
                writer.Write(data);
                writer.Flush();
            }
        }

        public void Stop()
        {
            stop = true;
        }

        private void Run()
        {
            try
            {
                socket = getSocket();
                OnConnected?.Invoke();
                NetworkStream stream = new NetworkStream(socket);
                stream.ReadTimeout = 100;
                using (BinaryReader reader = new BinaryReader(stream))
                using (writer = new BinaryWriter(stream))
                {
                    while (!stop)
                        try
                        {
                            var length = reader.ReadInt32();
                            var data = reader.ReadBytes(length);
                            OnDataReceived.Invoke(data);
                        }
                        catch (IOException)
                        {
                        }
                }

            }
            catch (Exception) { }
            finally
            {
                OnDisconnected?.Invoke();
            }
        }

        public void Start()
        {
            stop = false;
            new Thread(Run).Start();
        }
    }

    public delegate Socket GetSocket();
}