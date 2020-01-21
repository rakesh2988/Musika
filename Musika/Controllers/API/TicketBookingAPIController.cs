using ArtSeeker.Library.Common;
using Musika.Enums;
using Musika.Library.API;
using Musika.Library.CacheProvider;
using Musika.Library.JsonConverter;
using Musika.Library.Multipart;
using Musika.Library.PushNotfication;
using Musika.Library.Search;
using Musika.Library.Utilities;
using Musika.Models;
using Musika.Models.API.Input;
using Musika.Models.API.View;
using Musika.Repository.GRepository;
using Musika.Repository.Interface;
using Musika.Repository.SPRepository;
using Musika.Repository.UnitofWork;
using MvcPaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyWebAPI;
using SpotifyWebAPI.Web.Enums;
using SpotifyWebAPI.Web.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Xml;


namespace Musika.Controllers.API
{
    public class TicketBookingAPIController : ApiController
    {
        #region "Variable Initialization"
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpCache _Cache = new HttpCache();

        int _Imagethumbsize = 0;
        int _imageSize = 0;
        bool _ApiLogger = false;
        //double _FuzzySearchCri = 0.33;

        private readonly string[] _GenreFilter =
                                    {"Axé","Axe","Bachata","Baião",
                                    "Baiao" ,"Bambuco","Banda","Batucada","Biguine",
                                    "Bolero","Bomba","Boogaloo","Bossa nova",
                                    "Brazilian rock","Cha-cha-cha","Cha cha cha","Changüí",
                                    "Charanga","Choro" ,"Compas","Conga","Conjunto",
                                    "Contradanza","Corrido","Cuarteto","Cueca","Cumbia",
                                    "Danza","Danzón","Danzon","Duranguense","Filin","Forró",
                                    "Forro","Frevo","Funk carioca","Guaguancó","Guaguanco",
                                    "Guaracha","Gwo ka","Huapango","Huayno","Jarabe","Joropo",
                                    "Lambada","Latin","Lundu","Mambo","Mariachi","Mazouk","Merengue",
                                    "Méringue","Meringue","Milonga","Música popular brasileira",
                                    "Musica popular brasileira", "Norteño","Norteno","Nueva canción",
                                    "Nueva cancion","Nueva trova","Orquesta típica","Orquesta tipica",
                                    "Pachanga","Pagode","Pambiche","Pasillo","Payada","Plena","Porro","Pop in spanish",
                                    "Punto guajiro","Ranchera","Rasin","Reggaeton","Rondalla","Rumba","Salsa","Samba",
                                    "Sertanejo","Seis","Son","Son jalisciense","Son Jarocho","Son montuno","Songo","Tango",
                                    "Tango Tejano","Timba","Tonada","Trío romántico","Tropicália","Tropicalia","Twoubadou",
                                    "Vallenato","Vals criollo","Zouk"};
        #endregion

        #region "Constructor"
        public TicketBookingAPIController()
        {
            _unitOfWork = new UnitOfWork();
            _Imagethumbsize = Convert.ToInt16(WebConfigurationManager.AppSettings["ImageThumbSize"].ToString());
            _imageSize = Convert.ToInt16(WebConfigurationManager.AppSettings["ImageSize"].ToString());
            _ApiLogger = Convert.ToBoolean(WebConfigurationManager.AppSettings["Logger"].ToString());
        }
        #endregion

        #region "Sign In from Device"
        [HttpPost]
        [Route("api/TicketBooking/SignIn")]
        public HttpResponseMessage SignIn(string Email,string Password)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                InputSignUp input = new InputSignUp();
                input.Email = Email;
                input.Password = Password;

                Models.Users entity = null;
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                string _Email = "";
                string _Password = "";

                //_UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + input.UserName;
                _Email = System.Configuration.ConfigurationManager.AppSettings["XMPPPrefix"].ToString() + input.Email;
                _Password = AesCryptography.Encrypt(input.Password);

                entity = _UsersRepo.Repository.Get(p => p.Email.ToLower() == _Email && p.Password == _Password);    // && p.RecordStatus == "Active");

                if (entity != null)
                {

                    if (entity.RecordStatus != RecordStatus.Active.ToString())
                    {
                        //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "User has blocked, contact mailto:anm@yopmail.com"));
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "Account verification is pending from Admin"));
                    }
                    if (entity.Password != _Password)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.PasswordOrEmail));
                    }

                    entity.DeviceType = input.DeviceType;
                    entity.DeviceToken = input.DeviceToken;
                    entity.DeviceLong = input.DeviceLong;
                    entity.DeviceLat = input.DeviceLat;

                    _UsersRepo.Repository.Update(entity);

                    SetUserDevices(entity);

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(entity.UserID, false), "UserData"));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.PasswordOrEmail));
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }
        #endregion

        #region "SignUp"

        [HttpPost]
        [Route("api/TicketBooking/Signup")]
        public HttpResponseMessage Signup()
        {
            try
            {
                InputSignUp Input = new InputSignUp();
                var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

                #region "Commented Code"
                //Input.UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + httpContext.Request.Form["UserName"];
                //Input.Password = AesCryptography.Encrypt(httpContext.Request.Form["Password"]);
                //Input.Email = httpContext.Request.Form["Email"];
                #endregion

                Input.UserName = httpContext.Request.QueryString[0].ToString();
                Input.Password = AesCryptography.Encrypt(httpContext.Request.QueryString[2].ToString());
                Input.Email = httpContext.Request.QueryString[1].ToString();


                #region "Commented Code"
                //Input.MobileNumber = httpContext.Request.Form["MobileNumber"];
                //Input.MobileUserName = Input.Email;


                //Input.DeviceToken = httpContext.Request.Form["DeviceToken"];
                //Input.DeviceType = httpContext.Request.Form["DeviceType"];
                //Input.DeviceLat = Convert.ToDecimal(httpContext.Request.Form["DeviceLat"]);
                //Input.DeviceLong = Convert.ToDecimal(httpContext.Request.Form["DeviceLong"]);
                #endregion

                var context = new ValidationContext(Input, serviceProvider: null, items: null);
                var results = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(Input, context, results);

                if (!isValid)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, results[0].ErrorMessage));
                }

                #region "Commented Code"
                // Check whether the POST operation is MultiPart?
                //if (!Request.Content.IsMimeMultipartContent())
                //{
                //    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                //}
                #endregion

                string _UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + Input.UserName;
                Models.Users entity = new Models.Users();
                GenericRepository<Users> _userEntity = new GenericRepository<Users>(_unitOfWork);

                ///check USer Name 
                if (!Helper.IsValidPattern(_UserName, "^[a-zA-Z0-9. _-]{2,100}$"))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.Reg_InvalUserName));
                }

                ///check email 
                //bool Email = false;
                //Email = Helper.IsValidEmail(Input.Email);
                //if (Email == false)
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.Invalidemail));
                //}

                //check if UserName already exists
                Users _user = null;

                //check if Email already exists
                _user = _userEntity.Repository.Get(e => e.Email == Input.Email && e.RecordStatus != RecordStatus.Deleted.ToString());
                if (_user != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.EmailAlreadyExists));
                }

                _unitOfWork.StartTransaction();     // Start Transaction

                try
                {
                    entity.Password = Input.Password;
                    entity.Email = Input.Email;
                    entity.UserName = Input.UserName;

                    #region "Commented Code"
                    //entity.Password = Input.Password;

                    //entity.DeviceToken = Input.DeviceToken;
                    //entity.DeviceType = Input.DeviceType;
                    //entity.DeviceLat = Input.DeviceLat;
                    //entity.DeviceLong = Input.DeviceLong;
                    #endregion

                    entity.CreatedDate = DateTime.Now;
                    entity.RecordStatus = RecordStatus.Active.ToString();
                    entity.RecordStatus = "Inactive";

                    _userEntity.Repository.Add(entity);

                    SetUserDevices(entity);

                    GenericRepository<UserSettings> _UserSettingsEntity = new GenericRepository<UserSettings>(_unitOfWork);
                    UserSettings _UserSettings = new UserSettings();

                    _UserSettings.UserID = entity.UserID;
                    _UserSettings.SettingKey = EUserSettings.Musika.ToString();
                    _UserSettings.SettingValue = true;
                    _UserSettings.NotificationCount = 0;
                    

                    _UserSettingsEntity.Repository.Add(_UserSettings);

                    _unitOfWork.Commit();// End Transaction

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(entity.UserID, true), "UserData"));
                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    _unitOfWork.RollBack();//RollBack Transaction
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                if (_unitOfWork.IsTransaction == true) _unitOfWork.RollBack();//RollBack Transaction
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }
        }
        #endregion

        #region "User Details"
        [Route("api/TicketBooking/UserDetail")]
        private ViewUser UserDetail(Int64 _UserID, bool _IsNewUser)
        {
            ViewUser _viewUser = new ViewUser();
            GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

            Users entity;
            entity = _UsersRepo.Repository.Get(u => u.UserID == _UserID);

            if (entity != null)
            {
                _viewUser.UserID = entity.UserID;
                _viewUser.Email = entity.Email;
                _viewUser.UserName = entity.UserName;
                _viewUser.Password = entity.Password;

                _viewUser.ImageURL = entity.ImageURL ?? string.Empty;
                _viewUser.ThumbnailURL = entity.ThumbnailURL ?? string.Empty;
                _viewUser.IsNewUser = _IsNewUser;
                _viewUser.RecordStatus = entity.RecordStatus;
            }
            else
            {
                return null;
            }
            //string output = JsonConvert.SerializeObject(_viewUser);   // convert object to json string
            return _viewUser;
        }
        #endregion


        #region "Set User Devices"
        private void SetUserDevices(Users user)
        {
            GenericRepository<UserDevices> _UserDevices = new GenericRepository<UserDevices>(_unitOfWork);
            if (!String.IsNullOrEmpty(user.DeviceType) && !String.IsNullOrEmpty(user.DeviceToken))
            {
                var existingTokens = _UserDevices.Repository.GetAll(x => x.DeviceToken == user.DeviceToken && x.DeviceType == user.DeviceType);

                if (existingTokens != null && existingTokens.Count() > 0)
                    existingTokens.ForEach(x => _UserDevices.Repository.DeletePermanent(x.UserDeviceId));


                _UserDevices.Repository.Add(new UserDevices()
                {
                    UserId = user.UserID,
                    DeviceToken = user.DeviceToken,
                    DeviceType = user.DeviceType,
                    CreatedDate = DateTime.Now
                });
            }

        }
        #endregion


        #region "Unused Code"
        //#region "User Authentication / Login"
        //[HttpPost]
        //[Route("api/v2/TicketBooking/SignUp")]
        //public HttpResponseMessage SignUp()
        //{
        //    try
        //    {
        //        InputSignUp Input = new InputSignUp();
        //        var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

        //        Input.Password = AesCryptography.Encrypt(httpContext.Request.Form["Password"]);
        //        Input.Email = httpContext.Request.Form["Email"];
        //        Input.UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + httpContext.Request.Form["UserName"];

        //        Input.DeviceToken = httpContext.Request.Form["DeviceToken"];
        //        Input.DeviceType = httpContext.Request.Form["DeviceType"];
        //        Input.DeviceLat = Convert.ToDecimal(httpContext.Request.Form["DeviceLat"]);
        //        Input.DeviceLong = Convert.ToDecimal(httpContext.Request.Form["DeviceLong"]);

        //        var context = new ValidationContext(Input, serviceProvider: null, items: null);
        //        var results = new List<ValidationResult>();
        //        var isValid = Validator.TryValidateObject(Input, context, results);

        //        if (!isValid)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, results[0].ErrorMessage));
        //        }

        //        // Check whether the POST operation is MultiPart?
        //        if (!Request.Content.IsMimeMultipartContent())
        //        {
        //            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        //        }

        //        string _UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + Input.UserName;
        //        Models.Users entity = new Models.Users();
        //        GenericRepository<Users> _userEntity = new GenericRepository<Users>(_unitOfWork);

        //        ///check USer Name 
        //        if (!Helper.IsValidPattern(_UserName, "^[a-zA-Z0-9. _-]{2,100}$"))
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.Reg_InvalUserName));
        //        }

        //        ///check email 
        //        bool Email = false;
        //        Email = Helper.IsValidEmail(Input.Email);
        //        if (Email == false)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.Invalidemail));
        //        }

        //        //check if UserName already exists
        //        Users _user = null;

        //        //check if Email already exists
        //        _user = _userEntity.Repository.Get(e => e.Email == Input.Email && e.RecordStatus != RecordStatus.Deleted.ToString());
        //        if (_user != null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.EmailAlreadyExists));
        //        }

        //        _unitOfWork.StartTransaction();// Start Transaction

        //        try
        //        {
        //            entity.Password = Input.Password;
        //            entity.Email = Input.Email;
        //            entity.UserName = Input.UserName;

        //            entity.DeviceToken = Input.DeviceToken;
        //            entity.DeviceType = Input.DeviceType;
        //            entity.DeviceLat = Input.DeviceLat;
        //            entity.DeviceLong = Input.DeviceLong;

        //            entity.CreatedDate = DateTime.Now;
        //            entity.RecordStatus = RecordStatus.Active.ToString();

        //            entity.ThumbnailURL = WebConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
        //            entity.ImageURL = ConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";

        //            _userEntity.Repository.Add(entity);

        //            SetUserDevices(entity);

        //            GenericRepository<UserSettings> _UserSettingsEntity = new GenericRepository<UserSettings>(_unitOfWork);
        //            UserSettings _UserSettings = new UserSettings();

        //            _UserSettings.UserID = entity.UserID;
        //            _UserSettings.SettingKey = EUserSettings.Musika.ToString();
        //            _UserSettings.SettingValue = true;
        //            _UserSettings.NotificationCount = 0;
        //            _UserSettingsEntity.Repository.Add(_UserSettings);


        //            _unitOfWork.Commit();// End Transaction

        //            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(entity.UserID, true), "UserData"));
        //        }
        //        catch (Exception ex)
        //        {
        //            LogHelper.CreateLog3(ex, Request);
        //            _unitOfWork.RollBack();//RollBack Transaction
        //            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.CreateLog3(ex, Request);
        //        if (_unitOfWork.IsTransaction == true) _unitOfWork.RollBack();//RollBack Transaction
        //        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
        //    }
        //}
        //#endregion
        #endregion


        #region "Logout User"
        [HttpGet]
        [Route("api/v2/TicketBooking/Logout")]
        public ApiResponse Logout(int UserId, string DeviceToken)
        {
            try
            {
                GenericRepository<UserDevices> _UserDevices = new GenericRepository<UserDevices>(_unitOfWork);
                var userDevice = _UserDevices.Repository.Get(x => x.UserId == UserId && x.DeviceToken == DeviceToken);

                if (userDevice != null)
                    _UserDevices.Repository.DeletePermanent(userDevice.UserDeviceId);

                return JsonResponse.GetResponse(ResponseCode.Success, null);
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return JsonResponse.GetResponse(ResponseCode.Exception, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }
        }
        #endregion


        #region "Third Party SignIn"
        [HttpPost]
        [Route("api/v2/TicketBooking/SigninThirdParty")]
        public HttpResponseMessage SigninThirdParty(InputSignInWithThirdParty input)
        {
            try
            {
                _unitOfWork.StartTransaction();// Start Transaction

                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                Models.Users entity = null;
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);


                if (input.ThirdPartyType == ThirdPartyType.Facebook)
                {
                    entity = _UsersRepo.Repository.Get(p => p.FacebookID == input.ThirdPartyId && p.RecordStatus != RecordStatus.Deleted.ToString());
                }


                string strThumbnailURLfordb = null;
                string strIamgeURLfordb = null;

                string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];
                string strThumbnailImage = input.ImageURL;

                if (entity != null)
                {
                    if (entity.RecordStatus != RecordStatus.Active.ToString())
                    {
                        _unitOfWork.RollBack();// End Transaction
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "User has blocked"));
                    }

                    entity.UserName = input.UserName != null && input.UserName != "" ? input.UserName : entity.UserName;

                    entity.DeviceType = input.DeviceType != null && input.DeviceType != "" ? input.DeviceType : entity.DeviceType;
                    entity.DeviceToken = input.DeviceToken != null && input.DeviceToken != "" ? input.DeviceToken : entity.DeviceToken;

                    entity.DeviceLat = input.DeviceLat;
                    entity.DeviceLong = input.DeviceLong;

                    entity.ThumbnailURL = "";
                    entity.ImageURL = "";

                    if (input.ImageURL != null && input.ImageURL != "")
                    {
                        try
                        {
                            string strTempImageSave = ResizeImage.Download_Image(input.ImageURL);

                            //---temp Image path
                            string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                            //---New Image path
                            string newFilePath = _SiteRoot + @"\" + "ProfileImage" + @"\";
                            Helper.CreateDirectories(_SiteRoot + @"\" + "ProfileImage" + @"\");

                            string thumbnailresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_Thumb_" + strTempImageSave, _Imagethumbsize, _Imagethumbsize);
                            strThumbnailURLfordb = _SiteURL + "/ProfileImage/" + thumbnailresizename;

                            string imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_Logo_" + strTempImageSave, _imageSize, _imageSize);
                            strIamgeURLfordb = _SiteURL + "/ProfileImage/" + imageresizename;

                            entity.ThumbnailURL = strThumbnailURLfordb;
                            entity.ImageURL = strIamgeURLfordb;
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    _UsersRepo.Repository.Update(entity);

                    _unitOfWork.Commit();// End Transaction
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(entity.UserID, false), "UserData"));
                }
                else
                {
                    entity = new Models.Users();

                    entity.UserName = input.UserName;
                    entity.Email = input.Email;

                    bool Email = false;
                    Email = Helper.IsValidEmail(input.Email);

                    if (Email == false)
                    {
                        _unitOfWork.RollBack();// End Transaction
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.Invalidemail, "UserData"));
                    }

                    entity.Password = "";

                    entity.DeviceType = input.DeviceType;
                    entity.DeviceToken = input.DeviceToken;

                    entity.DeviceLat = input.DeviceLat;
                    entity.DeviceLong = input.DeviceLong;
                    entity.CreatedDate = DateTime.Now;
                    entity.RecordStatus = RecordStatus.Active.ToString();

                    if (input.ThirdPartyType == ThirdPartyType.Facebook)
                    {
                        entity.FacebookID = input.ThirdPartyId;
                    }

                    if (input.ImageURL != null && input.ImageURL != "")
                    {
                        try
                        {
                            string strTempImageSave = ResizeImage.Download_Image(input.ImageURL);

                            //---temp Image path
                            string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                            //---New Image path
                            string newFilePath = _SiteRoot + @"\" + "ProfileImage" + @"\";
                            Helper.CreateDirectories(_SiteRoot + @"\" + "ProfileImage" + @"\");

                            string thumbnailresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_Thumb_" + strTempImageSave, _Imagethumbsize, _Imagethumbsize);
                            strThumbnailURLfordb = _SiteURL + "/ProfileImage/" + thumbnailresizename;

                            string imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_Logo_" + strTempImageSave, _imageSize, _imageSize);
                            strIamgeURLfordb = _SiteURL + "/ProfileImage/" + imageresizename;
                        }
                        catch (Exception ex)
                        {
                            strThumbnailURLfordb = strThumbnailImage;
                            strIamgeURLfordb = strThumbnailImage;
                        }
                    }

                    entity.ThumbnailURL = strThumbnailURLfordb;
                    entity.ImageURL = strIamgeURLfordb;

                    _UsersRepo.Repository.Add(entity);

                    SetUserDevices(entity);

                    GenericRepository<UserSettings> _UserSettingsEntity = new GenericRepository<UserSettings>(_unitOfWork);
                    UserSettings _UserSettings = new UserSettings();


                    _UserSettings.UserID = entity.UserID;
                    _UserSettings.SettingKey = EUserSettings.Musika.ToString();
                    _UserSettings.SettingValue = true;
                    _UserSettings.NotificationCount = 0;
                    _UserSettingsEntity.Repository.Add(_UserSettings);

                    _unitOfWork.Commit();// End Transaction

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(entity.UserID, true), "UserData"));
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                _unitOfWork.RollBack();// End Transaction
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "UserData"));
            }
        }

        #endregion

        #region "Fetch Events"
        [HttpGet]
        [Route("api/v2/TicketBooking/GetEventByID")]
        /* Test Url :: 23.111.138.246/api/v2/TicketBooking/GetEventByID?TourID=4407383&UserID=7749 */
        public async Task<HttpResponseMessage> GetEventByID(string TourID, int UserID)
        {
            try
            {
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                Models.Users _Users = null;

                _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "EventsDetail"));
                }

                string strThumbnailURLfordb = null;
                string strIamgeURLfordb = null;
                string strTempImageSave = null;

                string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];


                string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
                string _Instagram_access_token = ConfigurationManager.AppSettings["instagram.access_token"].ToString();

                string _Eventful_app_key = ConfigurationManager.AppSettings["Eventful_app_key"].ToString();

                string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();
                string _SeatGeek_client_secret = ConfigurationManager.AppSettings["SeatGeek_client_secret"].ToString();

                string _Spotify_Country = ConfigurationManager.AppSettings["Spotify_Country"].ToString();

                string _result;

                string _MusicGraph_ID = null;
                string _Performer_ID = null;
                string _strEvent = null;

                Models.Venue _VenuEntity = null;
                Models.TourDate _TourDateEntity = null;
                Models.ArtistRelated _ArtistRelatedEntity = null;
                Models.Artists _Artists = null;
                Models.TourPerformers _TourPerformers = null;

                Models.ArtistGenre _ArtistGenre = null;
                Models.Genre _Genre = null;

                string _MG_Artist_ID = null;

                _unitOfWork.StartTransaction();

                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<ArtistPhotos>(_unitOfWork);
                GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<TourPerformers> _TourPerformersRepo = new GenericRepository<TourPerformers>(_unitOfWork);

                GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(_unitOfWork);
                GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(_unitOfWork);


                var serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
                dynamic obj = null;


                //Check Tour Date in Local DB
                try
                {
                    Int64 _TourID = Convert.ToInt64(TourID);
                    _TourDateEntity = _TourDateRepo.Repository.Get(p => p.TourDateID == _TourID);
                }
                catch { }


                if (_TourDateEntity == null)
                {
                    _TourDateEntity = _TourDateRepo.Repository.Get(p => p.SeatGeek_TourID == TourID);
                }


                if (_TourDateEntity != null)
                {
                    TimeSpan span = DateTime.Now - Convert.ToDateTime(_TourDateEntity.ModifiedDate);
                    if (span.Hours < 24 && span.Days == 0)
                    {
                        _unitOfWork.RollBack();
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, GetTourDetail_ByID(_TourDateEntity, UserID), "EventsDetail"));
                    }
                }

                try
                {
                    if (_TourDateEntity == null)
                    {
                        #region "Get SeatGeek Event By ID"

                        var _Get_Events_ByID = await SeatGeek_GetEventByID_Asyn(_TourDateEntity == null ? TourID : _TourDateEntity.SeatGeek_TourID);//await for the function to be completed.

                        if (_Get_Events_ByID != null)
                        {
                            #region "Add Venu Information"

                            SeatGeek4.Venue _Venue = _Get_Events_ByID.venue;

                            _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                           where (A.SeatGeek_VenuID == _Venue.id.ToString())
                                           select A).FirstOrDefault();


                            //search the venu using fuzzy searching
                            if (_VenuEntity == null)
                            {
                                string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ&\s]";
                                var search = Regex.Replace(_Venue.name.ToLower(), pattern, "");

                                var venueByName = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 60 && x.RecordStatus == RecordStatus.Eventful.ToString() && x.VenueName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                                if (venueByName == null || venueByName.Count() == 0)
                                {
                                    var allEventfulVenues = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 80 && x.RecordStatus == RecordStatus.Eventful.ToString()).ToList();
                                    foreach (var a in allEventfulVenues)
                                    {
                                        var eventfulVenue = Regex.Replace(a.VenueName, pattern, "");
                                        if (eventfulVenue.ToLower() == search.ToLower())
                                        {
                                            _VenuEntity = a;
                                            break;
                                        }
                                        if (search.Length > 11 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(2, search.Length - 2)))
                                        {
                                            _VenuEntity = a;
                                            break;
                                        }
                                        if (search.Length > 20 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(5, search.Length - 5)))
                                        {
                                            _VenuEntity = a;
                                            break;
                                        }
                                        if (search.Length > 40 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(10, search.Length - 10)))
                                        {
                                            _VenuEntity = a;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    _VenuEntity = venueByName.First();
                                }

                            }

                            if (_VenuEntity == null)
                            {
                                _VenuEntity = new Venue();
                                _VenuEntity.SeatGeek_VenuID = _Venue.id.ToString();
                                _VenuEntity.VenueName = _Venue.name;
                                _VenuEntity.Extended_Address = _Venue.extended_address;
                                _VenuEntity.VenueCountry = _Venue.country;
                                _VenuEntity.Display_Location = _Venue.display_location;
                                _VenuEntity.Slug = _Venue.slug;
                                _VenuEntity.VenueState = _Venue.state;
                                _VenuEntity.Postal_Code = _Venue.postal_code;
                                _VenuEntity.VenueCity = _Venue.city;
                                _VenuEntity.Address = _Venue.address;
                                _VenuEntity.Timezone = _Venue.timezone;

                                if (_Venue.location != null)
                                {
                                    _VenuEntity.VenueLat = _Venue.location.lat;
                                    _VenuEntity.VenueLong = _Venue.location.lon;
                                }



                                _VenuEntity.CreatedDate = DateTime.Now;
                                _VenuEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                _VenueRepo.Repository.Add(_VenuEntity);

                            }
                            #endregion


                            //loop through to get Artist Information
                            #region "Get Artists/Instagram Information"
                            List<SeatGeek4.Performer> _Performers = new List<SeatGeek4.Performer>();
                            SeatGeek4.Performer _PerformersChk = null;

                            _Performers = _Get_Events_ByID.performers.Where(p => p.type != "theater" && p.type == "band").ToList();
                            _Performers = _Performers.OrderByDescending(p => p.primary).ToList();
                            _PerformersChk = _Performers.Where(p => p.primary == true).FirstOrDefault();



                            foreach (SeatGeek4.Performer _perfomer in _Performers)
                            {
                                _MusicGraph_ID = null;

                                #region "Get Artist Info Using Name  (MusicGraph)"
                                try
                                {
                                    var _Search_ByName = await Spotify_SearchArtist(_perfomer.name.Trim());//await for the function to be completed


                                    if (_Search_ByName != null)
                                    {
                                        foreach (MusicGraph.Datum _Datum in _Search_ByName)
                                        {


                                            if (RemoveDiacritics(_Datum.name.ToLower()) == _perfomer.name.ToLower())
                                            {
                                                _MusicGraph_ID = _Datum.id;
                                                _Artists = _ArtistsRepo.Repository.Get(p => p.Spotify_ID == _MusicGraph_ID);



                                                #region "Add New"
                                                if (_Artists == null)
                                                {
                                                    bool isLatin = false;

                                                    if (!isLatin)
                                                        isLatin = CheckSeatGeekLatin(_Datum.name, _unitOfWork);
                                                    if (!isLatin)
                                                        isLatin = CheckLastResortSpotifyGenre(_Datum.spotify_id);


                                                    if (!isLatin)
                                                    {
                                                        _unitOfWork.RollBack();
                                                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "Its not a latin artist event", "EventsDetail"));
                                                    }

                                                    if (isLatin)
                                                    {
                                                        _Artists = new Artists();

                                                        _Artists.ArtistName = _Datum.name; // Regex.Replace(_Datum.name, "[^A-Za-z0-9 _]", "");

                                                        _Artists.Main_Genre = _Datum.main_genre;

                                                        _Artists.Seatgeek_ID = _perfomer.id.ToString();

                                                        _Artists.Spotify_ID = !String.IsNullOrEmpty(_Datum.id) ? _Datum.id : _Datum.name;
                                                        _Artists.RecordStatus = RecordStatus.MusicGraph.ToString();
                                                        _Artists.CreatedDate = DateTime.Now;
                                                        _Artists.ModifiedDate = DateTime.Now;

                                                        _Artists.OnTour = _perfomer.has_upcoming_events == true ? true : false;

                                                        _ArtistsRepo.Repository.Add(_Artists);

                                                        GetProfileImageFromSpotifyFeed(_Artists);

                                                        #region "Loop through the Genre"
                                                        if (_perfomer.genres != null && _perfomer.genres.Count > 0)
                                                        {
                                                            foreach (SeatGeek4.Genre _Ev in _perfomer.genres)
                                                            {
                                                                _Genre = _GenreRepo.Repository.Get(p => p.Name == _Ev.name.Trim());

                                                                if (_Genre == null)
                                                                {
                                                                    _Genre = new Genre();
                                                                    _Genre.Name = _Ev.name;
                                                                    _Genre.CreatedDate = DateTime.Now;
                                                                    _Genre.RecordStatus = RecordStatus.SeatGeek.ToString();
                                                                    _GenreRepo.Repository.Add(_Genre);

                                                                }

                                                                var check = !_ArtistGenreRepo.Repository.AsQueryable().Any(x => x.ArtistID == _Artists.ArtistID && x.GenreID == _Genre.GenreID);

                                                                if (check)
                                                                {
                                                                    _ArtistGenre = new ArtistGenre();
                                                                    _ArtistGenre.ArtistID = _Artists.ArtistID;
                                                                    _ArtistGenre.Slug = _Ev.slug;
                                                                    _ArtistGenre.Primary = _Ev.primary;
                                                                    _ArtistGenre.Name = _Ev.name;
                                                                    _ArtistGenre.CreatedDate = DateTime.Now;
                                                                    _ArtistGenre.RecordStatus = RecordStatus.SeatGeek.ToString();

                                                                    _ArtistGenreRepo.Repository.Add(_ArtistGenre);
                                                                }
                                                            }
                                                        }
                                                        #endregion

                                                        await Task.Factory.StartNew(() =>
                                                        {
                                                            if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                                                            {
                                                                #region "Get Artist Picture "
                                                                try
                                                                {
                                                                    if (_perfomer.images != null)
                                                                    {
                                                                        if (_perfomer.images.huge != null)
                                                                        {
                                                                            bool _ArtistPic = ArtistProfilePicture_Asyn(UserID, _Artists, _unitOfWork, true, _perfomer.images.huge);
                                                                        }
                                                                    }
                                                                }
                                                                catch (Exception ex)
                                                                {

                                                                }
                                                                #endregion

                                                            }

                                                            #region "Get Similar Artists (dont need this block while just updating the records)"
                                                            Task<List<MusicGraph.Datum>> _GetSimilarArtists_ByID = Spotify_GetSimilarArtistByID(_Artists.Spotify_ID);
                                                            #endregion

                                                            _GetSimilarArtists_ByID.Wait();


                                                            #region "Eventful API Implementation"
                                                            Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(UserID, _Artists, _unitOfWork, true);
                                                            #endregion



                                                            #region "Instagram Api Implementation"
                                                            //Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, _ArtistMatrics_ByID.Result, _unitOfWork, true, _TourDateEntity);
                                                            #endregion

                                                            #region "Spotify Api Implementation
                                                            Task<Dictionary<string, object>> _Spotify = Spotify_GetSongInfo_Asyn(UserID, _Artists, _unitOfWork, true);
                                                            #endregion

                                                        });
                                                    }
                                                }
                                                #endregion

                                                #region "Update Existing"
                                                else
                                                {
                                                    _Artists.ModifiedDate = DateTime.Now;
                                                    _Artists.OnTour = _perfomer.has_upcoming_events == true ? true : false;
                                                    await Task.Factory.StartNew(() =>
                                                    {
                                                        if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                                                        {
                                                            #region "Get Artist Picture "
                                                            try
                                                            {
                                                                if (_perfomer.images != null)
                                                                {
                                                                    if (_perfomer.images.huge != null)
                                                                    {
                                                                        bool _ArtistPic = ArtistProfilePicture_Asyn(UserID, _Artists, _unitOfWork, true, _perfomer.images.huge);
                                                                    }
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {

                                                            }
                                                            #endregion
                                                        }

                                                        #region "Instagram Api Implementation"
                                                        Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, null, _unitOfWork, false, _TourDateEntity);
                                                        #endregion

                                                        #region "Eventful API Implementation"
                                                        Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(UserID, _Artists, _unitOfWork, false);
                                                        #endregion

                                                        #region "Spotify Api Implementation
                                                        Task<Dictionary<string, object>> _Spotify = Spotify_GetSongInfo_Asyn(UserID, _Artists, _unitOfWork, false);
                                                        #endregion
                                                    });
                                                }
                                                #endregion

                                                if (_perfomer.primary == true || _PerformersChk == null)
                                                {
                                                    _PerformersChk = _perfomer;
                                                    #region "Add/Update Tour Information"
                                                    //Entering Tour records

                                                    DateTime _datetime_local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);

                                                    _TourDateEntity = (from A in _VenueRepo.Repository.GetAll(p => p.VenueID == _VenuEntity.VenueID)
                                                                       join B in _TourDateRepo.Repository.GetAll(p =>
                                                                                                       (p.SeatGeek_TourID == _Get_Events_ByID.id.ToString() && p.ArtistID == _Artists.ArtistID)
                                                                                                    || (DbFunctions.TruncateTime(p.Datetime_Local).Value.Month == _datetime_local.Month
                                                                                                        && DbFunctions.TruncateTime(p.Datetime_Local).Value.Year == _datetime_local.Year
                                                                                                        && DbFunctions.TruncateTime(p.Datetime_Local).Value.Day == _datetime_local.Day
                                                                                                        && p.ArtistID == _Artists.ArtistID
                                                                                                        && p.RecordStatus == RecordStatus.Eventful.ToString())
                                                                                                    ) on A.VenueID equals B.VenueID
                                                                       where B.ArtistID == _Artists.ArtistID
                                                                       select B).FirstOrDefault();


                                                    if (_TourDateEntity == null)
                                                    {
                                                        _TourDateEntity = new TourDate();

                                                        _TourDateEntity.SeatGeek_TourID = _Get_Events_ByID.id.ToString();
                                                        _TourDateEntity.ArtistID = _Artists.ArtistID;
                                                        _TourDateEntity.VenueID = _VenuEntity.VenueID;

                                                        _TourDateEntity.EventID = null;
                                                        _TourDateEntity.Score = _Get_Events_ByID.score;

                                                        _TourDateEntity.Announce_Date = Convert.ToDateTime(_Get_Events_ByID.announce_date);
                                                        _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Events_ByID.visible_until_utc);
                                                        _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Events_ByID.datetime_utc);
                                                        _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);

                                                        _TourDateEntity.CreatedDate = DateTime.Now;
                                                        _TourDateEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                                        if (!String.IsNullOrEmpty(_Get_Events_ByID.url))
                                                            _TourDateEntity.TicketURL = _Get_Events_ByID.url;
                                                        else if (_Get_Events_ByID.venue != null && !String.IsNullOrEmpty(_Get_Events_ByID.venue.url))
                                                            _TourDateEntity.TicketURL = _Get_Events_ByID.venue.url;
                                                        else
                                                            _TourDateEntity.TicketURL = "https://seatgeek.com/";


                                                        _TourDateRepo.Repository.Add(_TourDateEntity);
                                                    }
                                                    else
                                                    {

                                                        _TourDateEntity.SeatGeek_TourID = _Get_Events_ByID.id.ToString();
                                                        _TourDateEntity.ArtistID = _Artists.ArtistID;
                                                        _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                                        _TourDateEntity.Score = _Get_Events_ByID.score;

                                                        _TourDateEntity.Announce_Date = Convert.ToDateTime(_Get_Events_ByID.announce_date);
                                                        _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Events_ByID.visible_until_utc);
                                                        _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Events_ByID.datetime_utc);

                                                        _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);

                                                        _TourDateEntity.ModifiedDate = DateTime.Now;
                                                        _TourDateEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                                        if (!String.IsNullOrEmpty(_Get_Events_ByID.url))
                                                            _TourDateEntity.TicketURL = _Get_Events_ByID.url;
                                                        else if (_Get_Events_ByID.venue != null && !String.IsNullOrEmpty(_Get_Events_ByID.venue.url))
                                                            _TourDateEntity.TicketURL = _Get_Events_ByID.venue.url;
                                                        else
                                                            _TourDateEntity.TicketURL = "https://seatgeek.com/";

                                                        _TourDateRepo.Repository.Update(_TourDateEntity);

                                                    }
                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region "Save Other Artist in Tour Performer"
                                                    _TourPerformers = _TourPerformersRepo.Repository.Get(p => p.ArtistID == _Artists.ArtistID && p.TourDateID == _TourDateEntity.TourDateID);

                                                    if (_TourPerformers == null)
                                                    {
                                                        _TourPerformers = new TourPerformers();

                                                        _TourPerformers.TourDateID = _TourDateEntity.TourDateID;
                                                        _TourPerformers.ArtistID = _Artists.ArtistID;
                                                        _TourPerformers.CreatedDate = DateTime.Now;
                                                        _TourPerformers.RecordStatus = RecordStatus.MusicGraph.ToString();

                                                        _TourPerformersRepo.Repository.Add(_TourPerformers);
                                                    }
                                                    #endregion
                                                }

                                                _ArtistsRepo.Repository.Update(_Artists);

                                                break;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                }



                                #endregion

                                //if Artist not found in Music Graph (Save Direct)
                                if (_MusicGraph_ID == null)
                                {
                                    #region "use this block if Artist not found in (MusicGrapgh)"
                                    try
                                    {
                                        _Artists = _ArtistsRepo.Repository.Get(p => p.Seatgeek_ID == _perfomer.id.ToString());

                                        #region "Add New"
                                        if (_Artists == null)
                                        {
                                            bool isLatin = false;

                                            if (!isLatin)
                                                isLatin = CheckSeatGeekLatin(_perfomer.name, _unitOfWork);

                                            if (isLatin)
                                            {

                                                _Artists = new Artists();

                                                _Artists.ArtistName = _perfomer.name;//  Regex.Replace(_perfomer.name, "[^A-Za-z0-9 _]", "");
                                                _Artists.Gender = null;
                                                _Artists.Decade = null;

                                                if (_perfomer.genres != null)
                                                {
                                                    _Artists.Main_Genre = _perfomer.genres[0].name;
                                                }

                                                _Artists.Seatgeek_ID = _perfomer.id.ToString();

                                                _Artists.Musicgraph_ID = _perfomer.name;
                                                _Artists.Artist_Ref_ID = null;
                                                _Artists.Musicbrainz_ID = null;
                                                _Artists.Spotify_ID = null;
                                                _Artists.Youtube_ID = null;
                                                _Artists.Alternate_Names = null;

                                                _Artists.RecordStatus = RecordStatus.SeatGeek.ToString();
                                                _Artists.CreatedDate = DateTime.Now;
                                                _Artists.ModifiedDate = DateTime.Now;

                                                _Artists.OnTour = _perfomer.has_upcoming_events == true ? true : false;

                                                _ArtistsRepo.Repository.Add(_Artists);

                                                GetProfileImageFromSpotifyFeed(_Artists);

                                                _unitOfWork.Commit();
                                                _unitOfWork.StartTransaction();

                                                #region "Loop through the Genre"
                                                if (_perfomer.genres != null && _perfomer.genres.Count > 0)
                                                {
                                                    foreach (SeatGeek4.Genre _Ev in _perfomer.genres)
                                                    {
                                                        _Genre = _GenreRepo.Repository.Get(p => p.Name == _Ev.name.Trim());

                                                        if (_Genre == null)
                                                        {
                                                            _Genre = new Genre();
                                                            _Genre.Name = _Ev.name;
                                                            _Genre.CreatedDate = DateTime.Now;
                                                            _Genre.RecordStatus = RecordStatus.SeatGeek.ToString();
                                                            _GenreRepo.Repository.Add(_Genre);

                                                        }

                                                        var check = !_ArtistGenreRepo.Repository.AsQueryable().Any(x => x.ArtistID == _Artists.ArtistID && x.GenreID == _Genre.GenreID);

                                                        if (check)
                                                        {
                                                            _ArtistGenre = new ArtistGenre();
                                                            _ArtistGenre.GenreID = _Genre.GenreID;
                                                            _ArtistGenre.ArtistID = _Artists.ArtistID;
                                                            _ArtistGenre.Slug = _Ev.slug;
                                                            _ArtistGenre.Primary = _Ev.primary;
                                                            _ArtistGenre.Name = _Ev.name;
                                                            _ArtistGenre.CreatedDate = DateTime.Now;
                                                            _ArtistGenre.RecordStatus = RecordStatus.SeatGeek.ToString();

                                                            _ArtistGenreRepo.Repository.Add(_ArtistGenre);
                                                        }
                                                    }
                                                }
                                                #endregion

                                                await Task.Factory.StartNew(() =>
                                                {
                                                    if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                                                    {
                                                        #region "Get Artist Picture "
                                                        try
                                                        {
                                                            if (_perfomer.images != null)
                                                            {
                                                                if (_perfomer.images.huge != null)
                                                                {
                                                                    bool _ArtistPic = ArtistProfilePicture_Asyn(UserID, _Artists, _unitOfWork, true, _perfomer.images.huge);
                                                                }
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }
                                                        #endregion
                                                    }

                                                    #region "Instagram Api Implementation"
                                                    Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, null, _unitOfWork, true, _TourDateEntity);
                                                    #endregion

                                                    #region "Eventful API Implementation"
                                                    Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(UserID, _Artists, _unitOfWork, true);
                                                    #endregion

                                                });
                                            }
                                        }
                                        #endregion

                                        #region "Update Existing"
                                        else
                                        {
                                            _Artists.ModifiedDate = DateTime.Now;
                                            _Artists.OnTour = _perfomer.has_upcoming_events == true ? true : false;

                                            await Task.Factory.StartNew(() =>
                                            {
                                                if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                                                {
                                                    #region "Get Artist Picture "
                                                    try
                                                    {
                                                        if (_perfomer.images != null)
                                                        {
                                                            if (_perfomer.images.huge != null)
                                                            {
                                                                bool _ArtistPic = ArtistProfilePicture_Asyn(UserID, _Artists, _unitOfWork, true, _perfomer.images.huge);
                                                            }
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }
                                                    #endregion

                                                }
                                                #region "Instagram Api Implementation"
                                                Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, null, _unitOfWork, false, _TourDateEntity);
                                                #endregion

                                                #region "Eventful API Implementation"
                                                Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(UserID, _Artists, _unitOfWork, false);
                                                #endregion
                                            });
                                        }
                                        #endregion

                                        if (_perfomer.primary == true || _PerformersChk == null)
                                        {
                                            _PerformersChk = _perfomer;
                                            #region "Add/Update Tour Information"
                                            //Entering Tour records
                                            //_TourDateEntity = _TourDateRepo.Repository.Get(p => p.SeatGeek_TourID == _Get_Events_ByID.id.ToString() && p.ArtistID == _Artists.ArtistID);
                                            DateTime _datetime_local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);

                                            _TourDateEntity = (from A in _VenueRepo.Repository.GetAll(p => p.VenueID == _VenuEntity.VenueID)
                                                               join B in _TourDateRepo.Repository.GetAll(p =>
                                                                                               (p.SeatGeek_TourID == _Get_Events_ByID.id.ToString() && p.ArtistID == _Artists.ArtistID)
                                                                                            || (DbFunctions.TruncateTime(p.Datetime_Local).Value.Month == _datetime_local.Month
                                                                                                && DbFunctions.TruncateTime(p.Datetime_Local).Value.Year == _datetime_local.Year
                                                                                                && DbFunctions.TruncateTime(p.Datetime_Local).Value.Day == _datetime_local.Day)
                                                                                                && p.ArtistID == _Artists.ArtistID
                                                                                                && p.RecordStatus == RecordStatus.Eventful.ToString()
                                                                                            ) on A.VenueID equals B.VenueID
                                                               select B).FirstOrDefault();

                                            if (_TourDateEntity == null)
                                            {
                                                _TourDateEntity = new TourDate();

                                                _TourDateEntity.SeatGeek_TourID = _Get_Events_ByID.id.ToString();
                                                _TourDateEntity.ArtistID = _Artists.ArtistID;
                                                _TourDateEntity.VenueID = _VenuEntity.VenueID;

                                                _TourDateEntity.EventID = null;
                                                _TourDateEntity.Score = _Get_Events_ByID.score;

                                                _TourDateEntity.Announce_Date = Convert.ToDateTime(_Get_Events_ByID.announce_date);
                                                _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Events_ByID.visible_until_utc);
                                                _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Events_ByID.datetime_utc);
                                                _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);

                                                _TourDateEntity.CreatedDate = DateTime.Now;
                                                _TourDateEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                                if (!String.IsNullOrEmpty(_Get_Events_ByID.url))
                                                    _TourDateEntity.TicketURL = _Get_Events_ByID.url;
                                                else if (_Get_Events_ByID.venue != null && !String.IsNullOrEmpty(_Get_Events_ByID.venue.url))
                                                    _TourDateEntity.TicketURL = _Get_Events_ByID.venue.url;
                                                else
                                                    _TourDateEntity.TicketURL = "https://seatgeek.com/";

                                                _TourDateRepo.Repository.Add(_TourDateEntity);
                                            }
                                            else
                                            {

                                                _TourDateEntity.SeatGeek_TourID = _Get_Events_ByID.id.ToString();
                                                _TourDateEntity.ArtistID = _Artists.ArtistID;
                                                _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                                _TourDateEntity.Score = _Get_Events_ByID.score;

                                                _TourDateEntity.Announce_Date = Convert.ToDateTime(_Get_Events_ByID.announce_date);
                                                _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Events_ByID.visible_until_utc);
                                                _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Events_ByID.datetime_utc);

                                                _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);

                                                _TourDateEntity.ModifiedDate = DateTime.Now;

                                                if (!String.IsNullOrEmpty(_Get_Events_ByID.url))
                                                    _TourDateEntity.TicketURL = _Get_Events_ByID.url;
                                                else if (_Get_Events_ByID.venue != null && !String.IsNullOrEmpty(_Get_Events_ByID.venue.url))
                                                    _TourDateEntity.TicketURL = _Get_Events_ByID.venue.url;
                                                else
                                                    _TourDateEntity.TicketURL = "https://seatgeek.com/";

                                                _TourDateRepo.Repository.Update(_TourDateEntity);

                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region "Save Other Artist in Tour Performer"
                                            _TourPerformers = _TourPerformersRepo.Repository.Get(p => p.ArtistID == _Artists.ArtistID && p.TourDateID == _TourDateEntity.TourDateID);

                                            if (_TourPerformers == null)
                                            {
                                                _TourPerformers = new TourPerformers();

                                                _TourPerformers.TourDateID = _TourDateEntity.TourDateID;
                                                _TourPerformers.ArtistID = _Artists.ArtistID;
                                                _TourPerformers.CreatedDate = DateTime.Now;
                                                _TourPerformers.RecordStatus = RecordStatus.MusicGraph.ToString();

                                                _TourPerformersRepo.Repository.Add(_TourPerformers);
                                            }
                                            #endregion
                                        }

                                        _ArtistsRepo.Repository.Update(_Artists);

                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }
                        #endregion
                    }
                    else
                    {

                        await Task.Factory.StartNew(() =>
                        {
                            #region "Instagram Api Implementation"
                            Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, null, _unitOfWork, false, _TourDateEntity);
                            #endregion

                            _Instagram.Wait();
                        });

                        #region "EventFul"
                        if (_TourDateEntity.Eventful_TourID != null)
                        {

                        }
                        #endregion

                        #region "SeetGeek"
                        if (_TourDateEntity.SeatGeek_TourID != null)
                        {

                        }
                        #endregion
                    }

                    _unitOfWork.Commit();

                    if (_TourDateEntity != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, GetTourDetail_ByID(_TourDateEntity, UserID), "EventsDetail"));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "Event not found", "EventsDetail"));
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "EventsDetail"));
                }
                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.InvalidSearchCriteria, "EventsDetail"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "EventsDetail"));
            }
        }


        public ViewEventDetail GetTourDetail_ByID(TourDate _TourDate, int UserID)
        {
            ViewEventDetail _ViewEventDetail = new ViewEventDetail();

            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
            GenericRepository<ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<ArtistPhotos>(_unitOfWork);
            GenericRepository<TourPhoto> _TourPhotosRepo = new GenericRepository<TourPhoto>(_unitOfWork);
            GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(_unitOfWork);
            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
            GenericRepository<UserTourDate> UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);
            GenericRepository<UserFriends> _UserFriendsRepo = new GenericRepository<UserFriends>(_unitOfWork);
            GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
            GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
            GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

            Models.UserTourDate _UserTourDate = null;
            Models.Artists _Artist = null;

            var _users = _UsersRepo.Repository.GetById(UserID);

            _Artist = _ArtistsRepo.Repository.Get(p => p.ArtistID == _TourDate.ArtistID);

            _ViewEventDetail.TourID = _TourDate.TourDateID;
            _ViewEventDetail.ArtistID = _Artist.ArtistID;
            _ViewEventDetail.ArtistName = _Artist.ArtistName;

            _ViewEventDetail.About = _Artist.AboutES != null && _users.UserLanguage == EUserLanguage.ES.ToString() ? _Artist.AboutES : _Artist.About;

            if (_ViewEventDetail.About == null)
                _ViewEventDetail.About = "";

            _ViewEventDetail.ImageURL = _Artist.ImageURL ?? "";
            _ViewEventDetail.BannerImage_URL = _Artist.ImageURL ?? "";

            _ViewEventDetail.OnTour = _Artist.OnTour;
            _ViewEventDetail.Gender = _Artist.Gender ?? "";
            _ViewEventDetail.Main_Genre = _Artist.Main_Genre ?? "";
            _ViewEventDetail.Decade = _Artist.Decade ?? "";
            _ViewEventDetail.Alternate_Names = _Artist.Alternate_Names ?? "";
            _ViewEventDetail.CreatedDate = _Artist.CreatedDate;
            _ViewEventDetail.ModifiedDate = _Artist.ModifiedDate;
            _ViewEventDetail.RecordStatus = _Artist.RecordStatus;
            _ViewEventDetail.Instagram_Tag = _Artist.Instagram_Tag != null && _Artist.Instagram_Tag != "" ? _Artist.Instagram_Tag : _Artist.ArtistName;


            _UserTourDate = _UserTourDateRepo.Repository.Get(p => p.TourDateID == _TourDate.TourDateID && p.UserID == UserID && p.RecordStatus != RecordStatus.Deleted.ToString());

            if (_UserTourDate == null)
            {
                _ViewEventDetail.IsTracking = false;
            }
            else
            {
                _ViewEventDetail.IsTracking = true;
            }

            _ViewEventDetail.Date_Local = Convert.ToDateTime(_TourDate.Datetime_Local).ToString("d");
            _ViewEventDetail.Time_Local = Convert.ToDateTime(_TourDate.Datetime_Local).ToString("t");

            _ViewEventDetail.TicketURL = _TourDate.TicketURL;

            Models.UserGoing _UserGoing = null;
            _UserGoing = _UserGoingRepo.Repository.Get(p => p.TourDateID == _TourDate.TourDateID && p.UserID == UserID);

            if (_UserGoing != null)
            {
                _ViewEventDetail.IsGoing = _UserGoing.RecordStatus == EUserGoing.Going.ToString() ? true : false;
            }
            else
            {
                _ViewEventDetail.IsGoing = false;
            }

            Models.Venue _Venue = null;
            _Venue = _VenueRepo.Repository.Get(p => p.VenueID == _TourDate.VenueID);

            ViewEventVenue _ViewEventVenue = new ViewEventVenue();
            if (_Venue != null)
            {
                _ViewEventVenue.Address = _Venue.Address ?? "";
                _ViewEventVenue.VenueID = _Venue.VenueID;
                _ViewEventVenue.VenueName = _Venue.VenueName ?? "";
                _ViewEventVenue.ImageURL = _Venue.ImageURL ?? "";
                _ViewEventVenue.Extended_Address = _Venue.Extended_Address ?? "";
                _ViewEventVenue.Display_Location = _Venue.Display_Location ?? "";
                _ViewEventVenue.Slug = _Venue.Slug ?? "";
                _ViewEventVenue.Postal_Code = _Venue.Postal_Code ?? "";
                _ViewEventVenue.Address = _Venue.Address ?? "";
                _ViewEventVenue.Timezone = _Venue.Timezone ?? "";
                _ViewEventVenue.VenueCity = _Venue.VenueCity ?? "";
                _ViewEventVenue.VenueState = _Venue.VenueState ?? "";
                _ViewEventVenue.VenueCountry = _Venue.VenueCountry ?? "";
                _ViewEventVenue.VenueLat = _Venue.VenueLat ?? 0;
                _ViewEventVenue.VenueLong = _Venue.VenueLong ?? 0;
            }
            _ViewEventDetail.Venue = _ViewEventVenue;

            if (!String.IsNullOrEmpty(_TourDate.HashTag))
            {
                _ViewEventDetail.TourPhotos = (from A in _TourPhotosRepo.Repository.GetAll(p => p.TourDateId == _TourDate.TourDateID)
                                               select new viewEventTourPhoto
                                               {
                                                   ImageURL = A.ImageURL,
                                                   ImageThumbnailURL = A.ImageThumbnailURL,
                                                   HashTagName = _TourDate.HashTag,
                                                   PhotoID = A.TourPhotoID
                                               }).OrderByDescending(p => p.PhotoID).ToList();
            }
            else
            {
                //Get HashTags pictures
                _ViewEventDetail.TourPhotos = (from A in _ArtistPhotosRepo.Repository.GetAll(p => p.ArtistID == _Artist.ArtistID && p.RecordStatus == RecordStatus.Active.ToString())
                                               select new viewEventTourPhoto
                                               {
                                                   ImageURL = A.ImageUrl,
                                                   ImageThumbnailURL = A.ImageThumbnailUrl,
                                                   HashTagName = A.HashTagName ?? "",
                                                   PhotoID = A.PhotoID
                                               }).OrderByDescending(p => p.PhotoID).ToList();
            }

            var _lst = (from A1 in _UserFriendsRepo.Repository.GetAll(p => p.UserID == UserID && p.Matched_UserID != null)
                        join A in _UserGoingRepo.Repository.GetAll(p => p.TourDateID == _TourDate.TourDateID && p.RecordStatus == EUserGoing.Going.ToString()) on A1.Matched_UserID equals A.UserID
                        join B in _UsersRepo.Repository.GetAll() on A.UserID equals B.UserID
                        select new ViewEventUsers
                        {
                            ThumbnailURL = B.ThumbnailURL ?? "",
                            UserID = A.UserID.Value,
                            UserName = B.UserName.ToString(),
                            Email = B.Email ?? "",
                            CreatedDate = Convert.ToDateTime(A.CreatedDate).ToString("d")
                        }).OrderByDescending(p => p.CreatedDate).ToList();



            //who else is going
            _ViewEventDetail.UsersGoing = _lst.ToList();

            _ViewEventDetail.NoOfUserGoing = _lst.Count();


            //Get Similar Artists
            _ViewEventDetail.ArtistRelated = (from A in _ArtistRelatedRepo.Repository.GetAll(p => p.ArtistID == _Artist.ArtistID)
                                              select new viewEventRelated
                                              {
                                                  Musicgraph_ID = A.Musicgraph_ID,
                                                  RelatedArtistName = A.RelatedArtistName,
                                                  Similarity = A.Similarity
                                              }).ToList();

            return _ViewEventDetail;
        }

        private Task<SeatGeek4.Get_Events_ByID> SeatGeek_GetEventByID_Asyn(string vSeatGeekID)
        {

            HttpWebRequest httpWebRequest;
            HttpWebResponse httpResponse;
            string _result;
            string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();

            LogHelper.CreateLog("SeatGeek_GetEventByID_Asyn (" + vSeatGeekID + ")");

            return Task.Factory.StartNew(() =>
            {
                try
                {
                    string seatid = vSeatGeekID;
                    if (String.IsNullOrEmpty(vSeatGeekID))
                        seatid = "0";

                    httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.seatgeek.com/2/events/" + seatid + SGAuth());
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";
                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }

                    // deserializing 
                    return JsonConvert.DeserializeObject<SeatGeek4.Get_Events_ByID>(_result);

                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    LogHelper.CreateLog("SeatGeek_GetEventByID_Asyn " + ex.Message);
                    return null;
                }
            }

                );
        }

        private async Task<List<MusicGraph.Datum>> Spotify_SearchArtist(string _q)
        {
            try
            {
                List<MusicGraph.Datum> _artistlst = new List<MusicGraph.Datum>();

                string _Spotify_Country = ConfigurationManager.AppSettings["Spotify_Country"].ToString();

                Page<Artist> _lst = await Artist.Search(_q);


                foreach (var item in _lst.Items)
                {

                    string _Genre = "";
                    bool _Islatin = false;

                    if (item.Genres != null && item.Genres.Count > 0)
                    {
                        _Genre = item.Genres[0];

                        foreach (var _gen in item.Genres)
                        {
                            if (_GenreFilter.Any(x => x.ToLower() == _gen.ToString()))
                            {
                                _Islatin = true;
                                break;
                            }
                        }
                    }

                    _artistlst.Add(new MusicGraph.Datum
                    {
                        id = item.Id,
                        name = item.Name,
                        main_genre = _Genre,
                        IsLatin = _Islatin
                    });
                }

                return _artistlst;
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog(ex);
                return null;
            }

        }

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

        public bool CheckSeatGeekLatin(string artistName, IUnitOfWork vUnitOfWork)
        {
            bool isLatin = false;

            try
            {
                GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(vUnitOfWork);

                if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == artistName))
                {

                    var serializer = new JavaScriptSerializer();
                    serializer.RegisterConverters(new[] { new DynamicJsonConverter() });

                    GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(vUnitOfWork);
                    GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(vUnitOfWork);
                    GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(vUnitOfWork);

                    GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(vUnitOfWork);
                    GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(vUnitOfWork);
                    GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(vUnitOfWork);

                    string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();

                    string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                    string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                    string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];

                    HttpWebRequest httpWebRequest;
                    HttpWebResponse httpResponse;
                    string _result;
                    SeatGeek.Get_Performers _Get_Performers = null;

                    string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ\s]";
                    string cleanName = Regex.Replace(artistName, pattern, "");

                    httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.seatgeek.com/2/performers" + SGAuth() + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&q=" + cleanName.Trim());
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }

                    // deserializing 
                    _Get_Performers = JsonConvert.DeserializeObject<SeatGeek.Get_Performers>(_result);
                    SeatGeek.Performer _Performer = null;

                    if (_Get_Performers != null)
                    {
                        if (_Get_Performers.performers.Count > 0)
                        {
                            _Performer = (from A in _Get_Performers.performers
                                          where A.name == artistName.Trim()
                                          select A).OrderByDescending(p => p.score).FirstOrDefault();

                            if (_Performer == null)
                            {
                                _Performer = (from A in _Get_Performers.performers select A).OrderByDescending(p => p.score).FirstOrDefault();

                            }

                            #region "Loop through the Genre"
                            if (_Performer.genres != null && _Performer.genres.Count > 0)
                            {
                                foreach (SeatGeek.Genre _Ev in _Performer.genres)
                                {
                                    if (_GenreFilter.Contains(_Ev.name.Trim()))
                                        isLatin = true;

                                }
                            }
                            #endregion

                        }

                    }
                }
            }
            catch (Exception e)
            { }


            return isLatin;
        }

        private bool CheckLastResortSpotifyGenre(string spotifyID)
        {
            try
            {
                if (!String.IsNullOrEmpty(spotifyID))
                {
                    string spotifyUrl = "https://api.spotify.com/v1/artists/" + spotifyID;
                    HttpWebRequest httpWebRequest;
                    HttpWebResponse httpResponse;
                    string _result;
                    string imageUrl = "";

                    httpWebRequest = (HttpWebRequest)WebRequest.Create(spotifyUrl);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";
                    httpWebRequest.Headers.Add("Authorization", "Bearer " + RetSpotifyAccessToken());

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }

                    JObject joResponse = JObject.Parse(_result);
                    JArray items = (JArray)joResponse["genres"];

                    if (items != null && items.Count > 0)
                    {
                        foreach (var i in items)
                        {
                            if (_GenreFilter.Any(x => x.ToLower() == i.ToString()))
                                return true;
                        }
                    }
                    else
                        return false;

                }

                return false;
            }
            catch (Exception e)
            { return false; }
        }

        private async Task<string> GetProfileImageFromSpotifyFeed(Artists artist)
        {
            GenericRepository<Artists> _ArtistRepository = new GenericRepository<Artists>(_unitOfWork);
            string _response = string.Empty;
            _response = await Task.Factory.StartNew(() =>
            {
                string imageUrl = "";
                try
                {

                    if ((!String.IsNullOrEmpty(artist.Spotify_ID) && (String.IsNullOrEmpty(artist.ImageURL) || String.IsNullOrEmpty(artist.BannerImage_URL) || String.IsNullOrEmpty(artist.ThumbnailURL))))
                    {
                        string spotifyUrl = "https://api.spotify.com/v1/artists/" + artist.Spotify_ID;
                        HttpWebRequest httpWebRequest;
                        HttpWebResponse httpResponse;
                        string _result;


                        httpWebRequest = (HttpWebRequest)WebRequest.Create(spotifyUrl);
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "GET";
                        httpWebRequest.Headers.Add("Authorization", "Bearer " + RetSpotifyAccessToken());

                        httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            _result = streamReader.ReadToEnd();
                        }

                        JObject joResponse = JObject.Parse(_result);
                        JArray items = (JArray)joResponse["images"];

                        var image = items.FirstOrDefault();
                        if (image != null)
                            imageUrl = image.Value<string>("url");
                    }

                }
                catch (Exception ex)
                {
                    //  LogHelper.CreateLog("MusicGrapgh_GetSimilarArtists_Asyn " + ex.Message);
                    //  return null;
                }
                return imageUrl;
            });
            return _response;
        }

        private bool ArtistProfilePicture_Asyn(Int32 vUserID, Artists vArtists, IUnitOfWork vUnitOfWork, bool vNew, string Url)
        {
            try
            {
                if (String.IsNullOrEmpty(vArtists.ImageURL))
                    GetProfileImageFromSpotifyFeed(vArtists);

                if (String.IsNullOrEmpty(vArtists.ImageURL))
                {

                    string strpThumbnailURLfordb = null;
                    string strpBannerURLfordb = null;
                    string strpIamgeURLfordb = null;
                    string strpTempImageSave = null;

                    string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                    string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                    string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];


                    strpTempImageSave = ResizeImage.Download_Image(Url);

                    if (strpTempImageSave.Contains(".png"))
                    {
                        //---New Image path
                        string newFilePath = _SiteRoot + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\";
                        Helper.CreateDirectories(_SiteRoot + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\");

                        //Delete old files if any 
                        Helper.DeleteFiles(_SiteRoot + @"\" + "Artists" + @"\" + vArtists.ArtistID + @"\");

                        //Thumbnail image (Artist Listing)
                        string imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strpTempImageSave, newFilePath, "_T_" + strpTempImageSave, 400, 400);
                        strpThumbnailURLfordb = _SiteURL + "/Artists/" + vArtists.ArtistID + "/" + imageresizename;

                        //Scale up the image 
                        string imageresizenametmp = ResizeImage.ScaleImage(tempfilePath + strpTempImageSave, tempfilePath, "_S_" + strpTempImageSave, 650, 650);

                        //Banner Image (event listing)
                        imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_B_" + strpTempImageSave, 0, 100, 640, 270);
                        strpBannerURLfordb = _SiteURL + "/Artists/" + vArtists.ArtistID + "/" + imageresizename;

                        //Image  (used in artist detail screen)
                        imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_A_" + strpTempImageSave, 0, 50, 640, 360);
                        strpIamgeURLfordb = _SiteURL + "/Artists/" + vArtists.ArtistID + "/" + imageresizename;


                        vArtists.ImageURL = strpIamgeURLfordb;
                        vArtists.ThumbnailURL = strpThumbnailURLfordb;
                        vArtists.BannerImage_URL = strpBannerURLfordb;
                    }
                }
            }
            catch (Exception ex)
            {

                LogHelper.CreateLog("ArtistProfilePicture_Asyn" + ex.Message);
            }

            return true;
        }

        public string RetSpotifyAccessToken()
        {
            try
            {
                ClientCredentialsAuth _ClientCredentialsAuth = new ClientCredentialsAuth();
                _ClientCredentialsAuth.ClientId = WebConfigurationManager.AppSettings["Spotify_client_id"].ToString();
                _ClientCredentialsAuth.ClientSecret = WebConfigurationManager.AppSettings["Spotify_client_secret"].ToString();
                _ClientCredentialsAuth.Scope = Scope.Streaming;

                Token _Token = _ClientCredentialsAuth.DoAuth();

                return _Token.AccessToken;
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return "";
            }


        }

        private async Task<List<MusicGraph.Datum>> Spotify_GetSimilarArtistByID(string ID)
        {
            try
            {
                List<MusicGraph.Datum> _artistlst = new List<MusicGraph.Datum>();

                string _Spotify_Country = ConfigurationManager.AppSettings["Spotify_Country"].ToString();

                List<Artist> _lst = await Artist.GetRelatedArtists(ID);


                foreach (var item in _lst)
                {

                    string _Genre = "";
                    bool _Islatin = false;

                    if (item.Genres != null && item.Genres.Count > 0)
                    {
                        _Genre = item.Genres[0];

                        foreach (var _gen in item.Genres)
                        {
                            if (_GenreFilter.Any(x => x.ToLower() == _gen.ToString()))
                            {
                                _Islatin = true;
                                break;
                            }
                        }

                    }

                    _artistlst.Add(new MusicGraph.Datum
                    {
                        id = item.Id,
                        name = item.Name,
                        main_genre = _Genre,
                        IsLatin = _Islatin
                    });

                }

                return _artistlst;


            }
            catch (Exception ex)
            {
                LogHelper.CreateLog(ex);
                return null;
            }

        }

        private async Task<Dictionary<string, object>> EventFul_GetEventInfo_Asyn(Int32 vUserID, Artists vArtists, IUnitOfWork vUnitOfWork, bool vNew)
        {
            string strThumbnailURLfordb = null;
            string strIamgeURLfordb = null;
            string strTempImageSave = null;

            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(vUnitOfWork);
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(vUnitOfWork);
            GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

            Models.Venue _VenuEntity = null;
            Models.TourDate _TourDateEntity = null;
            UserTourDate _UserTourDate = null;

            string _Eventful_app_key = ConfigurationManager.AppSettings["Eventful_app_key"].ToString();


            HttpWebRequest httpWebRequest;
            HttpWebResponse httpResponse;
            string _result;

            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
            dynamic obj = null;
            bool _Eventful = false;


            Dictionary<string, object> response = new Dictionary<string, object>();



            #region "Task 1 to get Eventful Detail using Artist Name"

            if (string.IsNullOrEmpty(vArtists.Eventful_ID))
            {

                _Eventful = await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        #region "Use Eventfull API to Get Performer ID (Dont use this while updating the record"
                        //HttpClient client = new HttpClient(ServiceUri);

                        httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.eventful.com/json/performers/search?app_key=" + _Eventful_app_key + "&keywords=" + vArtists.ArtistName.Trim());
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "GET";

                        httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            _result = streamReader.ReadToEnd();
                        }

                        // deserializing 
                        dynamic _Search_Artist = serializer.Deserialize(_result, typeof(object));


                        if (_Search_Artist != null)
                        {
                            if (_Search_Artist.performers != null && String.IsNullOrEmpty(vArtists.About))
                            {
                                if (Convert.ToInt16(_Search_Artist.total_items) > 1)
                                {
                                    vArtists.About = _Search_Artist.performers.performer[0].short_bio;
                                    vArtists.Eventful_ID = _Search_Artist.performers.performer[0].id;
                                }
                                else
                                {
                                    vArtists.About = _Search_Artist.performers.performer.short_bio;
                                    vArtists.Eventful_ID = _Search_Artist.performers.performer.id;
                                }
                            }
                        }

                        #endregion

                        if (_Search_Artist != null)
                        {
                            if (_Search_Artist.performers != null)
                            {
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
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("EventFul_GetEventInfo_Asyn (Task 1) " + ex.Message);
                        return false;
                    }
                });
            }
            #endregion

            #region "Task2 will execute if above task completed sucessfully"

            if (!string.IsNullOrEmpty(vArtists.Eventful_ID))
            {
                dynamic _Get_Performer_Events = await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        #region "Use Eventfull API To Get Performer Detail"

                        httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.eventful.com/json/performers/get?app_key=" + _Eventful_app_key + "&id=" + vArtists.Eventful_ID + "&show_events=true&image_sizes=large");
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "GET";

                        httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            _result = streamReader.ReadToEnd();
                        }
                        // deserializing 
                        dynamic Get_Performer_Events = serializer.Deserialize(_result, typeof(object));
                        return Get_Performer_Events;

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("EventFul_GetEventInfo_Asyn (Task 2) " + ex.Message + " API : " + "http://api.eventful.com/json/performers/get?app_key=" + _Eventful_app_key + "&id=" + vArtists.Eventful_ID + "&show_events=true&image_sizes=large");
                        return new object();
                    }
                });

                response.Add("SeatGeekDetail", _Get_Performer_Events);

                if (_Get_Performer_Events != null)
                {
                    vArtists.About = _Get_Performer_Events.long_bio;

                    #region "Get Artist Picture "
                    if (_Get_Performer_Events.images != null)
                    {

                        if (_Get_Performer_Events.images.image.Count > 0)
                        {
                            if (WebConfigurationManager.AppSettings["Eventful.ArtistPicture"].ToString() == "True")
                            {
                                if (_Get_Performer_Events.images.image[0].large != null)
                                {
                                    Task<bool> _ArtistPic = ArtistProfilePicture_Asyn(vUserID, vArtists, vUnitOfWork, true, _Get_Performer_Events.images.image[0].large.url);
                                }
                            }
                        }
                    }
                    #endregion

                    #region "Loop through the events to get  Tour/Venue"
                    if (_Get_Performer_Events.events != null)
                    {
                        if (_Get_Performer_Events.events.@event is IEnumerable)
                        {

                            foreach (dynamic _event in _Get_Performer_Events.events.@event)
                            {
                                try
                                {
                                    httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.eventful.com/json/events/get?app_key=" + _Eventful_app_key + "&id=" + _event.id + "");
                                    httpWebRequest.ContentType = "application/json";
                                    httpWebRequest.Method = "GET";

                                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                    {
                                        _result = streamReader.ReadToEnd();
                                    }

                                    // deserializing 
                                    dynamic _Get_Event_ByID = serializer.Deserialize(_result, typeof(object));


                                    response.Add("GetEventByID_" + _event.id.ToString(), _Get_Event_ByID);



                                }
                                catch (Exception ex)
                                {
                                }

                            }

                        }
                    }
                    #endregion
                }
            }
            #endregion

            response.Add("Artist", vArtists);

            return response;
        }

        private string SGAuth()
        {
            return "?client_id=" + WebConfigurationManager.AppSettings["SeatGeek_client_id"] + "&client_secret=" + WebConfigurationManager.AppSettings["SeatGeek_client_secret"];
        }

        private async Task<Dictionary<string, object>> Spotify_GetSongInfo_Asyn(Int32 vUserID, Artists vArtists, IUnitOfWork vUnitOfWork, bool vNew)
        {
            HttpWebRequest httpWebRequest;
            HttpWebResponse httpResponse;
            string _result;

            string _Spotify_Country = ConfigurationManager.AppSettings["Spotify_Country"].ToString();
            Dictionary<string, object> dic = new Dictionary<string, object>();

            await Task.Factory.StartNew(() =>
            {
                try
                {

                    #region "Start Spotify to get music Preview"

                    if (vArtists.Spotify_ID != null && vArtists.Spotify_ID != "")
                    {
                        List<Track> _task = Task.Run(async () => await Track.GetArtistTopTracks(vArtists.Spotify_ID, _Spotify_Country)).Result;
                        dic.Add("Task", _task);
                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog("Spotify_GetSongInfo_Asyn " + ex.Message);
                }
            });

            return dic;
        }

        private async Task<Dictionary<string, object>> Instagram_GetPictures_Asyn(Int32 vUserID, Artists vArtists, MusicGraph2.ArtistMatrics_ByID vArtistMatrics_ByID, IUnitOfWork vUnitOfWork, bool vNew, TourDate vTour)
        {
            string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";
            string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
            string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];
            HttpWebRequest httpWebRequest;
            HttpWebResponse httpResponse;
            string _Instagram_access_token = ConfigurationManager.AppSettings["instagram.access_token"].ToString();
            Instagram.Instagram_Search _Instagram_Search = null;
            Instagram.Instagram_Search _Instagram_Search2 = null;
            Instagram.Instagram_Search _Instagram_Search3 = null;
            GenericRepository<ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<ArtistPhotos>(vUnitOfWork);
            GenericRepository<TourPhoto> _TourPhotosRepo = new GenericRepository<TourPhoto>(vUnitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(vUnitOfWork);
            Dictionary<string, object> Response = new Dictionary<string, object>();

            #region "New Rec"

            if (vNew == true) //used only for the new Artist 
            {
                Response.Add("vNew", true);
                #region "Task 1 to get Instagram Detail using Matrics ID"

                Task<Instagram.Instagram_Search> _Instagram_Search_Task1 = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        string _result;
                        if (vArtistMatrics_ByID != null)
                        {
                            if (vArtistMatrics_ByID.data.instagram != null && vArtistMatrics_ByID.data.instagram.url != null && vArtistMatrics_ByID.data.instagram.url != "")
                            {
                                vArtists.Instagram_Url = vArtistMatrics_ByID.data.instagram.url;
                                return _Instagram_Search;

                                string _instaGram_ID = vArtistMatrics_ByID.data.instagram.url.Replace("http:", "https:").Replace("https://instagram.com/", "").Replace("/", "");
                                string _Instagram_Tag = null;

                                #region "Get Instagram ID (dont need this block while just updating the records)"

                                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/users/search?q=" + _instaGram_ID + "&access_token=" + _Instagram_access_token);
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "GET";

                                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    _result = streamReader.ReadToEnd();
                                }

                                // deserializing 
                                _Instagram_Search = JsonConvert.DeserializeObject<Instagram.Instagram_Search>(_result);

                                if (_Instagram_Search != null)
                                {
                                    if (_Instagram_Search.data != null && _Instagram_Search.data.Count > 0)
                                    {
                                        vArtists.Instagram_ID = _Instagram_Search.data[0].id;
                                    }
                                }
                                #endregion
                            }
                        }

                        // deserializing 
                        return _Instagram_Search;

                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog3(ex, Request);
                        LogHelper.CreateLog("Instagram_GetPictures_Asyn (Task 1) " + ex.Message);
                        return null;
                    }
                });
                _Instagram_Search_Task1.Wait(); // wait for the task1 to complete first 
                #endregion

                #region "Task 2 to get Instagram Detail using Artist Name (use if Matrics ID does'nt contain the Instagram Information"

                Task<Instagram.Instagram_Search> _Instagram_Search_Task2 = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        string _result;
                        // If Instagram ID not found in MusicGraph Matrics then try direct Attempt
                        if (vArtists.Instagram_ID == null)
                        {
                            #region "Get Instagram ID (dont need this block while just updating the records)"

                            httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/users/search?q=" + vArtists.ArtistName.Trim() + "&access_token=" + _Instagram_access_token);
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "GET";

                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                _result = streamReader.ReadToEnd();
                            }

                            // deserializing 
                            _Instagram_Search2 = JsonConvert.DeserializeObject<Instagram.Instagram_Search>(_result);

                            if (_Instagram_Search2 != null)
                            {
                                if (_Instagram_Search2.data != null && _Instagram_Search2.data.Count > 0)
                                {
                                    vArtists.Instagram_ID = _Instagram_Search2.data[0].id;
                                }
                            }

                            #endregion
                        }

                        return _Instagram_Search2;

                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog3(ex, Request);
                        LogHelper.CreateLog("Instagram_GetPictures_Asyn (Task 2) " + ex.Message);
                        return null;
                    }
                });
                _Instagram_Search_Task2.Wait();
                #endregion

                if (_Instagram_Search_Task1.Result == null || _Instagram_Search_Task1.Result.data.Count == 0)
                {
                    if (_Instagram_Search_Task2.Result != null)
                    {
                        _Instagram_Search3 = _Instagram_Search_Task2.Result;
                    }
                }
                else
                {
                    _Instagram_Search3 = _Instagram_Search_Task1.Result;
                }


                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (WebConfigurationManager.AppSettings["Instagram.ArtistPicture"].ToString() == "True")
                        {
                            #region "Get Instagram Profile Picture"

                            string strpThumbnailURLfordb = null;
                            string strpIamgeURLfordb = null;
                            string strpTempImageSave = null;
                            string _result;

                            if (vArtists.Instagram_ID != null && vArtists.Instagram_ID != "")
                            {

                                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/users/" + vArtists.Instagram_ID + "/?access_token=" + _Instagram_access_token);
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "GET";

                                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    _result = streamReader.ReadToEnd();
                                }

                                // deserializing 
                                var _Profile_Picture = JsonConvert.DeserializeObject<Instagram2.Profile_Picture>(_result);

                                //download and save profile picture of the artists
                                if (_Profile_Picture != null)
                                {
                                    if (_Profile_Picture.data.profile_picture != "")
                                    {
                                        bool _ArtistPic = ArtistProfilePicture_Asyn(vUserID, vArtists, vUnitOfWork, true, _Profile_Picture.data.profile_picture.Replace("s150x150/", ""));
                                    }
                                }


                            }
                            #endregion
                        }

                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog3(ex, Request);
                        LogHelper.CreateLog("Instagram_GetPictures_Asyn (Profile Picture ) " + ex.Message);
                    }
                });

                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        string _result;

                        #region "Get Instagram Hash Tag (Recent) (Not coded yet)"
                        if (WebConfigurationManager.AppSettings["Instagram.HashTag"].ToString() == "True" && vTour != null)
                        {
                            httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/tags/" + vArtists.Instagram_Tag.ToString() + "/media/recent?access_token=" + _Instagram_access_token);
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "GET";

                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                _result = streamReader.ReadToEnd();
                            }

                            // deserializing 
                            var _User_TagPictures = JsonConvert.DeserializeObject<Instagram4.Tags_Images>(_result);
                        }
                        #endregion

                        #region "Get Instagram Recent Pictures (Using TourDate Hash Tag)"
                        //Change to tour date hash tage
                        if (vTour != null && !String.IsNullOrEmpty(vTour.HashTag))
                        {
                            httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/tags/" + vTour.HashTag + "/media/recent?access_token=" + _Instagram_access_token);
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "GET";

                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                _result = streamReader.ReadToEnd();
                            }

                            // deserializing 
                            var _User_TagPictures = JsonConvert.DeserializeObject<Instagram4.Tags_Images>(_result);

                            Response.Add("Tags_Images", _User_TagPictures);
                            vTour.ModifiedDate = DateTime.Now;
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("Instagram_GetPictures_Asyn (Recent Picture - Update) " + ex.Message);
                    }
                });

            }//End new artist condition here
            #endregion

            #region "Update Rec"

            else
            {
                //return true;
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (WebConfigurationManager.AppSettings["Instagram.ArtistPicture"].ToString() == "True")
                        {
                            #region "Get Instagram Profile Picture"
                            string _result;

                            if (vArtists.Instagram_ID != null && vArtists.Instagram_ID != "")
                            {

                                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/users/" + vArtists.Instagram_ID + "/?access_token=" + _Instagram_access_token);
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "GET";

                                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    _result = streamReader.ReadToEnd();
                                }

                                // deserializing 
                                var _Profile_Picture = JsonConvert.DeserializeObject<Instagram2.Profile_Picture>(_result);

                                //download and save profile picture of the artists
                                if (_Profile_Picture != null)
                                {
                                    if (_Profile_Picture.data.profile_picture != "")
                                    {
                                        bool _ArtistPic = ArtistProfilePicture_Asyn(vUserID, vArtists, vUnitOfWork, true, _Profile_Picture.data.profile_picture.Replace("s150x150/", ""));
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("Instagram_GetPictures_Asyn (Profile Picture - Update) " + ex.Message);
                    }
                });

                Instagram4.Tags_Images images = await Task.Factory.StartNew(() =>
                {
                    Instagram4.Tags_Images _User_TagPictures = null;
                    try
                    {
                        string _result;
                        #region "Get Instagram Recent Pictures (Using TourDate Hash Tag)"
                        //Change to tour date hash tage
                        if (vTour != null && !String.IsNullOrEmpty(vTour.HashTag))
                        {
                            httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/tags/" + vTour.HashTag + "/media/recent?access_token=" + _Instagram_access_token);
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "GET";

                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                _result = streamReader.ReadToEnd();
                            }

                            // deserializing 
                            _User_TagPictures = JsonConvert.DeserializeObject<Instagram4.Tags_Images>(_result);

                            vTour.ModifiedDate = DateTime.Now;
                        }
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("Instagram_GetPictures_Asyn (Recent Picture - Upadte) " + ex.Message);
                    }
                    return _User_TagPictures;
                });

                Response.Add("vNew", false);
                Response.Add("Tags_Images", images);
            }
            #endregion

            // _ArtistsRepo.Repository.Update(vArtists);

            return Response;
        }

        #endregion


        #region "Get NUmber of People Going"
        [HttpGet]
        [Route("api/v2/TicketBooking/GetPeopleGoing")]
        /* Test Url :: 23.111.138.246/api/v2/TicketBooking/GetPeopleGoing?TourID=12566&UserID=7749&Pageindex=1&Pagesize=10 */
        public HttpResponseMessage GetPeopleGoing(Int32 TourID, int UserID, int Pageindex, int Pagesize)
        {
            try
            {
                ViewPeopleGoing _ViewPeopleGoing = new ViewPeopleGoing();

                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                GenericRepository<UserFriends> _UserFriendsRepo = new GenericRepository<UserFriends>(_unitOfWork);

                Models.Artists _Artist = null;
                Models.TourDate _TourDate = null;

                _TourDate = _TourDateRepo.Repository.Get(p => p.TourDateID == TourID);

                if (_TourDate == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.EventNotDound, "PeopleGiong"));
                }

                _Artist = _ArtistsRepo.Repository.Get(p => p.ArtistID == _TourDate.ArtistID);

                _ViewPeopleGoing.TourID = _TourDate.TourDateID;
                _ViewPeopleGoing.ArtistID = _Artist.ArtistID;
                _ViewPeopleGoing.ArtistName = _Artist.ArtistName;
                _ViewPeopleGoing.ImageURL = _Artist.ImageURL ?? "";


                Models.Venue _Venue = null;
                _Venue = _VenueRepo.Repository.Get(p => p.VenueID == _TourDate.VenueID);

                _ViewPeopleGoing.VenueID = _Venue.VenueID;
                _ViewPeopleGoing.VenueName = _Venue.VenueName ?? "";
                _ViewPeopleGoing.VenuImageURL = _Venue.ImageURL ?? "";

                //who else is going
                _ViewPeopleGoing.Going = (from A1 in _UserFriendsRepo.Repository.GetAll(p => p.UserID == UserID && p.Matched_UserID != null)
                                          join A in _UserGoingRepo.Repository.GetAll(p => p.TourDateID == _TourDate.TourDateID && p.RecordStatus == EUserGoing.Going.ToString()) on A1.Matched_UserID equals A.UserID
                                          join B in _UsersRepo.Repository.GetAll() on A.UserID equals B.UserID
                                          select new ViewPeoples
                                          {
                                              ThumbnailURL = B.ThumbnailURL ?? "",
                                              UserID = A.UserID.Value,
                                              UserName = B.UserName.ToString(),
                                              Email = B.Email ?? "",
                                              CreatedDate = Convert.ToDateTime(A.CreatedDate).ToString("d")
                                          }).OrderByDescending(p => p.CreatedDate).ToPagedList(Pageindex - 1, Pagesize).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewPeopleGoing, "PeopleGiong"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "PeopleGiong"));
            }
        }

        #endregion


        #region "Get User Profile"
        [HttpGet]
        [Route("api/v2/TicketBooking/GetUserProfile")]
        /* Test Url :: 23.111.138.246/api/v2/TicketBooking/GetUserProfile?UserID=7721 */
        public HttpResponseMessage GetUserProfile(Int64 UserID)
        {
            try
            {
                if (string.IsNullOrEmpty(UserID.ToString()))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(UserID, false), "UserData"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }
        }
        #endregion
    }
}
