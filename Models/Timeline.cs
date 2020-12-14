using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class Timeline
    {
        public int ID { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public int? DisplayOrder { get; set; }
    }
}