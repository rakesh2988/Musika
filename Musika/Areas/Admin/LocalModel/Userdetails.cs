using Musika.Areas.Admin.LocalResource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace Musika.Areas.Admin.LocalModel
{
    public class Userdetails
    {
        [Display(Name = "Email", ResourceType = typeof(Resource))]
        public string Email { get; set; }

        [Display(Name = "Password", ResourceType = typeof(Resource))]
        public string Password { get; set; }
    }
}