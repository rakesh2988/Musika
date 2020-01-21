using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class TicketsListModelNew
    {
        public List<TicketingUserListEventNew> lstUsers { get; set; }
        public UserTicketsSummary lstTicketSummary { get; set; }
    }
    public class TicketingUserListEventNew
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
        public string ImageUrl { get; set; }
        public List<UserTicketsNumber> lstTicket { get; set; }
    }
    public class UserTicketsSummary
    {
        public string TotalTickets { get; set; }
        public string TicketsSold { get; set; }
        public string TicketsScanned { get; set; }
    }
    public class UserTicketsNumber
    {
        public string TicketNumber { get; set; }
        public string Status { get; set; }
    }
}