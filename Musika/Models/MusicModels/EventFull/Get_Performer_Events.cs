using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventFull2
{

    public class Comment
    {
        public string time { get; set; }
        public object user_reputation { get; set; }
        public string text { get; set; }
        public string id { get; set; }
        public string username { get; set; }
    }

    public class Comments
    {
        public List<Comment> comment { get; set; }
    }

    public class Link
    {
        public string time { get; set; }
        public object user_reputation { get; set; }
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

    public class Large
    {
        public string width { get; set; }
        public string url { get; set; }
        public string height { get; set; }
    }

    public class Image
    {
        public Large large { get; set; }
        public string creator { get; set; }
        public string id { get; set; }
    }

    public class Images
    {
        public List<Image> image { get; set; }
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

    public class Demand
    {
        public string location { get; set; }
        public string status { get; set; }
        public string member_count { get; set; }
        public string id { get; set; }
        public string description { get; set; }
    }

    public class Demands
    {
        public List<Demand> demand { get; set; }
    }

    public class Event
    {
        public string location { get; set; }
        public string all_day { get; set; }
        public object stop_time { get; set; }
        public string start_time { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string id { get; set; }
        public string description { get; set; }
    }

    public class Events
    {
        public List<Event> @event { get; set; }
    }

    public class Get_Performer_Events
    {
        public string withdrawn { get; set; }
        public string creator { get; set; }
        public string event_count { get; set; }
        public Comments comments { get; set; }
        public string url { get; set; }
        public string id { get; set; }
        public object trackbacks { get; set; }
        public Links links { get; set; }
        public Images images { get; set; }
        public string is_human { get; set; }
        public object withdrawn_note { get; set; }
        public string name { get; set; }
        public string demand_member_count { get; set; }
        public object vanity_url { get; set; }
        public Categories categories { get; set; }
        public string short_bio { get; set; }
        public Tags tags { get; set; }
        public string modified { get; set; }
        public string created { get; set; }
        public Demands demands { get; set; }
        public Events events { get; set; }
        public string long_bio { get; set; }
        public string demand_count { get; set; }
        public object user_id { get; set; }
    }
}