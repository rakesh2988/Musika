using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusikaDataService.Models
{
    public class Artists1
    {
        public int ArtistID { get; set; }
        public string ArtistName { get; set; }
        public string About { get; set; }
        public string ImageURL { get; set; }
        public bool OnTour { get; set; }
        public string Gender { get; set; }
        public string Seatgeek_ID { get; set; }
        public string Musicgraph_ID { get; set; }
        public string Eventful_ID { get; set; }
        public string Instagram_ID { get; set; }
        public string Artist_Ref_ID { get; set; }
        public string Musicbrainz_ID { get; set; }
        public string Spotify_ID { get; set; }
        public string Youtube_ID { get; set; }
        public string Main_Genre { get; set; }
        public string Decade { get; set; }
        public string Alternate_Names { get; set; }
        public string Spotify_Url { get; set; }
        public string Lastfm_Url { get; set; }
        public string Instagram_Url { get; set; }
        public string Instagram_Tag { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string RecordStatus { get; set; }
        public string Spotify_URL_Name { get; set; }
        public string BannerImage_URL { get; set; }
        public string ThumbnailURL { get; set; }
        public string AboutES { get; set; }
        public bool Isdefault { get; set; }
    }
}
