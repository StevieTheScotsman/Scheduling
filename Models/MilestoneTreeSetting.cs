using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class MilestoneTreeSetting
    {
        public int ID { get; set; }
        public int MilestoneField { get; set; }
        public int? MilestoneParentField { get; set; }
        public int? MilestoneFieldDependantUpon { get; set; }
        public int? CalculationID { get; set; }
        public int? CalcFiringOrder { get; set; }
        public int? RangeCalculationID { get; set; }
        public int MilestoneTreeSettingsProfileID { get; set; }
        public int? DisplayOrder { get; set; }
    }
}