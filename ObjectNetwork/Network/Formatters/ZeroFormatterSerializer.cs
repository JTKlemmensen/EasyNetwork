using ObjectNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Network.Formatters
{
    public class ZeroFormatterSerializer : ISerializer
    {
        public byte[] Serialize<T>(T t)
        {
            try
            {
                return ZeroFormatter.ZeroFormatterSerializer.Serialize(t);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public T Deserialize<T>(byte[] arr)
        {
            try
            {
                return ZeroFormatter.ZeroFormatterSerializer.Deserialize<T>(arr);
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}