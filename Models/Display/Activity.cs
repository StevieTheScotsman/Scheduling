using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    
    public class Activity
    {
        public int ID { get; set; }
        public string Message { get; set; }
        public DateTime DateModified { get; set; }
        public string ModifiedBy { get; set; }
        public int? MilestoneValueFk { get; set; }
        public int? ProjectFK { get; set; }
    }

    public class ActivityDisplay
    {

        public int ID { get; set; }
        public string Message { get; set; }
        public string DateModified { get; set; }
        public string ModifiedBy { get; set; }
        public string MilestoneFieldName { get; set; }
        public string ProjectFieldName { get; set; }

    }
}