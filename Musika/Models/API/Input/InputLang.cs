using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Musika.Models.API.Input
{
    public class InputLang
    {

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Language { get; set; }


    }
}