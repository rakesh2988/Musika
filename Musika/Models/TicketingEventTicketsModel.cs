using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class TicketingEventTicketsModel
    {
        public int TicketId { get; set; }
        public int? EventID { get; set; }
        public Guid? TicketNumber { get; set; }
        public int UserId { get; set; }
        public int NumberOfTickets { get; set; }
        public string TicketType { get; set; }
    }

    public class TixEventTixModel
    {
        public string TicketNumber { get; set; }
        public int? UserId { get; set; }
    }
}