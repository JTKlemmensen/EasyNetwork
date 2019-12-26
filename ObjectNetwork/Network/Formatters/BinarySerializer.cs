using ObjectNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ObjectNetwork.Network.Formatters
{
    public class BinarySerializer : ISerializer
    {
        public byte[] Serialize<T>(T t)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, t);
                return ms.ToArray();
            }
        }

        public T Deserialize<T>(byte[] arr)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                ms.Write(arr,0,arr.Length);
                ms.Seek(0, SeekOrigin.Begin);

                return (T)bf.Deserialize(ms);
            }
        }
    }
}