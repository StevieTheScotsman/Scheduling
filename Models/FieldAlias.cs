using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scheduling.Models
{
    public class FieldAlias
    {
        public int ID { get; set; }
        public int PubCodeFK { get; set; }
        public int FieldFK { get; set; }
        public string AliasValue { get; set; }
    }
}