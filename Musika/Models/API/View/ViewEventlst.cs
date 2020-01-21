using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models.API.View
{
    public class ViewEventlst
    {
        public Int32 ArtistID { get; set; }

        public string ArtistName { get; set; }

        public string ImageURL { get; set; }

        public string BannerImage_URL { get; set; }

        
        public Boolean OnTour { get; set; }


        public string FirstLetter
        {
            get
            {
                return ArtistName.Substring(0,1);
            }
           
        }


    }
}