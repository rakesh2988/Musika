using ArtSeeker.Library.Common;
using Musika.Areas.Admin.AdminModels.View;
using Musika.Models;
using Musika.Repository.GRepository;
using Musika.Repository.SPRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Musika.Repository.Interface;
using Musika.Repository.UnitofWork;
using Newtonsoft.Json.Linq;
using Musika.Library.Utilities;
using Musika.Enums;

namespace Musika.Areas.Admin.Controllers
{
    public class TicketingCategoryController : Controller
    {
        // GET: Admin/TicketInventory
        private static IUnitOfWork _unitOfWork;
        GenericRepository<TicketingCategory> _TicketingCategoryRepo = null;
        public TicketingCategoryController()
        {
            _unitOfWork = new UnitOfWork();
        }
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult GetTicketCategoryList(int PageIndex, string Name, string SortColumn, string SortOrder)
        {
            try
            {
                SpRepository _sp = new SpRepository();
                var dt = _sp.SpGetTicketingCategory(PageIndex, Name, SortColumn, SortOrder);

                List<TicketCategoryViewList> _list = new List<TicketCategoryViewList>();
                _list = General.DTtoList<TicketCategoryViewList>(dt);

                Dictionary<string, object> d = new Dictionary<string, object>();

                if (_list.Count() > 0)
                {

                    d.Add("Items", _list);
                    d.Add("PageNumber", _list[0].PageNo);
                    d.Add("PageCount", _list[0].T_Pages);
                    //d.Add("PageSize", _list[0].PageSize);
                }
                return Json(d, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Json(false, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetRecordById(int id)
        {
            try
            {
                _TicketingCategoryRepo = new GenericRepository<TicketingCategory>(_unitOfWork);
                var obj = _TicketingCategoryRepo.Repository.Get(x => x.CategoryId == id);
                return Json(new { Success = true, Model = obj }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Json(new { Success = false, Id = 0, TotalSeats = 0, TourDateId = 0 }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public ActionResult UpdateTicket(int CategoryId, string CategoryName, decimal CategoryPrice, string RecordStatus)
        {
            try
            {
                _TicketingCategoryRepo = new GenericRepository<TicketingCategory>(_unitOfWork);

                //duplicate check

                var items = _TicketingCategoryRepo.Repository.GetAll(x => x.CategoryName.Trim().ToLower().Equals(CategoryName.Trim().ToLower()));

                if (items.Count > 0)
                {
                    return Json(new { Success = false, RetMessage = "Record Already Exist." }, JsonRequestBehavior.AllowGet);
                }

                if (CategoryId > 0)
                {
                    var obj = _TicketingCategoryRepo.Repository.Get(x => x.CategoryId == CategoryId);
                    obj.CategoryName = CategoryName;
                    obj.CategoryPrice = CategoryPrice;
                    obj.RecordStatus = RecordStatus;
                    obj.ModifiedDatet = DateTime.Now;
                    _TicketingCategoryRepo.Repository.Update(obj);
                    return Json(new { Success = true, RetMessage = "Record Updated Successsfully." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var obj = new TicketingCategory();
                    obj.CategoryName = CategoryName;
                    obj.CategoryPrice = CategoryPrice;
                    obj.RecordStatus = RecordStatus;
                    obj.CreatedDate = DateTime.Now;
                    obj.ModifiedDatet = DateTime.Now;
                    _TicketingCategoryRepo.Repository.Add(obj);
                    return Json(new { Success = true, RetMessage = "Record Added Successsfully." }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(new { Success = false, RetMessage = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult Delete(int id)
        {
            try
            {
                _TicketingCategoryRepo = new GenericRepository<TicketingCategory>(_unitOfWork);
                var ticket = _TicketingCategoryRepo.Repository.Get(x => x.CategoryId == id);
                ticket.RecordStatus = RecordStatus.Deleted.ToString();
                _TicketingCategoryRepo.Repository.Update(ticket);
                return Json(new { Success = true, RetMessage = "Record Deleted Successsfully." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, RetMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}