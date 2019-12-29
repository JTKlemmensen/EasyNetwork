using EasyNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EasyNetwork.Network.Formatters
{
    public class RSAAsymmetricCipher : IAsymmetricCipher
    {
        private RSACryptoServiceProvider cipher;

        public RSAAsymmetricCipher()
        {
            cipher = new RSACryptoServiceProvider(2048);
        }

        public byte[] PublicKey
        {
            get => Encoding.ASCII.GetBytes(cipher.ToXmlString(false));
            set => cipher.FromXmlString(Encoding.ASCII.GetString(value));
        }

        public byte[] Decrypt(byte[] data)
        {
            return cipher.Decrypt(data, true);
        }

        public byte[] Encrypt(byte[] data)
        {
            return cipher.Encrypt(data, true);
        }
    }
}