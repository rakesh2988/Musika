using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class TicketingEventTicketsSummaryModel
    {
        public int Id { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; }
        public string TicketCategory { get; set; }
        public string TicketType { get; set; }
        public decimal Cost { get; set; }
        public int Quantity { get; set; }
        public int CountryId { get; set; }
        public string Currency { get; set; }
        public string RefundPolicy { get; set; }
    }
}