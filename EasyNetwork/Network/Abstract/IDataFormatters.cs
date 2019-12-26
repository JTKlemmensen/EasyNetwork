using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Network.Abstract
{
    /// <summary>
    /// Stores a ISerializer, ISymmetricCipher and a IAsymmetricCipher
    /// </summary>
    public interface IDataFormatters
    {
        ISerializer Serializer { get; }
        ISymmetricCipher SymmetricCipher { get; }
        IAsymmetricCipher AsymmetricCipher { get; }
    }
}