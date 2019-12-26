using EasyNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyNetwork.Network
{
    public class SecureClientConnection : SecureConnection
    {
        private bool handshakeDone;
        private bool receivedIV;
        public override event Connected OnConnected;

        public SecureClientConnection(IConnection connection, IAsymmetricCipher asymmetricCipher, ISymmetricCipher symmetricCipher) 
            : base(connection, asymmetricCipher, symmetricCipher)
        {

        }

        protected override void OnEncryptedMessageReceived(byte[] data)
        {
            if (handshakeDone)
                base.OnEncryptedMessageReceived(data);
            else
                EndHandshake(data);
        }

        public override void Start()
        {
            base.Start();
            StartHandshake();
        }

        private void StartHandshake()
        {
            Connection.SendData(AsymmetricCipher.PublicKey);
        }

        /// <summary>
        /// Ends handshake after receiving both IV and PublicKey.
        /// </summary>
        private void EndHandshake(byte[] data)
        {
            if (!receivedIV)
            {
                SymmetricCipher.IV = AsymmetricCipher.Decrypt(data);
                receivedIV = true;
            }
            else
            {
                SymmetricCipher.Key = AsymmetricCipher.Decrypt(data);
                handshakeDone = true;

                OnConnected?.Invoke();
            }
        }
    }
}