using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Scheduling.Models
{
    public class PublicationCode
    {
        public int ID { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public int? IsActive { get; set; }
        public int? PrinterFK { get; set; }
        public int? ProfitCenter { get; set; }
        public string ReportDesc { get; set; }
        public int? ParentPub { get; set; }
        public int IsAnnual { get; set; }
        public int ShowInNewsStandReport { get; set; }
        public int HasCustomOffset { get; set; }

    }



}