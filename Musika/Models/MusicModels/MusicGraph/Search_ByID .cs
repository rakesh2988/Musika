using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MusicGraph1
{

  
    public class Data
    {
        public string id { get; set; }
        public string decade { get; set; }
        public string country_of_origin { get; set; }
        public string entity_type { get; set; }
        public string artist_ref_id { get; set; }
        public string main_genre { get; set; }
        public string musicbrainz_id { get; set; }
        public List<string> alternate_names { get; set; }
        public string spotify_id { get; set; }
        public string name { get; set; }
        public string gender { get; set; }
        public string sort_name { get; set; }
        public string youtube_id { get; set; }
    }

    public class Status
    {
        public string api { get; set; }
        public string message { get; set; }
        public int code { get; set; }
    }

    public class Search_ByID
    {
        public Status status { get; set; }
        public Data data { get; set; }
    }


}