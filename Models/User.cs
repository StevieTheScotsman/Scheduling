using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace Scheduling.Models
{
    public class User
    {
        public int ID { get; set; }
        public string Email { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public int RoleFK { get; set; }
    }

    public class UserDisplay
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string RoleDesc { get; set; }
        public string GroupDesc {get;set;}

    }
}