using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models.ReportingObjects
{
    public class ReportingTable
    {

        public List<ReportingRow> Rows {get;set;}
    }

    public class ReportingRow
    {

        public List<ReportingCell> Cells {get;set;}
    }


    public class ReportingCell

    {
        public string Value { get; set; }

    }

    //both null implies empty
    public class NewsStandRowHeader
    {
        public int? YearID {get;set;}
        public int? TimelineID { get; set; }
        
        
    }
}