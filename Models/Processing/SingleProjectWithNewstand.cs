using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class SingleProjectWithNewstand
    {
        public int Year {get;set;}
        public int Profile { get; set; }
        public string NewsStandDate { get; set; }
        public string Product { get; set; }
        public string PubCode { get; set; }
        public string Timeline { get; set; }
    }
}