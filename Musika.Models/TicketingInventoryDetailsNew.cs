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
    
    public partial class TicketingInventoryDetailsNew
    {
        public int TicketDetailId { get; set; }
        public Nullable<int> TourDateId { get; set; }
        public Nullable<int> TicketId { get; set; }
        public Nullable<int> SeatNo { get; set; }
        public string SectionType { get; set; }
        public Nullable<decimal> SectionPrice { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDatet { get; set; }
        public string RecordStatus { get; set; }
    }
}
