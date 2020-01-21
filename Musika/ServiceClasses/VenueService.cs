using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.ServiceClasses
{
    public class VenueService
    {
        public class Meta
        {
            public object geolocation { get; set; }
            public int per_page { get; set; }
            public int total { get; set; }
            public int took { get; set; }
            public int page { get; set; }
        }

        public class Stats
        {
            public int event_count { get; set; }
        }

        public class Location
        {
            public double lat { get; set; }
            public double lon { get; set; }
        }

        public class AccessMethod
        {
            public bool employee_only { get; set; }
            public DateTime created_at { get; set; }
            public string method { get; set; }
        }

        public class Venue
        {
            public List<object> links { get; set; }
            public int metro_code { get; set; }
            public string postal_code { get; set; }
            public string timezone { get; set; }
            public bool has_upcoming_events { get; set; }
            public int id { get; set; }
            public string city { get; set; }
            public Stats stats { get; set; }
            public string extended_address { get; set; }
            public string display_location { get; set; }
            public string state { get; set; }
            public double score { get; set; }
            public Location location { get; set; }
            public AccessMethod access_method { get; set; }
            public int num_upcoming_events { get; set; }
            public string address { get; set; }
            public string slug { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string country { get; set; }
            public int popularity { get; set; }
            public string name_v2 { get; set; }
        }

        public class RootObject
        {
            public Meta meta { get; set; }
            public List<Venue> venues { get; set; }
        }
    }
}