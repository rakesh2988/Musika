using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models.API.View
{
    public class ViewUser
    {
        public long UserID { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }


        public string ThumbnailURL { get; set; }
        public string ImageURL { get; set; }

        public bool IsNewUser { get; set; }

        public string RecordStatus { get; set; }

        public string Addres { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public string status { get; set; }
    }
}