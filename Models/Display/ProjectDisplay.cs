using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    //still to determine if ProjectType is viable
    public class ProjectDisplay
    {

        public int ID { get; set; }
        public int? PubCodeFK { get; set; }
        public int? ProductFK { get; set; }
        public int? ProjectTypeFK { get; set; }
        public int YearFK { get; set; }
        public int? ProfitCenter { get; set; }
        public int? PrinterFK { get; set; }
        public int? MilestoneTreeSettingsProfileFK { get; set; }
        public int? ViewProfileFK { get; set; }
        public int? ProjectRangeFK { get; set; }
        public int CurrentVersion { get; set; }
        public int CurrentProjectVersionWorkflowState { get; set; }
        public int? EventProfileFK { get; set; }
        public string LastReviewedBy { get; set; }
        public DateTime? DateLastReviewed { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? DateLastModified { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateLastLocked { get; set; }
        public string LastLockedBy { get; set; }
        public DateTime? DateLastUnlocked { get; set; }
        public string LastUnlockedBy { get; set; }
        public int CurrentProjectStatus { get; set; }
        public int CurrentProjectStatusWorkflowState { get; set; }
        public int? IsLocked { get; set; }
        public string Comments { get; set; }
        public string Name { get; set; }
        public int? ProjectRangeSortOrder { get; set; }
    }


    public class ReportProjectDisplayViewModel
    {
        public List<ProjectDisplay> ProjectDisplayList { get; set; }
      
        public string PubCodeSummary { get; set; }
        public List<ReportProjectDisplayViewModelSection> DisplaySections {get;set;}
    }

    public class ReportProjectDisplayViewModelSection
    {
        public ProjectDisplaySectionHeader SectionHeader;
        public List<MilestoneDisplayRecord> TopRecords;
        public List<SubItemRow> SubItemRows;
    }

    public class MilestoneDisplayRecord
    {
        public string Description { get; set; }
        public string Value { get; set; }
        public string CssClass { get; set; }
    }

    public class ProjectDisplaySectionHeader
    {
        public string YearMonthHeader { get; set; }
        public string RevisedHeader { get; set; }
        public string PubcodeHeader {get;set;}
        public string KPCProjectNumber { get; set; }
        public string NextIssueCrossPromoNumber { get; set; }
        public string QuadJobNumber { get; set; }
        public bool NotesAreAvailable { get; set; }

    }

    public class SubItemRow
    {
        public string Description;
        public List<string> SubItemDates;

    }
}