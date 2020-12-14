using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class Project
    {
        public int YearID { get; set; }
        public int? PubCode { get; set; }
        public int? Product { get; set; }
        public int ProjectTypeID { get; set; }
        public int? ProfitCenter { get; set; }
        public int? Printer { get; set; }
        public int MilestoneTreeSettingsProfileID { get; set; }
        public int? ViewProfileID { get; set; }
        public int? ProjectRangeID {get;set; }
        public int CurrentVersion { get; set; }
        public int CurrentVersionWorkflowState { get; set; }
        public int EventProfileID { get; set; }
        public string LastReviewedBy { get; set; }
        public DateTime? DateLastReviewed { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateLastLocked { get; set; }
        public String LastLockedBy { get; set; }
        public DateTime? DateLastUnlocked { get; set; }
        public string LastUnlockedBy { get; set; }
        public int? CurrentProjectStatus { get; set; }
        public int? CurrentProjectStatusWorkflowState { get; set; }
        public int IsLocked { get; set; }
        public string Comments { get; set; }   
    }

  
}