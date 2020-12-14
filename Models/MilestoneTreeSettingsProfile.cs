using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Scheduling.Models
{
    public class MilestoneTreeSettingsProfile
    {
        public int ID { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int ProjectType { get; set; }
        public string ReportFooterNote { get; set; }
    }
}