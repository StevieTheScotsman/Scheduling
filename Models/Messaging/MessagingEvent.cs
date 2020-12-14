using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class MessagingEvent
    {
        public int ID { get; set; }
        public string ShortDesc { get; set; }
        public string Method { get; set; }
        public string LongDesc { get; set; }
    }
}