using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class ChangeRequest
    {
        public int ID { get; set; }
        public int ProjectFK { get; set; }
        public string RequestorComment { get; set; }
        public string ReviewerComment { get; set; }
        public string CreatedBy { get; set; } 
        public string ReviewedBy { get; set; }

        public DateTime DateRequested { get; set; }
        public DateTime?  DateReviewed { get; set; }
        public int RequestStatus { get; set; }
    }

    public class ChangeRequestStatus
    {
        public int ID { get; set; }
        public string Description { get; set; }

    }
}