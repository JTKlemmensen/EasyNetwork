using EasyNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyNetwork.Network.Formatters
{
    public class DefaultDataFormatters : IDataFormatters
    {
        public ISerializer Serializer { get; } = new BinarySerializer();

        public ISymmetricCipher SymmetricCipher { get; } = new AESSymmetricCipher();

        public IAsymmetricCipher AsymmetricCipher => new RSAAsymmetricCipher();
    }
}