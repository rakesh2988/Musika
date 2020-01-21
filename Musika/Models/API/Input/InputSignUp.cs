using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Musika.Enums;

namespace Musika.Models.API.Input
{
    public class InputSignUp
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z0-9._-]*$")]
        public string UserName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(50)]
        public string Password { get; set; }
       

        public string DeviceType { get; set; }
        public string DeviceToken { get; set; }
        public Nullable<decimal> DeviceLat { get; set; }
        public Nullable<decimal> DeviceLong { get; set; }

        public string ImageBase64 { get; set; }

        public string UserType { get; set; }

        public int UserID { get; set; }

        public string Addres { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Country { get; set; }

        public int? CreatedBy { get; set; }

        public string RecordStatus { get; set; }
    }
}