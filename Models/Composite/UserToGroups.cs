using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class UserToGroups
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public string GroupID { get; set; }
    }
}