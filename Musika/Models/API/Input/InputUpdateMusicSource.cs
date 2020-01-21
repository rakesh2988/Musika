
using Musika.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Musika.Models.API.Input
{
    public class InputUpdateMusicSource
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        public List<myMusicsource> MusicSource { get; set; }

    }

    public class myMusicsource {

        public Int16 MSourceID { get; set; }

        public string Source { get; set; }

        public DeviceType DeviceType { get; set; }
    }                                                                                                                                        
 
}