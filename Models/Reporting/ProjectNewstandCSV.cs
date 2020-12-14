using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FileHelpers;

namespace Scheduling.Models
{
     [DelimitedRecord(",")]
    public class ProjectNewstandCSV
    {
        public string NewstandDate { get; set; }
        public string ProjectName { get; set; }
    }
      
}