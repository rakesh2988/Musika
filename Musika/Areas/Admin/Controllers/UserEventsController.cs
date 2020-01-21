using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Musika.Areas.Admin.Controllers
{
    public class UserEventsController : Controller
    {
        // GET: Admin/UserEvents
        public ActionResult Index(Int64? ID, string search)
        {
            ViewBag.email = search;
            return View();
        }
    }
}