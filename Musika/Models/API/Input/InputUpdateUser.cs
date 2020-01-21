using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Musika.Models.API.Input
{
    public class InputUpdateUser
    {
        [Required]
        public int UserID { get; set; }

        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z0-9 ._-]{2,100}$")]
        public string UserName { get; set; }

      
        public string CurrentPassword { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string NewPassword { get; set; }

    }
}