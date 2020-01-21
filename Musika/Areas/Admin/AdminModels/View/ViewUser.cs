using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Areas.Admin.AdminModels.View
{
    public class ViewUser
    {
        public long UserID { get; set; }

        public string UserName { get; set; }
        
        public string Email { get; set; }
        
        public string ThumbnailURL { get; set; }
        
        public string ImageURL { get; set; }
        
        public Int32 ArtistCount { get; set; }

        public Int32 EventCount { get; set; }

        public string RecordStatus { get; set; }


    }
}