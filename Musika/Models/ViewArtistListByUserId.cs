using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class ViewArtistListByUserId
    {
        public Int32 ArtistID { get; set; }

        public string ArtistName { get; set; }

        public string ImageURL { get; set; }

        public string BannerImage_URL { get; set; }


        public bool OnTour { get; set; }
        public string FirstLetter { get; set; }
    }
}