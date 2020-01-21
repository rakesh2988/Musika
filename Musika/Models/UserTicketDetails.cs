using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class UserTicketDetails
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ImageURL { get; set; }
        public string Status { get; set; }
        public string TicketType { get; set; }
        public string CheckInDateTime { get; set; }
        public string TicketNumber { get; set; }
        public string VenueName { get; set; }
        public int EventId { get; set; }
    }
    public class TicketQRCodeDetail
    {
        public string EventQRCodeImage { get; set; }
        public string QRCodeNumber { get; set; }
    }
}