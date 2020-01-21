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
    public class ChangePasswordController : Controller
    {
       private readonly IUnitOfWork _unitOfWork;

       public ChangePasswordController()
        {
            _unitOfWork = new UnitOfWork();
        }

        // GET: Admin/ForgetPassword
        public ActionResult ForgetPassword()
        {

            return View();
        }

        public ActionResult ChangePassword()
        {

            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(FormCollection data)
        {
            int AdminID = Numerics.GetInt(Session["AdminID"]);
            if (AdminID > 0)
            {
                if (data.Count > 0)
                {
                    string oldPassword = AesCryptography.Encrypt(data["OldPassword"]);
                    string newPassword = AesCryptography.Encrypt(data["NewPassword"]);
                    GenericRepository<Users> _userRepo = new GenericRepository<Users>(_unitOfWork);

                    Users entity = _userRepo.Repository.Get(p => p.Password == oldPassword && p.UserID == AdminID);
                    if (entity != null)
                    {
                        entity.Password = newPassword;
                        _userRepo.Repository.Update(entity);
                        ViewBag.Message = "Password updated successfully.";
                        ViewBag.Type = "alert-success";
                    }
                    else
                    {
                        ViewBag.Message = "Old Password is incorrect.";
                        ViewBag.Type = "alert-danger";
                    }
                }
                return View();
            }
            else
            {
                return RedirectToAction("index", "login");
            }

        }


    }
}