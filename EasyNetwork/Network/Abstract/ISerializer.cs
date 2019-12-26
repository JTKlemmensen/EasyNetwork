using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Network.Abstract
{
    public interface ISerializer
    {
        /// <summary>
        /// Converts an object to a byte array
        /// </summary>
        byte[] Serialize<T>(T t);

        /// <summary>
        /// Converts a byte array to an object of type T
        /// </summary>
        T Deserialize<T>(byte[] arr);
    }
}