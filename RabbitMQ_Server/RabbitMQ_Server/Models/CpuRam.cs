using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ_Server.Models
{
    public class CpuRam
    {
        public string Name { get; set; }
        public float CPU { get; set; }
        public float Ram { get; set; }
        public string ModifyDate { get; set; }
    }
}
