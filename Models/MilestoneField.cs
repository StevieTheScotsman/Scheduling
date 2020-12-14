using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class MilestoneField
    {
        public int ID {get;set;}
        public string Description{get;set;}
        public int? IsCreatedByUser { get; set; }
    }
}