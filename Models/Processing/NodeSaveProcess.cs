using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class NodeSaveProcess
    {
        public int MilestoneField { get; set; }
        public int MilestoneParentField { get; set; }
        public int MilestoneDepField { get; set; }
        public int MilestoneDepCalcField { get; set; }
        public int MilestoneRangeCalcField { get; set; }
        public int ProjectRangeField { get; set; }
        public int ProjectYear { get; set; }
        public int ProjectPubCode { get; set; }
        public int ProjectProduct { get; set; }
        public int ProjectMilestoneTreeSettingsProfile { get; set; }
        public string MilestoneDueDate { get; set; }
    }
}