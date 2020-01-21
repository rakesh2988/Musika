using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class TicketingEventResultModel
    {
        public int TicketId { get; set; }
        public Nullable<int> EventID { get; set; }
        public Nullable<System.Guid> TicketNumber { get; set; }
        public Nullable<int> UserId { get; set; }
        public EventReponse MessageResponse { get; set; }
        public string TicketType { get; set; }
    }
}