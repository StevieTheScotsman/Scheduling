using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class ProjectLinkViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Year { get; set; }
        public string PubCodeName { get; set; }
        public int? PubCodeID { get; set; }
        public int YearFK { get; set; }
    }
}