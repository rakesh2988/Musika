using Musika.Enums;
using Musika.Library.CacheProvider;
using Musika.Models;
using Musika.Repository.GRepository;
using Musika.Repository.Interface;
using Musika.Repository.UnitofWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using MvcPaging;
using Musika.Areas.Admin.AdminModels.View;
using System.Linq.Dynamic;
using Musika.Library.Utilities;
using System.Configuration;
using Musika.Library.API;
using System.Net.Http.Formatting;
using System.IO;
using Musika.Library.Multipart;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Musika.Repository.SPRepository;
using System.Data;
using ArtSeeker.Library.Common;
using System.Text;
using Musika.Library.PushNotfication;
using System.Web.Http.Cors;
using Microsoft.Ajax.Utilities;
//using Musika.Models.API.View;

namespace Musika.Controllers.API
{
    [EnableCors(origins: "*", headers: "*", methods: "*", SupportsCredentials = true)]
    public class AdminAPIController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpCache _Cache = new HttpCache();

        int _Imagethumbsize = 0;
        int _imageSize = 0;

        public AdminAPIController()
        {
            _unitOfWork = new UnitOfWork();
            _Imagethumbsize = Convert.ToInt16(WebConfigurationManager.AppSettings["ImageThumbSize"].ToString());
            _imageSize = Convert.ToInt16(WebConfigurationManager.AppSettings["ImageSize"].ToString());
        }

        #region "Users"

        [AllowAnonymous]
        [Route("AdminAPI/GetUsers")]
        [HttpGet]
        public HttpResponseMessage GetUsers(string Name2, string Email2, int Pageindex, int Pagesize, string sortColumn, string sortOrder)
        {
            try
            {
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);

                GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);
                GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

                List<ViewUser> _ViewUser = new List<ViewUser>();

                DataSet ds = new DataSet();
                SpRepository _sp = new SpRepository();
                ds = _sp.SpGetUserList();

                var _list = General.DTtoList<ViewUser>(ds.Tables[0]);

                //Filters
                if (!string.IsNullOrEmpty(Name2))
                {
                    _list = _list.Where(p => p.UserName.IndexOf(Name2.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(Email2))
                {
                    _list = _list.Where(p => p.Email.IndexOf(Email2.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                //Sorting
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    _list = _list.OrderBy("" + sortColumn + " " + sortOrder).ToList();
                }

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
                    d.Add("Items", new List<ViewUser>());
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

        [AllowAnonymous]
        [Route("AdminAPI/DeleteUser/{id}")]
        [HttpDelete]
        public string DeleteUser(int id)
        {
            id = Numerics.GetInt(id);
            if (id > 0)
            {
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                var _Users = _UsersRepo.Repository.Get(p => p.UserID == id);

                if (_Users != null)
                {
                    _UsersRepo.Repository.Delete(_Users.UserID);
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            else
            {
                return "false";
            }
        }



        [AllowAnonymous]
        [Route("AdminAPI/GetUserByID")]
        [HttpGet]
        public HttpResponseMessage GetUserByID(Int32 ID)
        {
            try
            {
                Models.Users _Users = null;
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                _Users = _UsersRepo.Repository.Get(p => p.UserID == ID);

                if (_Users != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, _Users);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Not Found");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }


        [AllowAnonymous]
        [Route("AdminAPI/GetTicketingUserByID")]
        [HttpGet]
        public HttpResponseMessage GetTicketingUserByID(Int32 ID)
        {
            try
            {
                Models.TicketingUsers _Users = null;
                GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

                _Users = _UsersRepo.Repository.Get(p => p.UserID == ID);

                if (_Users != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, _Users);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Not Found");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }


        [AllowAnonymous]
        [Route("AdminAPI/ChangeUserStatus")]
        [HttpPost]
        public bool ChangeUserStatus(Int64 ID, Int16 InactivePeriod)
        {
            Models.Users _Users = null;
            GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

            _Users = _UsersRepo.Repository.Get(p => p.UserID == ID);

            if (_Users != null)
            {
                if (_Users.UserID != 1) // if not admin
                {
                    _Users.RecordStatus = _Users.RecordStatus == RecordStatus.Active.ToString() ? RecordStatus.InActive.ToString() : RecordStatus.Active.ToString();

                    #region "Commented Code"
                    //if (_Users.RecordStatus == RecordStatus.InActive.ToString())
                    //{
                    //    if (InactivePeriod != -1)
                    //    {
                    //        entity.InactiveDate = DateTime.Now;
                    //        entity.InactivePeriod = InactivePeriod;
                    //    }
                    //    else
                    //    {
                    //        entity.InactivePeriod = null;
                    //        entity.InactiveDate = null;
                    //    }
                    //}
                    //else
                    //{
                    //    entity.InactivePeriod = null;
                    //    entity.InactiveDate = null;
                    //}
                    #endregion

                    _UsersRepo.Repository.Update(_Users);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        [AllowAnonymous]
        [Route("AdminAPI/ChangeTicketingEventStatus")]
        [HttpPost]
        public bool ChangeTicketingEventStatus(Int64 ID, Int16 InactivePeriod)
        {
            Models.TicketingEventsNew _Events = null;
            GenericRepository<TicketingEventsNew> _EventsRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            _Events = _EventsRepo.Repository.Get(p => p.EventID == ID);

            if (_Events != null)
            {
                if (_Events.EventID > 0) // if not admin
                {
                    _Events.IsApproved = _Events.IsApproved == true ? false : true;

                    //_Events.RecordStatus = _Events.RecordStatus == RecordStatus.Active.ToString() ? RecordStatus.InActive.ToString() : RecordStatus.Active.ToString();

                    _EventsRepo.Repository.Update(_Events);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        [AllowAnonymous]
        [Route("AdminAPI/deletephoto/{id}")]
        [HttpDelete]
        public bool DeletePhoto(int id)
        {
            id = Numerics.GetInt(id);
            if (id > 0)
            {
                Models.Users _Users = null;
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                _Users = _UsersRepo.Repository.Get(p => p.UserID == id);

                if (_Users != null)
                {
                    _Users.ThumbnailURL = WebConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                    _Users.ImageURL = WebConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                    _UsersRepo.Repository.Update(_Users);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("AdminAPI/UpdateUser")]
        public AdminResponse UpdateUser(dynamic data)
        {
            AdminResponse _AdminResponse = new AdminResponse();
            try
            {
                //string _Email = "";
                //string _Password = "";

                Models.Users _Users = null;
                //Models.Users _Users2 = null;
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                int userid = Numerics.GetInt(data.UserId);
                _Users = _UsersRepo.Repository.Get(p => p.UserID == userid);

                if (_Users != null)
                {
                    if (data.UserName != null)
                    {
                        string _name = data.UserName.ToString();
                        //_Users2 = _UsersRepo.Repository.Get(e => e.UserName == _name && e.UserID != userid);
                        //if (_Users2 != null)
                        //{
                        //    _AdminResponse.Status = false;
                        //    _AdminResponse.RetMessage = "This user Name already exists";
                        //    return _AdminResponse;
                        //}
                    }

                    _Users.UserName = (data.UserName != null) ? data.UserName.ToString() != "" ? data.UserName : _Users.UserName : _Users.UserName;
                    _Users.Password = (data.Password != null) ? data.Password.ToString() != "" ? AesCryptography.Encrypt(data.Password.ToString()) : _Users.Password : _Users.Password;

                    _UsersRepo.Repository.Update(_Users);

                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "User updated successfully.";
                    return _AdminResponse;
                }
                else
                {
                    _AdminResponse.Status = false;
                    _AdminResponse.RetMessage = "Invalid user";
                    return _AdminResponse;
                }
            }
            catch (Exception ex)
            {
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;
            }
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("AdminAPI/UpdateTicketingUser")]
        public AdminResponse UpdateTicketingUser(dynamic data)
        {
            AdminResponse _AdminResponse = new AdminResponse();
            try
            {
                Models.TicketingUsers _Users = null;
                GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

                int userid = Numerics.GetInt(data.UserId);
                _Users = _UsersRepo.Repository.Get(p => p.UserID == userid);

                if (_Users != null)
                {
                    if (data.UserName != null)
                    {
                        string _name = data.UserName.ToString();

                    }

                    _Users.UserName = (data.UserName != null) ? data.UserName.ToString() != "" ? data.UserName : _Users.UserName : _Users.UserName;
                    _Users.Password = (data.Password != null) ? data.Password.ToString() != "" ? AesCryptography.Encrypt(data.Password.ToString()) : _Users.Password : _Users.Password;

                    _UsersRepo.Repository.Update(_Users);

                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Ticketing User updated successfully.";
                    return _AdminResponse;
                }
                else
                {
                    _AdminResponse.Status = false;
                    _AdminResponse.RetMessage = "Invalid user";
                    return _AdminResponse;
                }
            }
            catch (Exception ex)
            {
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;
            }
        }


        [AllowAnonymous]
        [Route("AdminAPI/downloadCSVFile")]
        [HttpPost]
        public string downloadCSVFile(dynamic data)
        {
            string fileName = "";
            string ddlTable = "";
            if (data.ddlTableName != null)
            {
                ddlTable = data.ddlTableName;
            }
            if (ddlTable == "users")
            {
                #region Users export csv
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                var list = _UsersRepo.Repository.GetAll().ToList();

                string sitePath = ConfigurationManager.AppSettings["SiteImgPath"] + "\\CSVFiles\\";
                if (!Directory.Exists(sitePath))
                {
                    Directory.CreateDirectory(sitePath);
                }
                fileName = "Users_" + Guid.NewGuid().ToString().Substring(0, 5) + ".csv";
                string csvFilePath = sitePath + fileName;

                using (CsvFileWriter writer = new CsvFileWriter(csvFilePath))
                {
                    ReadWriteCSVFile row = new ReadWriteCSVFile();
                    row.Add(String.Format(" {0}", "UserID"));
                    row.Add(String.Format(" {0}", "UserName"));
                    row.Add(String.Format(" {0}", "Email"));
                    row.Add(String.Format(" {0}", "Password"));
                    row.Add(String.Format(" {0}", "FacebookID"));
                    row.Add(String.Format(" {0}", "ThumbnailURL"));
                    row.Add(String.Format(" {0}", "ImageURL"));
                    row.Add(String.Format(" {0}", "DeviceType"));
                    row.Add(String.Format(" {0}", "DeviceToken"));
                    row.Add(String.Format(" {0}", "DeviceLat"));
                    row.Add(String.Format(" {0}", "DeviceLong"));
                    row.Add(String.Format(" {0}", "RecordStatus"));
                    row.Add(String.Format(" {0}", "ModifiedDate"));
                    row.Add(String.Format(" {0}", "CreatedDate"));
                    row.Add(String.Format(" {0}", "SynFacebookID"));
                    row.Add(String.Format(" {0}", "UserLanguage"));

                    writer.WriteRow(row);
                    foreach (var rows in list)
                    {
                        row = new ReadWriteCSVFile();
                        row.Add(String.Format(" {0}", rows.UserID));
                        row.Add(String.Format(" {0}", rows.UserName));
                        row.Add(String.Format(" {0}", rows.Email));
                        row.Add(String.Format(" {0}", rows.Password));
                        row.Add(String.Format(" {0}", rows.FacebookID));
                        row.Add(String.Format(" {0}", rows.ThumbnailURL));
                        row.Add(String.Format(" {0}", rows.ImageURL));
                        row.Add(String.Format(" {0}", rows.DeviceType));
                        row.Add(String.Format(" {0}", rows.DeviceToken));
                        row.Add(String.Format(" {0}", rows.DeviceLat));
                        row.Add(String.Format(" {0}", rows.DeviceLong));
                        row.Add(String.Format(" {0}", rows.RecordStatus));
                        row.Add(String.Format(" {0}", rows.ModifiedDate));
                        row.Add(String.Format(" {0}", rows.CreatedDate));
                        row.Add(String.Format(" {0}", rows.SynFacebookID));
                        row.Add(String.Format(" {0}", rows.UserLanguage));

                        writer.WriteRow(row);
                    }
                }
                #endregion
            }

            if (ddlTable == "artists")
            {
                #region artists export csv
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                var list = _ArtistsRepo.Repository.GetAll().ToList();

                string sitePath = ConfigurationManager.AppSettings["SiteImgPath"] + "\\CSVFiles\\";
                if (!Directory.Exists(sitePath))
                {
                    Directory.CreateDirectory(sitePath);
                }
                fileName = "Artists" + Guid.NewGuid().ToString().Substring(0, 5) + ".csv";
                string csvFilePath = sitePath + fileName;

                using (CsvFileWriter writer = new CsvFileWriter(csvFilePath))
                {
                    ReadWriteCSVFile row = new ReadWriteCSVFile();
                    row.Add(String.Format(" {0}", "ArtistID"));
                    row.Add(String.Format(" {0}", "ArtistName"));
                    row.Add(String.Format(" {0}", "About"));
                    row.Add(String.Format(" {0}", "ImageURL"));
                    row.Add(String.Format(" {0}", "OnTour"));
                    row.Add(String.Format(" {0}", "Gender"));
                    row.Add(String.Format(" {0}", "Seatgeek_ID"));
                    row.Add(String.Format(" {0}", "Musicgraph_ID"));
                    row.Add(String.Format(" {0}", "Eventful_ID"));
                    row.Add(String.Format(" {0}", "Instagram_ID"));
                    row.Add(String.Format(" {0}", "Artist_Ref_ID"));
                    row.Add(String.Format(" {0}", "Musicbrainz_ID"));
                    row.Add(String.Format(" {0}", "Spotify_ID"));
                    row.Add(String.Format(" {0}", "Youtube_ID"));
                    row.Add(String.Format(" {0}", "Main_Genre"));
                    row.Add(String.Format(" {0}", "Decade"));
                    row.Add(String.Format(" {0}", "Alternate_Names"));
                    row.Add(String.Format(" {0}", "Spotify_Url"));
                    row.Add(String.Format(" {0}", "Lastfm_Url"));
                    row.Add(String.Format(" {0}", "Instagram_Url"));
                    row.Add(String.Format(" {0}", "Instagram_Tag"));
                    row.Add(String.Format(" {0}", "CreatedDate"));
                    row.Add(String.Format(" {0}", "ModifiedDate"));
                    row.Add(String.Format(" {0}", "RecordStatus"));
                    row.Add(String.Format(" {0}", "Spotify_URL_Name"));
                    row.Add(String.Format(" {0}", "BannerImage_URL"));
                    row.Add(String.Format(" {0}", "ThumbnailURL"));
                    row.Add(String.Format(" {0}", "AboutES"));
                    row.Add(String.Format(" {0}", "Isdefault"));

                    writer.WriteRow(row);
                    foreach (var rows in list)
                    {
                        row = new ReadWriteCSVFile();
                        row.Add(String.Format(" {0}", rows.ArtistID));
                        row.Add(String.Format(" {0}", rows.ArtistName));
                        row.Add(String.Format(" {0}", rows.About));
                        row.Add(String.Format(" {0}", rows.ImageURL));
                        row.Add(String.Format(" {0}", rows.OnTour));
                        row.Add(String.Format(" {0}", rows.Gender));
                        row.Add(String.Format(" {0}", rows.Seatgeek_ID));
                        row.Add(String.Format(" {0}", rows.Musicgraph_ID));
                        row.Add(String.Format(" {0}", rows.Eventful_ID));
                        row.Add(String.Format(" {0}", rows.Instagram_ID));
                        row.Add(String.Format(" {0}", rows.Artist_Ref_ID));
                        row.Add(String.Format(" {0}", rows.Musicbrainz_ID));
                        row.Add(String.Format(" {0}", rows.Spotify_ID));
                        row.Add(String.Format(" {0}", rows.Youtube_ID));
                        row.Add(String.Format(" {0}", rows.Main_Genre));
                        row.Add(String.Format(" {0}", rows.Decade));
                        row.Add(String.Format(" {0}", rows.Alternate_Names));
                        row.Add(String.Format(" {0}", rows.Spotify_Url));
                        row.Add(String.Format(" {0}", rows.Lastfm_Url));
                        row.Add(String.Format(" {0}", rows.Instagram_Url));
                        row.Add(String.Format(" {0}", rows.Instagram_Tag));
                        row.Add(String.Format(" {0}", rows.CreatedDate));
                        row.Add(String.Format(" {0}", rows.ModifiedDate));
                        row.Add(String.Format(" {0}", rows.RecordStatus));
                        row.Add(String.Format(" {0}", rows.Spotify_URL_Name));
                        row.Add(String.Format(" {0}", rows.BannerImage_URL));
                        row.Add(String.Format(" {0}", rows.ThumbnailURL));
                        row.Add(String.Format(" {0}", rows.AboutES));
                        row.Add(String.Format(" {0}", rows.Isdefault));

                        writer.WriteRow(row);
                    }
                }
                #endregion
            }
            if (ddlTable == "venue")
            {
                #region venue exoport csv
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                var list = _VenueRepo.Repository.GetAll().ToList();

                string sitePath = ConfigurationManager.AppSettings["SiteImgPath"] + "\\CSVFiles\\";
                if (!Directory.Exists(sitePath))
                {
                    Directory.CreateDirectory(sitePath);
                }
                fileName = "Venue_" + Guid.NewGuid().ToString().Substring(0, 5) + ".csv";
                string csvFilePath = sitePath + fileName;

                using (CsvFileWriter writer = new CsvFileWriter(csvFilePath))
                {
                    ReadWriteCSVFile row = new ReadWriteCSVFile();
                    row.Add(String.Format(" {0}", "VenueID"));
                    row.Add(String.Format(" {0}", "SeatGeek_VenuID"));
                    row.Add(String.Format(" {0}", "VenueName"));
                    row.Add(String.Format(" {0}", "ImageURL"));
                    row.Add(String.Format(" {0}", "Extended_Address"));
                    row.Add(String.Format(" {0}", "Display_Location"));
                    row.Add(String.Format(" {0}", "Slug"));
                    row.Add(String.Format(" {0}", "Postal_Code"));
                    row.Add(String.Format(" {0}", "Address"));
                    row.Add(String.Format(" {0}", "Timezone"));
                    row.Add(String.Format(" {0}", "VenueCity"));
                    row.Add(String.Format(" {0}", "VenueState"));
                    row.Add(String.Format(" {0}", "VenueCountry"));
                    row.Add(String.Format(" {0}", "VenueLat"));
                    row.Add(String.Format(" {0}", "VenueLong"));
                    row.Add(String.Format(" {0}", "CreatedDate"));
                    row.Add(String.Format(" {0}", "ModifiedDate"));
                    row.Add(String.Format(" {0}", "RecordStatus"));
                    row.Add(String.Format(" {0}", "Eventful_VenueID"));

                    writer.WriteRow(row);
                    foreach (var rows in list)
                    {
                        row = new ReadWriteCSVFile();
                        row.Add(String.Format(" {0}", rows.VenueID));
                        row.Add(String.Format(" {0}", rows.SeatGeek_VenuID));
                        row.Add(String.Format(" {0}", rows.VenueName));
                        row.Add(String.Format(" {0}", rows.ImageURL));
                        row.Add(String.Format(" {0}", rows.Extended_Address));
                        row.Add(String.Format(" {0}", rows.Display_Location));
                        row.Add(String.Format(" {0}", rows.Slug));
                        row.Add(String.Format(" {0}", rows.Postal_Code));
                        row.Add(String.Format(" {0}", rows.Address));
                        row.Add(String.Format(" {0}", rows.Timezone));
                        row.Add(String.Format(" {0}", rows.VenueCity));
                        row.Add(String.Format(" {0}", rows.VenueState));
                        row.Add(String.Format(" {0}", rows.VenueCountry));
                        row.Add(String.Format(" {0}", rows.VenueLat));
                        row.Add(String.Format(" {0}", rows.VenueLong));
                        row.Add(String.Format(" {0}", rows.CreatedDate));
                        row.Add(String.Format(" {0}", rows.ModifiedDate));
                        row.Add(String.Format(" {0}", rows.RecordStatus));
                        row.Add(String.Format(" {0}", rows.Eventful_VenueID));

                        writer.WriteRow(row);
                    }
                }
                #endregion
            }
            return ConfigurationManager.AppSettings["WebPath"].ToString() + "/Content/Upload/CSVFiles/" + fileName;
        }


        [AllowAnonymous]
        [Route("AdminAPI/downloadEventCSVFile")]
        [HttpGet]
        public string downloadEventCSVFile(string Eventid)
        {
            string fileName = "";


            Models.TourDate _TourDate = null;

            //_unitOfWork.StartTransaction();
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);


            Int64 _EventID = Convert.ToInt64(Eventid);
            _TourDate = _TourDateRepo.Repository.Get(p => p.TicketingEventID == _EventID);

            ExportCsv _ExportCsv = new ExportCsv();
            Models.API.View.ViewEventDetail _ViewEventDetail = new Models.API.View.ViewEventDetail();


            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
            GenericRepository<UserTourDate> UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);
            GenericRepository<UserFriends> _UserFriendsRepo = new GenericRepository<UserFriends>(_unitOfWork);
            GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
            GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
            GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);
            GenericRepository<TicketingUsers> _TicketingUserRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

            GenericRepository<TicketingEventTickets> _TicketingRepo = new GenericRepository<TicketingEventTickets>(_unitOfWork);
            GenericRepository<TicketingEventTicketsSummary> _TicketingEventSummary = new GenericRepository<TicketingEventTicketsSummary>(_unitOfWork);
            GenericRepository<TicketingEventsNew> _TicketingEventNewRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            Models.EventReponse tmpResponse = new EventReponse();
            Musika.Models.DashboardSummary DashboardSummary = new DashboardSummary();
            //GenericRepository<TicketingEventsNew> _TicketingEvents = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            int TotalTickets = 0;
            int TotalSales = 0;
            int Attendees = 0;

            var _list = (from A in _TicketingEventNewRepo.Repository.GetAll()
                         select new
                         {
                             A.EventID,
                             A.EventTitle,
                             A.StartDate,
                             A.StartTime,
                             A.EndDate,
                             A.EndTime
                         }
                         ).Where(p => p.EventID == Convert.ToInt32(Eventid)).ToList();

            if (_list.Count > 0)
            {
                DashboardSummary.EventId = Convert.ToInt32(_list[0].EventID);
                DashboardSummary.EventName = Convert.ToString(_list[0].EventTitle);
                DashboardSummary.StartDate = Convert.ToDateTime(_list[0].StartDate);
                DashboardSummary.StartTime = Convert.ToString(_list[0].StartTime);
                DashboardSummary.EndDate = Convert.ToDateTime(_list[0].EndDate);
                DashboardSummary.EndTime = Convert.ToString(_list[0].EndTime);

                List<EventAttendeeCounts> lst = new List<EventAttendeeCounts>();

                DataSet ds = new DataSet();
                ds = new SpRepository().SpGetTicketSummaryStats(Convert.ToInt32(Eventid));

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];
                    EventAttendeeCounts counts = new EventAttendeeCounts();
                    counts.Category = dr["Category"].ToString();
                    counts.TotalTickets = Convert.ToInt32(dr["Total"].ToString());
                    counts.Sold = Convert.ToInt32(dr["Sold"].ToString());
                    counts.Attendees = Convert.ToInt32(dr["Attended"].ToString());
                    counts.UnSold = Convert.ToInt32(dr["Unsold"].ToString());
                    TotalTickets += counts.TotalTickets;
                    TotalSales += counts.Sold;
                    Attendees += counts.Attendees;
                    lst.Add(counts);
                }
                DashboardSummary.TotalSales = TotalSales;
                DashboardSummary.TotalTickets = TotalTickets;
                DashboardSummary.Attendees = Attendees;

                DashboardSummary.lstCounts = lst;

                List<EventAttendeeCounts> lst2 = new List<EventAttendeeCounts>();
                ds = new SpRepository().SpGetTicketSummaryStatsByGender(Convert.ToInt32(Eventid));

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];
                    EventAttendeeCounts counts = new EventAttendeeCounts();
                    counts.Category = dr["Gender"].ToString();
                    counts.TotalTickets = Convert.ToInt32(dr["Total"].ToString());
                    counts.Sold = Convert.ToInt32(dr["Sold"].ToString());
                    counts.Attendees = Convert.ToInt32(dr["Attended"].ToString());
                    counts.UnSold = Convert.ToInt32(dr["Unsold"].ToString());
                    TotalTickets += counts.TotalTickets;
                    TotalSales += counts.Sold;
                    Attendees += counts.Attendees;
                    lst2.Add(counts);
                }
                DashboardSummary.lstGenderAttendeeList = lst2;
            }


            Models.UserTourDate _UserTourDate = null;
            Models.Artists _Artist = null;

            _Artist = _ArtistsRepo.Repository.Get(p => p.ArtistID == _TourDate.ArtistID);

            _ViewEventDetail.TourID = _TourDate.TourDateID;
            _ViewEventDetail.ArtistID = _Artist.ArtistID;
            _ExportCsv.ArtistName = _Artist.ArtistName;


            _ExportCsv.ImageURL = _Artist.ImageURL ?? "";
            _ExportCsv.BannerImage_URL = _Artist.ImageURL ?? "";

            _ViewEventDetail.OnTour = _Artist.OnTour;
            _ExportCsv.Gender = _Artist.Gender ?? "";
            _ExportCsv.Main_Genre = _Artist.Main_Genre ?? "";
            _ExportCsv.Decade = _Artist.Decade ?? "";
            _ExportCsv.Alternate_Names = _Artist.Alternate_Names ?? "";
            _ExportCsv.CreatedDate = _Artist.CreatedDate;
            _ExportCsv.ModifiedDate = _Artist.ModifiedDate;
            _ViewEventDetail.RecordStatus = _Artist.RecordStatus;
            _ViewEventDetail.Instagram_Tag = _Artist.Instagram_Tag != null && _Artist.Instagram_Tag != "" ? _Artist.Instagram_Tag : _Artist.ArtistName;
            try
            {
                _ExportCsv.Date_Local = _TourDate.Datetime_Local.ToString().Substring(0, 10).Trim();
                _ExportCsv.Time_Local = _TourDate.Datetime_Local.ToString().Substring(10).Trim();
            }
            catch (Exception) { }


            if (!String.IsNullOrEmpty(_TourDate.TicketURL))
            {
                _ExportCsv.TicketURL = _TourDate.TicketURL;
            }
            else
            {
                _ExportCsv.TicketURL = "23.111.138.246/TicketEventCheckout.aspx";
            }


            _ExportCsv.Event_Name = _TourDate.EventName;


            Models.UserGoing _UserGoing = null;

            Models.Venue _Venue = null;
            _Venue = _VenueRepo.Repository.Get(p => p.VenueID == _TourDate.VenueID);


            //who else is going
            var _lst = (from A1 in _UserFriendsRepo.Repository.GetAll(p => p.Matched_UserID != null)
                        join A in _UserGoingRepo.Repository.GetAll(p => p.TourDateID == _TourDate.TourDateID && p.RecordStatus == EUserGoing.Going.ToString()) on A1.Matched_UserID equals A.UserID
                        join B in _UsersRepo.Repository.GetAll() on A.UserID equals B.UserID
                        select new ExportCsv.ViewEventUsers
                        {
                            //ThumbnailURL = B.ThumbnailURL ?? "",
                            UserID = A.UserID.Value,
                            GoingUserName = B.UserName.ToString(),
                            Email = B.Email ?? "",
                            CreatedDate = Convert.ToDateTime(A.CreatedDate).ToString("d")
                        }).DistinctBy(y => y.UserID).OrderByDescending(p => p.CreatedDate).ToList();

            //who else is going
            _ExportCsv.UsersGoing = _lst.ToList();
            // _ViewEventDetail.UsersGoing = lstViewEventUsers;

            _ExportCsv.NoOfUserGoing = _lst.Count();

            int? eventID;
            List<TourDate> lstSummary = new List<TourDate>();
            //eventID = _TourDateRepo.Repository.GetAll().Where(t => t.TourDateID == _TourDate.TourDateID).FirstOrDefault().TicketingEventID;
            eventID = Convert.ToInt32(Eventid);
            if (!String.IsNullOrEmpty(Eventid.ToString()))
            {
                lstSummary = _TourDateRepo.Repository.GetAll().Where(p => p.TicketingEventID == eventID).ToList();
            }

            List<ExportCsv.Ticket> lstTicket = new List<ExportCsv.Ticket>();
            if (lstSummary.Count > 0)
            {
                for (int i = 0; i < lstSummary.Count; i++)
                {
                    ExportCsv.Ticket tickets = new ExportCsv.Ticket();
                    //tickets.EventId = lstSummary[i].TicketingEventID ?? default(int);

                    TicketingEventTicketsSummary summary = new TicketingEventTicketsSummary();
                    summary = _TicketingEventSummary.Repository.GetAll().Where(t => t.EventID == eventID).ToList().FirstOrDefault();

                    if (summary != null)
                    {
                        tickets.CountryId = summary.CountryId;
                        tickets.Currency = summary.Currency;
                        tickets.RefundPolicy = summary.RefundPolicy;
                        //tickets.EventId = summary.EventID;
                    }
                    else
                    {
                        tickets.CountryId = 1;
                        tickets.Currency = "$";
                        tickets.RefundPolicy = "Yes";
                        //tickets.EventId = tickets.EventId = eventID ?? default(int);
                    }
                    List<TicketingEventTicketsSummary> lstTicketData = new List<TicketingEventTicketsSummary>();
                    List<ExportCsv.TicketData> lstData = new List<ExportCsv.TicketData>();
                    int eventid = eventID ?? default(int);
                    DataSet ds1 = new DataSet();
                    ds1 = new SpRepository().GetTicketBalanceSummary(eventid);
                    if (ds1.Tables[0].Rows.Count > 0)
                    {
                        for (int j = 0; j < ds1.Tables[0].Rows.Count; j++)
                        {
                            ExportCsv.TicketData data = new ExportCsv.TicketData();
                            DataRow dr = ds1.Tables[0].Rows[j];

                            data.EventId = Convert.ToInt32(dr["EventId"].ToString());
                            data.TicketType = Convert.ToString(dr["TicketType"].ToString());
                            data.TicketCategory = Convert.ToString(dr["TicketCategory"].ToString());
                            data.Price = Convert.ToDecimal(dr["Price"].ToString());
                            data.Quantity = Convert.ToInt32(dr["Total"].ToString());
                            data.BalanceTickets = Convert.ToInt32(dr["Balance"].ToString());
                            data.PackageStartDate = Convert.ToString(dr["PackageStartDate"].ToString());
                            data.PackageEndDate = Convert.ToString(dr["PackageEndDate"].ToString());
                            lstData.Add(data);
                        }
                    }

                    tickets.lstTicketData = lstData;
                    lstTicket.Add(tickets);
                }
            }

            TicketingEventsNew ticketingNew = _TicketingEventNewRepo.Repository.GetAll().Where(p => p.EventID == eventID).FirstOrDefault();


            try
            {
                _ExportCsv.Date_Local = Convert.ToDateTime(ticketingNew.StartDate).ToString("d");
                _ExportCsv.Time_Local = Convert.ToDateTime(ticketingNew.StartTime).ToString("t");
            }
            catch (Exception)
            { }



            if (ticketingNew != null)
            {
                _ExportCsv.Event_Description = Regex.Replace(ticketingNew.EventDescription, "<.*?>", String.Empty).Replace("\n", "").Replace("\t", "");
                _ExportCsv.Event_Name = Regex.Replace(_TourDate.EventName, "<.*?>", String.Empty).Replace("\n", "").Replace("\t", "");
                ticketingNew.OrganizerName = _TicketingUserRepo.Repository.GetById((int)ticketingNew.CreatedBy).UserName;
                _ExportCsv.Organizer_Name = ticketingNew.OrganizerName;
                _ExportCsv.Organizer_Description = Regex.Replace(ticketingNew.OrganizerDescription, "<.*?>", String.Empty).Replace("\n", "").Replace("\t", "");
            }
            else
            {
                _ExportCsv.Organizer_Name = "Musika";
                _ExportCsv.Organizer_Description = "Musika Event Organizer";

            }
            string csv = string.Empty;
            _ExportCsv.lstTicket = lstTicket;

            //===========================
            DataSet dsTicketBuyUser = new DataSet();
            List<ExportCsv.TicketBuyData> lstBuyData = new List<ExportCsv.TicketBuyData>();
            dsTicketBuyUser = new SpRepository().GetTicketBuyUser(Convert.ToInt32(Eventid));
            if (dsTicketBuyUser.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < dsTicketBuyUser.Tables[0].Rows.Count; j++)
                {
                    ExportCsv.TicketBuyData data = new ExportCsv.TicketBuyData();
                    DataRow dr = dsTicketBuyUser.Tables[0].Rows[j];

                    //data.EventId = Convert.ToInt32(dr["EventId"].ToString());
                    data.UserName = Convert.ToString(dr["UserName"].ToString());
                    data.Email = Convert.ToString(dr["Email"].ToString());
                    data.Gender = Convert.ToString(dr["Gender"].ToString());
                    //data.Mode = Convert.ToString(dr["Mode"].ToString());
                    data.TicketType = Convert.ToString(dr["TicketType"].ToString());
                    data.Country = Convert.ToString(dr["Country"].ToString());
                    data.TicketCategory = Convert.ToString(dr["TicketCategory"].ToString());
                    lstBuyData.Add(data);

                }
            }
            #region Users export csv
            

            string sitePath = ConfigurationManager.AppSettings["SiteImgPath"] + "\\CSVFiles\\";
            if (!Directory.Exists(sitePath))
            {
                Directory.CreateDirectory(sitePath);
            }
            fileName = "Events_" + _ExportCsv.Event_Name + ".csv";
            string csvFilePath = sitePath + fileName;

            using (CsvFileWriter writer = new CsvFileWriter(csvFilePath))
            {
                ReadWriteCSVFile row = new ReadWriteCSVFile();
                writer.WriteLine("Event Detail");
                writer.WriteLine();
              
                row.Add(String.Format(" {0}", "Event_Name"));
                row.Add(String.Format(" {0}", "Event_Description"));
                row.Add(String.Format(" {0}", "ArtistName"));

                row.Add(String.Format(" {0}", "BannerImage_URL"));
                row.Add(String.Format(" {0}", "CreatedDate"));
                row.Add(String.Format(" {0}", "EventDate"));
                row.Add(String.Format(" {0}", "Decade"));
                row.Add(String.Format(" {0}", "ImageURL"));
                row.Add(String.Format(" {0}", "Main_Genre"));
                row.Add(String.Format(" {0}", "ModifiedDate"));

                row.Add(String.Format(" {0}", "NoOfUserGoing"));
                row.Add(String.Format(" {0}", "Organizer_Description"));
                row.Add(String.Format(" {0}", "Organizer_Name"));
                row.Add(String.Format(" {0}", "TicketURL"));
                row.Add(String.Format(" {0}", "About"));
                row.Add(String.Format(" {0}", "VenueCity"));
                row.Add(String.Format(" {0}", "VenueCountry"));
                row.Add(String.Format(" {0}", "VenueName"));
                row.Add(String.Format(" {0}", "VenueState"));

                row.Add(String.Format(" {0}", "Alternate_Names"));
                row.Add(String.Format(" {0}", "Time_Local"));

                writer.WriteRow(row);

                row = new ReadWriteCSVFile();
                row.Add(String.Format(" {0}", _ExportCsv.Event_Name));
                row.Add(String.Format(" {0}", _ExportCsv.Event_Description));
                row.Add(String.Format(" {0}", _ExportCsv.ArtistName));

                row.Add(String.Format(" {0}", _ExportCsv.BannerImage_URL));
                row.Add(String.Format(" {0}", _ExportCsv.CreatedDate));
                row.Add(String.Format(" {0}", _ExportCsv.Date_Local));
                row.Add(String.Format(" {0}", _ExportCsv.Decade));


                row.Add(String.Format(" {0}", _ExportCsv.ImageURL));
                row.Add(String.Format(" {0}", _ExportCsv.Main_Genre));
                row.Add(String.Format(" {0}", _ExportCsv.ModifiedDate));
                row.Add(String.Format(" {0}", _ExportCsv.NoOfUserGoing));
                row.Add(String.Format(" {0}", _ExportCsv.Organizer_Description));
                row.Add(String.Format(" {0}", _ExportCsv.Organizer_Name));
                row.Add(String.Format(" {0}", _ExportCsv.TicketURL));
                row.Add(String.Format(" {0}", _ExportCsv.About));
                row.Add(String.Format(" {0}", _ExportCsv.VenueCity));
                row.Add(String.Format(" {0}", _ExportCsv.VenueCountry));
                row.Add(String.Format(" {0}", _ExportCsv.VenueName));
                row.Add(String.Format(" {0}", _ExportCsv.VenueState));
                row.Add(String.Format(" {0}", _ExportCsv.Alternate_Names));
                row.Add(String.Format(" {0}", _ExportCsv.Time_Local));


                writer.WriteRow(row);
                if (_ExportCsv.lstTicket[0].lstTicketData.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine("Event Tickets Detail");
                    writer.WriteLine();
                   
                    row = new ReadWriteCSVFile();
                    row.Add(String.Format(" {0}", "EventId"));
                    row.Add(String.Format(" {0}", "TicketCategory"));
                    row.Add(String.Format(" {0}", "TicketType"));
                    row.Add(String.Format(" {0}", "Price"));
                    row.Add(String.Format(" {0}", "Quantity"));
                    row.Add(String.Format(" {0}", "BalanceTickets"));
                    row.Add(String.Format(" {0}", "PackageStartDate"));
                    row.Add(String.Format(" {0}", "PackageEndDate"));
                    writer.WriteRow(row);
                    foreach (var rows in _ExportCsv.lstTicket[0].lstTicketData)
                    {
                        row = new ReadWriteCSVFile();
                        row.Add(String.Format(" {0}", rows.EventId));
                        row.Add(String.Format(" {0}", rows.TicketCategory));
                        row.Add(String.Format(" {0}", rows.TicketType));
                        row.Add(String.Format(" {0}", rows.Price));
                        row.Add(String.Format(" {0}", rows.Quantity));
                        row.Add(String.Format(" {0}", rows.BalanceTickets));
                        row.Add(String.Format(" {0}", rows.PackageStartDate));
                        row.Add(String.Format(" {0}", rows.PackageEndDate));
                        writer.WriteRow(row);
                    }
                }

                if (_ExportCsv.UsersGoing.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine("Going Users Detail");
                    writer.WriteLine();
                   
                    row = new ReadWriteCSVFile();
                    row.Add(String.Format(" {0}", "UserID"));
                    row.Add(String.Format(" {0}", "GoingUserName"));
                    row.Add(String.Format(" {0}", "Email"));
                    row.Add(String.Format(" {0}", "CreatedDate"));
                    writer.WriteRow(row);
                    foreach (var rows in _ExportCsv.UsersGoing)
                    {
                        row = new ReadWriteCSVFile();
                        row.Add(String.Format(" {0}", rows.UserID));
                        row.Add(String.Format(" {0}", rows.GoingUserName));
                        row.Add(String.Format(" {0}", rows.Email));
                        row.Add(String.Format(" {0}", rows.CreatedDate));
                        writer.WriteRow(row);
                    }
                }


                if (DashboardSummary.lstCounts.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine("Sold/Unsold Ticket Detail");
                    writer.WriteLine();
                   
                    row = new ReadWriteCSVFile();
                    row.Add(String.Format(" {0}", "Category"));
                    row.Add(String.Format(" {0}", "TotalTickets"));
                    row.Add(String.Format(" {0}", "Sold"));
                    row.Add(String.Format(" {0}", "Attendees"));
                    row.Add(String.Format(" {0}", "UnSold"));

                    writer.WriteRow(row);
                    foreach (var rows in DashboardSummary.lstCounts)
                    {
                        row = new ReadWriteCSVFile();
                        row.Add(String.Format(" {0}", rows.Category));
                        row.Add(String.Format(" {0}", rows.TotalTickets));
                        row.Add(String.Format(" {0}", rows.Sold));
                        row.Add(String.Format(" {0}", rows.Attendees));
                        row.Add(String.Format(" {0}", rows.UnSold));

                        writer.WriteRow(row);
                    }
                }

                if (lstBuyData.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine("User Ticket Buy Detail");
                    writer.WriteLine();
                  
                    row = new ReadWriteCSVFile();
                    row.Add(String.Format(" {0}", "UserName"));
                    row.Add(String.Format(" {0}", "Gender"));
                    row.Add(String.Format(" {0}", "Country"));
                    row.Add(String.Format(" {0}", "Email"));
                    row.Add(String.Format(" {0}", "TicketCategory"));
                    row.Add(String.Format(" {0}", "TicketType"));

                    writer.WriteRow(row);
                    foreach (var rows in lstBuyData)
                    {
                        row = new ReadWriteCSVFile();
                        row.Add(String.Format(" {0}", rows.UserName));
                        row.Add(String.Format(" {0}", rows.Gender));
                        row.Add(String.Format(" {0}", rows.Country));
                        row.Add(String.Format(" {0}", rows.Email));
                        row.Add(String.Format(" {0}", rows.TicketCategory));
                        row.Add(String.Format(" {0}", rows.TicketType));
                        writer.WriteRow(row);
                    }
                }
            }
            #endregion
            // }
            return ConfigurationManager.AppSettings["WebPath"].ToString() + "/Content/Upload/CSVFiles/" + fileName;
        }


        #endregion

        #region "User Artists"

        [AllowAnonymous]
        [Route("AdminAPI/GetUserArtists")]
        [HttpGet]
        public HttpResponseMessage GetUserArtists(string sUserName, string sEmail, string sArtistName, string sGenreName, int Pageindex, int Pagesize, string sortColumn, string sortOrder)
        {
            try
            {
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);

                GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);

                List<ViewUserArtistlst> _ViewArtistlst = new List<ViewUserArtistlst>();

                DataSet ds = new DataSet();
                SpRepository _sp = new SpRepository();
                ds = _sp.SpGetUserArtistList();
                var _list = General.DTtoList<ViewUserArtistlst>(ds.Tables[0]);



                //Filters
                if (!string.IsNullOrEmpty(sUserName))
                {
                    _list = _list.Where(p => p.UserName.IndexOf(sUserName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(sEmail))
                {
                    _list = _list.Where(p => p.Email.IndexOf(sEmail.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(sArtistName))
                {
                    _list = _list.Where(p => p.ArtistName.IndexOf(sArtistName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(sGenreName))
                {
                    _list = _list.Where(p => p.Main_Genre.IndexOf(sGenreName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }



                //Sorting
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    _list = _list.OrderBy("" + sortColumn + " " + sortOrder).ToList();
                }


                //Pagination
                var _list2 = _list.ToPagedList(Pageindex - 1, Pagesize);


                Dictionary<string, object> d = new Dictionary<string, object>();


                if (_list.Count() > 0)
                {

                    d.Add("Items", _list2);
                    d.Add("PageCount", _list2.PageCount);
                    d.Add("PageNumber", _list2.PageNumber);
                    d.Add("PageSize", Pagesize);

                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
                else
                {
                    d.Add("Items", new List<ViewUserArtistlst>());
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


        [AllowAnonymous]
        [Route("AdminAPI/UserArtistRemove/{id}/{userid}")]
        [HttpDelete]
        public bool UserArtistRemove(int id, int userid)
        {
            id = Numerics.GetInt(id);
            userid = Numerics.GetInt(userid);

            if (id > 0)
            {
                Models.UserArtists _UserArtists = null;
                GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);

                _UserArtists = _UserArtistsRepo.Repository.Get(p => p.UserID == userid && p.ArtistID == id);

                if (_UserArtists != null)
                {
                    _UserArtistsRepo.Repository.DeletePermanent(_UserArtists.UserArtistsID);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }


        #endregion

        #region "Artists"

        [AllowAnonymous]
        [Route("AdminAPI/GetArtists")]
        [HttpGet]
        public HttpResponseMessage GetArtists(string sArtistName, string sGenreName, int Pageindex, int Pagesize, string sortColumn, string sortOrder)
        {
            try
            {
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);
                GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(_unitOfWork);
                GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);

                List<ViewArtistlst> _ViewArtistlst = new List<ViewArtistlst>();

                DataSet ds = new DataSet();
                SpRepository _sp = new SpRepository();
                ds = _sp.SpGetArtistList();
                var _list = General.DTtoList<ViewArtistlst>(ds.Tables[0]);

                //Filters
                if (!string.IsNullOrEmpty(sArtistName))
                {
                    _list = _list.Where(p => RemoveDiacritics(p.ArtistName).IndexOf(sArtistName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(sGenreName))
                {
                    _list = _list.Where(p => p.Main_Genre.IndexOf(sGenreName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                //Sorting
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    _list = _list.OrderBy("" + sortColumn + " " + sortOrder).ToList();
                }


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

        [AllowAnonymous]
        [Route("AdminAPI/DeleteArtistByID")]
        [HttpGet]
        public AdminResponse DeleteArtistByID(Int32 ID)
        {
            AdminResponse _AdminResponse = new AdminResponse();

            try
            {
                Models.Artists _Artists = null;
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<ArtistGenre> _ArtistsGenreRepo = new GenericRepository<ArtistGenre>(_unitOfWork);
                GenericRepository<ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<ArtistPhotos>(_unitOfWork);
                GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<TourPerformers> _TourPerformersRepo = new GenericRepository<TourPerformers>(_unitOfWork);
                GenericRepository<TourPhoto> _TourPhotoRepo = new GenericRepository<TourPhoto>(_unitOfWork);
                GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
                GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);




                _Artists = _ArtistsRepo.Repository.Get(p => p.ArtistID == ID);

                if (_Artists != null)
                {

                    SpRepository _sp = new SpRepository();
                    DataSet ds = new DataSet();
                    ds = _sp.SpDeleteArtist(ID);

                    if (_sp.ErrorDescription != "")
                    {
                        _AdminResponse.Status = false;
                        _AdminResponse.RetMessage = "Record Not Found.";
                        return _AdminResponse;
                    }

                    
                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Artist  has deleted successfully.";
                    return _AdminResponse;

                }
                else
                {
                    _AdminResponse.Status = false;
                    _AdminResponse.RetMessage = "Record Not Found.";
                    return _AdminResponse;
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;

            }
        }



        [AllowAnonymous]
        [Route("AdminAPI/GetArtistByID")]
        [HttpGet]
        public HttpResponseMessage GetArtistByID(Int32 ID)
        {
            try
            {
                Models.Artists _Artists = null;
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);

                _Artists = _ArtistsRepo.Repository.Get(p => p.ArtistID == ID);

                if (_Artists != null)
                {
                    if (_TourDateRepo.Repository.AsQueryable().Count(x => x.ArtistID == _Artists.ArtistID && !x.IsDeleted && x.RecordStatus != RecordStatus.Deleted.ToString() && x.Tour_Utcdate > DateTime.UtcNow) > 0)
                    {
                        if (!_Artists.OnTour)
                        {
                            _Artists.OnTour = true;
                            _ArtistsRepo.Repository.Update(_Artists);
                        }
                    }
                    else if (_Artists.OnTour)
                    {
                        _Artists.OnTour = false;
                        _ArtistsRepo.Repository.Update(_Artists);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, _Artists);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Not Found");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);

            }
        }


        [AllowAnonymous]
        [Route("AdminAPI/deleteArtistphoto/{id}")]
        [HttpDelete]
        public bool deleteArtistphoto(int id)
        {
            id = Numerics.GetInt(id);
            if (id > 0)
            {
                Models.Artists _Artists = null;
                GenericRepository<Artists> ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);

                _Artists = ArtistsRepo.Repository.Get(p => p.ArtistID == id);

                if (_Artists != null)
                {
                    _Artists.ImageURL = WebConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                    _Artists.BannerImage_URL = _Artists.ThumbnailURL = _Artists.ImageURL;
                    ArtistsRepo.Repository.Update(_Artists);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("AdminAPI/updateArtist")]
        public AdminResponse updateArtist(dynamic data)
        {
            AdminResponse _AdminResponse = new AdminResponse();

            try
            {

                Models.Artists _Artists = null;
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<ArtistGenre> _ArtistsGenreRepo = new GenericRepository<ArtistGenre>(_unitOfWork);
                GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(_unitOfWork);
                GenericRepository<ArtistPhotos> _ArtistPhotoRepo = new GenericRepository<ArtistPhotos>(_unitOfWork);

                int ArtistID = Numerics.GetInt(data.ArtistID);
                _Artists = _ArtistsRepo.Repository.Get(p => p.ArtistID == ArtistID);

                if (_Artists != null)
                {


                    _Artists.ArtistName = (data.ArtistName != null) ? data.ArtistName : _Artists.ArtistName;
                    _Artists.Musicgraph_ID = (data.Musicgraph_ID != null) ? data.Musicgraph_ID : _Artists.Musicgraph_ID;

                    var _check = _ArtistsRepo.Repository.Get(p => p.Musicgraph_ID.Trim().ToLower() == _Artists.Musicgraph_ID.Trim().ToLower() && p.ArtistID != ArtistID);
                    if (_check != null)
                    {
                        _AdminResponse.Status = false;
                        _AdminResponse.RetMessage = "Musickgraph ID already exitst, please check ";
                        return _AdminResponse;
                    }


                    _Artists.About = (data.About != null) ? data.About : _Artists.About;

                    _Artists.AboutES = (data.AboutES != null) ? data.AboutES : _Artists.AboutES;

                    _Artists.Spotify_ID = (data.Spotify_ID != null) ? data.Spotify_ID : _Artists.Spotify_ID;
                    _Artists.Instagram_ID = (data.Instagram_ID != null) ? data.Instagram_ID : _Artists.Instagram_ID;
                    _Artists.OnTour = (data.OnTour != null) ? data.OnTour : _Artists.OnTour;
                    _Artists.Instagram_Tag = (data.Instagram_Tag != null) ? data.Instagram_Tag : _Artists.Instagram_Tag;

                    _Artists.Instagram_Url = (_Artists.Instagram_ID != null) ? "https://instagram.com/" + _Artists.Instagram_ID : _Artists.Instagram_Url;


                    _Artists.ModifiedDate = _Artists.CreatedDate;
                    _ArtistsRepo.Repository.Update(_Artists);

                    /*try
                    {
                        var aPhotoList = _ArtistPhotoRepo.Repository.AsQueryable().Where(x => x.ArtistID == ArtistID).Select(x => x.PhotoID).ToList();
                        _ArtistPhotoRepo.Repository.DeletePermanent(aPhotoList);
                    }
                    catch(Exception)
                    { }*/

                    if (data.Instagram_ID != null)
                    {
                        var _lst = _ArtistPhotoRepo.Repository.GetAll(x => x.ArtistID == _Artists.ArtistID && x.RecordStatus == RecordStatus.Active.ToString()).ToList();

                        foreach (var tourphoto in _lst)
                        {
                            tourphoto.RecordStatus = RecordStatus.InActive.ToString();
                            tourphoto.ModifiedDate = DateTime.Now;
                            _ArtistPhotoRepo.Repository.Update(tourphoto);
                        }
                    }


                    _AdminResponse.ID = _Artists.ArtistID.ToString();
                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Artist have been updated.";

                    // Send Push Notification                    
                    #region "Send Notification - Added by Mukesh (3-Aug-2018)"
                    int fromUserId = 1;
                    //int toUserId = data _Ar.UserID;
                    string message = Musika.Enums.Response.ArtistUpdate;

                    SendNotificationForArtist(1, message, "Artist");
                    _AdminResponse.ResponseType = "Artist";
                    #endregion

                    return _AdminResponse;
                }
                else
                {
                    _Artists = new Artists();

                    _Artists.ArtistName = data.ArtistName.ToString();
                    _Artists.Musicgraph_ID = !String.IsNullOrEmpty(data.Musicgraph_ID.ToString()) ? data.Musicgraph_ID.ToString() : "";

                    var _check = _ArtistsRepo.Repository.Get(p => p.Musicgraph_ID.Trim().ToLower() == _Artists.Musicgraph_ID.Trim().ToLower());
                    if (_check != null)
                    {
                        _AdminResponse.Status = false;
                        _AdminResponse.RetMessage = "Musickgraph ID already exitst, please check ";
                        return _AdminResponse;
                    }

                    _Artists.About = data.About.ToString();

                    _Artists.AboutES = data.AboutES.ToString();

                    _Artists.Spotify_ID = data.Spotify_ID.ToString();
                    _Artists.Instagram_ID = data.Instagram_ID.ToString();
                    _Artists.OnTour = data.OnTour;
                    _Artists.Instagram_Tag = data.Instagram_Tag.ToString();

                    _Artists.Instagram_Url = !String.IsNullOrEmpty(_Artists.Instagram_ID) ? "https://instagram.com/" + _Artists.Instagram_ID : _Artists.Instagram_Url;

                    _Artists.RecordStatus = RecordStatus.Active.ToString();
                    _Artists.CreatedDate = DateTime.Now;

                    string MusicGraphBio = "";
                    if (!String.IsNullOrEmpty(_Artists.Musicgraph_ID))
                        MusicGraphBio = GetMusicGraphBio(_Artists.Musicgraph_ID);
                    if (!String.IsNullOrEmpty(MusicGraphBio))
                        _Artists.About = MusicGraphBio;

                    _Artists.Main_Genre = "Latin";

                    _ArtistsRepo.Repository.Add(_Artists);


                    var latinGenre = _GenreRepo.Repository.Get(x => x.Name == "Latin");
                    if (latinGenre != null)
                    {
                        Models.ArtistGenre ag = new ArtistGenre();
                        ag.Name = latinGenre.Name;
                        ag.GenreID = latinGenre.GenreID;
                        ag.Primary = true;
                        ag.Slug = latinGenre.Name.ToLower();
                        ag.ArtistID = _Artists.ArtistID;
                        ag.CreatedDate = DateTime.Now;
                        ag.ModifiedDate = null;
                        ag.RecordStatus = RecordStatus.SeatGeek.ToString();
                        _ArtistsGenreRepo.Repository.Add(ag);
                    }

                    _AdminResponse.ID = _Artists.ArtistID.ToString();
                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Artist Added successfully.";

                    // Send Push Notification                    
                    #region "Send Notification - Added by Mukesh (3-Aug-2018)"
                    int fromUserId = 1;
                    //int toUserId = data _Ar.UserID;
                    string message = Musika.Enums.Response.ArtistRegistration;

                    SendNotificationForArtist(1, message, "Artist");
                    _AdminResponse.ResponseType = "Artist";
                    #endregion

                    return _AdminResponse;
                }
            }
            catch (Exception ex)
            {
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;
            }
        }

        public HttpResponseMessage SendNotificationForArtist(int fromUserId, string message, string eventType)
        {
            try
            {
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<UserSettings> _UserSettingsRepo = new GenericRepository<UserSettings>(_unitOfWork);
                GenericRepository<UserDevices> _UserDevicesRepo = new GenericRepository<UserDevices>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);

                Users _UsersTo = null;
                UserSettings _UserSettings = null;
                TourDate _TourEntity = null;

                //List<Users> lstUsers = _UsersRepo.Repository.GetAll();
                List<Users> lstUsers = _UsersRepo.Repository.GetAll().Where(x => x.DeviceToken != null).ToList();

                bool updateCount = false;

                for (int i = 0; i < lstUsers.Count; i++)
                {
                    _UsersTo = lstUsers[i];

                    //_UsersTo.UserLanguage
                    _UserSettings = _UserSettingsRepo.Repository.Get(p => p.UserID == _UsersTo.UserID && p.SettingKey == EUserSettings.Musika.ToString());

                    PushNotifications pNoty = new PushNotifications();
                    var deviceList = _UserDevicesRepo.Repository.GetAll(x => x.UserId == _UsersTo.UserID);

                    //if (tourDateId > 0)
                    //    _TourEntity = _TourDateRepo.Repository.GetById(tourDateId);

                    if (_UserSettings.SettingValue == false)
                    {
                        LogHelper.CreateLog2(message + " - NOTIFICATION OFF - ToUserID : " + _UsersTo.UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                    }


                    //for multiple devices
                    if (deviceList != null && deviceList.Count > 0 && _UserSettings.SettingValue == true && _UsersTo.RecordStatus == RecordStatus.Active.ToString() && _TourEntity != null)
                    {
                        updateCount = false;

                        message = SetMessageLang(_UsersTo.UserLanguage, _TourEntity.Tour_Utcdate, eventType, null);

                        foreach (var d in deviceList)
                        {
                            if (string.IsNullOrEmpty(d.DeviceToken))
                            {
                                LogHelper.CreateLog2(message + " - Device Token Not found - ToUserID : " + _UsersTo.UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                            }

                            if (d.DeviceToken != null)
                            {
                                if (d.DeviceType == "IOS" && !string.IsNullOrEmpty(d.DeviceToken))
                                {
                                    LogHelper.CreateLog2(message + " - IOS - ToUserID : " + _UsersTo.UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);


                                    //pNoty.SendNotification_IOS(d.DeviceToken, message, Numerics.GetInt(_UserSettings.NotificationCount + 1));
                                    //pNoty.SendNotification_Android(d.DeviceToken, message, "Artist");
                                    pNoty.SendNotification_IOS(d.DeviceToken, message, eventType);
                                    updateCount = true;
                                }
                                else if (d.DeviceType == "Android" && !string.IsNullOrEmpty(d.DeviceToken))
                                {
                                    LogHelper.CreateLog2(message + " - ANDROID - ToUserID : " + _UsersTo.UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                                    pNoty.SendNotification_Android(d.DeviceToken, message, eventType);
                                    updateCount = true;
                                }
                            }
                            else
                            {
                                updateCount = false;
                            }
                        }
                    }

                    if (updateCount)
                    {
                        _UserSettings.NotificationCount += 1;
                        _UserSettingsRepo.Repository.Update(_UserSettings);
                    }

                    // for single device
                    if (deviceList.Count == 0 && _UserSettings.SettingValue == true && _UsersTo.RecordStatus == RecordStatus.Active.ToString() && _TourEntity != null)
                    {
                        updateCount = false;

                        message = SetMessageLang(_UsersTo.UserLanguage, _TourEntity.Tour_Utcdate, eventType, null);

                        if (string.IsNullOrEmpty(_UsersTo.DeviceToken))
                        {
                            LogHelper.CreateLog2(message + " - Device Token Not found - ToUserID : " + _UsersTo.UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                        }

                        if (_UsersTo.DeviceType == "IOS" && !string.IsNullOrEmpty(_UsersTo.DeviceToken))
                        {
                            LogHelper.CreateLog2(message + " - IOS - ToUserID : " + _UsersTo.UserID.ToString() + " devicetoken= " + _UsersTo.DeviceToken, Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                            pNoty.SendNotification_IOS(_UsersTo.DeviceToken, message, eventType);


                            //pNoty.SendNotification_IOS(_UsersTo.DeviceToken, message, Numerics.GetInt(_UserSettings.NotificationCount + 1));
                            _UserSettings.NotificationCount += 1;
                            _UserSettingsRepo.Repository.Update(_UserSettings);
                        }
                        else if (_UsersTo.DeviceType == "Android" && !string.IsNullOrEmpty(_UsersTo.DeviceToken))
                        {
                            LogHelper.CreateLog2(message + " - ANDROID - ToUserID : " + _UsersTo.UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                            //pNoty.SendNotification_Android(_UsersTo.DeviceToken, message, eventType);
                            //_UserSettings.NotificationCount += 1;
                            //_UserSettingsRepo.Repository.Update(_UserSettings);
                        }
                    }

                    if (_UserSettings.SettingValue == true)
                    {
                        if (_UsersTo.DeviceType == "IOS")
                        {
                            //pNoty.SendNotification_IOS(_UsersTo.DeviceToken, message, Numerics.GetInt(_UserSettings.NotificationCount + 1));
                            //_UserSettings.NotificationCount += 1;
                            //_UserSettingsRepo.Repository.Update(_UserSettings);

                            #region "New Code"
                            //pNoty.SendNotification_IOS(_UsersTo.DeviceToken, message, eventType);
                            //_UserSettings.NotificationCount += 1;
                            //_UserSettingsRepo.Repository.Update(_UserSettings);
                            #endregion
                        }
                        else if (_UsersTo.DeviceType == "Android")
                        {
                            //pNoty.SendNotification_Android(_UsersTo.DeviceToken, message, eventType);
                            //_UserSettings.NotificationCount += 1;
                            //_UserSettingsRepo.Repository.Update(_UserSettings);
                        }
                    }
                    //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, null));
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, null));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }

        public string SetMessageLang(string lang, DateTime? tourDate, string artistName, string venueName)
        {

            var message = "";

            if (tourDate.HasValue)
            {
                var dayOfTheWeek = tourDate.Value.DayOfWeek.ToString();
                var isTwoDays = tourDate.Value > DateTime.UtcNow.AddDays(1);


                if (isTwoDays)
                {
                    if (lang == EUserLanguage.EN.ToString())
                        message = NotificationResponse.TwoDayEnglish;
                    else
                        message = NotificationResponse.TwoDaySpanish;
                }
                else
                {
                    if (lang == EUserLanguage.EN.ToString())
                        message = NotificationResponse.OneDayEnglish;
                    else
                        message = NotificationResponse.OneDaySpanish;
                }


                if (!String.IsNullOrEmpty(message))
                {
                    if (message.Contains("[day_of_week]"))
                        message = message.Replace("[day_of_week]", dayOfTheWeek);

                    if (message.Contains("[artist_name]"))
                        message = message.Replace("[artist_name]", artistName);

                    if (message.Contains("[venue_name]"))
                        message = message.Replace("[venue_name]", venueName);
                }
            }

            return message;
        }

        private string GetMusicGraphBio(string vMusicgraph_ID)
        {
            try
            {
                string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();

                HttpWebRequest httpWebRequest;
                HttpWebResponse httpResponse;
                string _result;
                string bioNoTags = "";
                string bioWithTags = "";

                httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/artist/" + vMusicgraph_ID + "/biography?api_key=" + _MusicGrapgh_api_key);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";

                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    _result = streamReader.ReadToEnd();
                }

                if (!String.IsNullOrEmpty(_result))
                {
                    var job = JObject.Parse(_result);
                    var data = job.SelectToken("data.artist_bio_nlp_no_tags");


                    if (data != null)
                    {
                        bioNoTags = data.Value<string>();

                        if (String.IsNullOrEmpty(bioNoTags))
                        {
                            data = job.SelectToken("data.artist_bio_short");
                            if (data != null)
                            {
                                bioWithTags = data.Value<string>();
                                if (!String.IsNullOrEmpty(bioWithTags))
                                {
                                    string input = bioWithTags;
                                    string regex = "(\\[.*\\])";
                                    bioWithTags = Regex.Replace(input, regex, "");
                                    return bioWithTags;
                                }
                            }
                        }
                        else
                            return bioNoTags;

                    }
                }
            }
            catch (Exception e)
            { }

            return "";
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AdminAPI/updateArtistPic")]
        public AdminResponse updateArtistPic()
        {
            AdminResponse _AdminResponse = new AdminResponse();

            try
            {

                Models.Artists _Artists = null;
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);

                var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];
                var _ArtistID = httpContext.Request.Form["artistid"] ?? "0";
                int ArtistID = Numerics.GetInt(_ArtistID);


                _Artists = _ArtistsRepo.Repository.Get(p => p.ArtistID == ArtistID);

                if (_Artists != null)
                {

                    if (HttpContext.Current.Request.Files.Count > 0)
                    {
                        ImageResponse imgResponse = MultipartFiles.GetMultipartImage(HttpContext.Current.Request.Files, "artistfile", "Artists" + @"\" + _Artists.ArtistID.ToString() + @"\", _Imagethumbsize, _Imagethumbsize, _imageSize, _imageSize, true, true, true, "Artists" + "/" + _Artists.ArtistID.ToString() + "/", true);

                        if (imgResponse.IsSuccess == true)
                        {
                            _Artists.ThumbnailURL = imgResponse.ThumbnailURL;
                            _Artists.ImageURL = imgResponse.ImageURL;
                            _Artists.BannerImage_URL = imgResponse.BannerImage_URL;
                        }

                    }

                    _ArtistsRepo.Repository.Update(_Artists);

                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Artist updated successfully.";
                    return _AdminResponse;
                }
                else
                {
                    _AdminResponse.Status = false;
                    _AdminResponse.RetMessage = "Invalid Artist";
                    return _AdminResponse;
                }
            }
            catch (Exception ex)
            {
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;
            }
        }



        #endregion

        #region "User Events"

        [AllowAnonymous]
        [Route("AdminAPI/GetUserEvents")]
        [HttpGet]
        public HttpResponseMessage GetUserEvents(string sUserName, string sEmail, string sArtistName, string sVenueName, int Pageindex, int Pagesize, string sortColumn, string sortOrder)
        {
            try
            {
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);

                GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);

                var _list = (from A in _UserGoingRepo.Repository.GetAll()
                             join B in _TourDateRepo.Repository.GetAll() on A.TourDateID equals B.TourDateID
                             join C in _ArtistsRepo.Repository.GetAll() on B.ArtistID equals C.ArtistID
                             join D in _UsersRepo.Repository.GetAll() on A.UserID equals D.UserID
                             join E in _VenueRepo.Repository.GetAll() on B.VenueID equals E.VenueID
                             where A.RecordStatus == EUserGoing.Going.ToString()
                             select new
                             {
                                 D.Email,
                                 B.Datetime_Local,
                                 C.ArtistName,
                                 E.VenueName,
                                 B.TourDateID,
                                 C.ArtistID,
                                 E.VenueID,
                                 D.UserName,
                                 D.UserID
                             }).AsQueryable();


                //Filters
                if (!string.IsNullOrEmpty(sUserName))
                {
                    _list = _list.Where(p => p.UserName.IndexOf(sUserName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).Distinct();
                }

                if (!string.IsNullOrEmpty(sEmail))
                {
                   _list = _list.Where(p => p.Email.IndexOf(sEmail.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).Distinct();
                   //--*s* and applied distinct in all filter
                }

                if (!string.IsNullOrEmpty(sArtistName))
                {
                    _list = _list.Where(p => p.ArtistName.IndexOf(sArtistName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).Distinct();
                }

                if (!string.IsNullOrEmpty(sVenueName))
                {
                    _list = _list.Where(p => p.VenueName.IndexOf(sVenueName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).Distinct();
                }

                //Sorting
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    _list = _list.OrderBy("" + sortColumn + " " + sortOrder);
                }

                //Pagination
                var _list2 = _list.ToPagedList(Pageindex - 1, Pagesize);

                Dictionary<string, object> d = new Dictionary<string, object>();

                if (_list.Count() > 0)
                {

                    d.Add("Items", _list2);
                    d.Add("PageCount", _list2.PageCount);
                    d.Add("PageNumber", _list2.PageNumber);
                    d.Add("PageSize", Pagesize);
                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
                else
                {
                    d.Add("Items", new List<string>());
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


        [AllowAnonymous]
        [Route("AdminAPI/UserEventRemove/{id}/{userid}")]
        [HttpDelete]
        public bool UserEventRemove(int id, int userid)
        {
            id = Numerics.GetInt(id);
            userid = Numerics.GetInt(userid);

            if (id > 0)
            {
                Models.UserGoing _UserGoing = null;
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);

                _UserGoing = _UserGoingRepo.Repository.Get(p => p.UserID == userid && p.TourDateID == id);

                if (_UserGoing != null)
                {
                    _UserGoingRepo.Repository.DeletePermanent(_UserGoing.GoingID);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        #endregion

        #region "Events"
        [AllowAnonymous]
        [Route("AdminAPI/GetEvents")]
        [HttpGet]
        public HttpResponseMessage GetEvents(string sArtistName, string sGenreName, string sEventName,
                                            string sVenueCountry, string sVenueName, string sMain_Genre,
                                            int Pageindex, int Pagesize, string sortColumn, string sortOrder,
                                            string sCity,
                                            bool archive = false, bool duplicate = false, bool deleted = false)
        {
            try
            {

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
                GenericRepository<HotTour> _HotTourRepo = new GenericRepository<HotTour>(_unitOfWork);

                List<ViewArtistlst> _ViewArtistlst = new List<ViewArtistlst>();

                DataSet ds = new DataSet();
                SpRepository _sp = new SpRepository();

                List<ViewEventlst> _list = null;

                if (duplicate)
                {
                    ds = _sp.SpGetEventDuplicates();
                    _list = General.DTtoList<ViewEventlst>(ds.Tables[0]);
                }
                else
                {
                    ds = _sp.SpGetEventList();
                    _list = General.DTtoList<ViewEventlst>(ds.Tables[0]);
                }

                //Filters
                /*if (archive)
                    _list = _list.Where(p => p.Datetime_Local < DateTime.Now).ToList();
                else
                    _list = _list.Where(p => p.Datetime_Local >= DateTime.Now).ToList();*/

                if (deleted)
                    _list = _list.Where(p => p.IsDeleted == true).ToList();
                else
                {
                    if (archive)
                        _list = _list.Where(p => p.Datetime_Local < DateTime.Now).ToList();
                    else
                        _list = _list.Where(p => p.Datetime_Local >= DateTime.Now).ToList();

                    _list = _list.Where(p => p.IsDeleted == false).ToList();
                }

                if (!string.IsNullOrEmpty(sArtistName))
                {
                    _list = _list.Where(p => p.ArtistName.IndexOf(sArtistName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(sGenreName))
                {
                    _list = _list.Where(p => p.Main_Genre.IndexOf(sGenreName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(sEventName))
                {
                    _list = _list.Where(p => p.EventName.IndexOf(sEventName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(sVenueCountry))
                {
                    _list = _list.Where(p => p.VenueCountry.IndexOf(sVenueCountry.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(sVenueName))
                {
                    _list = _list.Where(p => p.VenueName.IndexOf(sVenueName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(sMain_Genre))
                {
                    _list = _list.Where(p => p.Main_Genre.IndexOf(sMain_Genre.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(sCity))
                {
                    _list = _list.Where(p => p.VenueCity.IndexOf(sCity.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                //Sorting
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    _list = _list.OrderBy("" + sortColumn + " " + sortOrder).ToList();
                }
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
                    d.Add("Items", new List<ViewEventlst>());
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


        [AllowAnonymous]
        [Route("AdminAPI/GetEventsByID")]
        [HttpGet]
        public HttpResponseMessage GetEventsByID(Int32 ID)
        {
            try
            {
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<TicketingInventory> _TicketingInventoryRepo = new GenericRepository<TicketingInventory>(_unitOfWork);

                var _ArtistID = _TourDateRepo.Repository.Get(p => p.TourDateID == ID).ArtistID;

                var ticketing = _TicketingInventoryRepo.Repository.GetAll(x => x.TourDateId == ID).FirstOrDefault();
                int totalseats = 0;
                bool inappticketing = false;
                if (ticketing != null)
                {
                    totalseats = Convert.ToInt32(ticketing.TotalSeats);
                    inappticketing = Convert.ToBoolean(ticketing.InAppTicketing);
                }

                var _TourDate = (from A in _TourDateRepo.Repository.GetAll(p => p.TourDateID == ID)
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
                                     IsDeleted = A.IsDeleted,
                                     TotalSeats = totalseats,
                                     InAppTicketing = inappticketing
                                 }).ToList();

                if (_TourDate != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, _TourDate[0]);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Not Found");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }

        [AllowAnonymous]
        [Route("AdminAPI/GetSearchByArtistName")]
        [HttpGet]
        public IEnumerable<Artists> GetSearchByArtistName(string query = "")
        {
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);

            var _list = _ArtistsRepo.Repository.GetAll().ToList();

            if (!string.IsNullOrEmpty(query))
            {
                _list = _list.Where(p => RemoveDiacritics(p.ArtistName).IndexOf(query.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            return _list.OrderBy(x => x.ArtistName).ToList();
        }


        [AllowAnonymous]
        [Route("AdminAPI/GetSearchByVenueName")]
        [HttpGet]
        public IEnumerable<Venue> GetSearchByVenueName(string query = "")
        {
            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);

            var _list = _VenueRepo.Repository.GetAll().ToList();


            if (!string.IsNullOrEmpty(query))
            {
                _list = _list.Where(p => RemoveDiacritics(p.VenueName).IndexOf(query.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            return _list.OrderBy(x => x.VenueName).ToList();

            /*
            return String.IsNullOrEmpty(query) ? _VenueRepo.Repository.GetAll().ToList() :
                _VenueRepo.Repository.GetAll(p => p.VenueName.Contains(query)).OrderBy(x=>x.VenueName).ToList();
                */
        }



        [AllowAnonymous]
        [Route("AdminAPI/GetSearchByTimezone")]
        [HttpGet]
        public IEnumerable<UtcTimeZone> GetSearchByTimezone(string query = "")
        {
            GenericRepository<UtcTimeZone> _UtcTimeZone = new GenericRepository<UtcTimeZone>(_unitOfWork);

            return String.IsNullOrEmpty(query) ? _UtcTimeZone.Repository.GetAll().Select(p => new UtcTimeZone
            {
                TZ = p.TZ + " (UTC " + p.UTCoffset.ToString() + ")",
                TimezoneID = p.TimezoneID
            }).ToList() : _UtcTimeZone.Repository.GetAll(p => p.TZ.Contains(query)).Select(p => new UtcTimeZone
            {
                TZ = p.TZ + " (UTC " + p.UTCoffset.ToString() + ")",
                TimezoneID = p.TimezoneID
            }).ToList();
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("AdminAPI/updateEvent")]
        public AdminResponse updateEvent()
        {
           // _unitOfWork.StartTransaction();

            AdminResponse _AdminResponse = new AdminResponse();
            var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

            var _strTourDateID = httpContext.Request.Form["TourDateID"] ?? "0";
            Int32 _TourDateID = Convert.ToInt32(_strTourDateID);

            var _TicketURL = httpContext.Request.Form["TicketURL"];

            var _EventName = httpContext.Request.Form["EventName"];
            var _SeatGeek_TourID = httpContext.Request.Form["SeatGeek_TourID"];
            var _ArtistName = httpContext.Request.Form["ArtistName"];
            Int32 _artistid = Convert.ToInt32(httpContext.Request.Form["artistid"] ?? "0");

            var _Datetime_Local = httpContext.Request.Form["Datetime_Local"];
            Int32 _venueid = Convert.ToInt32(httpContext.Request.Form["venueid"] ?? "0");
            var _VenueName = httpContext.Request.Form["VenueName"];
            var _SeatGeek_VenuID = httpContext.Request.Form["SeatGeek_VenuID"];
            var _Address = httpContext.Request.Form["Address"];
            var _Extended_Address = httpContext.Request.Form["Extended_Address"];
            var _VenueCountry = httpContext.Request.Form["VenueCountry"];
            var _VenueCity = httpContext.Request.Form["VenueCity"];
            var _VenueState = httpContext.Request.Form["VenueState"];
            var _Postal_Code = httpContext.Request.Form["Postal_Code"];
            var _VenueLat = httpContext.Request.Form["VenueLat"];
            var _VenueLong = httpContext.Request.Form["VenueLong"];
            var _Timezone = httpContext.Request.Form["Timezone"];
            var _HashTag = httpContext.Request.Form["HashTag"];
            var _InAppTicketing = httpContext.Request.Form["InAppTicketing"];
            var _TotalSeats = httpContext.Request.Form["TotalSeats"];
            bool InApiTckt = false;
            int totalseats = 0;
            if (!string.IsNullOrEmpty(_InAppTicketing))
            {
                InApiTckt = true;
            }

            if (!string.IsNullOrEmpty(_TotalSeats))
            {
                totalseats = Convert.ToInt32(_TotalSeats);
            }

            try
            {
                Models.Venue _Venue = null;
                Models.TourDate _TourDate = null;
                Models.UtcTimeZone _UtcTimeZone = null;
                Models.TicketingInventory Ticket = null;
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<UtcTimeZone> _UtcTimeZoneRepo = new GenericRepository<UtcTimeZone>(_unitOfWork);
                GenericRepository<TicketingInventory> _TicketRepo = new GenericRepository<TicketingInventory>(_unitOfWork);

                _TourDate = _TourDateRepo.Repository.Get(p => p.TourDateID == _TourDateID);
                var _split = _Timezone.Split('(');
                var _TZ = _split[0];

                _UtcTimeZone = _UtcTimeZoneRepo.Repository.Get(p => p.TZ == _TZ);

                if (_UtcTimeZone == null)
                {
                    //_unitOfWork.RollBack();
                    _AdminResponse.Status = false;
                    _AdminResponse.RetMessage = "Invalid Time zone selected.";
                    return _AdminResponse;
                }

                int _UtcOffset = Convert.ToInt32(_UtcTimeZone.UTCoffset);

                _Venue = _VenueRepo.Repository.Get(p => p.VenueID == _venueid);

                #region "New Venue"
                if (_Venue == null)
                {
                    _Venue = new Venue();
                    _Venue.SeatGeek_VenuID = _SeatGeek_VenuID;
                    _Venue.VenueName = _VenueName;

                    if (HttpContext.Current.Request.Files.Count > 0)
                    {
                        ImageResponse imgResponse = MultipartFiles.GetMultipartImage(HttpContext.Current.Request.Files, "userfile", "Venue", _Imagethumbsize, _Imagethumbsize, _imageSize, _imageSize, true, false, true, "Venue");

                        if (imgResponse.IsSuccess == true)
                        {
                            _Venue.ImageURL = imgResponse.ImageURL;
                        }
                    }

                    _Venue.Extended_Address = _Extended_Address;
                    _Venue.Address = _Address;
                    _Venue.Postal_Code = _Postal_Code;
                    _Venue.Timezone = _Timezone;
                    _Venue.VenueCity = _VenueCity;
                    _Venue.VenueState = _VenueState;
                    _Venue.VenueCountry = _VenueCountry;
                    _Venue.VenueLat = Convert.ToDecimal(_VenueLat);
                    _Venue.VenueLong = Convert.ToDecimal(_VenueLong);
                    _Venue.CreatedDate = DateTime.Now;
                    _Venue.RecordStatus = RecordStatus.Eventful.ToString();
                    _VenueRepo.Repository.Add(_Venue);
                }
                #endregion

                #region "Update Venue"
                else
                {
                    _Venue.SeatGeek_VenuID = _SeatGeek_VenuID;
                    _Venue.VenueName = _VenueName;
                    if (HttpContext.Current.Request.Files.Count > 0)
                    {
                        ImageResponse imgResponse = MultipartFiles.GetMultipartImage(HttpContext.Current.Request.Files, "userfile", "Venue", _Imagethumbsize, _Imagethumbsize, _imageSize, _imageSize, true, false, true, "Venue");

                        if (imgResponse.IsSuccess == true)
                        {
                            _Venue.ImageURL = imgResponse.ImageURL;
                        }
                    }

                    _Venue.Extended_Address = _Extended_Address;
                    _Venue.Address = _Address;
                    _Venue.Postal_Code = _Postal_Code;
                    _Venue.Timezone = _Timezone;
                    _Venue.VenueCity = _VenueCity;
                    _Venue.VenueState = _VenueState;
                    _Venue.VenueCountry = _VenueCountry;
                    _Venue.VenueLat = Convert.ToDecimal(_VenueLat);
                    _Venue.VenueLong = Convert.ToDecimal(_VenueLong);
                    _Venue.ModifiedDate = DateTime.Now;

                    _VenueRepo.Repository.Update(_Venue);
                }
                #endregion

                #region "New Tour"
                if (_TourDate == null)// New Tour
                {
                    _TourDate = new TourDate();

                    _TourDate.EventName = _EventName;
                    _TourDate.TicketURL = _TicketURL;
                    _TourDate.SeatGeek_TourID = _SeatGeek_TourID;
                    _TourDate.ArtistID = _artistid;
                    _TourDate.HashTag = _HashTag;

                    _TourDate.Tour_Utcdate = Convert.ToDateTime(_Datetime_Local).AddHours(_UtcOffset);
                    _TourDate.Datetime_Local = Convert.ToDateTime(_Datetime_Local);
                    _TourDate.Visible_Until_utc = Convert.ToDateTime(_Datetime_Local).AddHours(_UtcOffset).AddHours(4);
                    _TourDate.Announce_Date = DateTime.Now;
                    _TourDate.RecordStatus = RecordStatus.Active.ToString();
                    _TourDate.CreatedDate = DateTime.Now;
                    _TourDate.VenueID = _Venue.VenueID;
                    _TourDateRepo.Repository.Add(_TourDate);

                    //_unitOfWork.Commit();

                    var obj = _TicketRepo.Repository.GetAll(x => x.TourDateId == _TourDate.TourDateID).FirstOrDefault();
                    if (obj != null)
                    {
                        obj.TotalSeats = totalseats;
                        obj.InAppTicketing = InApiTckt;
                        obj.TourDateId = _TourDate.TourDateID;
                        obj.ModifiedDatet = DateTime.Now;
                        _TicketRepo.Repository.Update(obj);
                    }
                    else
                    {
                        Ticket = new TicketingInventory();
                        Ticket.TourDateId = _TourDate.TourDateID;
                        Ticket.InAppTicketing = InApiTckt;
                        Ticket.TotalSeats = totalseats;
                        Ticket.CreatedDate = DateTime.Now;
                        Ticket.ModifiedDatet = DateTime.Now;
                        Ticket.RecordStatus = RecordStatus.Active.ToString();
                        _TicketRepo.Repository.Add(Ticket);
                    }

                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Event Added successfully.";

                    // Send Push Notification                    
                    #region "Send Notification - Added by Mukesh (3-Aug-2018)"
                    int fromUserId = 1;
                    //int toUserId = data _Ar.UserID;
                    string message = Musika.Enums.Response.EventNew;

                    SendNotificationForArtist(1, message, "Event");
                    _AdminResponse.ResponseType = "Event";
                    #endregion

                    return _AdminResponse;
                }
                #endregion

                #region "Update Tour"
                else
                {
                    _TourDate.TicketURL = _TicketURL;
                    _TourDate.EventName = _EventName;
                    _TourDate.SeatGeek_TourID = _SeatGeek_TourID;
                    _TourDate.ArtistID = _artistid;
                    _TourDate.HashTag = _HashTag;

                    _TourDate.Tour_Utcdate = Convert.ToDateTime(_Datetime_Local).AddHours(_UtcOffset);
                    _TourDate.Datetime_Local = Convert.ToDateTime(_Datetime_Local);
                    _TourDate.Visible_Until_utc = Convert.ToDateTime(_Datetime_Local).AddHours(_UtcOffset).AddHours(4);
                    _TourDate.ModifiedDate = DateTime.Now;
                    _TourDate.VenueID = _Venue.VenueID;
                    _TourDateRepo.Repository.Update(_TourDate);

                    //_unitOfWork.Commit();

                    var obj = _TicketRepo.Repository.GetAll(x => x.TourDateId == _TourDate.TourDateID).FirstOrDefault();
                    if (obj != null)
                    {
                        obj.TotalSeats = totalseats;
                        obj.InAppTicketing = InApiTckt;
                        obj.TourDateId = _TourDate.TourDateID;
                        obj.ModifiedDatet = DateTime.Now;
                        _TicketRepo.Repository.Update(obj);
                    }
                    else
                    {
                        Ticket = new TicketingInventory();
                        Ticket.TourDateId = _TourDate.TourDateID;
                        Ticket.InAppTicketing = InApiTckt;
                        Ticket.TotalSeats = totalseats;
                        Ticket.CreatedDate = DateTime.Now;
                        Ticket.ModifiedDatet = DateTime.Now;
                        Ticket.RecordStatus = RecordStatus.Active.ToString();
                        _TicketRepo.Repository.Add(Ticket);
                    }


                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Event Updated successfully.";

                    // Send Push Notification                    
                    #region "Send Notification - Added by Mukesh (3-Aug-2018)"
                    int fromUserId = 1;
                    //int toUserId = data _Ar.UserID;
                    string message = Musika.Enums.Response.EventUpdate;

                    SendNotificationForArtist(1, message, "Event");
                    _AdminResponse.ResponseType = "Event";
                    #endregion

                    return _AdminResponse;
                }
                #endregion
            }
            catch (Exception ex)
            {

                if (_unitOfWork.IsTransaction == true) _unitOfWork.RollBack();

                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;

            }
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("AdminAPI/UpdateHotTour")]
        public AdminResponse UpdateHotTour(dynamic data)
        {
            AdminResponse _AdminResponse = new AdminResponse();

            Int32 _TourDateID = Numerics.GetInt(data.TourDateID);

            try
            {
                GenericRepository<HotTour> _HotTourRepo = new GenericRepository<HotTour>(_unitOfWork);
                HotTour _HotTour = null;
                _HotTour = _HotTourRepo.Repository.Get(p => p.TourDateID == _TourDateID && p.RecordStatus != RecordStatus.Deleted.ToString());

                if (_HotTour == null)
                {
                    _HotTour = new HotTour();
                    _HotTour.TourDateID = _TourDateID;
                    _HotTour.CreatedDate = DateTime.Now;
                    _HotTour.RecordStatus = RecordStatus.Active.ToString();

                    _HotTourRepo.Repository.Add(_HotTour);
                }
                else
                {
                    _HotTour.ModifiedDate = DateTime.Now;
                    _HotTour.RecordStatus = RecordStatus.Deleted.ToString();

                    _HotTourRepo.Repository.Update(_HotTour);
                }

                _AdminResponse.Status = true;
                _AdminResponse.RetMessage = "Hot Tour updated successfully.";
                return _AdminResponse;

            }
            catch (Exception ex)
            {
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;
            }
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("AdminAPI/UpdateDefaultArtsit")]
        public AdminResponse UpdateDefaultArtsit(dynamic data)
        {
            AdminResponse _AdminResponse = new AdminResponse();

            Int32 _ArtistID = Numerics.GetInt(data.ArtistID);

            try
            {
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);

                var _artist = _ArtistsRepo.Repository.Get(p => p.ArtistID == _ArtistID && p.RecordStatus != RecordStatus.Deleted.ToString());

                if (_artist != null)
                {
                    if (_artist.Isdefault == true)
                    {
                        _artist.Isdefault = false;
                    }
                    else
                    {
                        _artist.Isdefault = true;
                    }

                    _ArtistsRepo.Repository.Update(_artist);
                }

                _AdminResponse.Status = true;
                _AdminResponse.RetMessage = "Default Artist updated successfully.";
                return _AdminResponse;

            }
            catch (Exception ex)
            {
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;
            }
        }


        [AllowAnonymous]
        [Route("AdminAPI/deleteEvent/{id}")]
        [AcceptVerbs("GET", "DELETE", "POST")]
        [HttpDelete]
        public bool DeleteEvent(int id)
        {
            try
            {
                GenericRepository<TourDate> _TourRepo = new GenericRepository<TourDate>(_unitOfWork);
                var tour = _TourRepo.Repository.GetById(id);
                if (tour != null)
                {
                    if (tour.IsDeleted)
                        tour.IsDeleted = false;
                    else
                        tour.IsDeleted = true;

                    _TourRepo.Repository.Update(tour);
                    return tour.IsDeleted;
                }
            }
            catch (Exception e)
            { }
            return false;
        }


        [AllowAnonymous]
        [Route("AdminAPI/deleteVenuephoto/{id}")]
        [HttpDelete]
        public bool deleteVenuephoto(int id)
        {
            id = Numerics.GetInt(id);
            if (id > 0)
            {
                Models.Venue _Venue = null;
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);

                _Venue = _VenueRepo.Repository.Get(p => p.VenueID == id);

                if (_Venue != null)
                {
                    _Venue.ImageURL = WebConfigurationManager.AppSettings["SiteImgURL"] + "/Default-Venue.jpg";
                    _VenueRepo.Repository.Update(_Venue);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        #endregion

        #region "Venue"

        [AllowAnonymous]
        [Route("AdminAPI/GetVenues")]
        [HttpGet]
        public HttpResponseMessage GetVenues(string sVenueName, string sVenueCountry, string sVenueCity, string sPostal_Code, int Pageindex, int Pagesize, string sortColumn, string sortOrder)
        {
            try
            {
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);

                List<ViewArtistlst> _ViewArtistlst = new List<ViewArtistlst>();

                DataSet ds = new DataSet();
                SpRepository _sp = new SpRepository();
                ds = _sp.SpGetVenueList();

                var _list = General.DTtoList<ViewVenuelst>(ds.Tables[0]);

                //Filters


                if (!string.IsNullOrEmpty(sVenueName))
                {
                    _list = _list.Where(p => p.VenueName.IndexOf(sVenueName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(sVenueCountry))
                {
                    _list = _list.Where(p => p.VenueCountry.IndexOf(sVenueCountry.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(sVenueCity))
                {
                    _list = _list.Where(p => p.VenueCity.IndexOf(sVenueCity.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(sPostal_Code))
                {
                    _list = _list.Where(p => p.Postal_Code.IndexOf(sPostal_Code.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                //Sorting
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    _list = _list.OrderBy("" + sortColumn + " " + sortOrder).ToList();
                }

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
                    d.Add("Items", new List<ViewVenuelst>());
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



        [AllowAnonymous]
        [Route("AdminAPI/GetVenueByID")]
        [HttpGet]
        public HttpResponseMessage GetVenueByID(Int32 ID)
        {
            try
            {
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);



                var _TourDate = (from A in _VenueRepo.Repository.GetAll(p => p.VenueID == ID)
                                 select new
                                 {
                                     VenueName = A.VenueName,
                                     VenueID = A.VenueID,
                                     SeatGeek_VenuID = A.SeatGeek_VenuID,
                                     Extended_Address = A.Extended_Address,
                                     Address = A.Address,
                                     VenueCountry = A.VenueCountry,
                                     VenueCity = A.VenueCity,
                                     VenueState = A.VenueState,
                                     Postal_Code = A.Postal_Code,
                                     VenueLat = A.VenueLat,
                                     VenueLong = A.VenueLong,
                                     Timezone = A.Timezone,
                                     ImageURL = A.ImageURL
                                 }).ToList();

                if (_TourDate != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, _TourDate[0]);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Not Found");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);

            }
        }



        [AllowAnonymous]
        [HttpPost]
        [Route("AdminAPI/updateVenue")]
        public AdminResponse updateVenue()
        {
            AdminResponse _AdminResponse = new AdminResponse();
            var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

            Int32 _venueid = Convert.ToInt32(httpContext.Request.Form["venueid"] ?? "0");
            var _VenueName = httpContext.Request.Form["VenueName"];
            var _SeatGeek_VenuID = httpContext.Request.Form["SeatGeek_VenuID"];
            var _Address = httpContext.Request.Form["Address"];
            var _Extended_Address = httpContext.Request.Form["Extended_Address"];
            var _VenueCountry = httpContext.Request.Form["VenueCountry"];
            var _VenueCity = httpContext.Request.Form["VenueCity"];
            var _VenueState = httpContext.Request.Form["VenueState"];
            var _Postal_Code = httpContext.Request.Form["Postal_Code"];
            var _VenueLat = httpContext.Request.Form["VenueLat"];
            var _VenueLong = httpContext.Request.Form["VenueLong"];
            var _Timezone = httpContext.Request.Form["Timezone"];

            try
            {

                Venue _Venue = null;
                TourDate _TourDate = null;
                UtcTimeZone _UtcTimeZone = null;
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<UtcTimeZone> _UtcTimeZoneRepo = new GenericRepository<UtcTimeZone>(_unitOfWork);

                var _split = _Timezone.Split('(');
                var _TZ = _split[0];


                _UtcTimeZone = _UtcTimeZoneRepo.Repository.Get(p => p.TZ == _TZ);

                if (_UtcTimeZone == null)
                {
                    _unitOfWork.RollBack();
                    _AdminResponse.Status = false;
                    _AdminResponse.RetMessage = "Invalid Time zone selected.";
                    return _AdminResponse;
                }

                int _UtcOffset = Convert.ToInt32(_UtcTimeZone.UTCoffset);

                _Venue = _VenueRepo.Repository.Get(p => p.VenueID == _venueid);

                #region "New Venue"
                if (_Venue == null)
                {
                    _Venue = new Venue();
                    _Venue.SeatGeek_VenuID = _SeatGeek_VenuID;
                    _Venue.VenueName = _VenueName;

                    if (HttpContext.Current.Request.Files.Count > 0)
                    {
                        ImageResponse imgResponse = MultipartFiles.GetMultipartImage(HttpContext.Current.Request.Files, "userfile", "Venue", _Imagethumbsize, _Imagethumbsize, _imageSize, _imageSize, true, false, true, "Venue");

                        if (imgResponse.IsSuccess == true)
                        {
                            _Venue.ImageURL = imgResponse.ImageURL;
                        }

                    }

                    _Venue.Extended_Address = _Extended_Address;
                    _Venue.Address = _Address;
                    _Venue.Postal_Code = _Postal_Code;
                    _Venue.Timezone = _Timezone;
                    _Venue.VenueCity = _VenueCity;
                    _Venue.VenueState = _VenueState;
                    _Venue.VenueCountry = _VenueCountry;
                    _Venue.VenueLat = Convert.ToDecimal(_VenueLat);
                    _Venue.VenueLong = Convert.ToDecimal(_VenueLong);
                    _Venue.CreatedDate = DateTime.Now;
                    _Venue.RecordStatus = RecordStatus.Eventful.ToString();
                    _VenueRepo.Repository.Add(_Venue);

                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Venue has added successfully";
                    return _AdminResponse;
                }
                #endregion

                #region "Update Venue"
                else
                {
                    _Venue.SeatGeek_VenuID = _SeatGeek_VenuID;
                    _Venue.VenueName = _VenueName;

                    if (HttpContext.Current.Request.Files.Count > 0)
                    {
                        ImageResponse imgResponse = MultipartFiles.GetMultipartImage(HttpContext.Current.Request.Files, "userfile", "Venue", _Imagethumbsize, _Imagethumbsize, _imageSize, _imageSize, true, false, true, "Venue");

                        if (imgResponse.IsSuccess == true)
                        {
                            _Venue.ImageURL = imgResponse.ImageURL;
                        }

                    }

                    _Venue.Extended_Address = _Extended_Address;
                    _Venue.Address = _Address;
                    _Venue.Postal_Code = _Postal_Code;
                    _Venue.Timezone = _Timezone;
                    _Venue.VenueCity = _VenueCity;
                    _Venue.VenueState = _VenueState;
                    _Venue.VenueCountry = _VenueCountry;
                    _Venue.VenueLat = Convert.ToDecimal(_VenueLat);
                    _Venue.VenueLong = Convert.ToDecimal(_VenueLong);
                    _Venue.ModifiedDate = DateTime.Now;

                    _VenueRepo.Repository.Update(_Venue);

                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Venue has updated successfully";
                    return _AdminResponse;
                }
                #endregion
            }
            catch (Exception ex)
            {
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;
            }
        }


        #endregion

        #region "Artist Genre"

        [AllowAnonymous]
        [Route("AdminAPI/GetArtistGenre")]
        [HttpGet]
        public HttpResponseMessage GetArtistGenre(string sArtistName, string sGenreName, int Pageindex, int Pagesize, string sortColumn, string sortOrder)
        {
            try
            {
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(_unitOfWork);
                GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(_unitOfWork);



                var _list = (from A in _ArtistGenreRepo.Repository.GetAll()
                             join B in _ArtistsRepo.Repository.GetAll() on A.ArtistID equals B.ArtistID
                             join C in _GenreRepo.Repository.GetAll() on A.GenreID equals C.GenreID
                             select new
                             {
                                 ImageURL = B.ImageURL,
                                 ArtistName = B.ArtistName,
                                 Genre_Name = C.Name,
                                 Primary = A.Primary
                             }).AsQueryable();


                //Filters


                if (!string.IsNullOrEmpty(sArtistName))
                {
                    _list = _list.Where(p => p.ArtistName.IndexOf(sArtistName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (!string.IsNullOrEmpty(sGenreName))
                {
                    _list = _list.Where(p => p.Genre_Name.IndexOf(sGenreName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
                }

                //Sorting
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    _list = _list.OrderBy("" + sortColumn + " " + sortOrder);
                }

                //Pagination
                var _list2 = _list.ToPagedList(Pageindex - 1, Pagesize);

                Dictionary<string, object> d = new Dictionary<string, object>();

                if (_list.Count() > 0)
                {

                    d.Add("Items", _list2);
                    d.Add("PageCount", _list2.PageCount);
                    d.Add("PageNumber", _list2.PageNumber);
                    d.Add("PageSize", Pagesize);

                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
                else
                {
                    d.Add("Items", new List<string>());
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


        [AllowAnonymous]
        [Route("AdminAPI/GetArtistGenreByID")]
        [HttpGet]
        public HttpResponseMessage GetArtistGenreByID(Int32 ArtistID, int Pageindex2, int Pagesize2, string sortColumn2, string sortOrder2)
        {
            try
            {
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(_unitOfWork);
                GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(_unitOfWork);



                var _list = (from A in _ArtistGenreRepo.Repository.GetAll()
                             join B in _ArtistsRepo.Repository.GetAll() on A.ArtistID equals B.ArtistID
                             join C in _GenreRepo.Repository.GetAll() on A.GenreID equals C.GenreID
                             where B.ArtistID == ArtistID
                             select new
                             {
                                 ArtistGenreID = A.ArtistGenreID,
                                 ImageURL = B.ImageURL,
                                 ArtistName = B.ArtistName,
                                 Genre_Name = C.Name,
                                 Primary = A.Primary
                             }).AsQueryable();







                //Sorting
                if (!string.IsNullOrEmpty(sortColumn2))
                {
                    _list = _list.OrderBy("" + sortColumn2 + " " + sortOrder2);
                }


                //Pagination
                var _list2 = _list.ToPagedList(Pageindex2 - 1, Pagesize2);


                Dictionary<string, object> d = new Dictionary<string, object>();


                if (_list.Count() > 0)
                {

                    d.Add("Items", _list2);
                    d.Add("PageCount", _list2.PageCount);
                    d.Add("PageNumber", _list2.PageNumber);
                    d.Add("PageSize", Pagesize2);

                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);

            }
        }


        [AllowAnonymous]
        [Route("AdminAPI/GetArtistGenreByID")]
        [HttpGet]
        public HttpResponseMessage GetArtistGenreByID(Int32 ID)
        {
            try
            {
                GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(_unitOfWork);
                GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(_unitOfWork);


                var _ArtistGenre = (from A in _ArtistGenreRepo.Repository.GetAll()
                                    join B in _GenreRepo.Repository.GetAll() on A.GenreID equals B.GenreID
                                    where A.ArtistGenreID == ID
                                    select new
                                    {
                                        GenreID = A.GenreID,
                                        ArtistGenreID = A.ArtistGenreID,
                                        Name = B.Name,
                                        Primary = A.Primary
                                    }).ToList();


                if (_ArtistGenre != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, _ArtistGenre[0]);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Not Found");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);

            }
        }

        [AllowAnonymous]
        [Route("AdminAPI/GetSearchByGenreName")]
        [HttpGet]
        public IEnumerable<Genre> GetSearchByGenreName(string query = "")
        {
            GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(_unitOfWork);

            return String.IsNullOrEmpty(query) ? _GenreRepo.Repository.GetAll().ToList() :
                _GenreRepo.Repository.GetAll(p => p.Name.Contains(query)).ToList();

        }


        [AllowAnonymous]
        [HttpPost]
        [Route("AdminAPI/updateArtistGenre")]
        public AdminResponse updateArtistGenre(dynamic data)
        {
            AdminResponse _AdminResponse = new AdminResponse();

            try
            {
                _unitOfWork.StartTransaction();

                Models.ArtistGenre _ArtistGenre = null;
                Models.ArtistGenre _ArtistGenre2 = null;
                GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(_unitOfWork);


                int _ArtistGenreID = Numerics.GetInt(data.ArtistGenreID);
                int _GenreID = Numerics.GetInt(data.GenreID);
                int _ArtistID = Numerics.GetInt(data.ArtistID);


                _ArtistGenre = _ArtistGenreRepo.Repository.Get(p => p.ArtistGenreID == _ArtistGenreID);

                if (_ArtistGenre != null)
                {
                    _ArtistGenre2 = _ArtistGenreRepo.Repository.Get(p => p.ArtistGenreID != _ArtistGenreID && p.ArtistID == _ArtistID
                                             && p.GenreID == _GenreID);
                    if (_ArtistGenre2 != null)
                    {
                        _unitOfWork.RollBack();
                        _AdminResponse.Status = false;
                        _AdminResponse.RetMessage = "This Genre already exist for this artist";
                        return _AdminResponse;
                    }

                    if (data.Primary == "1")
                    {
                        var _lst = _ArtistGenreRepo.Repository.GetAll(p => p.ArtistID == _ArtistID);
                        foreach (ArtistGenre _gen in _lst)
                        {
                            _gen.Primary = false;
                            _ArtistGenreRepo.Repository.Update(_gen);
                        }
                    }
                    else
                    {
                        if (_ArtistGenre.Primary == true)
                        {
                            _unitOfWork.RollBack();
                            _AdminResponse.Status = false;
                            _AdminResponse.RetMessage = "Cannot change the status of the Main Genre to false,";
                            return _AdminResponse;
                        }
                    }

                    _ArtistGenre.Name = data.Name;
                    _ArtistGenre.GenreID = data.GenreID;
                    _ArtistGenre.Primary = data.Primary == "1" ? true : false;

                    _ArtistGenreRepo.Repository.Update(_ArtistGenre);

                    _unitOfWork.Commit();
                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Genre updated successfully.";
                    return _AdminResponse;
                }
                else
                {
                    _ArtistGenre2 = _ArtistGenreRepo.Repository.Get(p => p.ArtistID == _ArtistID
                                               && p.GenreID == _GenreID);
                    if (_ArtistGenre2 != null)
                    {
                        _unitOfWork.RollBack();
                        _AdminResponse.Status = false;
                        _AdminResponse.RetMessage = "This Genre already exist for this artist";
                        return _AdminResponse;
                    }

                    if (data.Primary == "1")
                    {
                        var _lst = _ArtistGenreRepo.Repository.GetAll(p => p.ArtistID == _ArtistID);
                        foreach (ArtistGenre _gen in _lst)
                        {
                            _gen.Primary = false;
                            _ArtistGenreRepo.Repository.Update(_gen);
                        }
                    }

                    _ArtistGenre = new ArtistGenre();

                    _ArtistGenre.Name = data.Name;
                    _ArtistGenre.GenreID = data.GenreID;
                    _ArtistGenre.ArtistID = _ArtistID;
                    _ArtistGenre.RecordStatus = RecordStatus.Active.ToString();

                    _ArtistGenre2 = _ArtistGenreRepo.Repository.GetAll(p => p.ArtistID == _ArtistID).FirstOrDefault();

                    if (_ArtistGenre2 != null)
                    {
                        _ArtistGenre.Primary = data.Primary == "1" ? true : false;
                    }
                    else
                    {
                        _ArtistGenre.Primary = true;
                    }


                    _ArtistGenreRepo.Repository.Add(_ArtistGenre);

                    _unitOfWork.Commit();
                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Genre Added successfully.";
                    return _AdminResponse;
                }


            }
            catch (Exception ex)
            {
                if (_unitOfWork.IsTransaction == true) _unitOfWork.RollBack();
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;

            }
        }


        [AllowAnonymous]
        [Route("AdminAPI/DeleteArtistGenre/{id}")]
        [HttpDelete]
        public AdminResponse DeleteArtistGenre(int id)
        {
            AdminResponse _AdminResponse = new AdminResponse();
            try
            {
                id = Numerics.GetInt(id);
                if (id > 0)
                {
                    Models.ArtistGenre _ArtistGenre = null;
                    GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(_unitOfWork);
                    _ArtistGenre = _ArtistGenreRepo.Repository.Get(p => p.ArtistGenreID == id);

                    if (_ArtistGenre.Primary == true)
                    {
                        _AdminResponse.Status = false;
                        _AdminResponse.RetMessage = "Primary Genre cannot be deleted.";
                        return _AdminResponse;
                    }

                    _ArtistGenreRepo.Repository.DeletePermanent(id);

                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Artist genre has deleted successfully.";
                    return _AdminResponse;
                }
                else
                {
                    _AdminResponse.Status = false;
                    _AdminResponse.RetMessage = "error.";
                    return _AdminResponse;
                }
            }
            catch (Exception ex)
            {
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;
            }

        }


        #endregion

        #region "Artist Not Latin"

        [AllowAnonymous]
        [Route("AdminAPI/GetNotLatin")]
        [HttpGet]
        public HttpResponseMessage GetNotLatin(string sName, int Pageindex, int Pagesize, string sortColumn, string sortOrder)
        {
            try
            {
                GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(_unitOfWork);
                GenericRepository<ArtistsNotLatin> _ArtistsNotLatinRepo = new GenericRepository<ArtistsNotLatin>(_unitOfWork);


                var _list = (from A in _ArtistsNotLatinRepo.Repository.GetAll()
                             select new
                             {
                                 AID = A.AID,
                                 Name = A.ArtistName
                             }).AsQueryable();

                //Filters

                if (!string.IsNullOrEmpty(sName))
                {
                    _list = _list.Where(p => p.Name.IndexOf(sName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
                }

                //Sorting
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    _list = _list.OrderBy("" + sortColumn + " " + sortOrder);
                }

                //Pagination
                var _list2 = _list.ToPagedList(Pageindex - 1, Pagesize);

                Dictionary<string, object> d = new Dictionary<string, object>();

                if (_list.Count() > 0)
                {

                    d.Add("Items", _list2);
                    d.Add("PageCount", _list2.PageCount);
                    d.Add("PageNumber", _list2.PageNumber);
                    d.Add("PageSize", Pagesize);

                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
                else
                {
                    d.Add("Items", new List<string>());
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


        [AllowAnonymous]
        [Route("AdminAPI/GetNotLatinByID")]
        [HttpGet]
        public HttpResponseMessage GetNotLatinByID(Int32 ID)
        {
            try
            {
                GenericRepository<ArtistsNotLatin> _ArtistsNotLatinRepo = new GenericRepository<ArtistsNotLatin>(_unitOfWork);

                var _Notlatin = (from A in _ArtistsNotLatinRepo.Repository.GetAll()
                                 where A.AID == ID
                                 select new
                                 {
                                     AID = A.AID,
                                     Name = A.ArtistName
                                 }).ToList();


                if (_Notlatin != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, _Notlatin[0]);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Not Found");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);

            }
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("AdminAPI/updateNotLatin")]
        public AdminResponse updateNotLatin(dynamic data)
        {
            AdminResponse _AdminResponse = new AdminResponse();

            try
            {

                GenericRepository<ArtistsNotLatin> _ArtistsNotLatinRepo = new GenericRepository<ArtistsNotLatin>(_unitOfWork);

                int _AID = Numerics.GetInt(data.AID);
                string _Name = data.Name;
                var _NotLatin = _ArtistsNotLatinRepo.Repository.Get(p => p.AID == _AID);

                if (_NotLatin != null)
                {

                    _NotLatin.ArtistName = data.Name;

                    _ArtistsNotLatinRepo.Repository.Update(_NotLatin);

                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "updated successfully.";
                    return _AdminResponse;
                }
                else
                {


                    _NotLatin = new ArtistsNotLatin();

                    _NotLatin.ArtistName = data.Name;
                    _ArtistsNotLatinRepo.Repository.Add(_NotLatin);

                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Added successfully.";
                    return _AdminResponse;
                }


            }
            catch (Exception ex)
            {
                if (_unitOfWork.IsTransaction == true) _unitOfWork.RollBack();
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;

            }
        }



        [AllowAnonymous]
        [Route("AdminAPI/DeleteNotLatin/{id}")]
        [HttpDelete]
        public AdminResponse DeleteNotLatin(int id)
        {
            AdminResponse _AdminResponse = new AdminResponse();
            try
            {
                id = Numerics.GetInt(id);
                if (id > 0)
                {
                    GenericRepository<ArtistsNotLatin> _ArtistsNotLatinRepo = new GenericRepository<ArtistsNotLatin>(_unitOfWork);


                    var _NotLatin = _ArtistsNotLatinRepo.Repository.Get(p => p.AID == id);


                    _ArtistsNotLatinRepo.Repository.DeletePermanent(_NotLatin.AID);

                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "deleted successfully.";
                    return _AdminResponse;
                }
                else
                {
                    _AdminResponse.Status = false;
                    _AdminResponse.RetMessage = "error.";
                    return _AdminResponse;
                }
            }
            catch (Exception ex)
            {
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;
            }

        }


        #endregion

        #region "Genre"

        [AllowAnonymous]
        [Route("AdminAPI/GetGenre")]
        [HttpGet]
        public HttpResponseMessage GetGenre(string sName, int Pageindex, int Pagesize, string sortColumn, string sortOrder)
        {
            try
            {
                GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(_unitOfWork);
                GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(_unitOfWork);

                DataSet ds = new DataSet();
                SpRepository _sp = new SpRepository();
                ds = _sp.SpGetGenreList();
                var _list = General.DTtoList<ViewGenrelst>(ds.Tables[0]);




                //Filters

                if (!string.IsNullOrEmpty(sName))
                {
                    _list = _list.Where(p => p.Name.IndexOf(sName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }


                //Sorting
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    _list = _list.OrderBy("" + sortColumn + " " + sortOrder).ToList();
                }


                //Pagination
                var _list2 = _list.ToPagedList(Pageindex - 1, Pagesize);


                Dictionary<string, object> d = new Dictionary<string, object>();


                if (_list.Count() > 0)
                {

                    d.Add("Items", _list2);
                    d.Add("PageCount", _list2.PageCount);
                    d.Add("PageNumber", _list2.PageNumber);
                    d.Add("PageSize", Pagesize);

                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
                else
                {
                    d.Add("Items", new List<ViewGenrelst>());
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


        [AllowAnonymous]
        [Route("AdminAPI/GetGenreByID")]
        [HttpGet]
        public HttpResponseMessage GetGenreByID(Int32 ID)
        {
            try
            {
                GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(_unitOfWork);

                var _Genre = (from A in _GenreRepo.Repository.GetAll()
                              where A.GenreID == ID
                              select new
                              {
                                  GenreID = A.GenreID,
                                  Name = A.Name
                              }).ToList();


                if (_Genre != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, _Genre[0]);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Not Found");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);

            }
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("AdminAPI/updateGenre")]
        public AdminResponse updateGenre(dynamic data)
        {
            AdminResponse _AdminResponse = new AdminResponse();

            try
            {

                Models.Genre _Genre = null;
                Models.Genre _Genre2 = null;
                GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(_unitOfWork);

                int _GenreID = Numerics.GetInt(data.GenreID);
                string _Name = data.Name;
                _Genre = _GenreRepo.Repository.Get(p => p.GenreID == _GenreID);

                if (_Genre != null)
                {
                    _Genre2 = _GenreRepo.Repository.Get(p => p.GenreID != _GenreID && p.Name.ToLower() == _Name.ToLower()
                                             && p.RecordStatus != RecordStatus.Deleted.ToString());
                    if (_Genre2 != null)
                    {
                        _AdminResponse.Status = false;
                        _AdminResponse.RetMessage = "This Genre already exist";
                        return _AdminResponse;
                    }

                    _Genre.Name = data.Name;
                    _Genre.ModifiedDate = DateTime.Now;

                    _GenreRepo.Repository.Update(_Genre);

                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Genre updated successfully.";
                    return _AdminResponse;
                }
                else
                {
                    _Genre2 = _GenreRepo.Repository.Get(p => p.Name.ToLower() == _Name.ToLower()
                        && p.RecordStatus != RecordStatus.Deleted.ToString());
                    if (_Genre2 != null)
                    {
                        _AdminResponse.Status = false;
                        _AdminResponse.RetMessage = "This Genre already exist";
                        return _AdminResponse;
                    }


                    _Genre = new Genre();

                    _Genre.Name = data.Name;
                    _Genre.CreatedDate = DateTime.Now;
                    _Genre.RecordStatus = RecordStatus.Active.ToString();

                    _GenreRepo.Repository.Add(_Genre);

                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Genre Added successfully.";
                    return _AdminResponse;
                }


            }
            catch (Exception ex)
            {
                if (_unitOfWork.IsTransaction == true) _unitOfWork.RollBack();
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;

            }
        }



        [AllowAnonymous]
        [Route("AdminAPI/DeleteGenre/{id}")]
        [HttpDelete]
        public AdminResponse DeleteGenre(int id)
        {
            AdminResponse _AdminResponse = new AdminResponse();
            try
            {
                id = Numerics.GetInt(id);
                if (id > 0)
                {
                    Models.Genre _Genre = null;
                    Models.ArtistGenre _ArtistGenre = null;
                    GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(_unitOfWork);
                    GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(_unitOfWork);


                    _ArtistGenre = _ArtistGenreRepo.Repository.GetAll(p => p.GenreID == id).FirstOrDefault();

                    if (_ArtistGenre != null)
                    {
                        _AdminResponse.Status = false;
                        _AdminResponse.RetMessage = "Genre assigned to artist cannot be deleted.";
                        return _AdminResponse;
                    }

                    _GenreRepo.Repository.Delete(id);

                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Genre has deleted successfully.";
                    return _AdminResponse;
                }
                else
                {
                    _AdminResponse.Status = false;
                    _AdminResponse.RetMessage = "error.";
                    return _AdminResponse;
                }
            }
            catch (Exception ex)
            {
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;
            }

        }



        #endregion

        #region ADs

        [AllowAnonymous]
        [Route("AdminAPI/GetCities")]
        [HttpGet]
        public IEnumerable<UsCities> GetCities(string query = "")
        {
            // GenericRepository<UsCities> _UsCityRepo = new GenericRepository<UsCities>(_unitOfWork);
            GenericRepository<CountryCodes> _CountryCodesRepo = new GenericRepository<CountryCodes>(_unitOfWork);

            //var _list = _UsCityRepo.Repository.GetAll(x => x.City.StartsWith(query)).Take(30);
            var _list = _CountryCodesRepo.Repository.GetAll(x => x.Name.StartsWith(query)).Select(x => new
            UsCities
            {
                City = x.Name,
                CityID = x.CountryCodeId
            }
            ).Take(30).OrderBy(x => x.City).ToList();

            return _list;
        }


        [AllowAnonymous]
        [Route("AdminAPI/GetAds")]
        [HttpGet]
        public HttpResponseMessage GetAds()
        {
            try
            {
                GenericRepository<Ads> _AdsRepo = new GenericRepository<Ads>(_unitOfWork);

                var _list = _AdsRepo.Repository.GetAll();

                Dictionary<string, object> d = new Dictionary<string, object>();
                if (_list.Count() > 0)
                {
                    d.Add("Items", _list);
                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
                else
                {
                    d.Add("Items", new List<Ads>());
                    return Request.CreateResponse(HttpStatusCode.OK, d);
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        [AllowAnonymous]
        [Route("AdminAPI/DeleteAds/{id}")]
        [HttpDelete]
        public string DeleteAds(int id)
        {
            id = Numerics.GetInt(id);
            if (id > 0)
            {
                GenericRepository<Ads> _AdsRepo = new GenericRepository<Ads>(_unitOfWork);

                var _Ads = _AdsRepo.Repository.Get(p => p.AdId == id);

                if (_Ads != null)
                {
                    _AdsRepo.Repository.DeletePermanent(_Ads.AdId);
                    return "true";
                }
                else
                {
                    return "false";
                }

            }
            else
            {
                return "false";
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AdminAPI/updateAds")]
        public AdminResponse updateAds(dynamic data)
        {
            AdminResponse _AdminResponse = new AdminResponse();

            try
            {

                GenericRepository<Ads> _AdsRepo = new GenericRepository<Ads>(_unitOfWork);

                int _AdId = Numerics.GetInt(data.AdId);
                var _ads = _AdsRepo.Repository.Get(p => p.AdId == _AdId);

                if (_ads != null)
                {
                    _ads.City = data.CityName;
                    _ads.Recordstatus = data.Recordstatus;
                    _ads.LinkURL = data.link;

                    _AdsRepo.Repository.Update(_ads);

                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Ad updated successfully.";
                    return _AdminResponse;
                }
                else
                {
                    _AdminResponse.Status = false;
                    _AdminResponse.RetMessage = "Ad not found.";
                    return _AdminResponse;
                }


            }
            catch (Exception ex)
            {
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;

            }
        }

        #endregion

        public static string RemoveDiacritics(string value)
        {
            if (String.IsNullOrEmpty(value))
                return value;

            string normalized = value.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in normalized)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            Encoding nonunicode = Encoding.GetEncoding(850);
            Encoding unicode = Encoding.Unicode;

            byte[] nonunicodeBytes = Encoding.Convert(unicode, nonunicode, unicode.GetBytes(sb.ToString()));
            char[] nonunicodeChars = new char[nonunicode.GetCharCount(nonunicodeBytes, 0, nonunicodeBytes.Length)];
            nonunicode.GetChars(nonunicodeBytes, 0, nonunicodeBytes.Length, nonunicodeChars, 0);

            return new string(nonunicodeChars);
        }

        [AllowAnonymous]
        [Route("AdminAPI/downloadTicketingCSVFile")]
        [HttpPost]
        public string downloadTicketingCSVFile(dynamic data)
        {
            string fileName = "";
            string ddlTable = "";
            if (data.ddlTableName != null)
            {
                ddlTable = data.ddlTableName;
            }
            if (ddlTable == "users")
            {
                #region Users export csv
                GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);
                var list = _UsersRepo.Repository.GetAll().ToList();

                string sitePath = ConfigurationManager.AppSettings["SiteImgPath"] + "\\CSVFiles\\";
                if (!Directory.Exists(sitePath))
                {
                    Directory.CreateDirectory(sitePath);
                }
                fileName = "TicketingUsers_" + Guid.NewGuid().ToString().Substring(0, 5) + ".csv";
                string csvFilePath = sitePath + fileName;

                using (CsvFileWriter writer = new CsvFileWriter(csvFilePath))
                {
                    ReadWriteCSVFile row = new ReadWriteCSVFile();
                    row.Add(String.Format(" {0}", "UserID"));
                    row.Add(String.Format(" {0}", "UserName"));
                    row.Add(String.Format(" {0}", "Email"));
                    row.Add(String.Format(" {0}", "Password"));
                    row.Add(String.Format(" {0}", "FacebookID"));
                    row.Add(String.Format(" {0}", "ThumbnailURL"));
                    row.Add(String.Format(" {0}", "ImageURL"));
                    row.Add(String.Format(" {0}", "DeviceType"));
                    row.Add(String.Format(" {0}", "DeviceToken"));
                    row.Add(String.Format(" {0}", "DeviceLat"));
                    row.Add(String.Format(" {0}", "DeviceLong"));
                    row.Add(String.Format(" {0}", "RecordStatus"));
                    row.Add(String.Format(" {0}", "ModifiedDate"));
                    row.Add(String.Format(" {0}", "CreatedDate"));
                    row.Add(String.Format(" {0}", "SynFacebookID"));
                    row.Add(String.Format(" {0}", "UserLanguage"));

                    writer.WriteRow(row);
                    foreach (var rows in list)
                    {
                        row = new ReadWriteCSVFile();
                        row.Add(String.Format(" {0}", rows.UserID));
                        row.Add(String.Format(" {0}", rows.UserName));
                        row.Add(String.Format(" {0}", rows.Email));
                        row.Add(String.Format(" {0}", rows.Password));
                        row.Add(String.Format(" {0}", rows.FacebookID));
                        row.Add(String.Format(" {0}", rows.ThumbnailURL));
                        row.Add(String.Format(" {0}", rows.ImageURL));
                        row.Add(String.Format(" {0}", rows.DeviceType));
                        row.Add(String.Format(" {0}", rows.DeviceToken));
                        row.Add(String.Format(" {0}", rows.DeviceLat));
                        row.Add(String.Format(" {0}", rows.DeviceLong));
                        row.Add(String.Format(" {0}", rows.RecordStatus));
                        row.Add(String.Format(" {0}", rows.ModifiedDate));
                        row.Add(String.Format(" {0}", rows.CreatedDate));
                        row.Add(String.Format(" {0}", rows.SynFacebookID));
                        row.Add(String.Format(" {0}", rows.UserLanguage));

                        writer.WriteRow(row);
                    }
                }
                #endregion
            }

            if (ddlTable == "artists")
            {
                #region artists export csv
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                var list = _ArtistsRepo.Repository.GetAll().ToList();

                string sitePath = ConfigurationManager.AppSettings["SiteImgPath"] + "\\CSVFiles\\";
                if (!Directory.Exists(sitePath))
                {
                    Directory.CreateDirectory(sitePath);
                }
                fileName = "Artists" + Guid.NewGuid().ToString().Substring(0, 5) + ".csv";
                string csvFilePath = sitePath + fileName;

                using (CsvFileWriter writer = new CsvFileWriter(csvFilePath))
                {
                    ReadWriteCSVFile row = new ReadWriteCSVFile();
                    row.Add(String.Format(" {0}", "ArtistID"));
                    row.Add(String.Format(" {0}", "ArtistName"));
                    row.Add(String.Format(" {0}", "About"));
                    row.Add(String.Format(" {0}", "ImageURL"));
                    row.Add(String.Format(" {0}", "OnTour"));
                    row.Add(String.Format(" {0}", "Gender"));
                    row.Add(String.Format(" {0}", "Seatgeek_ID"));
                    row.Add(String.Format(" {0}", "Musicgraph_ID"));
                    row.Add(String.Format(" {0}", "Eventful_ID"));
                    row.Add(String.Format(" {0}", "Instagram_ID"));
                    row.Add(String.Format(" {0}", "Artist_Ref_ID"));
                    row.Add(String.Format(" {0}", "Musicbrainz_ID"));
                    row.Add(String.Format(" {0}", "Spotify_ID"));
                    row.Add(String.Format(" {0}", "Youtube_ID"));
                    row.Add(String.Format(" {0}", "Main_Genre"));
                    row.Add(String.Format(" {0}", "Decade"));
                    row.Add(String.Format(" {0}", "Alternate_Names"));
                    row.Add(String.Format(" {0}", "Spotify_Url"));
                    row.Add(String.Format(" {0}", "Lastfm_Url"));
                    row.Add(String.Format(" {0}", "Instagram_Url"));
                    row.Add(String.Format(" {0}", "Instagram_Tag"));
                    row.Add(String.Format(" {0}", "CreatedDate"));
                    row.Add(String.Format(" {0}", "ModifiedDate"));
                    row.Add(String.Format(" {0}", "RecordStatus"));
                    row.Add(String.Format(" {0}", "Spotify_URL_Name"));
                    row.Add(String.Format(" {0}", "BannerImage_URL"));
                    row.Add(String.Format(" {0}", "ThumbnailURL"));
                    row.Add(String.Format(" {0}", "AboutES"));
                    row.Add(String.Format(" {0}", "Isdefault"));

                    writer.WriteRow(row);
                    foreach (var rows in list)
                    {
                        row = new ReadWriteCSVFile();
                        row.Add(String.Format(" {0}", rows.ArtistID));
                        row.Add(String.Format(" {0}", rows.ArtistName));
                        row.Add(String.Format(" {0}", rows.About));
                        row.Add(String.Format(" {0}", rows.ImageURL));
                        row.Add(String.Format(" {0}", rows.OnTour));
                        row.Add(String.Format(" {0}", rows.Gender));
                        row.Add(String.Format(" {0}", rows.Seatgeek_ID));
                        row.Add(String.Format(" {0}", rows.Musicgraph_ID));
                        row.Add(String.Format(" {0}", rows.Eventful_ID));
                        row.Add(String.Format(" {0}", rows.Instagram_ID));
                        row.Add(String.Format(" {0}", rows.Artist_Ref_ID));
                        row.Add(String.Format(" {0}", rows.Musicbrainz_ID));
                        row.Add(String.Format(" {0}", rows.Spotify_ID));
                        row.Add(String.Format(" {0}", rows.Youtube_ID));
                        row.Add(String.Format(" {0}", rows.Main_Genre));
                        row.Add(String.Format(" {0}", rows.Decade));
                        row.Add(String.Format(" {0}", rows.Alternate_Names));
                        row.Add(String.Format(" {0}", rows.Spotify_Url));
                        row.Add(String.Format(" {0}", rows.Lastfm_Url));
                        row.Add(String.Format(" {0}", rows.Instagram_Url));
                        row.Add(String.Format(" {0}", rows.Instagram_Tag));
                        row.Add(String.Format(" {0}", rows.CreatedDate));
                        row.Add(String.Format(" {0}", rows.ModifiedDate));
                        row.Add(String.Format(" {0}", rows.RecordStatus));
                        row.Add(String.Format(" {0}", rows.Spotify_URL_Name));
                        row.Add(String.Format(" {0}", rows.BannerImage_URL));
                        row.Add(String.Format(" {0}", rows.ThumbnailURL));
                        row.Add(String.Format(" {0}", rows.AboutES));
                        row.Add(String.Format(" {0}", rows.Isdefault));

                        writer.WriteRow(row);
                    }
                }
                #endregion
            }
            if (ddlTable == "venue")
            {
                #region venue exoport csv
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                var list = _VenueRepo.Repository.GetAll().ToList();

                string sitePath = ConfigurationManager.AppSettings["SiteImgPath"] + "\\CSVFiles\\";
                if (!Directory.Exists(sitePath))
                {
                    Directory.CreateDirectory(sitePath);
                }
                fileName = "Venue_" + Guid.NewGuid().ToString().Substring(0, 5) + ".csv";
                string csvFilePath = sitePath + fileName;

                using (CsvFileWriter writer = new CsvFileWriter(csvFilePath))
                {
                    ReadWriteCSVFile row = new ReadWriteCSVFile();
                    row.Add(String.Format(" {0}", "VenueID"));
                    row.Add(String.Format(" {0}", "SeatGeek_VenuID"));
                    row.Add(String.Format(" {0}", "VenueName"));
                    row.Add(String.Format(" {0}", "ImageURL"));
                    row.Add(String.Format(" {0}", "Extended_Address"));
                    row.Add(String.Format(" {0}", "Display_Location"));
                    row.Add(String.Format(" {0}", "Slug"));
                    row.Add(String.Format(" {0}", "Postal_Code"));
                    row.Add(String.Format(" {0}", "Address"));
                    row.Add(String.Format(" {0}", "Timezone"));
                    row.Add(String.Format(" {0}", "VenueCity"));
                    row.Add(String.Format(" {0}", "VenueState"));
                    row.Add(String.Format(" {0}", "VenueCountry"));
                    row.Add(String.Format(" {0}", "VenueLat"));
                    row.Add(String.Format(" {0}", "VenueLong"));
                    row.Add(String.Format(" {0}", "CreatedDate"));
                    row.Add(String.Format(" {0}", "ModifiedDate"));
                    row.Add(String.Format(" {0}", "RecordStatus"));
                    row.Add(String.Format(" {0}", "Eventful_VenueID"));

                    writer.WriteRow(row);
                    foreach (var rows in list)
                    {
                        row = new ReadWriteCSVFile();
                        row.Add(String.Format(" {0}", rows.VenueID));
                        row.Add(String.Format(" {0}", rows.SeatGeek_VenuID));
                        row.Add(String.Format(" {0}", rows.VenueName));
                        row.Add(String.Format(" {0}", rows.ImageURL));
                        row.Add(String.Format(" {0}", rows.Extended_Address));
                        row.Add(String.Format(" {0}", rows.Display_Location));
                        row.Add(String.Format(" {0}", rows.Slug));
                        row.Add(String.Format(" {0}", rows.Postal_Code));
                        row.Add(String.Format(" {0}", rows.Address));
                        row.Add(String.Format(" {0}", rows.Timezone));
                        row.Add(String.Format(" {0}", rows.VenueCity));
                        row.Add(String.Format(" {0}", rows.VenueState));
                        row.Add(String.Format(" {0}", rows.VenueCountry));
                        row.Add(String.Format(" {0}", rows.VenueLat));
                        row.Add(String.Format(" {0}", rows.VenueLong));
                        row.Add(String.Format(" {0}", rows.CreatedDate));
                        row.Add(String.Format(" {0}", rows.ModifiedDate));
                        row.Add(String.Format(" {0}", rows.RecordStatus));
                        row.Add(String.Format(" {0}", rows.Eventful_VenueID));

                        writer.WriteRow(row);
                    }
                }
                #endregion
            }
            return ConfigurationManager.AppSettings["WebPath"].ToString() + "/Content/Upload/CSVFiles/" + fileName;
        }
        [AllowAnonymous]
        [Route("AdminAPI/Approve")]
        [HttpPost]
        public bool Approve(string ID)
        {
            DataSet ds;
            DataSet ds1;
            int EventID = Convert.ToInt32(ID);
            Models.TicketingEventsNew _Events = null;
            ds = new Musika.Repository.SPRepository.SpRepository().ShiftDatetoOriginalTables(EventID);
            GenericRepository<TicketingEventsNew> _EventsRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
            _Events = _EventsRepo.Repository.GetById(EventID);

            bool tourdata = new Musika.Repository.SPRepository.SpRepository().SpUpdateTourData(_Events.ArtistId, _Events.VenueName, _Events.StartDate, _Events.EventTitle, _Events.EventID);
            // Mail To Organizer
            ds1 = new Musika.Repository.SPRepository.SpRepository().SpGetTicketingEventUsersToSendEmail(EventID);
            if (ds1.Tables[0].Rows.Count > 0)
            {
                string html = string.Empty;
                DataRow dr = ds1.Tables[0].Rows[0];
                string Email = dr["Email"].ToString();
                html = "<p>Hi " + dr["UserName"].ToString() + "," + " </p>";
                html += "<p>Your Event Changes Has been approved by Admin." + "</p>";
                html += "<p><br>You can view your changes in you panel" + "<p>";
                //  html += "<p><br>User Name : " + dr["UserName"].ToString() + "<p>";
                html += "<p><br><br><strong>Thanks,<br><br>The " + WebConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";
                SendEmailHelper.SendMail(Email, "Event Changes Approved", html, "");
                return true;
            }
            return false;

        }

        [AllowAnonymous]
        [Route("AdminAPI/DeleteTicketingUser/{id}")]
        [HttpDelete]
        public string DeleteTicketingUser(int id)
        {
            id = Numerics.GetInt(id);
            if (id > 0)
            {
                GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

                var _Users = _UsersRepo.Repository.Get(p => p.UserID == id);

                if (_Users != null)
                {
                    _UsersRepo.Repository.Delete(_Users.UserID);
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            else
            {
                return "false";
            }
        }

        [AllowAnonymous]
        [Route("AdminAPI/ChangeTicketingUserStatus")]
        [HttpPost]
        public bool ChangeTicketingUserStatus(Int64 ID, Int16 InactivePeriod)
        {
            Models.TicketingUsers _Users = null;
            GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

            _Users = _UsersRepo.Repository.Get(p => p.UserID == ID);

            if (_Users != null)
            {
                if (_Users.UserID != -1) // if not admin
                {
                    _Users.RecordStatus = _Users.RecordStatus == RecordStatus.Active.ToString() ? RecordStatus.InActive.ToString() : RecordStatus.Active.ToString();
                    _UsersRepo.Repository.Update(_Users);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        [AllowAnonymous]
        [Route("AdminAPI/DeleteTicketingUserPhoto/{id}")]
        [HttpDelete]
        public bool DeleteTicketingUserPhoto(int id)
        {
            id = Numerics.GetInt(id);
            if (id > 0)
            {
                Models.TicketingUsers _Users = null;
                GenericRepository<TicketingUsers> _UsersRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

                _Users = _UsersRepo.Repository.Get(p => p.UserID == id);

                if (_Users != null)
                {
                    _Users.ThumbnailURL = WebConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                    _Users.ImageURL = WebConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                    _UsersRepo.Repository.Update(_Users);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        [AllowAnonymous]
        [Route("AdminAPI/GetTicketingEventByID")]
        [HttpGet]
        public HttpResponseMessage GetTicketingEventByID(Int32 ID)
        {
            try
            {
                Models.TicketingEvents _Users = null;
                GenericRepository<TicketingEvents> _UsersRepo = new GenericRepository<TicketingEvents>(_unitOfWork);

                _Users = _UsersRepo.Repository.Get(p => p.EventID == ID);

                if (_Users != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, _Users);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Not Found");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }

        #region "Coupons"
       
        [AllowAnonymous]
        [Route("AdminAPI/GetCoupons")]
        [HttpGet]
        public HttpResponseMessage GetCoupons(string sEventName, string packageName, string sCouponCode, int Pageindex, int Pagesize, string sortColumn, string sortOrder)
        {
            try
            {
                GenericRepository<TicketingEventsNew> _TicketingEventRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);

                DataSet ds = new DataSet();
                SpRepository _sp = new SpRepository();
                //ds = _sp.GetCouponsList();
                //ds = _sp.GetCouponsList(sEventName, packageName);
                ds = _sp.GetCouponsList();
                var _list = General.DTtoList<CouponsModel>(ds.Tables[0]);

                _list = _list.GroupBy(x => x.CouponCode, (key, group) => group.First()).OrderBy(p => p.ExpiryDate).ToList();
                #region "Commented Code"
                //Filters
                if (!string.IsNullOrEmpty(sEventName))
                {
                    _list = _list.Where(p => RemoveDiacritics(p.EventName).IndexOf(sEventName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }
                if (!string.IsNullOrEmpty(packageName))
                {
                    _list = _list.Where(p => RemoveDiacritics(p.TicketCategory).IndexOf(packageName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                if (!string.IsNullOrEmpty(sCouponCode))
                {
                    _list = _list.Where(p => p.CouponCode.IndexOf(sCouponCode.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                ////Sorting
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    _list = _list.OrderBy("" + sortColumn + " " + sortOrder).ToList();
                }
                #endregion

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


        [AllowAnonymous]
        [Route("AdminAPI/GetCouponByID")]
        [HttpGet]
        public HttpResponseMessage GetCouponByID(Int32 ID)
        {
            try
            {
                Models.CouponsModel _Coupons = null;
                GenericRepository<CouponsModel> _CouponsRepo = new GenericRepository<CouponsModel>(_unitOfWork);

                _Coupons = _CouponsRepo.Repository.Get(p => p.Id == ID);

                if (_Coupons != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, _Coupons);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Not Found");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.CreateLog(ex);
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("AdminAPI/UpdateCoupon")]
        public AdminResponse UpdateCoupon()
        {
            AdminResponse _AdminResponse = new AdminResponse();
            bool res;
            try
            {
                var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

                var _EventName = httpContext.Request.Form["sEventName"];
                var _CouponCode = httpContext.Request.Form["CouponCode"];
                var _Discount = httpContext.Request.Form["Discount"];
                var _ExpiryDate = httpContext.Request.Form["ExpiryDate"];
                var _TicketCategory = httpContext.Request.Form["TicketCategory"];

                CouponsModel _Coupons = new CouponsModel();
                _Coupons.CouponCode = _CouponCode;
                _Coupons.EventName = _EventName;
                _Coupons.Discount = Convert.ToDecimal(_Discount);
                _Coupons.ExpiryDate = Convert.ToDateTime(_ExpiryDate);
                _Coupons.CreateOn = DateTime.UtcNow;
                _Coupons.CreatedBy = 1;
                _Coupons.TicketCategory = _TicketCategory;

                // Check Existence of Coupon Code
                res = new Musika.Repository.SPRepository.SpRepository().CheckCouponCode(_Coupons.EventName, _Coupons.CouponCode);
                if (res == false)
                {
                    new Musika.Repository.SPRepository.SpRepository().SpAddCouponsNew(_EventName, _CouponCode, Convert.ToDecimal(_Discount), Convert.ToDateTime(_ExpiryDate), 1, _TicketCategory);
                    _AdminResponse.Status = true;
                    _AdminResponse.RetMessage = "Coupons updated successfully.";
                }
                else
                {
                    _AdminResponse.Status = false;
                    _AdminResponse.RetMessage = "Coupons Code Already Exists.";
                }
                return _AdminResponse;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                _AdminResponse.Status = false;
                _AdminResponse.RetMessage = ex.Message;
                return _AdminResponse;
            }
        }
        #endregion
    }
}