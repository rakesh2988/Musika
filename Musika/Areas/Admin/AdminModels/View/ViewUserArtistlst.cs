using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Areas.Admin.AdminModels.View
{
    public class ViewUserArtistlst
    {
        public Int32 UserID { get; set; }

        public string UserName { get; set; }
        
        public string Email { get; set; }

        public int ArtistID { get; set; }

        public string ArtistName { get; set; }

        public string Main_Genre { get; set; }
        
        public Int32 EventCount { get; set; }

        public string ImageURL { get; set; }

    }
}