
using Musika.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Musika.Models.API.Input
{
    public class InputAllUserSanning
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        public List<allMusicSource> AllMusicSource { get; set; }

        [Required]
        public string DeviceType { get; set; }

        [Required]
        public List<allArtistName> ArtistNames { get; set; }

    }

    public class allMusicSource{
        public Int32 MSourceID { get; set; }
        public string MSourceName { get; set; }
    }

    public class allArtistName {
        public string Name { get; set; }
        //test
    }                                                                                                                                        
 
}