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
using Musika.Areas.Admin.AdminModels.Input;
using System.Data;

namespace Musika.Areas.Admin.Controllers
{
    public class TicketInventoryController : Controller
    {
        // GET: Admin/TicketInventory
        private static IUnitOfWork _unitOfWork;
        GenericRepository<TicketingInventory> _TicketRepo = null;
        GenericRepository<TourDate> _TourDateRepo = null;
        SpRepository _sp = new SpRepository();
        List<ViewEventlst> _list = new List<ViewEventlst>();
        GenericRepository<Venue> _VenueRepo = null;
        GenericRepository<TicketingCategory> _TicketingCategoryRepo = null;
        GenericRepository<TicketingInventoryDetails> _TicketingDetailsRepo = null;
        public TicketInventoryController()
        {
            _unitOfWork = new UnitOfWork();
        }
        public ActionResult Index()
        {
            _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
            _TicketingCategoryRepo = new GenericRepository<TicketingCategory>(_unitOfWork);
            _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
            //get events list

            ViewBag.TourDate = _list;
            ViewBag.Section = _TicketingCategoryRepo.Repository.GetAll().ToList();
            return View();
        }

        public ActionResult GetEvents(string query)
        {
            try
            {
                DataSet ds = _sp.SpGetEventList();
                _list = General.DTtoList<ViewEventlst>(ds.Tables[0]);
                _list = _list.Where(x => x.EventName.Trim().ToLower().Contains(query)).ToList();
                Dictionary<string, object> d = new Dictionary<string, object>();
                d.Add("Items", _list);
                return Json(d, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new List<ViewEventlst>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetTicketingList(int PageIndex, string EventName, string ArtistName, string GenreName, string SortColumn, string SortOrder)
        {
            try
            {
                SpRepository _sp = new SpRepository();
                var dt = _sp.SpGetTicketingInventory(PageIndex, EventName, ArtistName, GenreName, SortColumn, SortOrder);

                List<TicketViewList> _list = new List<TicketViewList>();
                _list = General.DTtoList<TicketViewList>(dt);

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

        public ActionResult GetRecordById(int TicketId)
        {
            try
            {
                _TicketRepo = new GenericRepository<TicketingInventory>(_unitOfWork);
                _TicketingDetailsRepo = new GenericRepository<TicketingInventoryDetails>(_unitOfWork);
                _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                var obj = _TicketRepo.Repository.Get(x => x.TicketId == TicketId);
                var item = _TicketingDetailsRepo.Repository.GetAll(x => x.TicketId == obj.TicketId).ToList();
                var eventobj = _TourDateRepo.Repository.Get(x => x.TourDateID == obj.TourDateId);
                List<object> keylist = new List<object>();
                if (item.Count > 0)
                {
                    var listobj = item.GroupBy(x => x.SectionType);
                    foreach (var k in listobj)
                    {
                        var o = k.ToList().GroupBy(x => x.SectionType).Select(c => new
                        {
                            SeatFrom = c.Min(x => x.SeatNo),
                            SeatTo = c.Max(x => x.SeatNo),
                            TicketId = c.Max(x => x.TicketId),
                            SectionType = c.Key,
                            SectionPrice = c.Max(x => x.SectionPrice)
                        }).FirstOrDefault();
                        keylist.Add(o);
                    }
                }

                return Json(new { Success = true, Id = obj.TicketId, TotalSeats = obj.TotalSeats,TourDate= eventobj.EventName, TourDateId = obj.TourDateId, TicketsList = keylist }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Json(new { Success = false, Id = 0, TotalSeats = 0, TourDateId = 0 }, JsonRequestBehavior.AllowGet);

            }
        }


        public ActionResult GetEventDetails(int eventid)
        {
            try
            {
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);

                var _ArtistID = _TourDateRepo.Repository.Get(p => p.TourDateID == eventid).ArtistID;


                GenericRepository<TicketingInventory> _TicketingInventoryRepo = new GenericRepository<TicketingInventory>(_unitOfWork);

                var ticketing = _TicketingInventoryRepo.Repository.GetAll(x => x.TourDateId == eventid).FirstOrDefault();
                int totalseats = 0;
                bool inappticketing = false;
                if (ticketing != null)
                {
                    totalseats = Convert.ToInt32(ticketing.TotalSeats);
                    inappticketing = Convert.ToBoolean(ticketing.InAppTicketing);
                }


                var _TourDate = (from A in _TourDateRepo.Repository.GetAll(p => p.TourDateID == eventid)
                                 join B in _VenueRepo.Repository.GetAll() on A.VenueID equals B.VenueID
                                 join C in _ArtistsRepo.Repository.GetAll(p => p.ArtistID == _ArtistID) on A.ArtistID equals C.ArtistID
                                 select new
                                 {
                                     TourDateID = A.TourDateID,
                                     EventName = A.EventName,
                                     SeatGeek_TourID = A.SeatGeek_TourID,
                                     HashTag = A.HashTag,
                                     ArtistName = C.ArtistName,
                                     ArtistID = C.ArtistID,
                                     Datetime_Local = A.Datetime_Local,
                                     VenueName = B.VenueName,
                                     VenueID = B.VenueID,
                                     SeatGeek_VenuID = B.SeatGeek_VenuID,
                                     Extended_Address = B.Extended_Address,
                                     Address = B.Address,
                                     VenueCountry = B.VenueCountry,
                                     VenueCity = B.VenueCity,
                                     VenueState = B.VenueState,
                                     Postal_Code = B.Postal_Code,
                                     VenueLat = B.VenueLat,
                                     VenueLong = B.VenueLong,
                                     Timezone = B.Timezone,
                                     ImageURL = B.ImageURL,
                                     TicketURL = A.TicketURL,
                                     InAppTicketing = inappticketing,
                                     TotalSeats = totalseats,
                                     IsDeleted = A.IsDeleted
                                 }).ToList();

                if (_TourDate != null)
                {
                    return Json(new { Success = true, EventDetails = _TourDate[0] }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Success = false, RetMessage = "Event Not Found!" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, RetMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult UpdateTicket(InputTicketing data)
        {
            try
            {
                _TicketRepo = new GenericRepository<TicketingInventory>(_unitOfWork);
                _TicketingDetailsRepo = new GenericRepository<TicketingInventoryDetails>(_unitOfWork);



                if (data.TicketId > 0)
                {
                    var obj = _TicketRepo.Repository.Get(x => x.TicketId == data.TicketId);
                    //delete ticketing details 
                    var details = _TicketingDetailsRepo.Repository.GetAll(x => x.TicketId == data.TicketId).ToList();
                    foreach (var item in details)
                    {
                        _TicketingDetailsRepo.Repository.DeletePermanent(item.TicketDetailId);
                    }
                    _unitOfWork.StartTransaction();
                    //update ticket master info
                    obj.TotalSeats = data.TotalSeats;
                    obj.TourDateId = data.TourDate;
                    obj.InAppTicketing = true;
                    obj.ModifiedDatet = DateTime.Now;
                    _TicketRepo.Repository.Update(obj);

                    foreach (var item in data.List)
                    {
                        for (int i = item.SeatFrom; i <= item.SeatTo; i++)
                        {
                            _TicketingDetailsRepo.Repository
                            .Add(
                                    new TicketingInventoryDetails()
                                    {
                                        TicketId = data.TicketId,
                                        TourDateId = data.TourDate,
                                        SeatNo = i,
                                        SectionPrice = item.SectionPrice,
                                        SectionType = item.SectionType,
                                        CreatedDate = DateTime.Now,
                                        ModifiedDatet = DateTime.Now,
                                        RecordStatus = RecordStatus.Active.ToString()
                                    });
                        }
                    }

                    _unitOfWork.Commit();
                    return Json(new { Success = true, RetMessage = "Record Update Successfully." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _unitOfWork.StartTransaction();
                    var model = new TicketingInventory();
                    model.InAppTicketing = true;
                    model.TotalSeats = data.TotalSeats;
                    model.TourDateId = data.TourDate;
                    model.CreatedDate = DateTime.Now;
                    model.ModifiedDatet = DateTime.Now;
                    model.RecordStatus = RecordStatus.Active.ToString();
                    _TicketRepo.Repository.Add(model);

                    foreach (var item in data.List)
                    {
                        for (int i = item.SeatFrom; i <= item.SeatTo; i++)
                        {
                            _TicketingDetailsRepo.Repository
                            .Add(
                                    new TicketingInventoryDetails()
                                    {
                                        TicketId = model.TicketId,
                                        TourDateId = model.TourDateId,
                                        SeatNo = i,
                                        SectionPrice = item.SectionPrice,
                                        SectionType = item.SectionType,
                                        CreatedDate = DateTime.Now,
                                        ModifiedDatet = DateTime.Now,
                                        RecordStatus = RecordStatus.Active.ToString()
                                    });
                        }
                    }

                    _unitOfWork.Commit();
                    return Json(new { Success = true, RetMessage = "Record Save Successfully." }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                return Json(new { Success = false, RetMessage = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult Delete(int id)
        {
            try
            {
                _TicketRepo = new GenericRepository<TicketingInventory>(_unitOfWork);
                var ticket = _TicketRepo.Repository.Get(x => x.TicketId == id);
                ticket.RecordStatus = RecordStatus.Deleted.ToString();
                _TicketRepo.Repository.Update(ticket);
                return Json(new { Success = true, RetMessage = "Record Deleted Successsfully." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, RetMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}