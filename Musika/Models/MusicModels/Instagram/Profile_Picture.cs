using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Instagram2
{

    public class Meta
    {
        public int code { get; set; }
    }

    public class Counts
    {
        public int media { get; set; }
        public int followed_by { get; set; }
        public int follows { get; set; }
    }

    public class Data
    {
        public string username { get; set; }
        public string bio { get; set; }
        public string website { get; set; }
        public string profile_picture { get; set; }
        public string full_name { get; set; }
        public Counts counts { get; set; }
        public string id { get; set; }
    }

    public class Profile_Picture
    {
        public Meta meta { get; set; }
        public Data data { get; set; }
    }
}