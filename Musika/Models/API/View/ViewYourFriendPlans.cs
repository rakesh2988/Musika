using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models.API.View
{
   


    public class ViewYourFreiendplanlst
    {
        //public int EventID { get; set; }
        public int TourDateID { get; set; }

        public Nullable<int> ArtistID { get; set; }

        public string Date_Local { get; set; }

        public string Time_Local { get; set; }

        public DateTime Datetime_Local { get; set; }

        public bool IsApproved { get; set; }

        public string ArtistName { get; set; }

        public string ImageURL { get; set; }

        public string BannerImage_URL { get; set; }

        public int VenueID { get; set; }

        public string VenueName { get; set; }

        public Int32 GoingCount { get; set; }

        public List<ViewFriendPlans> Going { get; set; }



    }

    public class ViewFriendPlans
    {

        public long UserID { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string ThumbnailURL { get; set; }

        public string ImageURL { get; set; }
    }

}