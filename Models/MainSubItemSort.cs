using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class MainSubItemSort
    {
        public int ID { get; set; }
        public int? PubCodeFK { get;set; }
        public int? ProjectRangeFK {get;set;}
        public int MilestoneFieldSubItemFK {get;set;}
        public int SortOrder {get;set;}
    }

    

}