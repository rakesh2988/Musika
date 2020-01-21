using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Instagram3
{


    public class Meta
    {
        public int code { get; set; }
    }

    public class Datum
    {
        public int media_count { get; set; }
        public string name { get; set; }
    }

    public class Instagram_Tags
    {
        public Meta meta { get; set; }
        public List<Datum> data { get; set; }
    }
}