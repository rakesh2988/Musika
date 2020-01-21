using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Musika.Library.API;
using System.IO;
using System.Configuration;
using Musika.Models;
using Musika.Repository.GRepository;
using Musika.Repository.Interface;

namespace Musika.Areas.Admin.Controllers
{
    public class AdsController : Controller
    {
        // GET: Admin/Ads
        private readonly IUnitOfWork _unitOfWork;
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult AddNewAdsWithImage()
        {
            try
            {
                Dictionary<string, object> d = new Dictionary<string, object>();
                string Imagepath = "";
                string LinkURL = Request.Form["LinkURL"].ToString();
                string CityID = Request.Form["City"].ToString();
                string CityName = "";

                string fileKey = "File";
                string folderPath = "ADs";
                string MessageError = "";
                string FilePath = "";
                if (Request.Files.Count > 0)
                {

                    if (String.IsNullOrEmpty(CityID))
                    {
                        MessageError = "Please select the Country.";
                    }
                    else
                    {
                        int intCityID = Convert.ToInt32(CityID);
                        using (var db = new MusikaEntities())
                        {
                            var ObjCity = db.CountryCodes.Where(x => x.CountryCodeId == intCityID).Select(x => x.Name).FirstOrDefault();
                            if (ObjCity != null)
                            {
                                CityName = ObjCity;
                            }
                        }
                    }

                    var file = Request.Files.Count > 0 ? Request.Files[fileKey] : null;
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        string extension = Path.GetExtension(file.FileName).ToLower();
                        string[] arr = new string[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };

                        if (arr.Contains(extension))
                        {
                            string photoName = Guid.NewGuid() + extension;
                            string imagePath = ConfigurationManager.AppSettings["SiteImgPath"] + "\\" + folderPath + "\\" + photoName;
                            string newFilePath = ConfigurationManager.AppSettings["SiteImgPath"] + "\\" + folderPath + "\\";
                            if (!Directory.Exists(newFilePath))
                            {
                                Directory.CreateDirectory(newFilePath);
                            }
                            file.SaveAs(imagePath);

                            FilePath = ConfigurationManager.AppSettings["SiteImgURL"].ToString() + "/" + folderPath.Replace(@"\", "/") + "/" + photoName;
                            if (CityName != "")
                            {
                                InsertNewAds(FilePath, LinkURL, CityName);
                            }
                            else
                            {
                                MessageError = "Please select the proper Country.";
                            }
                        }
                        else
                        {
                            MessageError = "File extension not supported.";
                        }
                    }
                    else
                    {
                        MessageError = "File not found.";
                    }
                }
                else
                {
                    MessageError = "Please select an image.";
                }
                MessageError = "success";
                return Json(MessageError, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }
        }

        public bool InsertNewAds(string ImageURL, string LinkURL, string City)
        {
            bool result = false;
            try
            {
                Ads objAds = new Ads();
                objAds.ImageURL = ImageURL;
                objAds.ThumbnailURL = "";
                objAds.LinkURL = LinkURL;
                objAds.CreatedDate = DateTime.UtcNow;
                objAds.City = City;
                objAds.Recordstatus = Musika.Enums.RecordStatus.Active.ToString();
                using (var db = new MusikaEntities())
                {
                    db.Ads.Add(objAds);
                    db.SaveChanges();
                }

                result = true;
            }
            catch (Exception)
            {
                result = false;
                throw;
            }
            return result;
        }
    }
}