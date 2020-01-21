using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class TicketsPurchasedByUser
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string ProfileImage { get; set; }
        public string UserStatus { get; set; }
        public string TicketType { get; set; }
        public DateTime CheckInDate { get; set; }
        public string TicketNumber { get; set; }
        //all the ticket details
    }
}