using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class ProjectsGenerated
    {

        public int Year { get; set; }
        public int ScheduleType { get; set; }
        public string TimeLines { get; set; }
        public int? PubCode {get;set;}
        public int? ProductID { get; set; }

    }
}