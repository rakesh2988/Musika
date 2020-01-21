using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Areas.Admin.AdminModels.View
{
    public class ViewVenuelst
    {

        public int VenueID { get; set; }

        public string VenueName { get; set; }

        public string VenueCountry { get; set; }

        public string VenueCity { get; set; }


        public string Postal_Code { get; set; }


        public Int32 EventCount { get; set; }

        public Int32 TicketsAvailable { get; set; }
   
        
    }

}