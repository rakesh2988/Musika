using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class TicketEventDetails
    {
        public int ArtistID { get; set; }
        public int TourDateID { get; set; }
        public string EventTitle { get; set; }
        public string VenueName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string VenueLat { get; set; }
        public string VenueLong { get; set; }
        public string ArtistName { get; set; }
        public string Tour_Utcdate { get; set; }
        public Guid TicketNumber { get; set; }
        public int EventID { get; set; }

        public string UserType { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string DeviceType { get; set; }
        public string Gender { get; set; }

        public int UserID { get; set; }
        public DateTime Dob { get; set; }

        public string Address { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string TicketType { get; set; }
        public string Mode { get; set; }
        public string TicketSerialNumber { get; set; }
    }
}