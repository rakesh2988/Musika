using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Musika.Models.API.View
{
    public class ViewArtistList
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
                Regex MyRegex = new Regex("[^a-z]", RegexOptions.IgnoreCase);
                return MyRegex.Replace(ArtistName, @"").Trim().Substring(0, 1);
            }
            set { }
           
        }


    }
}