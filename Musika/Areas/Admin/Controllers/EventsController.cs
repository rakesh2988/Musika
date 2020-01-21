using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Musika.Areas.Admin.Controllers
{
    public class EventsController : Controller
    {
        // GET: Admin/Events
        public ActionResult Index(Int64? ID, string search,string search2)
        {
            if (search != "-")
            {
                ViewBag.venuesearch = search;
            }
            else {
                ViewBag.venuesearch = "";
            }
            ViewBag.ArtistName = search2;
            return View();
        }
    }
}