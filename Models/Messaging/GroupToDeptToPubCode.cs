using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class DeptToGroupsToPubCode
    {
        public int ID { get; set; }
        public int GroupID { get; set; }
        public int DeptID { get; set; }
        public int PubCodeID { get; set; }

    }

    public class DeptToGroupsToPubCodeDisplay
    {
        public int ID { get; set; }
        public string GroupName { get; set; }
        public string DeptName { get; set; }
        public string PubCodeName { get; set; }

    }
}