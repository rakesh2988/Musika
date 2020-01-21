//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Musika.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TicketingEventsNew
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
        public Nullable<System.DateTime> StartDate { get; set; }
        public string StartTime { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string EndTime { get; set; }
        public string EventImage { get; set; }
        public string EventDescription { get; set; }
        public string OrganizerName { get; set; }
        public string OrganizerDescription { get; set; }
        public string TicketType { get; set; }
        public string ListingPrivacy { get; set; }
        public string EventType { get; set; }
        public string EventTopic { get; set; }
        public Nullable<int> ShowTicketNumbers { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<bool> ISDELETED { get; set; }
        public Nullable<bool> IsApproved { get; set; }
        public Nullable<int> NumberOfTickets { get; set; }
        public Nullable<int> ArtistId { get; set; }
        public Nullable<int> StaffId { get; set; }
        public string TicketUrl { get; set; }
    }
}
