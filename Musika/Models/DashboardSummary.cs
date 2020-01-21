using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class DashboardSummary
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime StartDate { get; set; }
        public string StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public string EndTime { get; set; }
        public int TotalTickets { get; set; }
        public int TotalSales { get; set; }
        public int Attendees { get; set; }

        public List<EventAttendeeCounts> lstCounts { get; set; }

        public List<EventAttendeeCounts> lstGenderAttendeeList { get; set; }

        public EventReponse MessageResponse { get; set; }
    }

    public class DashboardSummaryNew
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime StartDate { get; set; }
        public string StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public string EndTime { get; set; }
        public int TotalTickets { get; set; }
        public int TotalSales { get; set; }
        public int Attendees { get; set; }

        public List<EventAttendeeCounts> lstCounts { get; set; }

        public List<EventAttendeeCounts> lstGenderCounts { get; set; }

        public List<EventAttendeCountsByGender> lstGenderAttendeeList { get; set; }

        public EventReponse MessageResponse { get; set; }
    }
    public class EventAttendeeCounts
    {
        //public int EventId { get; set; }
        public string Category { get; set; }
        public int TotalTickets { get; set; }
        public int Sold { get; set; }
        public int Attendees { get; set; }
        public int UnSold { get; set; }
    }

    public class EventAttendeCountsByGender
    {
        //public string Category { get; set; }
        public string Category { get; set; }
        public int TotalTickets { get; set; }
        public int Sold { get; set; }
        public int Attendees { get; set; }
        public int UnSold { get; set; }
    }
}