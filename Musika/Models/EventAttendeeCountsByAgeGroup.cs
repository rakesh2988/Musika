using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class EventAttendeeCountsByAgeGroup
    {
        public int EventID { get; set; }
        public int Age25 { get; set; }
        public int Age50 { get; set; }
        public int Age75 { get; set; }
        public int Age100 { get; set; }
        public int Age125 { get; set; }
    }
}