using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Areas.Admin.AdminModels.View
{
    public class TicketViewList
    {
        public int TicketId { get; set; }
        public bool InAppTicketing { get; set; }
        public int TotalSeats { get; set; }
        public string EventName { get; set; }
        public string ArtistName { get; set; }
        public string Genre { get; set; }

        public int RowIndex { get; set; }
        public int PageNo { get; set; }
        public int T_Rec { get; set; }
        public int T_Pages { get; set; }
    }
}