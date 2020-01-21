using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MusicGraph2
{


    public class Status
    {
        public string api { get; set; }
        public string message { get; set; }
        public int code { get; set; }
    }

    public class Data2
    {
        public int value { get; set; }
        public string time { get; set; }
    }

    public class Popularity
    {
        public Data2 data { get; set; }
    }

    public class Data3
    {
        public int value { get; set; }
        public string time { get; set; }
    }

    public class Followers
    {
        public Data3 data { get; set; }
    }

    public class Spotify
    {
        public string url { get; set; }
        public Popularity popularity { get; set; }
        public Followers followers { get; set; }
    }

    public class Data4
    {
        public int value { get; set; }
        public string time { get; set; }
    }

    public class ViewsLastMonth
    {
        public Data4 data { get; set; }
    }

    public class Data5
    {
        public int value { get; set; }
        public string time { get; set; }
    }

    public class ViewsLastDay
    {
        public Data5 data { get; set; }
    }

    public class Data6
    {
        public Int64 value { get; set; }
        public string time { get; set; }
    }

    public class ViewsTotal
    {
        public Data6 data { get; set; }
    }

    public class Data7
    {
        public int value { get; set; }
        public string time { get; set; }
    }

    public class ViewsLastWeek
    {
        public Data7 data { get; set; }
    }

    public class Vevo
    {
        public string url { get; set; }
        public ViewsLastMonth viewsLastMonth { get; set; }
        public ViewsLastDay viewsLastDay { get; set; }
        public ViewsTotal viewsTotal { get; set; }
        public ViewsLastWeek viewsLastWeek { get; set; }
    }

    public class Data8
    {
        public int value { get; set; }
        public string time { get; set; }
    }

    public class Following
    {
        public Data8 data { get; set; }
    }

    public class Data9
    {
        public int value { get; set; }
        public string time { get; set; }
    }

    public class Tweets
    {
        public Data9 data { get; set; }
    }

    public class Data10
    {
        public int value { get; set; }
        public string time { get; set; }
    }

    public class Followers2
    {
        public Data10 data { get; set; }
    }

    public class Twitter
    {
        public Following following { get; set; }
        public Tweets tweets { get; set; }
        public Followers2 followers { get; set; }
        public string url { get; set; }
    }

    public class Youtube
    {
        public List<string> url { get; set; }
    }

    public class Data11
    {
        public int value { get; set; }
        public string time { get; set; }
    }

    public class PeopleTalkingAbout
    {
        public Data11 data { get; set; }
    }

    public class Data12
    {
        public int value { get; set; }
        public string time { get; set; }
    }

    public class Likes
    {
        public Data12 data { get; set; }
    }

    public class Facebook
    {
        public string url { get; set; }
        public PeopleTalkingAbout people_talking_about { get; set; }
        public Likes likes { get; set; }
    }

    public class Data13
    {
        public int value { get; set; }
        public string time { get; set; }
    }

    public class Listeners
    {
        public Data13 data { get; set; }
    }

    public class Data14
    {
        public int value { get; set; }
        public string time { get; set; }
    }

    public class Playcount
    {
        public Data14 data { get; set; }
    }

    public class Lastfm
    {
        public string url { get; set; }
        public Listeners listeners { get; set; }
        public Playcount playcount { get; set; }
    }

    public class Data15
    {
        public int value { get; set; }
        public string time { get; set; }
    }

    public class Following2
    {
        public Data15 data { get; set; }
    }

    public class Data16
    {
        public int value { get; set; }
        public string time { get; set; }
    }

    public class MediaCount
    {
        public Data16 data { get; set; }
    }

    public class Data17
    {
        public int value { get; set; }
        public string time { get; set; }
    }

    public class Followers3
    {
        public Data17 data { get; set; }
    }

    public class Instagram
    {
        public Following2 following { get; set; }
        public string url { get; set; }
        public MediaCount media_count { get; set; }
        public Followers3 followers { get; set; }
    }

    public class Data
    {
        public Spotify spotify { get; set; }
        public Vevo vevo { get; set; }
        public string name { get; set; }
        public Twitter twitter { get; set; }
        public Youtube youtube { get; set; }
        public Facebook facebook { get; set; }
        public Lastfm lastfm { get; set; }
        public string id { get; set; }
        public Instagram instagram { get; set; }
    }

    public class ArtistMatrics_ByID
    {
        public Status status { get; set; }
        public Data data { get; set; }
    }


}