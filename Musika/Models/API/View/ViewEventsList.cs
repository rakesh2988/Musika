using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models.API.View
{
    public class ViewEventsList
    {
        public Int32 TourDateID { get; set; }

        public Int32 ArtistID { get; set; }

        public Int32 VenuID { get; set; }

        public string ArtistName { get; set; }

        public string ImageURL { get; set; }

        public string BannerImage_URL { get; set; }

        public Boolean OnTour { get; set; }

        public string VenueName { get; set; }

        public Int32? TicketingEventId { get; set; }

        public DateTime Tour_Utcdate { get; set; }

        public string Datetime_Local
        {
            get
            {
                return Datetime_dt.HasValue ? Datetime_dt.Value.ToString("d") : "";
            }
        }
        public Nullable<DateTime> Datetime_dt { get; set; }

        // Added by Mukesh
        public string CityName { get; set; }

    }
}