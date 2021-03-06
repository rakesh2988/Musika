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
    
    public partial class TourDate
    {
        public int TourDateID { get; set; }
        public string SeatGeek_TourID { get; set; }
        public Nullable<int> ArtistID { get; set; }
        public Nullable<int> EventID { get; set; }
        public int VenueID { get; set; }
        public Nullable<System.DateTime> Announce_Date { get; set; }
        public Nullable<System.DateTime> Tour_Utcdate { get; set; }
        public Nullable<System.DateTime> Visible_Until_utc { get; set; }
        public Nullable<System.DateTime> Datetime_Local { get; set; }
        public Nullable<decimal> Score { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string RecordStatus { get; set; }
        public string EventName { get; set; }
        public string Eventful_TourID { get; set; }
        public string TicketURL { get; set; }
        public string HashTag { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<int> TicketingEventID { get; set; }
    }
}
