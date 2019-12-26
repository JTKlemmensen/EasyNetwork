using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Network.Abstract
{
    /// <summary>
    /// Base class for SecureClientConnection & SecureServerConnection. 
    /// Encrypts outgoing messages and decrypts incoming messages
    /// </summary>
    public abstract class SecureConnection : IConnection
    {
        protected IConnection Connection { get; set; }
        protected ISymmetricCipher SymmetricCipher { get; set; }
        protected IAsymmetricCipher AsymmetricCipher { get; set; }

        public string Ip => Connection.Ip;

        public event DataReceived OnDataReceived;
        public event Disconnected OnDisconnected;
        public virtual event Connected OnConnected;

        public SecureConnection(IConnection connection, IAsymmetricCipher asymmetricCipher, ISymmetricCipher symmetricCipher)
        {
            this.Connection = connection;
            this.SymmetricCipher = symmetricCipher;
            this.AsymmetricCipher = asymmetricCipher;
            connection.OnDataReceived += OnEncryptedMessageReceived;
            connection.OnDisconnected += () => OnDisconnected?.Invoke();
        }

        protected virtual void OnEncryptedMessageReceived(byte[] data)
        {
            data = SymmetricCipher.Decrypt(data);
            OnDataReceived.Invoke(data);
        }

        /// <summary>
        /// Sends a byte array to a remote peer after encrypting it
        /// </summary>
        public void SendData(byte[] data)
        {
            data = SymmetricCipher.Encrypt(data);
            Connection.SendData(data);
        }

        public void Stop()
        {
            Connection.Stop();
        }

        public virtual void Start()
        {
            Connection.Start();
        }
    }
}