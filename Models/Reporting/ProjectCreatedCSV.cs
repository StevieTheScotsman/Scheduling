using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FileHelpers;

namespace Scheduling.Models
{
   
    [DelimitedRecord(",")]
     public class ProjectCreatedCSV
     {
         public string ProjectName { get; set; }
         public string CreationDate { get; set; }
         public string TimeOfCreation { get; set; }
         public string Year { get; set; }
         public string ProfileType { get; set; }
     }
}