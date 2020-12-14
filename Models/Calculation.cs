using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    
    public class Calculation
    {
        public int ID { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public string Diff { get; set; }
        public int IncWeekends { get; set; }
        public int IncHolidays { get; set; }
    }

}