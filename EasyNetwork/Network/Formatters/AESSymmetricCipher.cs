using EasyNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EasyNetwork.Network.Formatters
{
    public class AESSymmetricCipher : ISymmetricCipher
    {
        private Aes cipher;

        public AESSymmetricCipher()
        {
            cipher = Aes.Create();
            cipher.KeySize = 128;
        }

        public byte[] Key
        {
            get => cipher.Key;
            set => cipher.Key = value;
        }

        public byte[] IV
        {
            get => cipher.IV;
            set => cipher.IV = value;
        }

        public byte[] Decrypt(byte[] data)
        {
            return EncryptOrDecrypt(data, cipher.CreateDecryptor());
        }

        public byte[] Encrypt(byte[] data)
        {
            return EncryptOrDecrypt(data, cipher.CreateEncryptor());
        }

        private byte[] EncryptOrDecrypt(byte[] data, ICryptoTransform type)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, type, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}