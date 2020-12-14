using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class EditSingleMilestoneWithDueDate
    {
        public string DueDate { get; set; }
        public string ProjectID { get; set; }
        public string MilestoneFieldFK { get; set; }
    }

  public class EditSingleProjectMilestoneValueWithDueDate
  {

      public string DueDate { get; set; }
      public string ProjectID { get; set; }
      public string MilestoneValueID { get; set; }

  }

    public class EditSingleNoteField
    {
        public string ProjectID { get; set; }
        public string NoteLabelID { get; set; }
        public string NoteValue { get; set; }

    }
}