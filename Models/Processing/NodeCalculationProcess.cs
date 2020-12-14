using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class NodeCalculationProcess
    {
        public int MilestoneFieldID { get; set; }
        public int CalculationID { get; set; }
        public int DependantUponID { get; set; }
    }

    
}