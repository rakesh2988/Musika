using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class ViewTicketingEventList
    {
        public int EventID { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        public string Email { get; set; }
        public Int32 TourDateID { get; set; }
        public Int32 ArtistID { get; set; }
        public Int32 VenuID { get; set; }
        public string ArtistName { get; set; }
        public string ImageURL { get; set; }
        public string BannerImage_URL { get; set; }
        public Boolean OnTour { get; set; }
        public string VenueName { get; set; }
        public string Datetime_Local { get; set; }
        public string CityName { get; set; }
        public string EventTitle { get; set; }

        public string EventLocation { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public DateTime StartDate { get; set; }
        public string StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public string EndTime { get; set; }
        public string EventImage { get; set; }
        public string EventDescription { get; set; }
        public string OrganizerName { get; set; }
        public string OrganizerDescription { get; set; }
        public string TicketType { get; set; }
        public string ListingPrivacy { get; set; }
        public string EventType { get; set; }
        public string EventTopic { get; set; }
        public bool ShowTicketNumbers { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public EventReponse MessageResponse { get; set; }
    }
}