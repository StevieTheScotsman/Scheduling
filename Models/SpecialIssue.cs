using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class SpecialIssue
    {
        public int ID { get; set; }
        public int? PubCodeFK { get; set; }
        public int YearFk { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public DateTime? NewsstandDate { get; set; }
    }
}