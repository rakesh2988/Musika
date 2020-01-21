using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Musika.Areas.Admin.AdminModels.Input;
using Musika.Repository;
using Musika.Models;
using Musika.Library.Utilities;
using Musika.Repository.Interface;
using Musika.Repository.UnitofWork;
using Musika.Repository.GRepository;

namespace Musika.Areas.Admin.Controllers
{
    public class LoginController : Controller
    {
        // GET: Admin/Login
        private readonly IUnitOfWork _unitOfWork;

        public LoginController()
        {
            _unitOfWork = new UnitOfWork();
        }

        public ActionResult Index()
        {
            Session.RemoveAll();
            return View();
        }

        public ActionResult ChangeCurrentCulture(int id)
        {
            //
            // Change the current culture for this user.
            //
            CultureHelper.CurrentCulture = id;
            //
            // Cache the new current culture into the user HTTP session. 
            //
            Session["CurrentCulture"] = id;
            //
            // Redirect to the same page from where the request was made! 
            //
            return Redirect(Request.UrlReferrer.ToString());
        }


        [HttpPost]
        public ActionResult Index(InputLogin input)
        {
            if (ModelState.IsValid)
            {
                Users entity = null;

                if ("developer@musikaapp.com" != input.Email)
                // if ("salman@sdsol.com" != input.Email)
                {
                    ViewBag.Message = "Invalid Admin Email";
                    ViewBag.Type = "alert-danger";
                    Session.RemoveAll();
                    return View(input);
                }

                string _password = AesCryptography.Encrypt(input.Password);
                GenericRepository<Users> _userRepo = new GenericRepository<Users>(_unitOfWork);

                entity = _userRepo.Repository.Get(p => p.Email == input.Email && p.Password == _password);
                if (entity != null)
                {
                    Session["AdminID"] = entity.UserID;
                    Session["FullName"] = entity.UserName;
                    Session["ThumbnailURL"] = entity.ThumbnailURL;
                    Session["ImageURL"] = entity.ImageURL;

                    return RedirectToAction("Index", "Users");

                }
                else
                {
                    ViewBag.Message = "Invalid email or password";
                    ViewBag.Type = "alert-danger";

                }
            }
            Session.RemoveAll();
            return View(input);
        }
    }
}