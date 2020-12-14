using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class MessagingSetting
    {
        public int ID { get; set; }
        public int EventFK { get; set; }
        public int ActionFk { get; set; }
        public int? UserFK { get; set; }
        public int? RoleFk { get; set; }
        public int? GroupFK { get; set; }
        public int? DeptFK { get; set; }
    }
}