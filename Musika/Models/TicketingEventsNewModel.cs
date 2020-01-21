using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class TicketingEventsNewModel
    {
        public int EventID { get; set; }
        public string EventTitle { get; set; }
        public string EventLocation { get; set; }
        public string VenueName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public DateTime StartDate { get; set; }
        public string StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public string EndTime { get; set; }
        public string EventImage { get; set; }
        public string EventDescription { get; set; }
        public string OrganizerName { get; set; }
        public string OrganizerDescription { get; set; }
        public string TicketType { get; set; }
        public string ListingPrivacy { get; set; }
        public string EventType { get; set; }
        public string EventTopic { get; set; }
        public int? ShowTicketNumbers { get; set; }
        public int? CreatedBy { get; set; }
        //public string CreatedByUser { get; set; }
        public DateTime? CreatedOn { get; set; }

        //public int ShowTicketNumbers { get; set; }

        public bool Isdeleted { get; set; }
        public bool? IsApproved { get; set; }

        public int NumberOfTickets { get; set; }

        public int ArtistId { get; set; }
        public string ArtistName { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public EventReponse MessageResponse { get; set; }

        public List<TicketingEventsNewModel> lstTicketingEventsModel { get; set; }

        public Ticket Ticket { get; set; }

        //public int StaffId { get; set; }

        public List<Staff> lstStaff { get; set; }

        public string TicketUrl { get; set; }

        //public int? EventsID { get; set; }

        public int TotalTickets { get; set; }
        public int TicketsSold { get; set; }

      
    }

    public class Ticket
    {
        public int EventId { get; set; }
        public string Currency { get; set; }
        public int CountryId { get; set; }
        public string RefundPolicy { get; set; }
        public string ServiceFee { get; set; }
        public string Tax { get; set; }
        public List<TicketData> lstTicketData { get; set; }
    }

    public class TicketData
    {
        public int EventId { get; set; }
        public string TicketType { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string TicketCategory { get; set; }
        public int BalanceTickets { get; set; }

        public string PackageStartDate { get; set; }
        public string PackageEndDate { get; set; }
        //public string Summary { get; set; }
    }

    public class Staff
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
    }
}

