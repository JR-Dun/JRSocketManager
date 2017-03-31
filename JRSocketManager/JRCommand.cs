using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace JRSocketManager
{
    [DataContract]
    class JRCommand
    {
        [DataMember]
        public JROP key { get; set; }

        [DataMember]
        public string content { get; set; }
    }

    public enum JROP
    {
        start = 1,
        stop = 2,
    };
}
