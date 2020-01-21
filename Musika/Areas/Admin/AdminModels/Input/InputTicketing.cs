using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Areas.Admin.AdminModels.Input
{
    public class InputTicketing
    {
        public InputTicketing()
        {
            List = new List<TicketingDtails>();
        }
        public int TicketId { get; set; }
        public int TotalSeats { get; set; }
        public int TourDate { get; set; }
        public List<TicketingDtails> List { get; set; }
    }

    public class TicketingDtails
    {
        public int TicketDetailID { get; set; }
        public int SeatFrom { get; set; }
        public int SeatTo { get; set; }
        public string SectionType { get; set; }
        public decimal SectionPrice { get; set; }
    }
}