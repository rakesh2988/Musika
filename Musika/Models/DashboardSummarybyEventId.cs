using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class DashboardSummarybyEventId
    {
        public int EventId { get; set; }
        public DateTime StartDate { get; set; }
        public string StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public string EndTime { get; set; }
        public int TotalTickets { get; set; }
        public int TotalSales { get; set; }
        public int Attendees { get; set; }

        public EventReponse MessageResponse { get; set; }
    }
}