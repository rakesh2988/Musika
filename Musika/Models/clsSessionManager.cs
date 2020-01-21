using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class clsSessionManager
    {
        public string SUICulture
        {
            get
            {
                if (HttpContext.Current.Session["SUICulture"] == null)
                {
                    HttpContext.Current.Session["SUICulture"] = "es";
                }
                return HttpContext.Current.Session["SUICulture"].ToString();
            }
            set
            {
                HttpContext.Current.Session["SUICulture"] = value;
            }
        }
    }
}