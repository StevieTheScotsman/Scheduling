using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    //used in editprojects generation
    public class MilestoneValue
    {
        public int ID { get; set; }
        public int? ParentID { get; set; }
        public int MilestoneFieldFK { get; set; }
        public int? CalculationFK { get; set; }
        public int? RangeCalculationFK { get; set; }
        public int? DependantUpon { get; set; }
        public int ProjectPK { get; set; }
        public string EarlyDueDate { get; set;}
        public string DueDate { get; set; }
        public int? CalcFiringOrder { get; set; }
        public int? DisplaySortOrder { get; set; }

    }
}