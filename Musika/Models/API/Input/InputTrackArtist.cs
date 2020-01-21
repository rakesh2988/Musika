using Musika.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Musika.Models.API.Input
{
    public class InputTrackArtist
    {
        [Required]
        public Int32 UserID { get; set; }

        
        [Required]
        public Int32 ArtistID { get; set; }


        [Required]
        public ETracking TrackArtist { get; set; }
       
    }
}