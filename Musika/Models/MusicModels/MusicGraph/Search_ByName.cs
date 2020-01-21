using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MusicGraph
{

    public class Status
    {
        public string api { get; set; }
        public string message { get; set; }
        public int code { get; set; }
    }

    public class Pagination
    {
        public int count { get; set; }
        public int offset { get; set; }
    }

    public class Datum
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

        public bool IsLatin { get; set; }

    }

    public class Search_ByName
    {
        public Status status { get; set; }
        public Pagination pagination { get; set; }
        public List<Datum> data { get; set; }
    }


    public class ArtistList
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }


}