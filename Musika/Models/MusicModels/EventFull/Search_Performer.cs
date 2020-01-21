using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventFull
{

    public class Small
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

    public class Thumb
    {
        public string width { get; set; }
        public string url { get; set; }
        public string height { get; set; }
    }

    public class Image
    {
        public Small small { get; set; }
        public object caption { get; set; }
        public Medium medium { get; set; }
        public Thumb thumb { get; set; }
    }

    public class Performer
    {
        public string creator { get; set; }
        public object vanity_url { get; set; }
        public string demand_member_count { get; set; }
        public string name { get; set; }
        public string event_count { get; set; }
        public string short_bio { get; set; }
        public Image image { get; set; }
        public string created { get; set; }
        public object user { get; set; }
        public string url { get; set; }
        public object demand_count { get; set; }
        public string id { get; set; }
    }

    public class Performers
    {
        public Performer performer { get; set; }
    }

    public class Search_Performer
    {
        public object last_item { get; set; }
        public string version { get; set; }
        public string total_items { get; set; }
        public object first_item { get; set; }
        public string page_number { get; set; }
        public string page_size { get; set; }
        public object page_items { get; set; }
        public string search_time { get; set; }
        public string page_count { get; set; }
        public Performers performers { get; set; }
    }
}