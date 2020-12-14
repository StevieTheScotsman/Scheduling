using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Scheduling.Models
{
    public class ProjectNote
    {
        public int ID { get; set; }
        public int NoteLabelID { get; set; }
        public string NoteValue { get; set; }
        public string EntryDate { get; set; }
        public string Username { get; set; }
        public int ProjectFK { get; set; }
    }

    public class ProjectNoteLabel
    {

        public int ID { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public bool IsActive {get;set;}

    }
}