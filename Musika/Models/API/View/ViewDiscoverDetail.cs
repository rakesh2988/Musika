using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models.API.View
{
    public class ViewDiscoverDetail
    {

        public List<ViewDiscoverlst> HotNewTour { get; set; }

        public List<ViewDiscoverlst> Recommended { get; set; }

        public List<ViewDiscoverlst> MostPopular { get; set; }

    }


    public class ViewDiscoverlst
    {

        public int TourDateID { get; set; }

        public int ArtistID { get; set; }
        
       public string ArtistName { get; set; }

        public string ImageURL { get; set; }

        public int VenuID { get; set; }

        public string VenueName { get; set; }

        public string VenuImageURL { get; set; }

        public string Date_Local { get; set; }


    }
}