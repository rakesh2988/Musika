using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Musika.Areas.Admin.Controllers
{
    public class ArtistGenreController : Controller
    {
        // GET: Admin/ArtistGenre
        public ActionResult Index(Int64? ID, string search)
        {
            ViewBag.ArtistName = search;
            return View();
        }
    }
}