using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Instagram
{
    public class Datum
    {
        public string username { get; set; }
        public string first_name { get; set; }
        public string profile_picture { get; set; }
        public string id { get; set; }
        public string last_name { get; set; }
    }

    public class Instagram_Search
    {
        public List<Datum> data { get; set; }
    }
}