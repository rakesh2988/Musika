using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeatGeek
{

    public class Meta
    {
        public int per_page { get; set; }
        public int total { get; set; }
        public int page { get; set; }
        public object geolocation { get; set; }
    }

    public class Stats
    {
        public int event_count { get; set; }
    }

    public class Images
    {
        public string huge { get; set; }
    }

    public class Taxonomy
    {
        public object parent_id { get; set; }
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Genre
    {
        public string slug { get; set; }
        public bool primary { get; set; }
        public string name { get; set; }
    }

    public class Performer
    {
        public Stats stats { get; set; }
        public string name { get; set; }
        public string short_name { get; set; }
        public string url { get; set; }
        public string type { get; set; }
        public string image { get; set; }
        public List<object> links { get; set; }
        public object home_venue_id { get; set; }
        public string slug { get; set; }
        public double? score { get; set; }
        public Images images { get; set; }
        public List<Taxonomy> taxonomies { get; set; }
        public bool has_upcoming_events { get; set; }
        public int id { get; set; }
        public List<Genre> genres { get; set; }
    }

    public class Get_Performers
    {
        public Meta meta { get; set; }
        public List<Performer> performers { get; set; }
    }
}