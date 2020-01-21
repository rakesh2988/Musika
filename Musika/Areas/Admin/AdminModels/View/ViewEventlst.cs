using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Areas.Admin.AdminModels.View
{
    public class ViewEventlst
    {

        public int TourDateID { get; set; }

        public string SeatGeek_TourID { get; set; }

        public string HashTag { get; set; }

        public string EventName { get; set; }

        public string VenueName { get; set; }
        
        public string Main_Genre { get; set; }

        public DateTime Datetime_Local { get; set; }

        public string ArtistName { get; set; }

        public string VenueCountry { get; set; }

        public string VenueCity { get; set; }

        public Int32 AttendingCount { get; set; }

        public Int32 TicketsAvailable { get; set; }

        public string HotTour { get; set; }

        public bool IsDeleted { get; set; }
        public string EventDate { get; set; }

    }

}