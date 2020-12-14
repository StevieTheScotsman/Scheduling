using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class ProductScheduleType
    {
        public int ID { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public string DefaultMilestoneTreeSettingsProfile { get; set; }
        public string DefaultViewProfile { get; set; }
        public string DefaultNoteSettingsProfile { get; set; }
        public string DefaultTopLeveDateScheduleCalculation { get; set; }
        public string DefaultEventProfile { get; set; }
    }
}