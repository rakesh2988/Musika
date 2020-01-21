using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class TicketsListModel
    {
        public List<TicketingUserListEventModel> lstUsers { get; set; }
        public TicketsSummary lstTicketSummary { get; set; }
    }

    public class TicketingUserListEventModel
    {
        public int UserID { get; set; }
        public string UserType { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Addres { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string TicketID { get; set; }
        public string EventID { get; set; }
        public string TicketNumber { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }        
    }

    public class TicketsSummary
    {
        public string TotalTickets { get; set; }
        public string TicketsSold { get; set; }
        public string TicketsScanned { get; set; }
    }

}