using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Areas.Admin.AdminModels.View
{
    public class ViewArtistlst
    {

        public int ArtistID { get; set; }

        public string ArtistName { get; set; }

        public string Main_Genre { get; set; }
        
        public Int32 EventCount { get; set; }

        public bool OnTour { get; set; }

        public Int32 TrackCount { get; set; }

        public string ImageURL { get; set; }


        public string ThumbnailURL { get; set; }


        public bool Isdefault { get; set; }

    }
}