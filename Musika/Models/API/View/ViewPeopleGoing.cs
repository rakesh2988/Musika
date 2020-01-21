using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models.API.View
{
    public class ViewPeopleGoing
    {
        public int TourID { get; set; }

        public int ArtistID { get; set; }
        public string ArtistName { get; set; }
        public string ImageURL { get; set; }

        public int VenueID { get; set; }
        public string VenueName { get; set; }
        
        public string VenuImageURL { get; set; }

        public List<ViewPeoples> Going { get; set; }

    }

    public partial class ViewPeoples
    {
        public int UserID { get; set; }
        public string UserName { get; set; }

        public string Email { get; set; }

        public string ThumbnailURL { get; set; }

       public string CreatedDate { get; set; }

    }




 

}