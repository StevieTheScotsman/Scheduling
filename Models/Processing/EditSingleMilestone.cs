using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class EditSingleMilestone
    {
        public string ProjectID { get; set; }
        public string MilestoneFieldID { get; set; }
    }

  public class AddSingleMilestone
  {
      public string ProjectID { get; set; }
      public string MilestoneFieldID { get; set; }
      public string MilestoneParentID { get; set; }
      public string DependencyID { get; set; }
      public string CalculationID { get; set; }
      
  }
}