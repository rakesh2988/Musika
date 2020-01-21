using Musika.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Musika.Models.API.Input
{
    public class InputUpdateisGoing
    {

        [Required]
        public Int32 UserID { get; set; }

        [Required]
        public Int32 TourID { get; set; }


        [Required]
        public EUserGoing UserGoing { get; set; }
       
    }
}