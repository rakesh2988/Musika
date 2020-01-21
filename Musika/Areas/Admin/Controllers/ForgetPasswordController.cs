using Musika.Library.Utilities;
using Musika.Models;
using Musika.Repository.GRepository;
using Musika.Repository.Interface;
using Musika.Repository.UnitofWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Musika.Areas.Admin.Controllers
{
    public class ForgetPasswordController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ForgetPasswordController()
        {
            _unitOfWork = new UnitOfWork();
        }

        // GET: Admin/ForgetPassword
        public ActionResult ForgetPassword()
        {

            return View();
        }

        [HttpPost]
        public ActionResult ForgetPassword(FormCollection data)
        {
            string _Email = data["Email"];
            GenericRepository<Users> _userRepo = new GenericRepository<Users>(_unitOfWork);

            var entity = _userRepo.Repository.Get(p => p.Email == _Email && p.UserID == 1);
            if (entity != null)
            {
                string password = AesCryptography.Decrypt(entity.Password);

                string html = "<p>Hi " + entity.UserName + "</p>";
                html += "<p>Here is the password : " + password + "</p>";
                html += "<p><br><br>Thanks for using Ditto<br><strong>Ditto team</strong></p>";

                 EmailHelper.SendEmail(entity.Email, "Ditto : Forget Password", html);

                ViewBag.Message = "Please check your email to get password.";
                ViewBag.Type = "alert-success";

                // return RedirectToAction("index", "login");

            }
            else
            {
                ViewBag.Message = "No user registered with this email.";
                ViewBag.Type = "alert-danger";
            }

            return View();

        }

    }
}