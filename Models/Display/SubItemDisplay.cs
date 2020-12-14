using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models.Display
{
    public class SubItemDisplay
    {
        public int ID { get; set; }
        public string ParentFieldName { get; set; }
        public string MilestoneFieldName { get; set; }
        public string DependancyFieldName { get; set; }
        public string CalculationName { get; set; }
        public string CalcFiringOrder { get; set; }
        public int MilestoneTreeSettingProfileID { get; set; }
        public int? DisplayOrder { get; set; }

    }
}