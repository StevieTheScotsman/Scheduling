using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class MessagingSettingDisplay
    {
        public int ID { get; set; }
        public string EventName { get; set; }
        public string ActionName { get; set; }
        public string UserName{ get; set; }
        public string RoleName{ get; set; }
        public string GroupName { get; set; }
        public string DeptName { get; set; }
    }
}