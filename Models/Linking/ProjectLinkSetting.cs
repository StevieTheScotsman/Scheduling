using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Scheduling.Models
{
    public class ProjectLinkSetting
    {
        public int ID { get; set; }
        public int PrimaryProfileTypeID { get; set; }        
        public int PrimaryMilestoneID { get; set; }        
        public int SecondaryProfileTypeID { get; set; }
        public int SecondaryMilestoneID { get; set; }
        [Required]
        public int CalculationID { get; set; }
        public int? PubCode { get; set; }
        public int? Timeline { get; set; }
        [Required]
        public string Name { get; set; }
       
    }

    public class ProjectLinkSettingDisplay
    {
        public int ID { get; set; }
        public string PrimaryProfileType { get; set; }
        public string PrimaryMilestone { get; set; }
        public string SecondaryProfileType { get; set; }
        public string SecondaryMilestone { get; set; }
        public string Calculation { get; set; }
        public string PubCode { get; set; }
        public string Timeline { get; set; }
        public string Name { get; set; }
    }
}