using System;
using System.Collections.Generic;
using System.Text;

namespace EasyNetwork.Network.Abstract
{
    /// <summary>
    /// Cipher that encrypts and decrypts data asymmetrically.
    /// </summary>
    public interface IAsymmetricCipher
    {
        /// <summary>
        /// Gets or sets the public key which is used when encrypting data
        /// </summary>
        byte[] PublicKey { get; set; }

        /// <summary>
        /// Asymmetrically encrypts data based on the stored PublicKey
        /// </summary>
        byte[] Encrypt(byte[] data);

        /// <summary>
        /// Asymmetrically decrypts data based on the stored PrivateKey
        /// </summary>
        byte[] Decrypt(byte[] data);
    }
}