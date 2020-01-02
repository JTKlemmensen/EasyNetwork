using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkEntities
{
    [Serializable]
    public class Message
    {
        public string SenderName { get; set; }
        public string Content { get; set; }
    }
}
