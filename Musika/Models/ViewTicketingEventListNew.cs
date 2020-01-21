using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class ViewTicketingEventListNew
    {
        public int EventID { get; set; }
        public DateTime StartDate { get; set; }
        public string StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public string EndTime { get; set; }
        public string EventImage { get; set; }
        public string VenueName { get; set; }
        public string ArtistName { get; set; }

        public EventReponse MessageResponse { get; set; }
    }
}