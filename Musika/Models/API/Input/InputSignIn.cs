using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Musika.Models.API.Input
{
    public class InputSignIn
    {

        [Required]
        public string UserName { get; set; }

       
        [Required]
        public string Password { get; set; }


        public string DeviceType { get; set; }
        public string DeviceToken { get; set; }
        public Nullable<decimal> DeviceLat { get; set; }
        public Nullable<decimal> DeviceLong { get; set; }

        public int UserId { get; set; }

    }
}