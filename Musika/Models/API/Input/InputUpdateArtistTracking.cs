
using Musika.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Musika.Models.API.Input
{
    public class InputUpdateArtistTracking
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        public int ArtistID { get; set; }

        [Required]
        public bool Tracking { get; set; }

    }
                                                                                                                                     
 
}