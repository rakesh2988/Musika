using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class TicketingVenues
    {
        public int VenueID { get; set; }
        public string VenueName { get; set; }
        public string Address { get; set; }
        public string Extended_Address { get; set; }
        public string VenueCity { get; set; }
        public string VenueState { get; set; }
        public string Postal_Code { get; set; }
//        VenueLat    decimal
//VenueLong   decimal
    }
}