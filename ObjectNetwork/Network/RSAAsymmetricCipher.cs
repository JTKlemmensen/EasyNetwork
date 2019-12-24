using ObjectNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ObjectNetwork.Network
{
    public class RSAAsymmetricCipher : IAsymmetricCipher
    {
        private RSA cipher;

        public RSAAsymmetricCipher()
        {
            cipher = RSA.Create();
        }

        public byte[] PublicKey
        {
            get => Encoding.ASCII.GetBytes(cipher.ToXmlString(false));
            set => cipher.FromXmlString(Encoding.ASCII.GetString(value));
        }

        public byte[] Decrypt(byte[] data)
        {
            return cipher.Decrypt(data, RSAEncryptionPadding.OaepSHA512);
        }

        public byte[] Encrypt(byte[] data)
        {
            return cipher.Encrypt(data, RSAEncryptionPadding.OaepSHA512);
        }
    }
}