using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class EditProjectWithMilestones
    {
        public int ID { get; set; }
        public string PubCodeDesc { get; set; }
        public string ProductDesc { get; set; }
        public string Name { get; set; }
        public string DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public string ProfileDesc { get; set; }
        public string Timeline { get; set; }
        public string Year { get; set; }
        public string NewstandDate { get; set; }
        public int CurrentProjectStatus { get; set; }
        public int CurrentVersion { get; set; }
        public int? IsLocked { get; set; }

        public List<MilestoneValue> MileValueList {get;set;}
    }

    
}