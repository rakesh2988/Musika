using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusikaDataService.Models
{
    public class SeatGeekEventResponse
    {
        public class Meta
        {
            public object geolocation { get; set; }
            public int per_page { get; set; }
            public int total { get; set; }
            public int took { get; set; }
            public int page { get; set; }
        }

        public class InHand
        {
        }

        public class Stats
        {
            public int? visible_listing_count { get; set; }
            public List<int> dq_bucket_counts { get; set; }
            public decimal? average_price { get; set; }
            public int? lowest_price_good_deals { get; set; }
            public decimal? median_price { get; set; }
            public int? listing_count { get; set; }
            public decimal? lowest_price { get; set; }
            public decimal? highest_price { get; set; }
        }

        public class Announcements
        {
        }

        public class Taxonomy
        {
            public string name { get; set; }
            public int? parent_id { get; set; }
            public int id { get; set; }
        }

        public class Images
        {
            public string huge { get; set; }
        }

        public class Stats2
        {
            public int event_count { get; set; }
        }

        public class Location
        {
            public double lat { get; set; }
            public double lon { get; set; }
        }

        public class DocumentSource
        {
            public string source_type { get; set; }
            public string generation_type { get; set; }
        }

        public class Taxonomy2
        {
            public string name { get; set; }
            public int? parent_id { get; set; }
            public int id { get; set; }
            public DocumentSource document_source { get; set; }
        }

        public class PerformerEvent
        {
            public string image { get; set; }
            public bool primary { get; set; }
            public object colors { get; set; }
            public Images images { get; set; }
            public bool has_upcoming_events { get; set; }
            public int id { get; set; }
            public Stats2 stats { get; set; }
            public string image_license { get; set; }
            public double score { get; set; }
            public Location location { get; set; }
            public List<Taxonomy2> taxonomies { get; set; }
            public string type { get; set; }
            public int num_upcoming_events { get; set; }
            public string short_name { get; set; }
            public int? home_venue_id { get; set; }
            public string slug { get; set; }
            public object divisions { get; set; }
            public bool home_team { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public int popularity { get; set; }
            public string image_attribution { get; set; }
            public bool? away_team { get; set; }
        }

        public class Location2
        {
            public double lat { get; set; }
            public double lon { get; set; }
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
            public string extended_address { get; set; }
            public string display_location { get; set; }
            public string state { get; set; }
            public double score { get; set; }
            public Location2 location { get; set; }
            public object access_method { get; set; }
            public int num_upcoming_events { get; set; }
            public string address { get; set; }
            public string slug { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string country { get; set; }
            public int popularity { get; set; }
            public string name_v2 { get; set; }
        }

        public class Event
        {
            public List<object> links { get; set; }
            public object event_promotion { get; set; }
            public bool is_open { get; set; }
            public int id { get; set; }
            public Stats stats { get; set; }
            public string title { get; set; }
            public DateTime announce_date { get; set; }
            public double score { get; set; }
            public object access_method { get; set; }
            public Announcements announcements { get; set; }
            public List<Taxonomy> taxonomies { get; set; }
            public string type { get; set; }
            public string status { get; set; }
            public string description { get; set; }
            public DateTime datetime_local { get; set; }
            public DateTime visible_until_utc { get; set; }
            public bool time_tbd { get; set; }
            public bool date_tbd { get; set; }
            public List<Performer> performers { get; set; }
            public string url { get; set; }
            public DateTime created_at { get; set; }
            public double popularity { get; set; }
            public Venue venue { get; set; }
            public string short_title { get; set; }
            public DateTime datetime_utc { get; set; }
            public bool datetime_tbd { get; set; }
        }

        public class RootObject
        {
            public Meta meta { get; set; }
            public InHand in_hand { get; set; }
            public List<Event> events { get; set; }
        }
    }

}