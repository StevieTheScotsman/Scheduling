using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class Holiday
    {
        public int ID { get; set; }
        public int MonthFK { get; set; }
        public int YearFk { get; set; }
        public int Digit { get; set; }
    }
}