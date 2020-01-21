using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventFull3
{

    public class Link
    {
        public string time { get; set; }
        public string url { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public string username { get; set; }
    }

    public class Links
    {
        public List<Link> link { get; set; }
    }

    public class Small
    {
        public string width { get; set; }
        public string url { get; set; }
        public string height { get; set; }
    }

    public class Thumb
    {
        public string width { get; set; }
        public string url { get; set; }
        public string height { get; set; }
    }

    public class Medium
    {
        public string width { get; set; }
        public string url { get; set; }
        public string height { get; set; }
    }

    public class Image
    {
        public Small small { get; set; }
        public string width { get; set; }
        public string creator { get; set; }
        public Thumb thumb { get; set; }
        public string height { get; set; }
        public string url { get; set; }
        public Medium medium { get; set; }
        public string id { get; set; }
    }

    public class Images
    {
        public List<Image> image { get; set; }
    }

    public class Performer
    {
        public string creator { get; set; }
        public string linker { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string id { get; set; }
        public string short_bio { get; set; }
    }

    public class Performers
    {
        public Performer performer { get; set; }
    }

    public class Category
    {
        public string name { get; set; }
        public string id { get; set; }
    }

    public class Categories
    {
        public List<Category> category { get; set; }
    }

    public class Tag
    {
        public string owner { get; set; }
        public string id { get; set; }
        public string title { get; set; }
    }

    public class Tags
    {
        public List<Tag> tag { get; set; }
    }

    public class Get_Event_ByID
    {
        public string withdrawn { get; set; }
        public string olson_path { get; set; }
        public object children { get; set; }
        public object comments { get; set; }
        public string region_abbr { get; set; }
        public string postal_code { get; set; }
        public string latitude { get; set; }
        public string all_day { get; set; }
        public object groups { get; set; }
        public string url { get; set; }
        public string id { get; set; }
        public string address { get; set; }
        public string privacy { get; set; }
        public Links links { get; set; }
        public Images images { get; set; }
        public object withdrawn_note { get; set; }
        public string longitude { get; set; }
        public string country_abbr { get; set; }
        public string region { get; set; }
        public string start_time { get; set; }
        public object tz_id { get; set; }
        public object description { get; set; }
        public object properties { get; set; }
        public object recurrence { get; set; }
        public string modified { get; set; }
        public string venue_display { get; set; }
        public object tz_country { get; set; }
        public Performers performers { get; set; }
        public string price { get; set; }
        public string title { get; set; }
        public object parents { get; set; }
        public string geocode_type { get; set; }
        public object tz_olson_path { get; set; }
        public string city { get; set; }
        public object free { get; set; }
        public object trackbacks { get; set; }
        public object calendars { get; set; }
        public string country { get; set; }
        public string owner { get; set; }
        public object going { get; set; }
        public string country_abbr2 { get; set; }
        public Categories categories { get; set; }
        public Tags tags { get; set; }
        public string venue_type { get; set; }
        public string created { get; set; }
        public string venue_id { get; set; }
        public object tz_city { get; set; }
        public object stop_time { get; set; }
        public string venue_name { get; set; }
    }
}