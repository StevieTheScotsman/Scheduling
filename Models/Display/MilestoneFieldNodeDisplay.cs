using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class MilestoneFieldNodeDisplay
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int MilestoneProfile{ get; set; }
        public int Timeline { get; set; }
        public int MilestoneField { get; set; }
    }

    public class EditProjectMilestoneFieldNodeDisplay
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int MilestoneFieldID { get; set; }
        public int ProjectID { get; set; }
    }
}