using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusikaDataService.Models
{

    public class Meta
    {
        public object geolocation { get; set; }
        public int per_page { get; set; }
        public int total { get; set; }
        public int took { get; set; }
        public int page { get; set; }
    }

    public class Images
    {
        public string huge { get; set; }
    }

    public class Stats
    {
        public int event_count { get; set; }
    }

    public class Taxonomy
    {
        public string name { get; set; }
        public int? parent_id { get; set; }
        public int id { get; set; }
    }

    public class Images2
    {
        public string __invalid_name__500_700 { get; set; }
        public string huge { get; set; }
        public string fb_100x72 { get; set; }
        public string criteo_170_235 { get; set; }
        public string mongo { get; set; }
        public string __invalid_name__800x320 { get; set; }
        public string fb_600_315 { get; set; }
        public string ipad_mini_explore { get; set; }
        public string criteo_130_160 { get; set; }
        public string __invalid_name__1200x525 { get; set; }
        public string criteo_205_100 { get; set; }
        public string __invalid_name__1200x627 { get; set; }
        public string square_mid { get; set; }
        public string triggit_fb_ad { get; set; }
        public string ipad_event_modal { get; set; }
        public string criteo_400_300 { get; set; }
        public string __invalid_name__136x136 { get; set; }
        public string banner { get; set; }
        public string block { get; set; }
        public string ipad_header { get; set; }
    }

    public class Genre
    {
        public string name { get; set; }
        public string image { get; set; }
        public bool primary { get; set; }
        public int id { get; set; }
        public Images2 images { get; set; }
        public string slug { get; set; }
    }

    public class Performer
    {
        public List<object> links { get; set; }
        public string image { get; set; }
        public bool primary { get; set; }
        public object colors { get; set; }
        public Images images { get; set; }
        public bool has_upcoming_events { get; set; }
        public int id { get; set; }
        public Stats stats { get; set; }
        public string image_license { get; set; }
        public double score { get; set; }
        public object location { get; set; }
        public List<Taxonomy> taxonomies { get; set; }
        public string type { get; set; }
        public int num_upcoming_events { get; set; }
        public string short_name { get; set; }
        public object home_venue_id { get; set; }
        public string slug { get; set; }
        public object divisions { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public int popularity { get; set; }
        public string image_attribution { get; set; }
        public List<Genre> genres { get; set; }
    }

    public class RootObject
    {
        public Meta meta { get; set; }
        public List<Performer> performers { get; set; }
    }

}


