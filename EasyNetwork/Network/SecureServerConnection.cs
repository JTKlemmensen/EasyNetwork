using ObjectNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Network
{
    public class SecureServerConnection : SecureConnection
    {
        public override event Connected OnConnected;
        private bool handshakeDone;

        public SecureServerConnection(IConnection connection, IAsymmetricCipher asymmetricCipher, ISymmetricCipher symmetricCipher)
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

        /// <summary>
        /// Ends handshake by sending the symmetric iv and key to the remote peer
        /// </summary>
        private void EndHandshake(byte[] data)
        {
            AsymmetricCipher.PublicKey = data;
            Connection.SendData(AsymmetricCipher.Encrypt(SymmetricCipher.IV));
            Connection.SendData(AsymmetricCipher.Encrypt(SymmetricCipher.Key));
            handshakeDone = true;

            OnConnected?.Invoke();
        }
    }
}