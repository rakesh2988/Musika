using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models.API.SP
{
    public class DiscoverDetail
    {
        public Int32 TourDateID { get; set; }

        public Int32 ArtistID { get; set; }

        public string ImageURL { get; set; }

        public Int32 VenuID { get; set; }

        public string VenueName { get; set; }

        public string VenuImageURL { get; set; }

        public string Date_Local { get; set; }


    
    }
}