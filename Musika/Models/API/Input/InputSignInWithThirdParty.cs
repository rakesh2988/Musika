﻿using Musika.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Musika.Models.API.Input
{
    public class InputSignInWithThirdParty
    {
        [Required]
        public ThirdPartyType ThirdPartyType { get; set; }

        [Required]
        public string ThirdPartyId { get; set; }


        
        public string Email { get; set; }


        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z0-9. _-]{0,100}$")]
        public string UserName { get; set; }


        public string DeviceType { get; set; }
        public string DeviceToken { get; set; }
        public Nullable<decimal> DeviceLat { get; set; }
        public Nullable<decimal> DeviceLong { get; set; }

        public string ImageURL { get; set; }

        public System.DateTime CreatedDate { get; set; }
    }
}