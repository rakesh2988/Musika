using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models.API.View
{
    public class ViewEventsPreList : ViewEventsList
    {

        public decimal? VenueLat { get; set; }

        public decimal? VenueLon { get; set; }

        public bool IsDeleted { get; set; }

        public string TicketUrl { get; set; }

        public static List<ViewEventsList> GetEventsList(List<ViewEventsPreList> lp)
        {
            return lp.Where(x => !x.IsDeleted).Select(x => new ViewEventsList
            {
                ArtistID = x.ArtistID,
                ArtistName = x.ArtistName,
                BannerImage_URL = x.BannerImage_URL,
                Datetime_dt = Convert.ToDateTime(x.Datetime_Local),
                ImageURL = x.ImageURL,
                OnTour = x.OnTour,
                TourDateID = x.TourDateID,
                VenueName = x.VenueName,
                VenuID = x.VenuID,
                TicketingEventId =x.TicketingEventId
            }).ToList();
        }

    }
}