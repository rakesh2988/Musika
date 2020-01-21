using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class TicketingEventsList
    {
        public int EventID { get; set; }
        public string EventTitle { get; set; }
        public string UserName { get; set; }
        public DateTime StartDate { get; set; }
        public string StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public string VenueName { get; set; }
        public string VenueCity { get; set; }
        public string VenueState { get; set; }
        public string Postal_Code { get; set; }
        public int IsApproved { get; set; }
        public string Email { get; set; }
        public string ArtistName { get; set; }

        public string bIsArrproved { get; set; }
    }
}