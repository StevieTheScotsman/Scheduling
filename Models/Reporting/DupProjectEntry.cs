using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class DupProjectEntry
    {
        public int PubCodeFK { get; set; }
        public int ProjectRangeFK { get; set; }
        public int YearFK {get;set;}
        public int Count {get;set;}
    }
    
}