using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class ProjectLink
    {
        public int ID { get; set; }
        public int PrimaryProjectID { get; set; }
        public int SecondaryProjectID { get; set; }
        public int LinkSettingID { get; set; }
    }

    public class ProjectLinkDisplay
    {
        public int ID { get; set; }
        public string PrimaryProject { get; set; }
        public string SecondaryProject { get; set; }
        public string PrimaryProjectProfileType { get; set; }
        public string SecondaryProjectProfileType { get; set; }
        public int PrimaryProjectID { get; set; }
        public int SecondaryProjectID { get; set; }
    }
}