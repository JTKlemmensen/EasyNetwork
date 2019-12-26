using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Network.Abstract
{
    /// <summary>
    /// Cipher that encrypts and decrypts data symmetrically.
    /// </summary>
    public interface ISymmetricCipher
    {
        /// <summary>
        /// Gets or sets the Key part of symmetric algorithm
        /// </summary>
        byte[] Key { get; set; }

        /// <summary>
        /// Gets or sets the Initialization Vector part of symmetric algorithm
        /// </summary>
        byte[] IV { get; set; }

        /// <summary>
        /// Encrypts data based on the Key and IV properties.
        /// </summary>
        byte[] Encrypt(byte[] data);

        /// <summary>
        /// Decrypts data based on the Key and IV properties.
        /// </summary>
        byte[] Decrypt(byte[] data);
    }
}