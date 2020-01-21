using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class DashboardSummaryByAgeGroup
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

        public List<EventAttendeeCountsByAgeGroup> lstCounts { get; set; }

        public EventReponse MessageResponse { get; set; }
    }

    public class HeadCountByAgeGroup
    {
        //public int EventId { get; set; }
        public int Under25 { get; set; }
        public int Under50 { get; set; }
        public int Under75 { get; set; }
        public int Under100 { get; set; }
        public int Under125 { get; set; }
    }

}