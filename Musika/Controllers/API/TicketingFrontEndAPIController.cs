using ArtSeeker.Library.Common;
using Musika.Areas.Admin.AdminModels.View;
using Musika.Enums;
using Musika.Library.API;
using Musika.Library.CacheProvider;
using Musika.Library.Utilities;
using Musika.Models;
using Musika.Models.API.Input;
using Musika.Models.API.View;
using Musika.Repository.GRepository;
using Musika.Repository.Interface;
using Musika.Repository.SPRepository;
using Musika.Repository.UnitofWork;
using MvcPaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;
using System.IO;
using System.Drawing;
using Musika.Library.Multipart;
using ThoughtWorks.QRCode.Codec;
using ZXing;
using ZXing.Common;
using System.Drawing.Imaging;
using System.Xml;
using System.Net.Mail;
using System.Net.Mime;

namespace Musika.Controllers.API
{
    [EnableCors(origins: "*", headers: "*", methods: "*", SupportsCredentials = true)]
    public class TicketingFrontEndAPIController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpCache _Cache = new HttpCache();

        int _Imagethumbsize = 0;
        int _imageSize = 0;

        public TicketingFrontEndAPIController()
        {
            _unitOfWork = new UnitOfWork();
            _Imagethumbsize = Convert.ToInt16(WebConfigurationManager.AppSettings["ImageThumbSize"].ToString());
            _imageSize = Convert.ToInt16(WebConfigurationManager.AppSettings["ImageSize"].ToString());
        }

        #region "Final List of Events by User ID for Mobile"
        [HttpGet]
        [Route("api/TicketingFrontEndAPI/GetTicketingEventsByUserID")]
        public HttpResponseMessage GetTicketingEventsByUserID(string userID)
        {
            string result = string.Empty;
            List<Musika.Models.TicketingEventsNew> lstTicketingEvents = new List<Musika.Models.TicketingEventsNew>();

            Models.TicketingEventsNewModel model = new TicketingEventsNewModel();

            GenericRepository<TicketingEventsNew> _ticketingEntity = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            GenericRepository<TicketingUsers> _UserEntity = new GenericRepository<TicketingUsers>(_unitOfWork);
            GenericRepository<TicketingEventNewStaff> _StaffEvents = new GenericRepository<TicketingEventNewStaff>(_unitOfWork);

            TicketingUsers user = new TicketingUsers();
            _unitOfWork.StartTransaction();

            try
            {
                user = _UserEntity.Repository.GetAll().Where(p => p.UserID == Convert.ToUInt32(userID)).FirstOrDefault();
                if (user != null)
                {
                    if (user.UserType == "Event Organizer")
                    {
                        lstTicketingEvents = _ticketingEntity.Repository.GetAll().Where(p => p.CreatedBy == Convert.ToInt32(userID) && p.ISDELETED != true && p.IsApproved == true).ToList();
                    }
                    else if (user.UserType == "Staff")
                    {
                        List<TicketingEventNewStaff> lstEventStaff = new List<TicketingEventNewStaff>();
                        lstEventStaff = _StaffEvents.Repository.GetAll().Where(p => p.StaffId == Convert.ToInt32(userID)).ToList();
                        if (lstEventStaff.Count > 0)
                        {
                            for (int i = 0; i < lstEventStaff.Count; i++)
                            {
                                int evtId = Convert.ToInt32(lstEventStaff[i].EventId);
                                TicketingEventsNew evt = new TicketingEventsNew();
                                evt = _ticketingEntity.Repository.GetAll().Where(p => p.EventID == evtId).FirstOrDefault();

                                lstTicketingEvents.Add(evt);
                            }
                        }
                    }
                }

                List<Models.ViewTicketingEventListNew> lstEvents = new List<Models.ViewTicketingEventListNew>();
                if (lstTicketingEvents.Count > 0)
                {
                    for (int i = 0; i < lstTicketingEvents.Count; i++)
                    {
                        Models.ViewTicketingEventListNew temp = new Models.ViewTicketingEventListNew();
                        temp.EventID = lstTicketingEvents[i].EventID;
                        temp.StartDate = lstTicketingEvents[i].StartDate ?? DateTime.Now;
                        temp.StartTime = lstTicketingEvents[i].StartTime;
                        temp.EndDate = lstTicketingEvents[i].EndDate ?? DateTime.Now;
                        temp.EndTime = lstTicketingEvents[i].EndTime;
                        temp.VenueName = lstTicketingEvents[i].VenueName;
                        if (!String.IsNullOrEmpty(lstTicketingEvents[i].ArtistId.ToString()))
                        {
                            temp.ArtistName = new TicketingAPIController().GetArtistName(lstTicketingEvents[i].ArtistId);
                        }
                        else
                        {
                            temp.ArtistName = string.Empty;
                        }
                        if (!String.IsNullOrEmpty(lstTicketingEvents[i].EventImage))
                        {
                            temp.EventImage = lstTicketingEvents[i].EventImage;
                        }
                        else
                        {
                            temp.EventImage = string.Empty;
                        }
                        lstEvents.Add(temp);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, lstEvents.OrderByDescending(p => p.StartDate)));
            }
            catch (Exception ee)
            {
                LogHelper.CreateLog3(ee, Request);
                _unitOfWork.RollBack();//RollBack Transaction
                return Request.CreateResponse(HttpStatusCode.BadRequest, "");
            }
        }

        public string GetArtistName(int? artistId)
        {
            int id;
            List<Musika.Models.Artists> lstTicketingEvents = new List<Musika.Models.Artists>();
            GenericRepository<Musika.Models.Artists> _artistsEntity = new GenericRepository<Musika.Models.Artists>(_unitOfWork);
            if (String.IsNullOrEmpty(artistId.ToString()) || (artistId == 0))
            {
                id = 0;
                return String.Empty;
            }
            else
            {
                id = Convert.ToInt32(artistId);
                return _artistsEntity.Repository.GetById(id).ArtistName;
            }
        }
        #endregion

        #region "Get List of Ticket Category by Event ID"
        [HttpGet]
        [Route("api/TicketingFrontEndAPI/GetTicketingCategoryByEventID")]
        public HttpResponseMessage GetTicketingCategoryByEventID(int eventID)
        {
            List<Musika.Models.TicketingCategory> lstTicketingCategory = new List<Musika.Models.TicketingCategory>();
            DataSet ds = new Musika.Repository.SPRepository.SpRepository().GetTicketCategoryListByEventId(eventID);

            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];

                    TicketingCategory model = new TicketingCategory();
                    model.CategoryName = Convert.ToString(dr[1].ToString());
                    lstTicketingCategory.Add(model);
                }

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, lstTicketingCategory.OrderByDescending(p => p.CategoryName)));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, null));
            }
        }
        #endregion

        #region ""
        [AllowAnonymous]
        [Route("TicketingFrontEndAPI/GetCoupons")]
        [HttpGet]
        public HttpResponseMessage GetCoupons(string sEventName, string packageName, string sCouponCode, int Pageindex, int Pagesize, string sortColumn, string sortOrder)
        {
            try
            {
                GenericRepository<TicketingEventsNew> _TicketingEventRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);

                DataSet ds = new DataSet();
                SpRepository _sp = new SpRepository();                
                ds = _sp.GetCouponsList();
                var _list = General.DTtoList<CouponsModel>(ds.Tables[0]);

                _list = _list.GroupBy(x => x.CouponCode, (key, group) => group.First()).OrderBy(p => p.ExpiryDate).ToList();             

                //Pagination
                var _list2 = _list.ToPagedList(Pageindex - 1, Pagesize);

                Dictionary<string, object> d = new Dictionary<string, object>();
                if (_list2.Count() > 0)
                {
                    d.Add("Items", _list2);
                    d.Add("PageCount", _list2.PageCount);
                    d.Add("PageNumber", _list2.PageNumber);
                    d.Add("PageSize", Pagesize);

                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
                else
                {
                    d.Add("Items", new List<ViewArtistlst>());
                    d.Add("PageCount", 0);
                    d.Add("PageNumber", 0);
                    d.Add("PageSize", 0);
                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
        #endregion
    }
}