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
    
    public partial class TicketingEventTicketsSummaryTemp
    {
        public int Id { get; set; }
        public int EventID { get; set; }
        public string TicketCategory { get; set; }
        public string TicketType { get; set; }
        public decimal Cost { get; set; }
        public int Quantity { get; set; }
        public int CountryId { get; set; }
        public string Currency { get; set; }
        public string ServiceFee { get; set; }
        public string Tax { get; set; }
        public string RefundPolicy { get; set; }
        public Nullable<System.DateTime> PackageStartDate { get; set; }
        public Nullable<System.DateTime> PackageEndDate { get; set; }
    }
}
