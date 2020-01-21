using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Musika.Areas.Admin.Controllers
{
    public class GenreController : Controller
    {
        // GET: Admin/Genre
        public ActionResult Index()
        {
            return View();
        }
    }
}