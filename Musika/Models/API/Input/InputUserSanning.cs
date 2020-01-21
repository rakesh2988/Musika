
using Musika.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Musika.Models.API.Input
{
    public class InputUserSanning
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        public string MusicSource { get; set; }


        [Required]
        public Int32 MSourceID { get; set; }

        
        [Required]
        public string DeviceType { get; set; }


        [Required]
        public List<myArtistName> ArtistNames { get; set; }

    }

    public class myArtistName {
        public string Name { get; set; }
    }                                                                                                                                        
 
}