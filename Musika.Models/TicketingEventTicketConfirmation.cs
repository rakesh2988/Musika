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
    
    public partial class TicketingEventTicketConfirmation
    {
        public int Id { get; set; }
        public Nullable<int> EventID { get; set; }
        public Nullable<int> UserID { get; set; }
        public Nullable<System.DateTime> Dob { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Nullable<System.Guid> TicketNumber { get; set; }
        public string TicketType { get; set; }
        public string Mode { get; set; }
        public string TicketSerialNumber { get; set; }
        public string ScannedTicket { get; set; }
        public Nullable<int> TourDateID { get; set; }
        public Nullable<int> Quantity { get; set; }
        public string OrderNum { get; set; }
    }
}
