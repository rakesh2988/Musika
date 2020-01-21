using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Musika.Models
{
    [KnownTypeAttribute(typeof(TicketingEventDetailsModel))]
    public class TicketingEventDetailsModel
    {
        public int EventID { get; set; }
        public string EventTitle { get; set; }
        public string EventLocation { get; set; }
        public string VenueName { get; set; }
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
        public string EventType { get; set; }
        public string ListingPrivacy { get; set; }
        public string EventTopic { get; set; }
        public string ShowTicketNumbers { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ArtistId { get; set; }
        public string ArtistName { get; set; }
        public decimal VenueLat { get; set; }
        public decimal VenueLong { get; set; }
        public string TicketPackage { get; set; }
        public List<StaffDetail> lststaff { get; set; }
    }
    public class StaffDetail
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
    }
}