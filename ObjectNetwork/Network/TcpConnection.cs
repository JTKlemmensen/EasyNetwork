using ObjectNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectNetwork.Network
{
    /// <summary>
    /// Implements a tcp connection to a remote peer.
    /// </summary>
    public class TcpConnection : IConnection
    {
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

        private readonly object writeLock = new object();

        private string ip;
        public string Ip => ip;

        public void SendData(byte[] data)
        {
            if (writer != null && stop != true && data != null)
                lock (writeLock)
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

                if(socket.RemoteEndPoint is IPEndPoint endPoint)
                    ip = endPoint.Address.ToString();

                NetworkStream stream = new NetworkStream(socket);
                stream.ReadTimeout = 100;
                using (BinaryReader reader = new BinaryReader(stream))
                using (writer = new BinaryWriter(stream))
                {
                    Task.Run(() => OnConnected?.Invoke());
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
                Task.Run(() =>OnDisconnected?.Invoke());
            }
        }

        public void Start()
        {
            stop = false;
            new Task(Run, TaskCreationOptions.LongRunning).Start();
        }
    }

    public delegate Socket GetSocket();
}