using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models.API.View
{
    public class ViewVenueDetail
    {
        public int VenueID { get; set; }
        public string VenueName { get; set; }
        public string ImageURL { get; set; }
        public string Extended_Address { get; set; }
        public string Display_Location { get; set; }
        public string Slug { get; set; }
        public string Postal_Code { get; set; }
        public string Address { get; set; }
        public string Timezone { get; set; }
        public string VenueCity { get; set; }
        public string VenueState { get; set; }
        public string VenueCountry { get; set; }
        public Nullable<decimal> VenueLat { get; set; }
        public Nullable<decimal> VenueLong { get; set; }

        public List<ViewVenueTours> UpcomingEvents { get; set; }

    }


    public class ViewVenueTours
    {

        public int TourDateID { get; set; }

        public Nullable<int> ArtistID { get; set; }
        
        public string Date_Local { get; set; }

        public string Time_Local { get; set; }

        public string ArtistName { get; set; }

        public string ImageURL { get; set; }


        public string BannerImage_URL { get; set; }

    }
}