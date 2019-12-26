using System;
using System.Collections.Generic;
using System.Text;

namespace EasyNetwork.Network
{
    [Serializable]
    public class NetworkMessage
    {
        public string Name { get; set; }
        public byte[] Data { get; set; } 
    }
}