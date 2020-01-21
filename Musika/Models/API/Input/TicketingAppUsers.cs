using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Musika.Models.API.Input
{
    public class TicketingAppUsers
    {
        [Required]
        public string UserType { get; set; }

        public int UserID { get; set; }

        [Required]
        public string UserName { get; set; }

        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Addres { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string PostalCode { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        public string FacebookID { get; set; }
        public string ThumbnailURL { get; set; }
        public string ImageURL { get; set; }
        public string DeviceType { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceLat { get; set; }
        public string DeviceLong { get; set; }
        public string RecordStatus { get; set; }
        public string ModifiedDate { get; set; }
        public string CreatedDate { get; set; }
        public string SynFacebookID { get; set; }
        public string UserLanguage { get; set; }      

    }
}