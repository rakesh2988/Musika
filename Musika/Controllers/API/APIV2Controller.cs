using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Configuration;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Specialized;
using System.Xml.Serialization;
using System.Web.Caching;
using System.Net.Http.Headers;
using System.Drawing;
using Newtonsoft.Json;
using System.Configuration;
using Newtonsoft.Json.Linq;
using MvcPaging;
using System.Web.Script.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

using Musika.Repository.Interface;
using Musika.Repository.UnitofWork;
using Musika.Repository.GRepository;
using Musika.Models;
using Musika.Library.Utilities;
using Musika.Enums;
using Musika.Library.API;
using Musika.Repository.SPRepository;
using Musika.Models.API.Input;
using Musika.Library.Multipart;
using Musika.Models.API.View;
using Musika.Library.CacheProvider;
using Musika.Library.JsonConverter;

using SpotifyWebAPI;
using SpotifyWebAPI.Web.Enums;
using SpotifyWebAPI.Web.Models;
using Musika.Library.Search;
using Musika.Library.PushNotfication;
using System.Data;
using ArtSeeker.Library.Common;
using System.Text;
using System.Threading;
using System.Collections;
using System.Data.Entity;
using System.Xml;
using Microsoft.Ajax.Utilities;
using Musika.Common;

namespace Musika.Controllers.API
{
    public class APIV2Controller : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpCache _Cache = new HttpCache();

        int _Imagethumbsize = 0;
        int _imageSize = 0;
        bool _ApiLogger = false;
        double _FuzzySearchCri = 0.33;


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


        public APIV2Controller()
        {
            _unitOfWork = new UnitOfWork();
            _Imagethumbsize = Convert.ToInt16(WebConfigurationManager.AppSettings["ImageThumbSize"].ToString());
            _imageSize = Convert.ToInt16(WebConfigurationManager.AppSettings["ImageSize"].ToString());
            _ApiLogger = Convert.ToBoolean(WebConfigurationManager.AppSettings["Logger"].ToString());
        }

        public APIV2Controller(string str)
        {

        }

        #region "Users"

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

                _viewUser.ImageURL = entity.ImageURL ?? string.Empty;
                _viewUser.ThumbnailURL = entity.ThumbnailURL ?? string.Empty;
                _viewUser.IsNewUser = _IsNewUser;
                _viewUser.RecordStatus = entity.RecordStatus;
            }
            else
            {
                return null;
            }
            return _viewUser;
        }


        [HttpPost]
        [Route("api/v2/Users/SetLanguage")]
        public HttpResponseMessage SetLanguage(InputLang input)
        {
            try
            {
                if (input.UserId > 0)
                {
                    Users user;
                    GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                    if ((user = _UsersRepo.Repository.GetById(input.UserId)) != null ? true : false)
                    {
                        user.UserLanguage = input.Language.ToUpper() == EUserLanguage.ES.ToString() ? EUserLanguage.ES.ToString() : EUserLanguage.EN.ToString();
                        _UsersRepo.Repository.Update(user);
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, null, "Updated"));
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }
        }


        [HttpGet]
        [Route("api/v2/Users/GetUserProfile")]
        public HttpResponseMessage GetUserProfile(Int64 UserID)
        {
            try
            {
                if (UserID == null)
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


        [HttpPost]
        [Route("api/v2/Users/Signup")]
        public HttpResponseMessage Signup()
        {
            try
            {
                InputSignUp Input = new InputSignUp();
                var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

                Input.Password = AesCryptography.Encrypt(httpContext.Request.Form["Password"]);
                Input.Email = httpContext.Request.Form["Email"];
                Input.UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + httpContext.Request.Form["UserName"];

                Input.DeviceToken = httpContext.Request.Form["DeviceToken"];
                Input.DeviceType = httpContext.Request.Form["DeviceType"];
                Input.DeviceLat = Convert.ToDecimal(httpContext.Request.Form["DeviceLat"]);
                Input.DeviceLong = Convert.ToDecimal(httpContext.Request.Form["DeviceLong"]);

                var context = new ValidationContext(Input, serviceProvider: null, items: null);
                var results = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(Input, context, results);

                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "salman test 4"));

                if (!isValid)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, results[0].ErrorMessage));
                }

                // Check whether the POST operation is MultiPart?
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                string _UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + Input.UserName;
                Models.Users entity = new Models.Users();
                GenericRepository<Users> _userEntity = new GenericRepository<Users>(_unitOfWork);

                ///check USer Name 
                if (!Helper.IsValidPattern(_UserName, "^[a-zA-Z0-9. _-]{2,100}$"))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.Reg_InvalUserName));
                }

                ///check email 
                bool Email = false;
                Email = Helper.IsValidEmail(Input.Email);
                if (Email == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.Invalidemail));
                }

                //check if UserName already exists
                Users _user = null;

                //_user = _userEntity.Repository.Get(e => e.UserName == _UserName);
                //if (_user != null)
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNameAlreadyExists));
                //}

                //check if Email already exists
                _user = _userEntity.Repository.Get(e => e.Email == Input.Email && e.RecordStatus != RecordStatus.Deleted.ToString());
                if (_user != null)
                {

                    if(_user.RecordStatus == RecordStatus.Active.ToString())
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.EmailAlreadyExists));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.AcccountAlreadyExistsSignUp));

                    }
                }

                // _unitOfWork.StartTransaction();// Start Transaction
                // _unitOfWork.Commit();// End Transaction

                try
                {
                    entity.Password = Input.Password;
                    entity.Email = Input.Email;
                    entity.UserName = Input.UserName;

                    entity.DeviceToken = Input.DeviceToken;
                    entity.DeviceType = Input.DeviceType;
                    entity.DeviceLat = Input.DeviceLat;
                    entity.DeviceLong = Input.DeviceLong;

                    entity.CreatedDate = DateTime.Now;
                    entity.RecordStatus = RecordStatus.Active.ToString();


                    //if (HttpContext.Current.Request.Files.Count == 0)
                    //{
                    entity.ThumbnailURL = WebConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                    entity.ImageURL = ConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                    //}

                    //if (HttpContext.Current.Request.Files.Count > 0)
                    //{
                    //    ImageResponse imgResponse = MultipartFiles.GetMultipartImage(HttpContext.Current.Request.Files, "Image", "profileimage", _Imagethumbsize, _Imagethumbsize, _imageSize, _imageSize, true, true, true, "profileimage");

                    //    if (imgResponse.IsSuccess == true)
                    //    {
                    //        entity.ThumbnailURL = imgResponse.ThumbnailURL;
                    //        entity.ImageURL = imgResponse.ImageURL;
                    //    }
                    //    else
                    //    {
                    //        _unitOfWork.RollBack();//RollBack Transaction
                    //        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, imgResponse.ResponseMessage));
                    //    }
                    //}

                    _userEntity.Repository.Add(entity);

                    SetUserDevices(entity);
                    //OpenFireRepository _OpenFireRepository = new OpenFireRepository();
                    //_OpenFireRepository.InsertUser(WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + input.UserName, input.Email, input.Password);

                    GenericRepository<UserSettings> _UserSettingsEntity = new GenericRepository<UserSettings>(_unitOfWork);
                    UserSettings _UserSettings = new UserSettings();


                    _UserSettings.UserID = entity.UserID;
                    _UserSettings.SettingKey = EUserSettings.Musika.ToString();
                    _UserSettings.SettingValue = true;
                    _UserSettings.NotificationCount = 0;
                    _UserSettingsEntity.Repository.Add(_UserSettings);

                    //_UserSettings = new UserSettings();
                    //_UserSettings.UserID = entity.UserID;
                    //_UserSettings.SettingKey = EUserSettings.Orders.ToString();
                    //_UserSettings.SettingValue = true;
                    //_UserSettings.NotificationCount = 0;
                    //_UserSettingsEntity.Repository.Add(_UserSettings);

                    //_UserSettings = new UserSettings();
                    //_UserSettings.UserID = entity.UserID;
                    //_UserSettings.SettingKey = EUserSettings.Offers.ToString();
                    //_UserSettings.SettingValue = true;
                    //_UserSettings.NotificationCount = 0;
                    //_UserSettingsEntity.Repository.Add(_UserSettings);

                    //_UserSettings = new UserSettings();
                    //_UserSettings.UserID = entity.UserID;
                    //_UserSettings.SettingKey = EUserSettings.Messages.ToString();
                    //_UserSettings.SettingValue = true;
                    //_UserSettings.NotificationCount = 0;
                    //_UserSettingsEntity.Repository.Add(_UserSettings);


                    //   _unitOfWork.Commit();// End Transaction

                    // Send Push Notification                    
                    #region "Send Notification - Added by Mukesh (3-Aug-2018)"
                    //int fromUserId = 1;
                    //int toUserId = entity.UserID;
                    //string message = Musika.Enums.Response.UserRegistration;
                    //SendNotification(fromUserId, toUserId, message, -1, null, null,"Registration");
                    #endregion

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

        [HttpGet]
        [Route("api/v2/logout")]
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


        [HttpPost]
        [Route("api/v2/Users/signin")]
        public HttpResponseMessage Signin(InputSignIn input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                // var _st = AesCryptography.Decrypt("w2fl54iG1nrKcJbuSwHMCA==");


                Models.Users entity = null;
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                string _UserName = "";
                string _Password = "";


                _UserName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + input.UserName;
                _Password = AesCryptography.Encrypt(input.Password);


                //entity = _UsersRepo.Repository.Get(p => p.UserName.ToLower() == _UserName.ToLower() && p.Password == _Password);

                //if (entity == null)
                //{
                entity = _UsersRepo.Repository.Get(p => p.Email.ToLower() == input.UserName.ToLower() && p.Password == _Password
                                                    && p.RecordStatus != RecordStatus.Deleted.ToString());
                //}

                if (entity != null)
                {
                    if (entity.RecordStatus != RecordStatus.Active.ToString())
                    {
                        //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "User has blocked"));
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.InactiveAccountSignIn));
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

        [HttpPost]
        [Route("api/v2/Users/UpdateSignUp")]
        public HttpResponseMessage UpdateSignUp(int userId, string deviceToken, string deviceType)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                Models.Users entity = null;
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                entity = _UsersRepo.Repository.Get(p => p.UserID == userId && p.RecordStatus != RecordStatus.Deleted.ToString());

                if (entity != null)
                {
                    if (entity.RecordStatus != RecordStatus.Active.ToString())
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "User has blocked"));
                    }

                    entity.DeviceToken = deviceToken;
                    entity.DeviceType = deviceType;

                    _UsersRepo.Repository.Update(entity);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.PasswordOrEmail));
                }

                GenericRepository<UserDevices> _UserDevices = new GenericRepository<UserDevices>(_unitOfWork);
                Models.UserDevices entity2 = null;
                entity2 = _UserDevices.Repository.Get(p => p.UserId == userId);

                if (entity2 != null)
                {
                    entity2.DeviceToken = deviceToken;
                    entity2.DeviceType = deviceType;
                    _UserDevices.Repository.Update(entity2);
                }
                //SetUserDevices(entity);

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(entity.UserID, false), "UserData"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }

        [HttpPost]
        [Route("api/v2/Users/GetForgetPassword")]
        public ApiResponse GetForgetPassword(dynamic data)
        {
            if (data.Email != null)
            {
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                string _email = data.Email;

                var entity = _UsersRepo.Repository.Get(p => p.Email == _email);
                if (entity != null)
                {
                    string password = AesCryptography.Decrypt(entity.Password);

                    string html = "<p>Hi " + entity.UserName + "</p>";
                    html += "<p>Thanks for using " + WebConfigurationManager.AppSettings["AppName"] + "! </p>";
                    html += "<p>Your password is : " + password + "</p>";
                    html += "<p><br>If you would like to change your password, go to the profile area of the app</p>";
                    html += "<p><br><br><strong>Thanks,The " + WebConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";

                    EmailHelper.SendEmail(entity.Email, WebConfigurationManager.AppSettings["AppName"] + " : Forget Password", html);

                    return JsonResponse.GetResponse(ResponseCode.Success, "Please check your email to get password.");
                }
                else
                {
                    return JsonResponse.GetResponse(ResponseCode.Failure, "User not found against this email.");
                }
            }
            else
            {
                return JsonResponse.GetResponse(ResponseCode.Failure, "Email cannot be empty.");
            }
        }

        [HttpPost]
        [Route("api/v2/Users/signinthirdparty")]
        public HttpResponseMessage SigninThirdParty(InputSignInWithThirdParty input)
        {
            try
            {
                // _unitOfWork.StartTransaction();  // Start Transaction

                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                Models.Users entity = null;
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                if (input.ThirdPartyType == ThirdPartyType.Facebook)
                {
                    entity = _UsersRepo.Repository.Get(p => p.FacebookID == input.ThirdPartyId
                                        && p.RecordStatus != RecordStatus.Deleted.ToString());
                }

                string strThumbnailURLfordb = null;
                string strIamgeURLfordb = null;
                string strThumbnailURLfordbBlur = null;

                string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];
                byte[] converted;
                string strThumbnailImage = input.ImageURL;


                if (entity != null)
                {
                    //if (entity.RecordStatus == RecordStatus.Deleted.ToString())
                    //{
                    //    _unitOfWork.RollBack();
                    //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "User has been removed by the admin", "UserData"));
                    //}

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

                    //entity.ThumbnailURL = "";
                    //entity.ImageURL = "";

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

                            //Thumbnail Blur Filter

                            //string _BlurImage = ResizeImage.Get_BlurImage(newFilePath + thumbnailresizename, newFilePath, ImageBlurFilter.ExtBitmap.BlurType.Mean9x9);
                            //strThumbnailURLfordbBlur = _SiteURL + "/ProfileImage/" + _BlurImage;

                            entity.ThumbnailURL = strThumbnailURLfordb;
                            entity.ImageURL = strIamgeURLfordb;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        entity.ThumbnailURL = "http://appserver.musikaapp.com/Content/Upload/default-user.png";
                        entity.ImageURL = "http://appserver.musikaapp.com/Content/Upload/default-user.png";
                    }

                    _UsersRepo.Repository.Update(entity);

                    //     _unitOfWork.Commit();// End Transaction
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(entity.UserID, false), "UserData"));
                }
                else
                {
                    entity = new Models.Users();

                    entity.UserName = input.UserName;
                    entity.Email = input.Email;

                    bool Email = false;
                    Email = Helper.IsValidEmail(input.Email);

                    //if (Email == false)
                    //{
                    //    _unitOfWork.RollBack();// End Transaction
                    //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.Invalidemail, "UserData"));
                    //}

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

                            //Thumbnail Blur Filter

                            //string _BlurImage = ResizeImage.Get_BlurImage(newFilePath + thumbnailresizename, newFilePath, ImageBlurFilter.ExtBitmap.BlurType.Mean9x9);
                            //strThumbnailURLfordbBlur = _SiteURL + "/ProfileImage/" + _BlurImage;
                        }
                        catch (Exception ex)
                        {
                            strThumbnailURLfordb = strThumbnailImage;
                            strIamgeURLfordb = strThumbnailImage;
                        }
                    }
                    else
                    {
                        strThumbnailURLfordb = "http://appserver.musikaapp.com/Content/Upload/default-user.png";
                        strIamgeURLfordb = "http://appserver.musikaapp.com/Content/Upload/default-user.png";
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

                    #region "Commented Code"
                    //_UserSettings = new UserSettings();
                    //_UserSettings.UserID = entity.UserID;
                    //_UserSettings.SettingKey = EUserSettings.Orders.ToString();
                    //_UserSettings.SettingValue = true;
                    //_UserSettings.NotificationCount = 0;
                    //_UserSettingsEntity.Repository.Add(_UserSettings);

                    //_UserSettings = new UserSettings();
                    //_UserSettings.UserID = entity.UserID;
                    //_UserSettings.SettingKey = EUserSettings.Offers.ToString();
                    //_UserSettings.SettingValue = true;
                    //_UserSettings.NotificationCount = 0;
                    //_UserSettingsEntity.Repository.Add(_UserSettings);

                    //_UserSettings = new UserSettings();
                    //_UserSettings.UserID = entity.UserID;
                    //_UserSettings.SettingKey = EUserSettings.Messages.ToString();
                    //_UserSettings.SettingValue = true;
                    //_UserSettings.NotificationCount = 0;
                    //_UserSettingsEntity.Repository.Add(_UserSettings);
                    #endregion

                    // _unitOfWork.Commit();// End Transaction

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

        [HttpPost]
        [Route("api/v2/Users/UpdateUserPicture")]
        public HttpResponseMessage UpdateUserPicture()
        {
            try
            {
                var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];

                if (httpContext.Request.Form["UserID"] == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "provide UserID"));
                }

                // Check whether the POST operation is MultiPart?
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                int _UserID = Convert.ToInt16(httpContext.Request.Form["UserID"]);

                Models.Users entity = null;
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                entity = _UsersRepo.Repository.Get(p => p.UserID == _UserID);
                entity.ModifiedDate = DateTime.Now;

                if (entity != null)
                {
                    if (HttpContext.Current.Request.Files.Count > 0)
                    {
                        ImageResponse imgResponse = MultipartFiles.GetMultipartImage(HttpContext.Current.Request.Files, "Image", "profileimage", _Imagethumbsize, _Imagethumbsize, _imageSize, _imageSize, true, true, true, "profileimage");

                        if (imgResponse.IsSuccess == true)
                        {
                            entity.ThumbnailURL = imgResponse.ThumbnailURL;
                            entity.ImageURL = imgResponse.ImageURL;
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, imgResponse.ResponseMessage));
                        }
                        _UsersRepo.Repository.Update(entity);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "Image file is not provided", "UserData"));
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(entity.UserID, false), "UserData"));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound));
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }

        [HttpPost]
        [Route("api/v2/Users/UpdateUser")]
        public HttpResponseMessage UpdateUser(InputUpdateUser input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                Models.Users entity = null;
                Models.Users _Users = null;
                string _Email = "";
                string _Password = "";

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                entity = _UsersRepo.Repository.Get(p => p.UserID == input.UserID);

                if (entity != null)
                {
                    string _password = "";

                    //Check the Current Password
                    if (input.NewPassword != null)
                    {
                        if (input.NewPassword != "")
                        {
                            if (input.CurrentPassword == null || input.CurrentPassword == "")
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "Current password is not entered"));
                            }
                            _password = AesCryptography.Encrypt(input.CurrentPassword);

                            Models.Users _user = null;

                            _user = _UsersRepo.Repository.Get(e => e.Password == _password && e.UserID == input.UserID);

                            if (_user == null)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.CurrentPasswordNotMatched));
                            }
                        }
                    }

                    //Check Email 
                    bool Email = false;

                    if (input.Email != null && input.Email != "")
                    {
                        Email = Helper.IsValidEmail(input.Email);
                        if (Email == false)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.Invalidemail));
                        }

                        //check if Email already exists
                        _Users = _UsersRepo.Repository.Get(e => e.Email == input.Email && e.UserID != input.UserID);
                        if (_Users != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNameAlreadyExists));
                        }
                    }

                    //check if UserName already exists
                    //string _userName = WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + input.UserName;
                    //_Users = _UsersRepo.Repository.Get(e => e.UserName == _userName && e.UserID != input.UserID);
                    //if (_Users != null)
                    //{
                    //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNameAlreadyExists));
                    //}

                    //entity.Email = (input.Email != null) ? input.Email.ToString() != "" ? input.Email : entity.Email : entity.Email;
                    entity.UserName = (input.UserName != null) ? input.UserName.ToString() != "" ? WebConfigurationManager.AppSettings["XMPPPrefix"].ToString() + input.UserName : entity.UserName : entity.UserName;
                    entity.Password = (input.NewPassword != null) ? input.NewPassword.ToString() != "" ? AesCryptography.Encrypt(input.NewPassword) : entity.Password : entity.Password;

                    _UsersRepo.Repository.Update(entity);

                    //Sending Welcome Email
                    if (input.NewPassword != null)
                    {
                        if (input.NewPassword != "")
                        {
                            try
                            {
                                string html = "<p>Hi " + entity.UserName + "</p>";
                                html += "<p><br>Your password has been changed successfully.</p>";
                                html += "<p><br><br><strong>Thanks,The " + WebConfigurationManager.AppSettings["AppName"] + " Team</strong></p>";
                                EmailHelper.SendEmail(entity.Email, WebConfigurationManager.AppSettings["AppName"] + " Password Changed.", html);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                    //

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserDetail(entity.UserID, false), "UserData"));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound));
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }

        [HttpGet]
        [Route("api/v2/Users/GetUserSetting")]
        public HttpResponseMessage GetUserSetting(Int64 UserID)
        {
            try
            {
                if (UserID == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
                }

                GenericRepository<UserSettings> _UserSetting = new GenericRepository<UserSettings>(_unitOfWork);

                List<UserSettings> list = _UserSetting.Repository.GetAll(p => p.UserID == UserID).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, list[0]));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }
        }

        [HttpPost]
        [Route("api/v2/Users/UpdateUserSetting")]
        public HttpResponseMessage UpdateUserSetting(InputUpdateUserSetting input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                Models.UserSettings entity = null;
                GenericRepository<UserSettings> _UserSettingsRep = new GenericRepository<UserSettings>(_unitOfWork);

                entity = _UserSettingsRep.Repository.Get(p => p.UserID == input.UserID && p.SettingKey == input.SettingKey.ToString());

                if (entity != null)
                {
                    entity.SettingValue = (input.SettingValue != null) ? input.SettingValue.ToString() != "" ? input.SettingValue : entity.SettingValue : entity.SettingValue;
                    entity.NotificationCount = (input.NotificationCount != null) ? input.NotificationCount.ToString() != "" ? input.NotificationCount : entity.NotificationCount : entity.NotificationCount;

                    _UserSettingsRep.Repository.Update(entity);

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, entity));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserSettingNotFound));
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }

        [HttpPost]
        [Route("api/v2/Users/ResetNotification")]
        public HttpResponseMessage ResetNotification(InputUpdateUserSetting input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                Models.UserSettings entity = null;
                GenericRepository<UserSettings> _UserSettingsRep = new GenericRepository<UserSettings>(_unitOfWork);

                entity = _UserSettingsRep.Repository.Get(p => p.UserID == input.UserID && p.SettingKey == input.SettingKey.ToString());

                if (entity != null)
                {
                    entity.NotificationCount = (input.NotificationCount != null) ? input.NotificationCount.ToString() != "" ? input.NotificationCount : entity.NotificationCount : entity.NotificationCount;
                    _UserSettingsRep.Repository.Update(entity);
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, entity));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserSettingNotFound));
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }

        [HttpGet]
        [Route("api/v2/Users/GetMusicSource")]
        public HttpResponseMessage GetMusicSource(string MusicSource)
        {
            try
            {
                GenericRepository<MSource> _MSourceRepo = new GenericRepository<MSource>(_unitOfWork);
                List<MSource> _lst = new List<MSource>();

                if (MusicSource == "IOS")
                {
                    _lst = (from A in _MSourceRepo.Repository.GetAll(p => p.RecordStatus == "IOS")
                            select new MSource
                            {
                                MSourceID = A.MSourceID,
                                MSourceName = A.MSourceName,
                                MSourceURL = WebConfigurationManager.AppSettings["WebPath"].ToString() + A.MSourceURL,
                                CreatedDate = A.CreatedDate,
                                ModifiedDate = A.ModifiedDate,
                                RecordStatus = A.RecordStatus
                            }).ToList();
                }
                else
                {
                    _lst = (from A in _MSourceRepo.Repository.GetAll(p => p.RecordStatus == "Android")
                            select new MSource
                            {
                                MSourceID = A.MSourceID,
                                MSourceName = A.MSourceName,
                                MSourceURL = WebConfigurationManager.AppSettings["WebPath"].ToString() + A.MSourceURL,
                                CreatedDate = A.CreatedDate,
                                ModifiedDate = A.ModifiedDate,
                                RecordStatus = A.RecordStatus
                            }).ToList();
                }

                if (_lst.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _lst, "MusicSource"));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.RecordNotFound));
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }
        }

        [HttpPost]
        [Route("api/v2/Users/UpdateMusicSource")]
        public HttpResponseMessage UpdateMusicSource(InputUpdateMusicSource input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }
                _unitOfWork.StartTransaction();

                GenericRepository<MusicSource> _MusicSourceRepo = new GenericRepository<MusicSource>(_unitOfWork);

                foreach (myMusicsource _item in input.MusicSource)
                {
                    var _lst = _MusicSourceRepo.Repository.GetAll(p => p.UserID == input.UserID && p.DeviceType == _item.DeviceType.ToString() && p.MsourceID == _item.MSourceID).Select(p => p.SourceID).ToList();
                    _MusicSourceRepo.Repository.DeletePermanent(_lst);

                    Models.MusicSource entity = new MusicSource();

                    entity.Source = _item.Source.ToString();
                    entity.UserID = input.UserID;
                    entity.MsourceID = _item.MSourceID;
                    entity.DeviceType = _item.DeviceType.ToString();
                    entity.CreatedDate = DateTime.Now;
                    entity.RecordStatus = RecordStatus.Active.ToString();

                    _MusicSourceRepo.Repository.Add(entity);
                }

                _unitOfWork.Commit();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, null));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                if (_unitOfWork.IsTransaction == true) _unitOfWork.RollBack();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }


        [Route("api/v2/Users/syncFacebookFriends")]
        [HttpPost]
        public HttpResponseMessage syncFacebookFriends(JObject jObject)
        {
            /*{
             "UserID": 1,
             "UserFacebookID":"123232323",
                 "Friends": [
                   {
                     "name": "Kate Bell",
                     "id":"23232323"
                   },
                   {
                     "name": "alber don",
                     "id":"55656545" 
                   }
                  ]
             }*/

            try
            {
                // _unitOfWork.StartTransaction();

                int myUserID = Convert.ToInt32((string)jObject["UserID"]);
                string myUserFacebookID = (string)jObject["UserFacebookID"];
                JArray Friends = (JArray)jObject["Friends"];

                GenericRepository<UserFriends> _UserFriendsRepo = new GenericRepository<UserFriends>(_unitOfWork);
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                UserFriends _UserFriends = null;
                Users _myUser = null;

                _myUser = _UsersRepo.Repository.Get(p => p.UserID == myUserID);

                if (_myUser != null)
                {
                    if (String.IsNullOrEmpty(_myUser.FacebookID))
                    {
                        _myUser.SynFacebookID = myUserFacebookID;
                        _UsersRepo.Repository.Update(_myUser);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "FacebookFriends"));
                }

                foreach (var item in Friends)
                {
                    JObject jFriend = (JObject)item;

                    string jFriendName = (string)jFriend["name"];
                    string jFriendFacebookID = (string)jFriend["id"];

                    if (!String.IsNullOrEmpty(jFriendFacebookID))
                    {
                        var friendUsers = _UsersRepo.Repository.GetAll(x => x.UserID != myUserID && (x.FacebookID == jFriendFacebookID || x.SynFacebookID == jFriendFacebookID));

                        foreach (var friend in friendUsers)
                        {

                            if (!_UserFriendsRepo.Repository.AsQueryable().Any(x => x.UserID == myUserID && x.Matched_UserID == friend.UserID && x.FacebookID == jFriendFacebookID))
                            {
                                _UserFriends = new UserFriends();

                                _UserFriends.Name = jFriendName;
                                _UserFriends.UserID = _myUser.UserID;
                                _UserFriends.FacebookID = jFriendFacebookID;
                                _UserFriends.Matched_UserID = friend.UserID;
                                _UserFriends.CreatedDate = DateTime.Now;
                                _UserFriends.RecordStatus = RecordStatus.Active.ToString();

                                _UserFriendsRepo.Repository.Add(_UserFriends);
                            }

                            if (!_UserFriendsRepo.Repository.AsQueryable().Any(x => x.UserID == friend.UserID && x.Matched_UserID == myUserID && x.FacebookID == myUserFacebookID))
                            {
                                _UserFriends = new UserFriends();

                                _UserFriends.Name = _myUser.UserName;
                                _UserFriends.UserID = friend.UserID;
                                _UserFriends.FacebookID = myUserFacebookID;
                                _UserFriends.Matched_UserID = myUserID;
                                _UserFriends.CreatedDate = DateTime.Now;
                                _UserFriends.RecordStatus = RecordStatus.Active.ToString();

                                _UserFriendsRepo.Repository.Add(_UserFriends);
                            }
                        }
                    }
                }
                // _unitOfWork.Commit();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, null, "FacebookFriends"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                _unitOfWork.RollBack();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "FacebookFriends"));
            }
        }


        [HttpGet]
        [Route("api/v2/Users/GetLinkedAccount")]
        public HttpResponseMessage GetLinkedAccount(Int64 UserID, string devicetype)
        {
            try
            {
                if (UserID == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
                }

                if (devicetype == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "Devicetype required"));
                }

                GenericRepository<MusicSource> _MusicSourceRepo = new GenericRepository<MusicSource>(_unitOfWork);
                GenericRepository<MSource> _MSourceRepo = new GenericRepository<MSource>(_unitOfWork);


                var _lst = (from A in _MusicSourceRepo.Repository.GetAll(p => p.UserID == UserID && p.DeviceType == devicetype)
                            join B in _MSourceRepo.Repository.GetAll() on A.MsourceID equals B.MSourceID
                            select new
                            {
                                B.MSourceID,
                                B.MSourceName,
                                MSourceURL = WebConfigurationManager.AppSettings["WebPath"].ToString() + B.MSourceURL
                            });

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _lst, "MusicSource"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message, "MusicSource"));
            }
        }


        #endregion

        #region "Artists"
        [HttpGet]
        [Route("api/v2/Artists/GetArtistByName")]
        public HttpResponseMessage GetArtistByName(string search)
        {
            //changed
            try
            {
                //Spotify_SearchArtist(search);

                HttpCache _HttpCache = new HttpCache();
                List<MusicGraph.ArtistList> _ArtistList = new List<MusicGraph.ArtistList>();
                GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(_unitOfWork);

                //Get Cache
                if (_HttpCache.Get(search, out _ArtistList) == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ArtistList, "Artist"));
                }

                //Check DB Artists First

                string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ\s]";
                search = Regex.Replace(search, pattern, "");

                List<MusicGraph.ArtistList> _ArtistListDB = new List<MusicGraph.ArtistList>();

                if (search.Length < 3)
                    //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, "", "Artist"));
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ArtistListDB, "Artist"));


                //var a = _unitOfWork.Db.Artists.AsQueryable().Select(x => new { x.Spotify_ID, x.ArtistName }).Where(x => x.ArtistName.Contains(search)).ToList();

                var a = _unitOfWork.Db.Artists.AsQueryable().Select(x => new { x.Spotify_ID, x.ArtistName }).Where(x => x.ArtistName.StartsWith(search)).OrderBy(x => x.ArtistName).ToList();


                foreach (var artist in a)
                {
                    _ArtistListDB.Add(new MusicGraph.ArtistList
                    {
                        ID = string.IsNullOrEmpty(artist.Spotify_ID) ? artist.ArtistName : artist.Spotify_ID.ToString(),
                        Name = artist.ArtistName
                    });
                }

                if (_ArtistListDB.Count() > 3)
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ArtistListDB, "Artist"));


                //Then Check Music Graph API
                Task<List<MusicGraph.Datum>> _Search_ByName = Task.Run(async () => await Spotify_SearchArtist(search));
                _Search_ByName.Wait();

                //string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
                string _result;

                //var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/artist/suggest?api_key=" + _MusicGrapgh_api_key + "&limit=5&prefix=" + search);
                //httpWebRequest.ContentType = "application/json";
                //httpWebRequest.Method = "GET";

                //var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                //{
                //    _result = streamReader.ReadToEnd();
                //}


                //// deserializing 
                //var _Search_ByName = JsonConvert.DeserializeObject<MusicGraph.Search_ByName>(_result);

                bool isLatin = false;


                if (_Search_ByName != null)
                {
                    if (_Search_ByName.Result.Count > 0)
                    {
                        _ArtistList = new List<MusicGraph.ArtistList>();

                        foreach (MusicGraph.Datum _datum in _Search_ByName.Result)
                        {
                            if (!_ArtistListDB.Any(x => x.ID == _datum.id || x.Name == _datum.name))
                            {
                                //Check to filter Latin artist only
                                if (_GenreFilter.Contains(_datum.main_genre))
                                    isLatin = true;
                                else
                                {
                                    //if (!isLatin)
                                    //    isLatin = CheckMusicGraphLatin(_datum.id, _unitOfWork);

                                    if (!isLatin)
                                    {
                                        //seat geek latin check
                                        isLatin = CheckSeatGeekLatin(_datum.name, _unitOfWork);
                                    }

                                    if (!isLatin)
                                        isLatin = CheckLastResortSpotifyGenre(_datum.spotify_id);
                                }

                                if (isLatin)
                                {
                                    _ArtistList.Add(new MusicGraph.ArtistList
                                    {
                                        ID = _datum.id,
                                        Name = _datum.name
                                    });
                                }
                                else
                                {
                                    if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _datum.name))
                                        _ArtistNotLatinRepo.Repository.Add(new ArtistsNotLatin() { ArtistName = _datum.name });
                                }
                            }
                            isLatin = false;
                        }

                        //Save cache
                        if (_ArtistList.Count > 0)
                        {
                            _ArtistList = _ArtistListDB.Union(_ArtistList).ToList();
                            _HttpCache.Set(search, _ArtistList, 24);
                        }
                        else
                            _ArtistList = _ArtistListDB;

                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ArtistList, "Artist"));
                    }

                    //Save cache
                    if (_ArtistListDB.Count > 0)
                    {
                        _HttpCache.Set(search, _ArtistListDB, 24);
                    }

                    _ArtistList = _ArtistListDB;
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ArtistList, "Artist"));
                }

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.InvalidArtistName, "Artist"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message, "Artist"));
            }
        }

        public bool CheckMusicGraphLatin(string mId, IUnitOfWork vUnitOfWork)
        {
            bool isLatin = false;

            try
            {
                HttpWebRequest httpWebRequest;
                HttpWebResponse httpResponse;
                string _result;
                string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
                httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/v2/artist/" + mId + "/profile?api_key=" + _MusicGrapgh_api_key);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";

                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    _result = streamReader.ReadToEnd();
                }

                JObject joResponse = JObject.Parse(_result);
                JArray items = (JArray)joResponse["data"];

                foreach (var i in items)
                {
                    if (i.Value<string>("type").ToLower() == "language" && i.Value<string>("tag").ToLower() == "spanish" && i.Value<int>("count") > 2)
                    {
                        isLatin = true;
                        break;
                    }
                }
            }
            catch (Exception)
            { }

            return isLatin;
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
            catch (Exception)
            { }
            return isLatin;
        }


        [HttpGet]
        [Route("api/v2/Artists/GetArtistInfoByID")]
        public async Task<HttpResponseMessage> GetArtistInfoByID(string ID, int UserID)
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new[] { new DynamicJsonConverter() });

                _unitOfWork.StartTransaction();

                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                Models.Users _Users = new Users();
                DataSet ds = new DataSet();
                if (UserID > 0 && !String.IsNullOrEmpty(UserID.ToString()))
                {
                    // _Users = _UsersRepo.Repository.GetById(UserID);

                    ds = new SpRepository().GetUserProfileByUserId(UserID);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        // _Users =
                        DataRow dr = ds.Tables[0].Rows[0];
                        _Users.UserID = Convert.ToInt32(dr["UserID"].ToString());
                        _Users.UserName = Convert.ToString(dr["UserName"].ToString());
                        // _Users.Password = Convert.ToString(dr["Password"].ToString()) ?? string.Empty;
                        //_Users.ImageURL = HttpUtility.UrlPathEncode(Convert.ToString(dr["ImageURL"].ToString()));
                        // _Users.ThumbnailURL = Convert.ToString(dr["ThumbnailURL"].ToString()) ?? string.Empty;
                        _Users.Email = Convert.ToString(dr["Email"].ToString()) ?? string.Empty;
                        _Users.RecordStatus = Convert.ToString(dr["RecordStatus"].ToString());
                    }
                    if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                    {
                        _unitOfWork.RollBack();
                        //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "ArtistDetail"));
                    }
                }


                Models.Artists _Artists = null;

                try
                {
                    int _ArtistID = Convert.ToInt16(ID);
                    _Artists = _ArtistsRepo.Repository.GetById(_ArtistID);
                }
                catch { }

                if (_Artists == null)
                {
                    _Artists = _ArtistsRepo.Repository.Get(p => p.Spotify_ID == ID);
                }

                //check if artist manually added by admin
                if (_Artists == null)
                {
                    _Artists = _ArtistsRepo.Repository.Get(p => p.ArtistName == ID && string.IsNullOrEmpty(p.Spotify_ID));

                    if (_Artists != null)
                    {
                        Task<List<MusicGraph.Datum>> _search = Task.Run(async () => await Spotify_SearchArtist(ID));
                        _search.Wait();

                        //var _search = await MusicGrapgh_GetArtistByName_Asyn(ID);

                        if (_search != null)
                        {
                            foreach (MusicGraph.Datum _Datum in _search.Result)
                            {
                                if (RemoveDiacritics(_Datum.name.ToLower()) == ID.ToLower())
                                {
                                    _Artists.Spotify_ID = _Datum.id;
                                    _ArtistsRepo.Repository.Update(_Artists);
                                }
                            }
                        }
                    }
                }

                //Get the artist information if found in Local DB and information is not more then 24 hours old
                Task<string> _ImageFromSpotify = null;

                if (_Artists != null)
                {
                    TimeSpan span = DateTime.Now - Convert.ToDateTime(_Artists.ModifiedDate);
                    if (span.Hours < 24 && span.Days == 0)
                    {
                        _unitOfWork.RollBack();
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, GetArtistDetail_ByID(_Artists, UserID), "ArtistDetail"));
                    }

                    //Get HashTags pictures
                    if (String.IsNullOrEmpty(_Artists.ImageURL))
                        _ImageFromSpotify = GetProfileImageFromSpotifyFeed(_Artists);
                }

                //MusicGraph1.Search_ByID _Search_ByID = null;
                MusicGraph.Datum _Search_ByID = null;

                #region "Get Artist Info Using ID"


                if (_Artists == null)
                {
                    ID = _Artists != null ? _Artists.Spotify_ID : ID;

                    _Search_ByID = await Spotify_GetArtistByID(ID);

                    #region "Commented Code"
                    //if (_Search_ByID == null)
                    //{
                    //    _unitOfWork.RollBack();
                    //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.InvalidArtistID, "Artist"));
                    //}

                    //if (_Search_ByID.status.message != "Success")
                    //{
                    //    _unitOfWork.RollBack();
                    //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.InvalidArtistID, "Artist"));
                    //}
                    #endregion
                }
                #endregion


                if (_Artists == null)
                {
                    bool isLatin = false;

                    //if (!isLatin)
                    //    isLatin = CheckMusicGraphLatin(_Search_ByID.data.id, _unitOfWork);

                    if (!isLatin)
                        isLatin = CheckSeatGeekLatin(_Search_ByID.name, _unitOfWork);

                    if (!isLatin)
                    {
                        isLatin = CheckLastResortSpotifyGenre(_Search_ByID.id);

                        if (!isLatin)
                        {
                            _unitOfWork.RollBack();
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.InvalidArtistID, "Artist"));
                        }
                    }
                }

                _unitOfWork.Commit();

                await Task.Factory.StartNew(() =>
                {
                    #region "Extract data from Third party API to Save/Update"

                    #region "Add New Rec"

                    //_unitOfWork.StartTransaction();

                    //Start- Saving New Data 
                    if (_Artists == null)
                    {
                        _Artists = new Artists();

                        _Artists.ArtistName = _Search_ByID.name;//Regex.Replace(_Search_ByID.data.name, "[^A-Za-z0-9 _]", "");
                        _Artists.Gender = "";
                        _Artists.Decade = "";//_Search_ByID.data.decade;
                        _Artists.Main_Genre = _Search_ByID.main_genre;

                        #region "Commented Code"
                        //_Artists.Musicgraph_ID = !String.IsNullOrEmpty(_Search_ByID.data.id) ? _Search_ByID.data.id : _Search_ByID.data.name;
                        //_Artists.Artist_Ref_ID = _Search_ByID.data.artist_ref_id;
                        //_Artists.Musicbrainz_ID = _Search_ByID.data.musicbrainz_id;

                        //_Artists.Youtube_ID = _Search_ByID.data.youtube_id;
                        //_Artists.Alternate_Names = _Search_ByID.data.alternate_names != null && _Search_ByID.data.alternate_names.Count > 0 ? _Search_ByID.data.alternate_names[0] : "";
                        #endregion

                        _Artists.Spotify_ID = _Search_ByID.id;

                        _Artists.RecordStatus = RecordStatus.MusicGraph.ToString();
                        _Artists.CreatedDate = DateTime.Now;
                        _Artists.ModifiedDate = DateTime.Now;

                        _ArtistsRepo.Repository.Add(_Artists);

                        //_unitOfWork.Commit();

                        #region "Get profile picture"
                        _ImageFromSpotify = GetProfileImageFromSpotifyFeed(_Artists);

                        #endregion

                        #region "SeatGeek Api Implementation"
                        Task<Dictionary<string, object>> _seatGeek = SeatGeek_GetEventByArtistName_Asyn(UserID, _Artists, _unitOfWork, true);
                        #endregion


                        #region "Eventful API Implementation"
                        Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(UserID, _Artists, _unitOfWork, true);
                        #endregion


                        #region "Get Artist Matrics (dont need this block while just updating the records)"

                        #region "Commented Code"
                        //Task<MusicGraph2.ArtistMatrics_ByID> _ArtistMatrics_ByID = MusicGrapgh_GetArtistMatrics_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);



                        ////Get Instagram ID from the MusicGraph matrcis 
                        //if (_ArtistMatrics_ByID.Result.data.instagram != null && _ArtistMatrics_ByID.Result.data.instagram.url != null && _ArtistMatrics_ByID.Result.data.instagram.url != "")
                        //{
                        //    _Artists.Instagram_Url = _ArtistMatrics_ByID.Result.data.instagram.url;
                        //    string _instaGram_ID = _ArtistMatrics_ByID.Result.data.instagram.url.Replace("http:", "https:").Replace("https://instagram.com/", "").Replace("/", "");
                        //    _Artists.Instagram_ID = _instaGram_ID;
                        //}
                        #endregion


                        #endregion

                        #region "Get Similar Artists (dont need this block while just updating the records)"
                        //Task<MusicGraph5.GetSimilarArtists_ByID> _GetSimilarArtists_ByID = MusicGrapgh_GetSimilarArtists_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);
                        Task<List<MusicGraph.Datum>> _GetSimilarArtists_ByID = Spotify_GetSimilarArtistByID(_Artists.Spotify_ID);

                        #endregion


                        #region "Instagram Api Implementation"
                        //Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, _ArtistMatrics_ByID.Result, _unitOfWork, true, null);
                        #endregion

                        #region "Instagram HashTag Images"
                        Task<Dictionary<string, object>> _ImagesFromInstagram = GetImagesFromInstagramFeed(_Artists);
                        #endregion


                        #region "Spotify Api Implementation
                        Task<Dictionary<string, object>> _Spotify = Spotify_GetSongInfo_Asyn(UserID, _Artists, _unitOfWork, true);
                        #endregion

                        _seatGeek.Wait();// wait for the function to complete 
                        _Eventful.Wait();// wait for the function to complete 
                        //_ArtistMatrics_ByID.Wait(); // wait for the completion of the matrics function
                        _GetSimilarArtists_ByID.Wait(); // wait for the function to complete
                                                        // _Instagram.Wait();// wait for the function to complete 
                        _Spotify.Wait();// wait for the function to complete 

                        _ImageFromSpotify.Wait();

                        _unitOfWork.StartTransaction();

                        string MusicGraphBio = "";

                        #region "Commented Code"
                        //if (!String.IsNullOrEmpty(_Artists.Musicgraph_ID))
                        //    MusicGraphBio = GetMusicGraphBio(_Artists.Musicgraph_ID);

                        //if (!String.IsNullOrEmpty(MusicGraphBio))
                        //    _Artists.About = MusicGraphBio;

                        // ================Commented on 09 Nov 2018 ==================
                        //SeatGeek_Asyn_Operation(UserID, _Artists, _unitOfWork, _seatGeek, false);
                        //EventFul_Asyn_Operation(UserID, _Artists, _unitOfWork, _Eventful.Result, false);
                        ////Instagram_Asyn_Operation(UserID, _Artists, null, _unitOfWork, null, _Instagram);
                        //ImagesFromInstagram_Asyn_Operation(_Artists, _ImagesFromInstagram);
                        //SpotifyImage_Asyn_Operation(_Artists, _ImageFromSpotify);
                        //Spotify_GetSimilarArtistByID_Operation(_Artists, _unitOfWork, _GetSimilarArtists_ByID.Result);
                        //_ArtistsRepo.Repository.Update(_Artists);
                        //Spotify_GetSongInfo_Asyn_Operation(0, _Artists, _unitOfWork, _Spotify);
                        // ======================================
                        #endregion


                        if (String.IsNullOrEmpty(_Artists.ImageURL))
                        {
                            if (!string.IsNullOrEmpty(_ImageFromSpotify.Result))
                            {
                                _Artists.ImageURL = _ImageFromSpotify.Result;
                                _Artists.ThumbnailURL = _ImageFromSpotify.Result;
                                _Artists.BannerImage_URL = _ImageFromSpotify.Result;
                            }
                        }

                        _unitOfWork.Commit();

                    }//End- Saving New Data 

                    #endregion

                    #region "Update Existing Record"
                    //Start - Update the data
                    else
                    {

                        //_unitOfWork.Commit();

                        //_Artists.ArtistName = Regex.Replace(_Search_ByID.data.name, "[^A-Za-z0-9 _]", "");
                        _Artists.ModifiedDate = DateTime.Now;

                        #region "SeatGeek Api Implementation"
                  //     Task<Dictionary<string, object>> _seatGeek = SeatGeek_GetEventByArtistName_Asyn(UserID, _Artists, _unitOfWork, false);
                        #endregion

                        #region "Spotify Api Implementation
                    //    Task<Dictionary<string, object>> _Spotify = Spotify_GetSongInfo_Asyn(UserID, _Artists, _unitOfWork, false);
                        #endregion

                        #region "Get Similar Artists (dont need this block while just updating the records)"
                        //Task<MusicGraph5.GetSimilarArtists_ByID> _GetSimilarArtists_ByID = MusicGrapgh_GetSimilarArtists_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);
                        Task<List<MusicGraph.Datum>> _GetSimilarArtists_ByID = Spotify_GetSimilarArtistByID(_Artists.Spotify_ID);

                        #endregion

                        #region "Eventful API Implementation"
                        //    Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(UserID, _Artists, _unitOfWork, false);
                        #endregion

                        #region "Instagram Api Implementation"
                        //Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, null, _unitOfWork, false, null);
                        #endregion

                        #region "Instagram HashTag Images"
                        //  Task<Dictionary<string, object>> _ImagesFromInstagram = GetImagesFromInstagramFeed(_Artists);
                        #endregion

                        //   _seatGeek.Wait();
                        //    _Spotify.Wait();
                        //      _Eventful.Wait();
                        // _ImagesFromInstagram.Wait();
                        _GetSimilarArtists_ByID.Wait();

                        //if (_ImageFromSpotify != null)
                        //{
                        //    _ImageFromSpotify.Wait();
                        //}
                        // _Instagram.Wait();


                        #region "Commented  on 9 Sept 2018"
                        //_unitOfWork.StartTransaction();

                        //SeatGeek_Asyn_Operation(UserID, _Artists, _unitOfWork, _seatGeek, false);
                        //EventFul_Asyn_Operation(UserID, _Artists, _unitOfWork, _Eventful.Result, false);
                        ////Instagram_Asyn_Operation(UserID, _Artists, null, _unitOfWork, null, _Instagram);
                        //ImagesFromInstagram_Asyn_Operation(_Artists, _ImagesFromInstagram);
                        //SpotifyImage_Asyn_Operation(_Artists, _ImageFromSpotify);
                        //Spotify_GetSongInfo_Asyn_Operation(0, _Artists, _unitOfWork, _Spotify);
                        Spotify_GetSimilarArtistByID_Operation(_Artists, _unitOfWork, _GetSimilarArtists_ByID.Result);

                        //_ArtistsRepo.Repository.Update(_Artists);    //---> Commented on 09 Sept 2018

                        //_unitOfWork.Commit();
                        #endregion
                    }
                    #endregion

                    #endregion
                });

                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, GetArtistDetail_ByID(_Artists, UserID), "ArtistDetail"));
                if (UserID > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, GetArtistDetail_ByID(_Artists, UserID), "ArtistDetail"));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, GetArtistDetail_ByID(_Artists), "ArtistDetail"));
                }
            }
            catch (Exception ex)
            {
                string errMsg = string.Empty;
                errMsg += ex.Message + "\n" + ex.StackTrace;

                LogHelper.CreateLog3(ex, Request);
                if (_unitOfWork.IsTransaction == true) _unitOfWork.RollBack();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message, "Artist"));
            }
        }
        public ViewArtist GetArtistDetail_ByID(Artists _Artist)
        {
            ViewArtist _ViewArtist = new ViewArtist();
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
            GenericRepository<ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<ArtistPhotos>(_unitOfWork);
            GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(_unitOfWork);
            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
            GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);
            GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

            //var _users = _UsersRepo.Repository.GetById(UserID);

            Models.UserArtists _UserArtists = null;

            _ViewArtist.ArtistID = _Artist.ArtistID;
            _ViewArtist.ArtistName = _Artist.ArtistName;
            //_ViewArtist.About = _Artist.AboutES != null;    // && _users.UserLanguage == EUserLanguage.ES.ToString() ? _Artist.AboutES : _Artist.About;
            _ViewArtist.About = null;

            if (_ViewArtist.About == null)
                _ViewArtist.About = "";

            _ViewArtist.ImageURL = _Artist.ImageURL ?? "";
            _ViewArtist.BannerImage_URL = _Artist.ImageURL ?? "";

            _ViewArtist.OnTour = _Artist.OnTour;
            _ViewArtist.Gender = _Artist.Gender ?? "";
            _ViewArtist.Main_Genre = _Artist.Main_Genre ?? "";
            _ViewArtist.Decade = _Artist.Decade ?? "";
            _ViewArtist.Alternate_Names = _Artist.Alternate_Names ?? "";
            _ViewArtist.CreatedDate = _Artist.CreatedDate;
            _ViewArtist.ModifiedDate = _Artist.ModifiedDate;
            _ViewArtist.RecordStatus = _Artist.RecordStatus;

            _UserArtists = _UserArtistsRepo.Repository.Get(p => p.ArtistID == _Artist.ArtistID && p.RecordStatus != RecordStatus.Deleted.ToString());    //&& p.UserID == UserID && p.RecordStatus != RecordStatus.Deleted.ToString());

            if (_UserArtists == null)
            {
                _ViewArtist.IsTracking = false;
            }
            else
            {
                _ViewArtist.IsTracking = true;
            }

            _ViewArtist.Spotify_URL = _Artist.Spotify_Url ?? "";
            _ViewArtist.Spotify_URL_Name = _Artist.Spotify_URL_Name ?? "";
            _ViewArtist.Spotify_Follow = "http://open.spotify.com/artist/" + _Artist.Spotify_ID;
            _ViewArtist.Instagram_Tag = _Artist.Instagram_Tag != null && _Artist.Instagram_Tag != "" ? "#" + _Artist.Instagram_Tag :
                                            (_Artist.ArtistName != "" && _Artist.ArtistName != null) ? "#" + _Artist.ArtistName : "";


            //Get Tour Dates
            _ViewArtist.TourDates = (from A in _TourDateRepo.Repository.GetAll(p => p.ArtistID == _Artist.ArtistID)
                                     join B in _VenueRepo.Repository.GetAll() on A.VenueID equals B.VenueID
                                     where !A.IsDeleted && Helper.GetUTCDateTime(DateTime.Now) < A.Tour_Utcdate && B.VenueLat.HasValue && B.VenueLong.HasValue && B.VenueLat.Value != 0 && B.VenueLong != 0
                                     select new viewTour
                                     {
                                         Address = B.Address ?? "",
                                         Announce_Date = A.Announce_Date != null ? Convert.ToDateTime(A.Announce_Date).ToString("d") : "",
                                         Datetime_Local = Convert.ToDateTime(A.Datetime_Local).ToString("g"),
                                         Display_Location = B.Display_Location ?? "",
                                         Extended_Address = B.Extended_Address ?? "",
                                         ImageURL = B.ImageURL ?? "",
                                         Postal_Code = B.Postal_Code ?? "",
                                         Slug = B.Slug ?? "",
                                         Timezone = B.Timezone ?? "",
                                         Tour_Utcdate = Convert.ToDateTime(A.Tour_Utcdate).ToString("d"),
                                         TourDateID = A.TourDateID,
                                         VenueCity = B.VenueCity ?? "",
                                         VenueCountry = B.VenueCountry ?? "",
                                         VenueID = B.VenueID,
                                         VenueLat = B.VenueLat,
                                         VenueLong = B.VenueLong,
                                         VenueName = B.VenueName ?? "",
                                         VenueState = B.VenueState ?? ""
                                     }).OrderBy(p => Convert.ToDateTime(p.Datetime_Local)).ToList();

            if (_ViewArtist.TourDates.Count > 0)
                _ViewArtist.OnTour = true;
            else
                _ViewArtist.OnTour = false;


            //Get HashTags pictures
            //GetImagesFromInstagramFeed(_Artist);

            _ViewArtist.TourPhotos = (from A in _ArtistPhotosRepo.Repository.GetAll(p => p.ArtistID == _Artist.ArtistID && p.RecordStatus == RecordStatus.Active.ToString())
                                      select new viewTourPhoto
                                      {
                                          ImageURL = A.ImageUrl,
                                          ImageThumbnailURL = A.ImageThumbnailUrl ?? "",
                                          HashTagName = A.HashTagName ?? "",
                                          PhotoID = A.PhotoID
                                      }).ToList();

            //Get Similar Artists
            _ViewArtist.ArtistRelated = (from A in _ArtistRelatedRepo.Repository.GetAll(p => p.ArtistID == _Artist.ArtistID)
                                         select new viewRelated
                                         {
                                             Musicgraph_ID = A.Musicgraph_ID,
                                             RelatedArtistName = A.RelatedArtistName ?? "",
                                             Similarity = A.Similarity
                                         }).ToList();
            return _ViewArtist;
        }
        [HttpPost]
        [Route("api/v2/Artists/ScanAllUserLibrary")]
        public async Task<HttpResponseMessage> ScanAllUserLibrary(InputAllUserSanning input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
                var serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
                dynamic obj = null;

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                Models.Users _Users = null;
                _Users = _UsersRepo.Repository.Get(p => p.UserID == input.UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "ArtistDetail"));
                }

                string strThumbnailURLfordb = null;
                string strIamgeURLfordb = null;
                string strTempImageSave = null;
                string _result;

                string _Performer_ID = null;
                string _strEvent = null;

                Models.Venue _VenuEntity = null;
                Models.TourDate _TourDateEntity = null;
                Models.ArtistRelated _ArtistRelatedEntity = null;

                string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];

                string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
                string _Instagram_access_token = ConfigurationManager.AppSettings["instagram.access_token"].ToString();

                string _Eventful_app_key = ConfigurationManager.AppSettings["Eventful_app_key"].ToString();

                string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();
                string _SeatGeek_client_secret = ConfigurationManager.AppSettings["SeatGeek_client_secret"].ToString();

                string _Spotify_Country = ConfigurationManager.AppSettings["Spotify_Country"].ToString();


                string _MG_Artist_ID = null;
                List<string> _TotalNames = new List<string>();

                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<ArtistPhotos>(_unitOfWork);
                GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(_unitOfWork);
                GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);

                GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);
                GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

                //Remove duplicates from artist list
                var names = input.ArtistNames.Select(x => x.Name);
                var uniqueNameList = names.Distinct().ToList();

                GenericRepository<MusicSource> _MusicSourceRepo = new GenericRepository<MusicSource>(_unitOfWork);
                Models.MusicSource _MusicSource = null;

                foreach (var m in input.AllMusicSource)
                {
                    _MusicSource = _MusicSourceRepo.Repository.Get(p => p.UserID == input.UserID && p.DeviceType == input.DeviceType.ToString() && p.MsourceID == m.MSourceID);

                    if (_MusicSource == null)
                    {
                        _MusicSource = new MusicSource();
                        _MusicSource.UserID = input.UserID;
                        _MusicSource.Source = m.MSourceName.ToString();
                        _MusicSource.MsourceID = m.MSourceID;
                        _MusicSource.DeviceType = input.DeviceType;
                        _MusicSource.CreatedDate = DateTime.Now;
                        _MusicSource.ModifiedDate = DateTime.Now;
                        _MusicSource.RecordStatus = RecordStatus.Active.ToString();

                        _MusicSourceRepo.Repository.Add(_MusicSource);
                    }
                    else
                    {
                        _MusicSource.ModifiedDate = DateTime.Now;
                        _MusicSourceRepo.Repository.Update(_MusicSource);
                    }
                }


                //Scanning the User Libraries
                foreach (var n in uniqueNameList)
                {
                    myArtistName _name = new myArtistName();
                    _name.Name = n;

                    bool isLatin = false;
                    TimeSpan span = TimeSpan.Zero;
                    Models.Artists _Artists = null;
                    Models.UserArtists _UserArtists = null;
                    Models.UserTourDate _UserTourDate = null;

                    //_unitOfWork.StartTransaction();

                    _Artists = _ArtistsRepo.Repository.Get(p => p.ArtistName.ToLower() == _name.Name.ToLower());

                    //Get the artist information if found in Local DB and information is not more then 24 hours old

                    if (_Artists != null)
                    {
                        span = DateTime.Now - Convert.ToDateTime(_Artists.ModifiedDate);
                        //if (span.Hours < 24 && span.Days == 0)
                        //{
                        //    _unitOfWork.RollBack();
                        //    continue;
                        //}
                    }

                    #region "Get Artist Info by Artist Name"

                    HttpWebRequest httpWebRequest;
                    HttpWebResponse httpResponse;

                    if (_Artists == null)
                    {
                        string spotifyID = "";

                        if (_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _name.Name))
                        {
                           // _unitOfWork.RollBack();
                            continue;
                        }

                        Task<List<MusicGraph.Datum>> _Search_ByName = Task.Run(async () => await Spotify_SearchArtist(_name.Name.ToString()));
                        _Search_ByName.Wait();
                        var _Search_ByName2 = _Search_ByName.Result;

                        //var _Search_ByName = await MusicGrapgh_GetArtistByName_Asyn(_name.Name.ToString()); // await for the funtion to completed

                        if (_Search_ByName2 != null)
                        {
                            if (_Search_ByName2.Count > 0)
                            {
                                if (_Search_ByName2.Count > 1)
                                {
                                    _Search_ByName2 = _Search_ByName2.OrderByDescending(x => !String.IsNullOrEmpty(x.id)).ToList();
                                    _Search_ByName2.RemoveRange(1, _Search_ByName2.Count - 1);
                                }

                                foreach (MusicGraph.Datum _datum in _Search_ByName2)
                                {
                                    _Artists = _ArtistsRepo.Repository.Get(p => p.Spotify_ID == _datum.id);
                                    if (_Artists != null)
                                    {
                                        isLatin = true;
                                        _MG_Artist_ID = _datum.id;

                                    }
                                    else
                                    {
                                        if (_name.Name.ToString().ToLower().Equals(_datum.name.ToString().ToLower()))
                                        {
                                            //Check to filter Latin artist only
                                            if (_GenreFilter.Contains(_datum.main_genre))
                                                isLatin = true;
                                            //else
                                            //    isLatin = CheckMusicGraphLatin(_datum.id, _unitOfWork);

                                            spotifyID = _datum.spotify_id;

                                            _MG_Artist_ID = _datum.id;
                                            break;
                                        }
                                    }
                                }

                                if (_MG_Artist_ID == null)
                                {
                                    //_unitOfWork.RollBack();
                                    continue;
                                }
                            }
                            else
                            {
                               // _unitOfWork.RollBack();
                                continue;
                            }
                        }
                        else
                        {
                            //_unitOfWork.RollBack();
                            continue;
                        }


                        //If music graph main genre is not latin then check seat geek sub genres for latin
                        if (!isLatin)
                        {
                            isLatin = await SeatGeek_CheckLatinGenre_Asyn(_name.Name, _unitOfWork);

                            if (!isLatin)
                            {
                                isLatin = CheckLastResortSpotifyGenre(spotifyID);

                                if (!isLatin)
                                {
                                    if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _name.Name))
                                    {
                                        _ArtistNotLatinRepo.Repository.Add(new ArtistsNotLatin() { ArtistName = _name.Name });
                                        _unitOfWork.Commit();
                                        _unitOfWork.StartTransaction();
                                    }

                                    _MG_Artist_ID = null;
                                    _unitOfWork.RollBack();
                                    continue;
                                }
                            }
                        }

                    }

                    #endregion

                    #region "Get Artist Info using ID"

                    MusicGraph.Datum _Search_ByID = null;

                    if (_Artists == null)
                    {

                        //_Search_ByID = await MusicGrapgh_GetArtistByID_Asyn(_MG_Artist_ID);// await for the funtion to completed

                        _Search_ByID = await Spotify_GetArtistByID(_MG_Artist_ID);


                        if (_Search_ByID == null)
                        {
                            //_unitOfWork.RollBack();
                            continue;
                        }

                        //if (_Search_ByID.status.message != "Success")
                        //{
                        //    _unitOfWork.RollBack();
                        //    continue;
                        //}

                        //If musicgraph ID already exists then rollback
                        if (_ArtistsRepo.Repository.AsQueryable().Any(p => p.Spotify_ID == _Search_ByID.id))
                        {
                            //_unitOfWork.RollBack();
                            continue;
                        }
                    }
                    #endregion

                    await Task.Factory.StartNew(() =>
                    {
                        #region "Extract data from Third party API to Save/Update"

                        #region "Add New Rec"
                        //Start- Saving New Data 
                        if (_Artists == null)
                        {
                            _Artists = new Artists();
                            _Artists.ArtistName = _Search_ByID.name;// Regex.Replace(_Search_ByID.data.name, "[^A-Za-z0-9 _]", "");


                            _Artists.Gender = "";//_Search_ByID.data.gender;
                            _Artists.Decade = "";//_Search_ByID.data.decade;
                            _Artists.Main_Genre = _Search_ByID.main_genre;

                            //_Artists.Musicgraph_ID = !String.IsNullOrEmpty(_Search_ByID.data.id) ? _Search_ByID.data.id : _Search_ByID.data.name;
                            _Artists.Artist_Ref_ID = "";//_Search_ByID.data.artist_ref_id;
                            _Artists.Musicbrainz_ID = "";// _Search_ByID.data.musicbrainz_id;
                            _Artists.Spotify_ID = _Search_ByID.id;

                            // _Artists.Youtube_ID = _Search_ByID.data.youtube_id;
                            // _Artists.Alternate_Names = _Search_ByID.data.alternate_names != null && _Search_ByID.data.alternate_names.Count > 0 ? _Search_ByID.data.alternate_names[0] : "";

                            _Artists.RecordStatus = RecordStatus.MusicGraph.ToString();
                            _Artists.CreatedDate = DateTime.Now;
                            _Artists.ModifiedDate = DateTime.Now;

                            _ArtistsRepo.Repository.Add(_Artists);

                            GetProfileImageFromSpotifyFeed(_Artists);

                            _UserArtists = new UserArtists();
                            _UserArtists.UserID = input.UserID;
                            _UserArtists.ArtistID = _Artists.ArtistID;
                            _UserArtists.CreatedDate = DateTime.Now;
                            _UserArtists.RecordStatus = RecordStatus.Active.ToString();

                            _UserArtistsRepo.Repository.Add(_UserArtists);
                            _unitOfWork.Commit();

                            #region "Get Similar Artists (dont need this block while just updating the records)"
                            //Task<Dictionary<string,object>> _GetSimilarArtists_ByID = MusicGrapgh_GetSimilarArtists_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);
                            Task<List<MusicGraph.Datum>> _GetSimilarArtists_ByID = Spotify_GetSimilarArtistByID(_Artists.Spotify_ID);
                            #endregion

                            #region "Get Artist Matrics (dont need this block while just updating the records)"

                            //Task<MusicGraph2.ArtistMatrics_ByID> _ArtistMatrics_ByID = MusicGrapgh_GetArtistMatrics_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);

                            //_ArtistMatrics_ByID.Wait(); // wait for the task to be completed

                            ////Get Instagram ID from the MusicGraph matrcis 
                            //if (_ArtistMatrics_ByID.Result.data.instagram != null && _ArtistMatrics_ByID.Result.data.instagram.url != null && _ArtistMatrics_ByID.Result.data.instagram.url != "")
                            //{
                            //    _Artists.Instagram_Url = _ArtistMatrics_ByID.Result.data.instagram.url;
                            //    string _instaGram_ID = _ArtistMatrics_ByID.Result.data.instagram.url.Replace("http:", "https:").Replace("https://instagram.com/", "").Replace("/", "");
                            //    _Artists.Instagram_ID = _instaGram_ID;
                            //}

                            #endregion

                            #region "SeatGeek Api Implementation"
                            Task<Dictionary<string, object>> _seatGeek = SeatGeek_GetEventByArtistName_Asyn(input.UserID, _Artists, _unitOfWork, true, true);
                            #endregion

                            #region "Eventful API Implementation"
                            Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(input.UserID, _Artists, _unitOfWork, true);
                            #endregion

                            #region "Instagram Api Implementation"
                            // Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(input.UserID, _Artists, _ArtistMatrics_ByID.Result, _unitOfWork, true, null);
                            #endregion

                            #region "Instagram HashTag Images"
                            Task<Dictionary<string, object>> _ImagesFromInstagram = GetImagesFromInstagramFeed(_Artists);
                            #endregion

                            #region "Spotify Api Implementation
                            Task<Dictionary<string, object>> _Spotify = Spotify_GetSongInfo_Asyn(input.UserID, _Artists, _unitOfWork, true);
                            #endregion

                            _Eventful.Wait();
                            //_Instagram.Wait();
                            _GetSimilarArtists_ByID.Wait();
                            _seatGeek.Wait();
                            _Spotify.Wait();
                            _ImagesFromInstagram.Wait();

                            if (_TourDateRepo.Repository.AsQueryable().Any(x => x.ArtistID == _Artists.ArtistID && x.Datetime_Local >= DateTime.Now))
                            {
                                _Artists.OnTour = true;
                            }

                            string MusicGraphBio = "";
                            //if (!String.IsNullOrEmpty(_Artists.Musicgraph_ID))
                            //    MusicGraphBio = GetMusicGraphBio(_Artists.Musicgraph_ID);
                            //if (!String.IsNullOrEmpty(MusicGraphBio))
                            //    _Artists.About = MusicGraphBio;

                            _unitOfWork.StartTransaction();

                            SeatGeek_Asyn_Operation(input.UserID, _Artists, _unitOfWork, _seatGeek);
                            EventFul_Asyn_Operation(input.UserID, _Artists, _unitOfWork, _Eventful.Result, false);
                            ImagesFromInstagram_Asyn_Operation(_Artists, _ImagesFromInstagram);

                            //Instagram_Asyn_Operation(input.UserID, _Artists, null, _unitOfWork, null, _Instagram);
                            Spotify_GetSongInfo_Asyn_Operation(input.UserID, _Artists, _unitOfWork, _Spotify);
                            Spotify_GetSimilarArtistByID_Operation(_Artists, _unitOfWork, _GetSimilarArtists_ByID.Result);

                            _unitOfWork.Commit();

                        }//End- Saving New Data 

                        #endregion

                        #region "Update Existing Record"
                        //Start - Update the data
                        else
                        {
                            _Artists.ModifiedDate = DateTime.Now;

                            _UserArtists = _UserArtistsRepo.Repository.Get(p => p.UserID == input.UserID && p.ArtistID == _Artists.ArtistID);

                            if (_UserArtists == null)
                            {
                                _UserArtists = new UserArtists();
                                _UserArtists.UserID = input.UserID;
                                _UserArtists.ArtistID = _Artists.ArtistID;
                                _UserArtists.CreatedDate = DateTime.Now;
                                _UserArtists.RecordStatus = RecordStatus.Active.ToString();

                                _UserArtistsRepo.Repository.Add(_UserArtists);
                            }


                            //if (span.Days > 1 || span.Hours > 24)
                            //{
                            #region "SeatGeek Api Implementation"
                            Task<Dictionary<string, object>> _seatGeek = SeatGeek_GetEventByArtistName_Asyn(input.UserID, _Artists, _unitOfWork, false, true);
                            #endregion

                            #region "Eventful API Implementation"
                            Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(input.UserID, _Artists, _unitOfWork, false);
                            #endregion

                            #region "Spotify Api Implementation
                            Task<Dictionary<string, object>> _Spotify = Spotify_GetSongInfo_Asyn(input.UserID, _Artists, _unitOfWork, false);
                            #endregion

                            #region "Instagram Api Implementation"
                            //Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(input.UserID, _Artists, null, _unitOfWork, false, null);
                            #endregion

                            _Eventful.Wait();
                            _Spotify.Wait();
                            //_Instagram.Wait();
                            _seatGeek.Wait();

                            if (_TourDateRepo.Repository.GetAll(x => x.ArtistID == _Artists.ArtistID && x.Datetime_Local >= DateTime.Now).Count() > 0)
                                _Artists.OnTour = true;


                            _unitOfWork.StartTransaction();
                            SeatGeek_Asyn_Operation(input.UserID, _Artists, _unitOfWork, _seatGeek);
                            EventFul_Asyn_Operation(input.UserID, _Artists, _unitOfWork, _Eventful.Result, false);
                            //Instagram_Asyn_Operation(input.UserID, _Artists, null, _unitOfWork, null, _Instagram);
                            Spotify_GetSongInfo_Asyn_Operation(input.UserID, _Artists, _unitOfWork, _Spotify);
                            //}
                            _ArtistsRepo.Repository.Update(_Artists);

                            _unitOfWork.Commit();
                        }
                        #endregion

                        #endregion
                    });

                    _TotalNames.Add(_name.Name);
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _TotalNames.Count, "Scanned"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                if (_unitOfWork.IsTransaction == true) _unitOfWork.RollBack();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }

        [HttpPost]
        [Route("api/v2/Artists/ScanUserLibrary")]
        public async Task<HttpResponseMessage> ScanUserLibrary(InputUserSanning input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }
                var serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
                dynamic obj = null;

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                Models.Users _Users = null;
                _Users = _UsersRepo.Repository.Get(p => p.UserID == input.UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "ArtistDetail"));
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

                string _Performer_ID = null;
                string _strEvent = null;

                Models.Venue _VenuEntity = null;
                Models.TourDate _TourDateEntity = null;
                Models.ArtistRelated _ArtistRelatedEntity = null;

                string _MG_Artist_ID = null;
                List<string> _TotalNames = new List<string>();

                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<ArtistPhotos>(_unitOfWork);
                GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(_unitOfWork);

                GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);
                GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

                //Remove duplicates from artist list
                var names = input.ArtistNames.Select(x => x.Name);
                var uniqueNameList = names.Distinct().ToList();

                GenericRepository<MusicSource> _MusicSourceRepo = new GenericRepository<MusicSource>(_unitOfWork);
                Models.MusicSource _MusicSource = null;
                _MusicSource = _MusicSourceRepo.Repository.Get(p => p.UserID == input.UserID && p.DeviceType == input.DeviceType.ToString() && p.MsourceID == input.MSourceID);

                if (_MusicSource == null)
                {
                    _MusicSource = new MusicSource();
                    _MusicSource.UserID = input.UserID;
                    _MusicSource.Source = input.MusicSource.ToString();
                    _MusicSource.MsourceID = input.MSourceID;
                    _MusicSource.DeviceType = input.DeviceType;
                    _MusicSource.CreatedDate = DateTime.Now;
                    _MusicSource.ModifiedDate = DateTime.Now;
                    _MusicSource.RecordStatus = RecordStatus.Active.ToString();

                    _MusicSourceRepo.Repository.Add(_MusicSource);
                }
                else
                {
                    _MusicSource.ModifiedDate = DateTime.Now;
                    _MusicSourceRepo.Repository.Update(_MusicSource);
                }

                //Scanning the User Libraries
                foreach (var n in uniqueNameList)
                {
                    myArtistName _name = new myArtistName();
                    _name.Name = n;

                    bool isLatin = false;
                    TimeSpan span = TimeSpan.Zero;
                    Models.Artists _Artists = null;
                    Models.UserArtists _UserArtists = null;
                    Models.UserTourDate _UserTourDate = null;

                    _unitOfWork.StartTransaction();

                    _Artists = _ArtistsRepo.Repository.Get(p => p.ArtistName.ToLower() == _name.Name.ToLower());

                    //Get the artist information if found in Local DB and information is not more then 24 hours old

                    if (_Artists != null)
                    {
                        span = DateTime.Now - Convert.ToDateTime(_Artists.ModifiedDate);
                        //if (span.Hours < 24 && span.Days == 0)
                        //{
                        //    _unitOfWork.RollBack();
                        //    continue;
                        //}
                    }

                    #region "Get Artist Info by Artist Name"

                    HttpWebRequest httpWebRequest;
                    HttpWebResponse httpResponse;

                    if (_Artists == null)
                    {
                        string spotifyId = "";

                        if (_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _name.Name))
                        {
                            _unitOfWork.RollBack();
                            continue;
                        }

                        Task<List<MusicGraph.Datum>> _search = Task.Run(async () => await Spotify_SearchArtist(_name.Name.ToString()));
                        _search.Wait();
                        var _Search_ByName = _search.Result;

                        //var _Search_ByName = await MusicGrapgh_GetArtistByName_Asyn(_name.Name.ToString()); // await for the funtion to completed

                        if (_Search_ByName != null)
                        {
                            if (_Search_ByName.Count > 0)
                            {
                                if (_Search_ByName.Count > 1)
                                {
                                    _Search_ByName = _Search_ByName.OrderByDescending(x => !String.IsNullOrEmpty(x.id)).ToList();
                                    _Search_ByName.RemoveRange(1, _Search_ByName.Count - 1);
                                }

                                foreach (MusicGraph.Datum _datum in _Search_ByName)
                                {
                                    _Artists = _ArtistsRepo.Repository.Get(p => p.Spotify_ID == _datum.id);
                                    if (_Artists != null)
                                    {
                                        isLatin = true;
                                        _MG_Artist_ID = _datum.id;

                                    }
                                    else
                                    {
                                        if (_name.Name.ToString().ToLower().Equals(_datum.name.ToString().ToLower()))
                                        {
                                            //Check to filter Latin artist only
                                            if (_GenreFilter.Contains(_datum.main_genre))
                                                isLatin = true;
                                            else
                                                isLatin = CheckSeatGeekLatin(_datum.id, _unitOfWork);

                                            spotifyId = _datum.spotify_id;

                                            _MG_Artist_ID = _datum.id;
                                            break;
                                        }
                                    }

                                }

                                if (_MG_Artist_ID == null)
                                {
                                    _unitOfWork.RollBack();
                                    continue;
                                }
                            }
                            else
                            {
                                _unitOfWork.RollBack();
                                continue;
                            }



                        }
                        else
                        {
                            _unitOfWork.RollBack();
                            continue;
                        }


                        //If music graph main genre is not latin then check seat geek sub genres for latin
                        if (!isLatin)
                        {
                            isLatin = await SeatGeek_CheckLatinGenre_Asyn(_name.Name, _unitOfWork);

                            if (!isLatin)
                            {
                                isLatin = CheckLastResortSpotifyGenre(spotifyId);

                                if (!isLatin)
                                {
                                    if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _name.Name))
                                    {
                                        _ArtistNotLatinRepo.Repository.Add(new ArtistsNotLatin() { ArtistName = _name.Name });
                                        _unitOfWork.Commit();
                                        _unitOfWork.StartTransaction();
                                    }

                                    _MG_Artist_ID = null;
                                    _unitOfWork.RollBack();
                                    continue;
                                }
                            }
                        }

                    }

                    #endregion

                    #region "Get Artist Info using ID"

                    MusicGraph.Datum _Search_ByID = null;

                    if (_Artists == null)
                    {

                        _Search_ByID = await Spotify_GetArtistByID(_MG_Artist_ID);

                        // _Search_ByID = await MusicGrapgh_GetArtistByID_Asyn(_MG_Artist_ID);// await for the funtion to completed


                        if (_Search_ByID == null)
                        {
                            _unitOfWork.RollBack();
                            continue;
                        }

                        //if (_Search_ByID.status.message != "Success")
                        //{
                        //    _unitOfWork.RollBack();
                        //    continue;
                        //}

                        //If musicgraph ID already exists then rollback
                        if (_ArtistsRepo.Repository.AsQueryable().Any(p => p.Spotify_ID == _MG_Artist_ID))
                        {
                            _unitOfWork.RollBack();
                            continue;
                        }


                    }
                    #endregion

                    await Task.Factory.StartNew(() =>
                    {
                        #region "Extract data from Thirdparty API to Save/Update"

                        #region "Add New Rec"
                        //Start- Saving New Data 
                        if (_Artists == null)
                        {
                            _unitOfWork.StartTransaction();
                            _Artists = new Artists();
                            _Artists.ArtistName = _Search_ByID.name;// Regex.Replace(_Search_ByID.data.name, "[^A-Za-z0-9 _]", "");

                            // _Artists.Gender = _Search_ByID.data.gender;
                            // _Artists.Decade = _Search_ByID.data.decade;

                            _Artists.Main_Genre = _Search_ByID.main_genre;

                            //_Artists.Musicgraph_ID = !String.IsNullOrEmpty(_Search_ByID.data.id) ? _Search_ByID.data.id : _Search_ByID.data.name;
                            //_Artists.Artist_Ref_ID = _Search_ByID.data.artist_ref_id;
                            //_Artists.Musicbrainz_ID = _Search_ByID.data.musicbrainz_id;

                            _Artists.Spotify_ID = _Search_ByID.id;

                            //_Artists.Youtube_ID = _Search_ByID.data.youtube_id;
                            //_Artists.Alternate_Names = _Search_ByID.data.alternate_names != null && _Search_ByID.data.alternate_names.Count > 0 ? _Search_ByID.data.alternate_names[0] : "";



                            _Artists.RecordStatus = RecordStatus.MusicGraph.ToString();
                            _Artists.CreatedDate = DateTime.Now;
                            _Artists.ModifiedDate = DateTime.Now;

                            _ArtistsRepo.Repository.Add(_Artists);

                            GetProfileImageFromSpotifyFeed(_Artists);

                            _UserArtists = new UserArtists();
                            _UserArtists.UserID = input.UserID;
                            _UserArtists.ArtistID = _Artists.ArtistID;
                            _UserArtists.CreatedDate = DateTime.Now;
                            _UserArtists.RecordStatus = RecordStatus.Active.ToString();

                            _UserArtistsRepo.Repository.Add(_UserArtists);

                            _unitOfWork.Commit();


                            #region "Get Similar Artists (dont need this block while just updating the records)"
                            //Task<Dictionary<string,object>> _GetSimilarArtists_ByID = MusicGrapgh_GetSimilarArtists_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);
                            Task<List<MusicGraph.Datum>> _GetSimilarArtists_ByID = Spotify_GetSimilarArtistByID(_Artists.Spotify_ID);
                            #endregion

                            #region "Get Artist Matrics (dont need this block while just updating the records)"
                            //Task<MusicGraph2.ArtistMatrics_ByID> _ArtistMatrics_ByID = MusicGrapgh_GetArtistMatrics_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);

                            //_ArtistMatrics_ByID.Wait(); // wait for the task to be completed

                            ////Get Instagram ID from the MusicGraph matrcis 
                            //if (_ArtistMatrics_ByID.Result.data.instagram != null && _ArtistMatrics_ByID.Result.data.instagram.url != null && _ArtistMatrics_ByID.Result.data.instagram.url != "")
                            //{
                            //    _Artists.Instagram_Url = _ArtistMatrics_ByID.Result.data.instagram.url;
                            //    string _instaGram_ID = _ArtistMatrics_ByID.Result.data.instagram.url.Replace("http:", "https:").Replace("https://instagram.com/", "").Replace("/", "");
                            //    _Artists.Instagram_ID = _instaGram_ID;
                            //}

                            #endregion

                            #region "SeatGeek Api Implementation"
                            Task<Dictionary<string, object>> _seatGeek = SeatGeek_GetEventByArtistName_Asyn(input.UserID, _Artists, _unitOfWork, true, true);
                            #endregion

                            #region "Eventful API Implementation"
                            Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(input.UserID, _Artists, _unitOfWork, true);
                            #endregion

                            #region "Instagram Api Implementation"
                            //Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(input.UserID, _Artists, _ArtistMatrics_ByID.Result, _unitOfWork, true, null);
                            #endregion

                            #region "Instagram HashTag Images"
                            Task<Dictionary<string, object>> _ImagesFromInstagram = GetImagesFromInstagramFeed(_Artists);
                            #endregion


                            #region "Spotify Api Implementation
                            Task<Dictionary<string, object>> _Spotify = Spotify_GetSongInfo_Asyn(input.UserID, _Artists, _unitOfWork, true);
                            #endregion

                            _GetSimilarArtists_ByID.Wait();
                            _seatGeek.Wait();
                            _Eventful.Wait();
                            //_Instagram.Wait();
                            _Spotify.Wait();

                            _unitOfWork.StartTransaction();

                            if (_TourDateRepo.Repository.GetAll(x => x.ArtistID == _Artists.ArtistID && x.Datetime_Local >= DateTime.Now).Count > 0)
                            {
                                _Artists.OnTour = true;
                            }

                            //string MusicGraphBio = "";
                            //if (!String.IsNullOrEmpty(_Artists.Musicgraph_ID))
                            //    MusicGraphBio = GetMusicGraphBio(_Artists.Musicgraph_ID);
                            //if (!String.IsNullOrEmpty(MusicGraphBio))
                            //    _Artists.About = MusicGraphBio;

                            SeatGeek_Asyn_Operation(input.UserID, _Artists, _unitOfWork, _seatGeek);
                            EventFul_Asyn_Operation(input.UserID, _Artists, _unitOfWork, _Eventful.Result, false);
                            //Instagram_Asyn_Operation(input.UserID, _Artists, null, _unitOfWork, null, _Instagram);
                            ImagesFromInstagram_Asyn_Operation(_Artists, _ImagesFromInstagram);
                            Spotify_GetSongInfo_Asyn_Operation(input.UserID, _Artists, _unitOfWork, _Spotify);

                            _ArtistsRepo.Repository.Update(_Artists);

                            _unitOfWork.Commit();
                        }//End- Saving New Data 

                        #endregion

                        #region "Update Existing Record"
                        //Start - Update the data
                        else
                        {
                            _unitOfWork.StartTransaction();

                            _Artists.ModifiedDate = DateTime.Now;

                            _UserArtists = _UserArtistsRepo.Repository.Get(p => p.UserID == input.UserID && p.ArtistID == _Artists.ArtistID);

                            if (_UserArtists == null)
                            {
                                _UserArtists = new UserArtists();
                                _UserArtists.UserID = input.UserID;
                                _UserArtists.ArtistID = _Artists.ArtistID;
                                _UserArtists.CreatedDate = DateTime.Now;
                                _UserArtists.RecordStatus = RecordStatus.Active.ToString();

                                _UserArtistsRepo.Repository.Add(_UserArtists);
                            }

                            //if (span.Days > 1 || span.Hours > 24)
                            //{
                            #region "Instagram Api Implementation"
                            //Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(input.UserID, _Artists, null, _unitOfWork, false, null);
                            #endregion

                            #region "SeatGeek Api Implementation"
                            Task<Dictionary<string, object>> _seatGeek = SeatGeek_GetEventByArtistName_Asyn(input.UserID, _Artists, _unitOfWork, false, true);
                            #endregion

                            #region "Eventful API Implementation"
                            Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(input.UserID, _Artists, _unitOfWork, false);
                            #endregion

                            #region "Spotify Api Implementation
                            Task<Dictionary<string, object>> _Spotify = Spotify_GetSongInfo_Asyn(input.UserID, _Artists, _unitOfWork, false);
                            #endregion

                            //_Instagram.Wait();
                            _Eventful.Wait();
                            _Spotify.Wait();
                            _seatGeek.Wait();

                            if (_TourDateRepo.Repository.GetAll(x => x.ArtistID == _Artists.ArtistID && x.Datetime_Local >= DateTime.Now).Count > 0)
                                _Artists.OnTour = true;

                            SeatGeek_Asyn_Operation(input.UserID, _Artists, _unitOfWork, _seatGeek);
                            EventFul_Asyn_Operation(input.UserID, _Artists, _unitOfWork, _Eventful.Result, false);
                            //Instagram_Asyn_Operation(input.UserID, _Artists, null, _unitOfWork, null, _Instagram);
                            Spotify_GetSongInfo_Asyn_Operation(input.UserID, _Artists, _unitOfWork, _Spotify);
                            //}
                            _ArtistsRepo.Repository.Update(_Artists);
                            _unitOfWork.Commit();


                        }
                        #endregion

                        #endregion
                    });

                    _TotalNames.Add(_name.Name);
                }

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _TotalNames.Count, "Scanned"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                if (_unitOfWork.IsTransaction == true) _unitOfWork.RollBack();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }

        public ViewArtist GetArtistDetail_ByID(Artists _Artist, Int32 UserID)
        {
            ViewArtist _ViewArtist = new ViewArtist();
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
            GenericRepository<ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<ArtistPhotos>(_unitOfWork);
            GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(_unitOfWork);
            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
            GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);
            GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

            var _users = _UsersRepo.Repository.GetById(UserID);

            Models.UserArtists _UserArtists = null;

            _ViewArtist.ArtistID = _Artist.ArtistID;
            _ViewArtist.ArtistName = _Artist.ArtistName;
            if (_users == null)
            {
                _ViewArtist.About = _Artist.AboutES != null ? _Artist.AboutES : _Artist.About;
            }
            else
            {
                _ViewArtist.About = _Artist.AboutES != null && _users.UserLanguage == EUserLanguage.ES.ToString() ? _Artist.AboutES : _Artist.About;
            }

            if (_ViewArtist.About == null)
                _ViewArtist.About = "";

            _ViewArtist.ImageURL = _Artist.ImageURL ?? "";
            _ViewArtist.BannerImage_URL = _Artist.ImageURL ?? "";

            _ViewArtist.OnTour = _Artist.OnTour;
            _ViewArtist.Gender = _Artist.Gender ?? "";
            _ViewArtist.Main_Genre = _Artist.Main_Genre ?? "";
            _ViewArtist.Decade = _Artist.Decade ?? "";
            _ViewArtist.Alternate_Names = _Artist.Alternate_Names ?? "";
            _ViewArtist.CreatedDate = _Artist.CreatedDate;
            _ViewArtist.ModifiedDate = _Artist.ModifiedDate;
            _ViewArtist.RecordStatus = _Artist.RecordStatus;
            if (_users == null)
            {
                _UserArtists = _UserArtistsRepo.Repository.Get(p => p.ArtistID == _Artist.ArtistID && p.RecordStatus != RecordStatus.Deleted.ToString());
            }
            else
            {
                _UserArtists = _UserArtistsRepo.Repository.Get(p => p.ArtistID == _Artist.ArtistID && p.UserID == UserID && p.RecordStatus != RecordStatus.Deleted.ToString());
            }

            if (_UserArtists == null)
            {
                _ViewArtist.IsTracking = false;
            }
            else
            {
                _ViewArtist.IsTracking = true;
            }

            _ViewArtist.Spotify_URL = _Artist.Spotify_Url ?? "";
            _ViewArtist.Spotify_URL_Name = _Artist.Spotify_URL_Name ?? "";
            _ViewArtist.Spotify_Follow = "http://open.spotify.com/artist/" + _Artist.Spotify_ID;
            _ViewArtist.Instagram_Tag = _Artist.Instagram_Tag != null && _Artist.Instagram_Tag != "" ? "#" + _Artist.Instagram_Tag :
                                            (_Artist.ArtistName != "" && _Artist.ArtistName != null) ? "#" + _Artist.ArtistName : "";

            //    Models.UserTracking _UserTracking = null;
            //_UserTracking = _UserTrackingRepo.Repository.Get(p => p.ArtistID == _Artist.ArtistID);

            //if (_UserTracking != null)
            //{
            //    _ViewArtist.IsTracking = true;
            //}
            //else {
            //    _ViewArtist.IsTracking = false;
            //}

            //Get Tour Dates
            _ViewArtist.TourDates = (from A in _TourDateRepo.Repository.GetAll(p => p.ArtistID == _Artist.ArtistID)
                                     join B in _VenueRepo.Repository.GetAll() on A.VenueID equals B.VenueID
                                     where !A.IsDeleted && Helper.GetUTCDateTime(DateTime.Now) < A.Tour_Utcdate && B.VenueLat.HasValue && B.VenueLong.HasValue && B.VenueLat.Value != 0 && B.VenueLong != 0
                                     select new viewTour
                                     {
                                         Address = B.Address ?? "",
                                         Announce_Date = A.Announce_Date != null ? Convert.ToDateTime(A.Announce_Date).ToString("d") : "",
                                         Datetime_Local = A.Datetime_Local !=null? Convert.ToDateTime(A.Datetime_Local).ToString("g"): Convert.ToDateTime(A.Tour_Utcdate).ToString("g"),
                                         Display_Location = B.Display_Location ?? "",
                                         Extended_Address = B.Extended_Address ?? "",
                                         ImageURL = B.ImageURL ?? "",
                                         Postal_Code = B.Postal_Code ?? "",
                                         Slug = B.Slug ?? "",
                                         Timezone = B.Timezone ?? "",
                                         Tour_Utcdate = Convert.ToDateTime(A.Tour_Utcdate).ToString("d"),
                                         TourDateID = A.TourDateID,
                                         VenueCity = B.VenueCity ?? "",
                                         VenueCountry = B.VenueCountry ?? "",
                                         VenueID = B.VenueID,
                                         VenueLat = B.VenueLat,
                                         VenueLong = B.VenueLong,
                                         VenueName = B.VenueName ?? "",
                                         VenueState = B.VenueState ?? ""
                                     }).OrderBy(p => Convert.ToDateTime(p.Datetime_Local)).ToList();

            if (_ViewArtist.TourDates.Count > 0)
                _ViewArtist.OnTour = true;
            else
                _ViewArtist.OnTour = false;


            //Get HashTags pictures
            //GetImagesFromInstagramFeed(_Artist);

            _ViewArtist.TourPhotos = (from A in _ArtistPhotosRepo.Repository.GetAll(p => p.ArtistID == _Artist.ArtistID && p.RecordStatus == RecordStatus.Active.ToString())
                                      select new viewTourPhoto
                                      {
                                          ImageURL = A.ImageUrl,
                                          ImageThumbnailURL = A.ImageThumbnailUrl ?? "",
                                          HashTagName = A.HashTagName ?? "",
                                          PhotoID = A.PhotoID
                                      }).ToList();

            //Get Similar Artists
            _ViewArtist.ArtistRelated = (from A in _ArtistRelatedRepo.Repository.GetAll(p => p.ArtistID == _Artist.ArtistID)
                                         select new viewRelated
                                         {
                                             Musicgraph_ID = A.Musicgraph_ID,
                                             RelatedArtistName = A.RelatedArtistName ?? "",
                                             Similarity = A.Similarity
                                         }).ToList();
            return _ViewArtist;
        }



        //[HttpPost]
        //[Route("api/v2/Artists/UpdateArtsistTracking")]
        //public HttpResponseMessage UpdateArtsistTracking(InputUpdateArtistTracking input)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
        //        }

        //        GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
        //        GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);

        //        Models.Artists _Artists = null;
        //        Models.UserArtists _UserArtists = null;

        //        _Artists = _ArtistsRepo.Repository.Get(p => p.ArtistID == input.ArtistID);
        //        if (_Artists == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.ArtistNotFound));
        //        }

        //        _UserArtists = _UserArtistsRepo.Repository.Get(p => p.ArtistID == input.ArtistID && p.UserID == input.UserID);

        //        if (input.Tracking == true)
        //        {
        //            if (_UserArtists == null)
        //            {

        //                _UserArtists = new UserArtists();
        //                _UserArtists.UserID = input.UserID;
        //                _UserArtists.ArtistID = input.ArtistID;
        //                _UserArtists.CreatedDate = DateTime.Now;
        //                _UserArtists.RecordStatus = RecordStatus.Active.ToString();

        //                _UserArtistsRepo.Repository.Add(_UserArtists);
        //            }

        //        }
        //        else
        //        {
        //            if (_UserArtists != null)
        //            {
        //                _UserArtistsRepo.Repository.DeletePermanent(_UserArtists.UserArtistsID);
        //            }
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, null));

        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));

        //    }
        //}


        [HttpGet]
        [Route("api/v2/Artists/GetArtistsList")]
        public HttpResponseMessage GetArtistsList(Int32 UserID, Int16 Pageindex, Int16 Pagesize)
        {
            DataSet dsArtistsList = new DataSet();
            try
            {
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                MusikaEntities ms = new MusikaEntities();
                //if (UserID == null)
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
                //}
                if (UserID <= 0)
                {
                    UserID = 0;
                }
                dsArtistsList = new Musika.Repository.SPRepository.SpRepository().GetAritstsByUserId(UserID);

                var _list = dsArtistsList.Tables[0].AsEnumerable().Select(
                              dataRow => new ViewArtistListByUserId
                              {
                                  ArtistID = dataRow.Field<Int32>("ArtistID"),
                                  ArtistName = dataRow.Field<String>("ArtistName").Trim(),
                                  ImageURL = dataRow.Field<String>("ImageURL"),
                                  BannerImage_URL = dataRow.Field<String>("BannerImage_URL"),
                                  OnTour = dataRow.Field<bool>("OnTour"),
                                  FirstLetter = dataRow.Field<String>("FirstLetter")

                              }).ToPagedList(Pageindex - 1, Pagesize).ToList();

                #endregion
                foreach (var l in _list)
                {
                    //var onTour = _TourDateRepo.Repository.AsQueryable().Any(x => x.ArtistID == l.ArtistID && x.Tour_Utcdate.Value > DateTime.UtcNow && !x.IsDeleted);
                    var onTour = ms.TourDate.Any(x => x.ArtistID == l.ArtistID && x.Tour_Utcdate.Value > DateTime.UtcNow && !x.IsDeleted);
                    l.OnTour = onTour;
                }

                _list = _list.OrderBy(p => p.FirstLetter).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list, "ArtistList"));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));

            }
            #region "Old Code"
            //try
            //{
            //    if (UserID == null)
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
            //    }

            //    GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
            //    Models.Users _Users = null;

            //    _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

            //    if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "EventsDetail"));
            //    }

            //    GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
            //    GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);
            //    GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);

            //    // check if user artist list is not empty, if empty then fill it up with default artists
            //    var _check = _UserArtistsRepo.Repository.GetAll(p => p.UserID == UserID);
            //    if (_check != null && _check.Count < 15)
            //    {
            //        var _artistIds = _ArtistsRepo.Repository.GetAll(p => p.Isdefault == true && p.RecordStatus != RecordStatus.Deleted.ToString()).Select(p => p.ArtistID);
            //        foreach (Int32 _artistid in _artistIds)
            //        {
            //            var _AlreadyExists = _UserArtistsRepo.Repository.Get(p => p.UserID == UserID && p.ArtistID == _artistid);

            //            if (_AlreadyExists == null)
            //            {
            //                var _UserArtists = new UserArtists();
            //                _UserArtists.UserID = UserID;
            //                _UserArtists.ArtistID = _artistid;
            //                _UserArtists.CreatedDate = DateTime.Now;
            //                _UserArtists.RecordStatus = RecordStatus.Active.ToString();
            //                _UserArtistsRepo.Repository.Add(_UserArtists);
            //            }
            //        }
            //    }

            //    var _list = (from A in _UserArtistsRepo.Repository.GetAll(p => p.UserID == UserID)
            //                 join B in _ArtistsRepo.Repository.GetAll() on A.ArtistID equals B.ArtistID
            //                 select new ViewArtistList
            //                 {
            //                     ArtistID = B.ArtistID,
            //                     ArtistName = B.ArtistName ?? "",
            //                     ImageURL = B.ThumbnailURL ?? B.ImageURL ?? "",
            //                     BannerImage_URL = B.BannerImage_URL ?? "",
            //                     OnTour = B.OnTour
            //                 }).ToList();

            //    foreach (var l in _list)
            //    {
            //        var onTour = _TourDateRepo.Repository.AsQueryable().Any(x => x.ArtistID == l.ArtistID && x.Tour_Utcdate.Value > DateTime.UtcNow && !x.IsDeleted);
            //        l.OnTour = onTour;
            //    }

            //    _list = _list.OrderBy(p => p.FirstLetter).ToList();

            //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list, "ArtistList"));
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.CreateLog3(ex, Request);
            //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            //}
            #endregion
        }

        [HttpGet]
        [Route("api/v2/Artists/GetArtistsList1")]
        public HttpResponseMessage GetArtistsList1(Int32 UserID, bool onTour, Int16 Pageindex, Int16 Pagesize)
        {
            DataSet dsArtistsList = new DataSet();
            try
            {
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                MusikaEntities ms = new MusikaEntities();
                //if (UserID == null)
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
                //}
                if (UserID <= 0)
                {
                    UserID = 0;
                }
                string _onTour = onTour.ToString();
                dsArtistsList = new Musika.Repository.SPRepository.SpRepository().GetAritstsByUserId1(UserID, _onTour.ToLower());

                var _list = dsArtistsList.Tables[0].AsEnumerable().Select(
                              dataRow => new ViewArtistListByUserId
                              {
                                  ArtistID = dataRow.Field<Int32>("ArtistID"),
                                  ArtistName = dataRow.Field<String>("ArtistName").Trim(),
                                  ImageURL = dataRow.Field<String>("ImageURL"),
                                  BannerImage_URL = dataRow.Field<String>("BannerImage_URL"),
                                  OnTour = dataRow.Field<bool>("OnTour"),
                                  FirstLetter = dataRow.Field<String>("FirstLetter")

                              }).ToPagedList(Pageindex - 1, Pagesize).ToList();

                if (_onTour.ToLower() == "true")
                {
                    foreach (var l in _list)
                    {
                        l.OnTour = true;
                    }
                }
                else
                {
                    foreach (var l in _list)
                    {
                        //var onTour = _TourDateRepo.Repository.AsQueryable().Any(x => x.ArtistID == l.ArtistID && x.Tour_Utcdate.Value > DateTime.UtcNow && !x.IsDeleted);
                        var onTour1 = ms.TourDate.Any(x => x.ArtistID == l.ArtistID && x.Tour_Utcdate.Value > DateTime.UtcNow && !x.IsDeleted);
                        l.OnTour = onTour1;
                    }
                }
                //foreach (var l in _list)
                //{
                //    //var onTour = _TourDateRepo.Repository.AsQueryable().Any(x => x.ArtistID == l.ArtistID && x.Tour_Utcdate.Value > DateTime.UtcNow && !x.IsDeleted);
                //    var onTour1 = ms.TourDate.Any(x => x.ArtistID == l.ArtistID && x.Tour_Utcdate.Value > DateTime.UtcNow && !x.IsDeleted);
                //    l.OnTour = onTour1;
                //}

                _list = _list.OrderBy(p => p.FirstLetter).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list, "ArtistList"));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));

            }

        }

        //Creating this function without pagination
        [HttpGet]
        [Route("api/v2/Artists/GetArtistsListHome")]
        public HttpResponseMessage GetArtistsListHome(Int32 UserID, bool onTour)
        {
            DataSet dsArtistsList = new DataSet();
            try
            {
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                MusikaEntities ms = new MusikaEntities();

                if (UserID <= 0)
                {
                    UserID = 0;
                }
                string _onTour = onTour.ToString();
                dsArtistsList = new Musika.Repository.SPRepository.SpRepository().GetAritstsByUserIdHome(UserID, _onTour.ToLower());

                var _list = dsArtistsList.Tables[0].AsEnumerable().Select(
                              dataRow => new ViewArtistListByUserId
                              {
                                  ArtistID = dataRow.Field<Int32>("ArtistID"),
                                  ArtistName = dataRow.Field<String>("ArtistName").Trim(),
                                  ImageURL = dataRow.Field<String>("ImageURL"),
                                  BannerImage_URL = dataRow.Field<String>("BannerImage_URL"),
                                  OnTour = Convert.ToString(dataRow["OnTour"]) == "1" ? true : false,// dataRow.Field<bool>("OnTour"),
                                  FirstLetter = dataRow.Field<String>("FirstLetter")
                              }).ToList();
                //if user don't have scanned and followed artist then load default artist.
                //And if you login and scan under 10 artist. You will also get default artist
                if (_list.Count < 10 && UserID > 0)
                {
                    UserID = 0;
                    dsArtistsList = new Musika.Repository.SPRepository.SpRepository().GetAritstsByUserIdHome(UserID, _onTour.ToLower());
                   var  _defaultList = dsArtistsList.Tables[0].AsEnumerable().Select(
                             dataRow => new ViewArtistListByUserId
                             {
                                 ArtistID = dataRow.Field<Int32>("ArtistID"),
                                 ArtistName = dataRow.Field<String>("ArtistName").Trim(),
                                 ImageURL = dataRow.Field<String>("ImageURL"),
                                 BannerImage_URL = dataRow.Field<String>("BannerImage_URL"),
                                 OnTour = Convert.ToString(dataRow["OnTour"]) == "1" ? true : false,// dataRow.Field<bool>("OnTour"),
                                  FirstLetter = dataRow.Field<String>("FirstLetter")
                             }).ToList();

                    
                    _list.AddRange(_defaultList);

                }
                
                _list = _list.Distinct().OrderBy(p => p.FirstLetter).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list, "ArtistList"));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));

            }

        }


        [HttpPost]
        [Route("api/v2/Artists/UpdateTrackArtist")]
        public HttpResponseMessage UpdateTrackArtist(InputTrackArtist input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);

                Models.Users _Users = null;
                Models.Artists _Artists = null;
                Models.UserArtists _UserArtists = null;

                _Artists = _ArtistsRepo.Repository.Get(p => p.ArtistID == input.ArtistID);
                if (_Artists == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.ArtistNotFound));
                }

                _UserArtists = _UserArtistsRepo.Repository.Get(p => p.ArtistID == input.ArtistID && p.UserID == input.UserID);

                //Tracking
                if (input.TrackArtist.ToString() == ETracking.Tracking.ToString())
                {
                    if (_UserArtists == null)
                    {
                        _UserArtists = new UserArtists();
                        _UserArtists.UserID = input.UserID;
                        _UserArtists.ArtistID = input.ArtistID;
                        _UserArtists.CreatedDate = DateTime.Now;
                        _UserArtists.RecordStatus = RecordStatus.Active.ToString();

                        _UserArtistsRepo.Repository.Add(_UserArtists);
                    }
                }
                else
                {
                    if (_UserArtists != null)
                    {
                        _UserArtistsRepo.Repository.DeletePermanent(_UserArtists.UserArtistsID);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, input.TrackArtist.ToString(), "TrackArtist"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "TrackArtist"));
            }
        }





        [HttpGet]
        [Route("api/v2/Events/GetEventsList")]
        public HttpResponseMessage GetEventsList(Int32 UserID, double? Lat, double? lon, decimal? radius, string from, string to)
        {
            try
            {
                if (UserID == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
                }

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                Models.Users _Users = null;

                _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "EventList"));
                }

                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

                //  DateTime? _st = Helper.GetUTCDateTime(DateTime.Now);

                #region "Old Code"
                //var _list = (from A1 in _UserTourDateRepo.Repository.GetAll(p => p.UserID == UserID)
                //             join A in _TourDateRepo.Repository.GetAll() on A1.TourDateID equals A.TourDateID
                //             join B in _ArtistsRepo.Repository.GetAll() on A.ArtistID equals B.ArtistID
                //             join C in _VenueRepo.Repository.GetAll() on A.VenueID equals C.VenueID
                //             where Helper.GetUTCDateTime(DateTime.Now) < A.Tour_Utcdate && !A.IsDeleted
                //             select new ViewEventsList
                //             {
                //                 TourDateID = A.TourDateID,
                //                 ArtistID = A.ArtistID.Value,
                //                 ArtistName = B.ArtistName,
                //                 Datetime_Local = Convert.ToDateTime(A.Datetime_Local).ToString("d"),
                //                 ImageURL = B.ImageURL,
                //                 BannerImage_URL = B.BannerImage_URL,
                //                 OnTour = B.OnTour,
                //                 VenueName = C.VenueName,
                //                 VenuID = C.VenueID
                //             }).ToList();

                //_list = _list.OrderBy(p => p.Datetime_Local).ToList();
                #endregion

                #region "New Code using SP"
                DataSet dsEventList = new DataSet();
                dsEventList = new Musika.Repository.SPRepository.SpRepository().GetEventsListSorted();
                var _list = dsEventList.Tables[0].AsEnumerable().Select(
                           dataRow => new ViewEventsList
                           {
                               TourDateID = dataRow.Field<Int32>("TourDateId"),
                               ArtistID = dataRow.Field<Int32>("ArtistID"),
                               ArtistName = dataRow.Field<String>("ArtistName"),
                               Datetime_dt = dataRow.Field<DateTime>("Datetime_Local"),
                               ImageURL = dataRow.Field<String>("ImageURL"),
                               BannerImage_URL = dataRow.Field<String>("BannerImage_URL"),
                               OnTour = (dataRow.Field<Int32>("OnTour") == 1 ? true : false),
                               VenueName = dataRow.Field<String>("VenueName"),
                               VenuID = dataRow.Field<Int32>("VenuID"),
                           }).ToList();
                #endregion

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list, "EventList"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }
        }


        private class dbEventSearchModel
        {
            public string id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
        }

        private List<dbEventSearchModel> DbEventLookup(string s)
        {
            Dictionary<string, object> dblookups = new Dictionary<string, object>();

            List<SeatGeek3.ViewEventLookup> _ViewDBLookup = new List<SeatGeek3.ViewEventLookup>();
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);

            string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ&\s]";
            var search = Regex.Replace(s, pattern, "");

            var venueByName = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 60 && x.VenueName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).Select(x => new dbEventSearchModel { id = x.VenueID.ToString(), name = x.VenueName, type = EventSearchType.Venue.ToString() }).ToList();

            //var venueByCity = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueCity.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).Select(x => new dbEventSearchModel { id = x.VenueID.ToString(), name = x.VenueCity, type = EventSearchType.Venue.ToString() }).ToList();

            var artistByName = _ArtistsRepo.Repository.AsQueryable().Where(x => x.ArtistName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).Select(x => new dbEventSearchModel { id = x.ArtistID.ToString(), name = x.ArtistName, type = EventSearchType.Artist.ToString() }).ToList();

            //var eventByName = _TourDateRepo.Repository.AsQueryable().Where(x => x.EventName != null && x.EventName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).Select(x => new dbEventSearchModel { id = x.TourDateID.ToString(), name = x.EventName, type = EventSearchType.Event.ToString() }).ToList();


            var tourDates = venueByName.Union(artistByName).GroupBy(x => x.name).Select(g => g.FirstOrDefault()).ToList();

            return tourDates;
        }

        private List<ViewEventsPreList> DbEventListCheck(Int32 userId, string id, string name, string type)
        {
            MusikaEntities db = new MusikaEntities();
            List<SeatGeek3.ViewEventLookup> _ViewDBLookup = new List<SeatGeek3.ViewEventLookup>();
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);

            GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

            List<int> dbTourDateList = new List<int>();
            List<ViewEventsPreList> dbViewEventsList = new List<ViewEventsPreList>();

            if (type == EventSearchType.Artist.ToString())
            {
                dbTourDateList = db.TourDate.Where(x => x.ArtistID.ToString() == id)
                    .OrderBy(x => x.Datetime_Local).Select(x => x.TourDateID).ToList();
            }
            else if (type == EventSearchType.Venue.ToString())
            {
                dbTourDateList = db.TourDate.Where(x => x.VenueID.ToString() == id)
                    .OrderBy(x => x.Datetime_Local).Select(x => x.TourDateID).ToList();
            }
            else
            {
                dbTourDateList = db.TourDate.Where(x => x.EventName == name)
                    .OrderBy(x => x.Datetime_Local).Select(x => x.TourDateID).ToList();
            }

            //foreach (var d in dbTourDateList)
            //{
            //date
            //if (dbTourDateList.Count(x => x.Datetime_Local == d.Datetime_Local) > 1 && d.RecordStatus == RecordStatus.Eventful.ToString())
            //{ continue; }
            //else
            //{
            //var _list = (from A in _TourDateRepo.Repository.GetAll(x => x.TourDateID == d.TourDateID)
            //             join B in _ArtistsRepo.Repository.GetAll() on A.ArtistID equals B.ArtistID
            //             join C in _VenueRepo.Repository.GetAll() on A.VenueID equals C.VenueID
            //             where Helper.GetUTCDateTime(DateTime.Now) < A.Tour_Utcdate

            DateTime? _date = Helper.GetUTCDateTime(DateTime.Now);
            var _list = (from A in db.TourDate
                         join B in db.Artists on A.ArtistID equals B.ArtistID
                         join C in db.Venue on A.VenueID equals C.VenueID
                         where dbTourDateList.Contains(A.TourDateID) && A.Tour_Utcdate > _date && A.IsDeleted == false
                         select new ViewEventsPreList
                         {
                             TourDateID = A.TourDateID,
                             ArtistID = A.ArtistID.Value,
                             ArtistName = B.ArtistName,
                             Datetime_dt = A.Datetime_Local ?? A.Tour_Utcdate,
                             ImageURL = B.ImageURL,
                             BannerImage_URL = B.BannerImage_URL,
                             OnTour = B.OnTour,
                             VenueName = C.VenueName,
                             VenuID = C.VenueID,
                             VenueLat = C.VenueLat,
                             VenueLon = C.VenueLong,
                             IsDeleted = A.IsDeleted,
                             TicketingEventId=A.TicketingEventID
                         }).ToList();


            dbViewEventsList.AddRange(_list);
            //}
            //            }
            return dbViewEventsList.OrderBy(x => x.Datetime_Local).ToList();
        }


        [HttpGet]
        [Route("api/v2/Events/GetEventsSearchResult")]
        public HttpResponseMessage GetEventsSearchResult(string search, int UserID)
        {
            try
            {
                List<dbEventSearchModel> _eventDBLookup;
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(_unitOfWork);

                Models.Users _Users = null;
                string pattern = @"[^a-zA-Z0-9&\s]";
                search = Regex.Replace(search, pattern, "");

                //if (search.Length < 3)
                //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, "", "Events"));


                if (!String.IsNullOrEmpty(UserID.ToString()))
                {
                    _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);
                }
                else
                {
                    _Users = null;
                }

                //if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "Events"));
                //}
                search = search.ToLower();
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

                string _result;

                _eventDBLookup = DbEventLookup(search);
                //commented below code on 26-9-2019 to avoid search from third party
                //if (_eventDBLookup == null || _eventDBLookup.Count() < 10)
                //{
                //    try
                //    {
                //        //Search for artist name
                //        List<dbEventSearchModel> _ArtistList = new List<dbEventSearchModel>();
                //        try
                //        {
                //            ServicePointManager.Expect100Continue = true;
                //            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                //                   | SecurityProtocolType.Tls11
                //                   | SecurityProtocolType.Tls12
                //                   | SecurityProtocolType.Ssl3;
                //            var httpMusicRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/v2/artist/suggest?api_key=" + _MusicGrapgh_api_key + "&limit=10&prefix=" + search);
                //            httpMusicRequest.ContentType = "application/json";
                //            httpMusicRequest.Method = "GET";

                //            var httpMusicResponse = (HttpWebResponse)httpMusicRequest.GetResponse();
                //            using (var streamReader = new StreamReader(httpMusicResponse.GetResponseStream()))
                //            {
                //                _result = streamReader.ReadToEnd();
                //            }


                //            // deserializing 
                //            var _Search_ByName = JsonConvert.DeserializeObject<MusicGraph.Search_ByName>(_result);
                //            bool isLatin = false;


                //            if (_Search_ByName != null)
                //            {
                //                if (_Search_ByName.data.Count > 0)
                //                {
                //                    foreach (MusicGraph.Datum _datum in _Search_ByName.data)
                //                    {
                //                        if (_datum.name.Length < 30)
                //                        {
                //                            if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _datum.name))
                //                            {
                //                                //Check to filter Latin artist only
                //                                if (_GenreFilter.Contains(_datum.main_genre))
                //                                    isLatin = true;
                //                                else
                //                                {
                //                                    //if (!isLatin)
                //                                    //    isLatin = CheckMusicGraphLatin(_datum.id, _unitOfWork);
                //                                    if (!isLatin)
                //                                        isLatin = CheckSeatGeekLatin(_datum.name, _unitOfWork);
                //                                    if (!isLatin)
                //                                    {
                //                                        isLatin = CheckLastResortSpotifyGenre(_datum.spotify_id);
                //                                    }
                //                                }

                //                                if (isLatin)
                //                                {
                //                                    _ArtistList.Add(new dbEventSearchModel
                //                                    {
                //                                        id = _datum.id,
                //                                        name = _datum.name,
                //                                        type = EventSearchType.Artist.ToString()

                //                                    });
                //                                }
                //                                else
                //                                {
                //                                    if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _datum.name))
                //                                        _ArtistNotLatinRepo.Repository.Add(new ArtistsNotLatin() { ArtistName = _datum.name });
                //                                }
                //                            }
                //                        }

                //                        isLatin = false;
                //                    }
                //                }
                //            }
                //        }
                //        catch (Exception ex)
                //        {

                //        }

                //        List<dbEventSearchModel> seatGeekVenueSearch = new List<dbEventSearchModel>();
                //        seatGeekVenueSearch = SeatGeekVenueSearch(search);


                //        if (seatGeekVenueSearch.Count() < 10)
                //        {
                //            //var venueWordMatches = new List<string>();
                //            foreach (var d in _eventDBLookup)
                //            {
                //                if (seatGeekVenueSearch.Count() < 15)
                //                {
                //                    if (d.type == EventSearchType.Venue.ToString())
                //                    {
                //                        var str = d.name.Split(' ').FirstOrDefault(x => x.ToLower().Contains(search));
                //                        if (!String.IsNullOrEmpty(str))
                //                        {
                //                            seatGeekVenueSearch = seatGeekVenueSearch.Union(SeatGeekVenueSearch(str)).ToList();
                //                        }
                //                    }
                //                }
                //                else
                //                {
                //                    break;
                //                }
                //            }
                //        }

                //        _eventDBLookup = _eventDBLookup.Union(_ArtistList).Union(seatGeekVenueSearch).GroupBy(x => x.name).Select(g => g.FirstOrDefault()).ToList();
                //    }
                //    catch (Exception ex)
                //    {

                //    }
                //}
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _eventDBLookup, "Events"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "Events"));
            }
        }

        private List<dbEventSearchModel> SeatGeekVenueSearch(string search)
        {
            //search for venue name
            string _result;
            List<dbEventSearchModel> seatGeekVenueSearch = new List<dbEventSearchModel>();
            string _concat = SGAuth() + "&";

            var strEncoded = HttpUtility.UrlEncode(search);
            _concat = _concat + "name=" + strEncoded + "&per_page=10&per_page=1";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.seatgeek.com/2/venues" + _concat);

            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                _result = streamReader.ReadToEnd();
            }

            //LogHelper.CreateLog(_result);

            //_result = "";
            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
            dynamic obj = null;

            // deserializing 
            var _Get_Events_ByLat = JsonConvert.DeserializeObject<SeatGeek3.Get_Venues>(_result);
            //dynamic _Get_Events_ByLat = serializer.Deserialize(_result, typeof(object));


            if (_Get_Events_ByLat != null)
            {
                if (_Get_Events_ByLat.venues != null && _Get_Events_ByLat.venues.Count > 0)
                {
                    foreach (SeatGeek3.Venue _Event in _Get_Events_ByLat.venues)
                    {
                        if (_Event != null && _Event.name != null && _Event.name.Length < 30)
                        {
                            if (!seatGeekVenueSearch.Any(x => x.name == _Event.name))
                            {
                                seatGeekVenueSearch.Add(new dbEventSearchModel { id = _Event.id.ToString(), name = _Event.name, type = EventSearchType.Venue.ToString() });
                            }
                        }
                    }
                }
            }
            return seatGeekVenueSearch;
        }

        [HttpGet]
        [Route("api/v2/Events/GetEventsSearchList")]
        public HttpResponseMessage GetEventsSearchList(Int32 UserID, string id, string name, string type, double? Lat, double? Lon, decimal? radius, string from, string to, Int16 Pageindex, Int16 Pagesize)
        {
            try
            {
                #region "Commented Code"
                //if (UserID == null || UserID == -1)
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
                //}
                #endregion

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);
                GenericRepository<UserArtists> _UserArtistsRepo = new GenericRepository<UserArtists>(_unitOfWork);
                GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(_unitOfWork);
                //  DateTime? _st = Helper.GetUTCDateTime(DateTime.Now);
                MusikaEntities db = new MusikaEntities();
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                //Users _Users = null;
                Users _Users = new Users();
                if (UserID > 0)
                {
                    _Users.UserID = UserID;
                }
                else
                {
                    _Users.UserID = -1;
                }
                HttpCache _HttpCache = new HttpCache();
                //List<SeatGeek3.ViewEventLookup> _ViewEventLookup = new List<SeatGeek3.ViewEventLookup>();
                List<ViewEventsPreList> _ViewEventsList = new List<ViewEventsPreList>();

                #region "Commented Code"
                //if (UserID != null || UserID > 0)
                //{
                //    _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);
                //}
                //if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "EventList"));
                //}
                #endregion

                if (type == EventSearchType.Venue.ToString()) //If venue search ignore lat lon radius filter
                {
                    radius = 0;
                    Lat = 0;
                    Lon = 0;
                }

                if (String.IsNullOrEmpty(type))
                {
                    List<ViewEventsPreList> _list = null;

                    if (Lat != null && Lat != 0 && Lon != null && Lon != 0)
                    {
                        if (radius == 0)
                        {
                            radius = 25;
                        }
                    }

                    if (radius != null && radius > 0 && Lat != null && Lat != 0 && Lon != null && Lon != 0)
                    {
                        DataSet dsEventList = new DataSet();
                        dsEventList = new SpRepository().SpGetEventList();
                        _list = dsEventList.Tables[0].AsEnumerable().Select(
                                   dataRow => new ViewEventsPreList
                                   {
                                       VenueLat = dataRow.Field<decimal>("VenueLat"),
                                       VenueLon = dataRow.Field<decimal>("VenueLong"),
                                       IsDeleted = dataRow.Field<bool>("IsDeleted"),
                                       ArtistID = dataRow.Field<Int32>("ArtistID"),
                                       ArtistName = dataRow.Field<string>("ArtistName"),
                                       BannerImage_URL = dataRow.Field<string>("BannerImage_URL"),
                                       // Datetime_Local = dataRow.Field<DateTime>("Datetime_Local").ToString(),
                                       Datetime_dt = dataRow.Field<Nullable<DateTime>>("Datetime_Local"),
                                       ImageURL = dataRow.Field<string>("ImageURL"),
                                       TourDateID = dataRow.Field<Int32>("TourDateID"),
                                       VenueName = dataRow.Field<string>("VenueName"),
                                       VenuID = dataRow.Field<Int32>("VenueID"),
                                       TicketUrl = dataRow.Field<string>("TicketUrl")
                                   }).ToList();
                    }
                    else
                    {
                        // Ticketing Event Portal Events
                        DataSet dsEventList2 = new DataSet();
                        dsEventList2 = new SpRepository().GetEventsList2();
                        _list = dsEventList2.Tables[0].AsEnumerable().Select(
                                   dataRow => new ViewEventsPreList
                                   {
                                       VenueLat = dataRow.Field<decimal>("VenueLat"),
                                       VenueLon = dataRow.Field<decimal>("VenueLong"),
                                       IsDeleted = dataRow.Field<bool>("IsDeleted"),
                                       ArtistID = dataRow.Field<Int32>("ArtistID"),
                                       ArtistName = dataRow.Field<string>("ArtistName"),
                                       BannerImage_URL = dataRow.Field<string>("BannerImage_URL"),
                                       //Datetime_Local = dataRow.Field<DateTime>("Datetime_Local").ToString(),
                                       Datetime_dt = dataRow.Field<Nullable<DateTime>>("Datetime_Local"),
                                       ImageURL = dataRow.Field<string>("ImageURL"),
                                       TourDateID = dataRow.Field<Int32>("TourDateID"),
                                       VenueName = dataRow.Field<string>("VenueName"),
                                       VenuID = dataRow.Field<Int32>("VenueID"),
                                       TicketUrl = dataRow.Field<string>("TicketUrl")
                                   }).ToList();
                    }

                    if (_list.Count() > 0)
                    {
                        if (radius != null && radius > 0 && Lat != null && Lat != 0 && Lon != null && Lon != 0) //using the filter
                        {
                            if (_Users.UserID > -1)
                            {
                                _list.RemoveAll(p => CalculateDistance.distance(p.VenueLat.GetDouble(), p.VenueLon.GetDouble(), Lat.GetDouble(), Lon.GetDouble(), _Users.UserLanguage == EUserLanguage.EN.ToString() ? CalculateDistance.Measure.M : CalculateDistance.Measure.K) > radius.GetDouble());
                            }
                            else
                            {
                                _Users.UserLanguage = EUserLanguage.EN.ToString();
                                _list.RemoveAll(p => CalculateDistance.distance(p.VenueLat.GetDouble(), p.VenueLon.GetDouble(), Lat.GetDouble(), Lon.GetDouble(), _Users.UserLanguage == EUserLanguage.EN.ToString() ? CalculateDistance.Measure.M : CalculateDistance.Measure.K) > radius.GetDouble());
                            }
                        }
                        if (_list.Count() > 0)
                        {
                            DateTime? fromDate = null;
                            DateTime? toDate = null;

                            if (!String.IsNullOrEmpty(to))
                                toDate = Convert.ToDateTime(to);

                            if (!String.IsNullOrEmpty(from))
                                fromDate = Convert.ToDateTime(from);

                            if (fromDate == null && toDate != null)
                                fromDate = DateTime.Now;
                            else if (fromDate != null && toDate == null)
                                toDate = DateTime.MaxValue;

                            if (fromDate != null && toDate != null)
                            {
                                string strFromDate = fromDate.Value.ToString();
                                string strToDate = toDate.Value.ToString();
                                _list.RemoveAll(x => (!string.IsNullOrEmpty(x.Datetime_Local) ? Convert.ToDateTime(x.Datetime_Local) < fromDate : 1 == 1) || (!string.IsNullOrEmpty(x.Datetime_Local) ? Convert.ToDateTime(x.Datetime_Local) > toDate : 1 == 1));
                            }
                            // Added by Mukesh
                            _list = _list.OrderBy(p => !string.IsNullOrEmpty(p.Datetime_Local) ? Convert.ToDateTime(p.Datetime_Local) : DateTime.MinValue).Where(p => p.IsDeleted == false).ToList();

                            _list = _list.GroupBy(x => x.TourDateID, (key, group) => group.First()).ToList();
                        }
                    }

                    //#region "Ticketing Events List"
                    DataSet ds = new DataSet();
                    ds = new SpRepository().SpGetTicketingEventsForMusika();

                    #region "New Code"
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            ViewEventsPreList data = new ViewEventsPreList();
                            DataRow dr = ds.Tables[0].Rows[i];
                            try
                            {
                                if (!String.IsNullOrEmpty(dr["VenueLat"].ToString()))
                                {
                                    data.VenueLat = Convert.ToDecimal(dr["VenueLat"].ToString());
                                }
                                else
                                {
                                    data.VenueLat = 0;
                                }
                                if (!String.IsNullOrEmpty(dr["VenueLon"].ToString()))
                                {
                                    data.VenueLon = Convert.ToDecimal(dr["VenueLon"].ToString());
                                }
                                else
                                {
                                    data.VenueLon = 0;
                                }
                                data.IsDeleted = (dr["IsDeleted"].ToString() == "1" ? true : false);
                                data.ArtistID = Convert.ToInt32(dr["ArtistID"].ToString());
                                data.ArtistName = dr["ArtistName"].ToString();

                                data.BannerImage_URL = dr["BannerImage_URL"].ToString();
                                data.Datetime_dt = Convert.ToDateTime(dr["Datetime_Local"]);
                                if (!String.IsNullOrEmpty(data.ImageURL))
                                {
                                    data.ImageURL = dr["ImageURL"].ToString();
                                }
                                else
                                {
                                    data.ImageURL = string.Empty;
                                }

                                if (dr["TourDateID"].ToString() == "")
                                {

                                    data.TourDateID = 0;
                                }
                                else
                                {
                                    data.TourDateID = Convert.ToInt32(dr["TourDateID"].ToString());
                                }
                                data.VenueName = dr["VenueName"].ToString();
                                if (!String.IsNullOrEmpty(dr["VenuID"].ToString()))
                                {
                                    data.VenuID = Convert.ToInt32(dr["VenuID"].ToString());
                                }
                                else
                                {
                                    data.VenuID = -1;
                                }
                                data.TicketUrl = Convert.ToString(dr["TicketUrl"].ToString());
                                _list.Add(data);
                            }
                            catch (Exception)
                            { }
                        }
                    }

                    // Filter records on the basis of latitude and longitude
                    _list.RemoveAll(p => CalculateDistance.distance(p.VenueLat.GetDouble(), p.VenueLon.GetDouble(), Lat.GetDouble(), Lon.GetDouble(), _Users.UserLanguage == EUserLanguage.EN.ToString() ? CalculateDistance.Measure.M : CalculateDistance.Measure.K) > radius.GetDouble());

                    #endregion

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, ViewEventsPreList.GetEventsList(_list.GroupBy(x => x.TourDateID, (key, group) => group.First()).OrderBy(p => DateTime.Parse(p.Datetime_Local)).ToPagedList(Pageindex - 1, Pagesize).ToList()), "EventList", _list.Count));
                }
                else
                {
                    //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewEventLookup, "Events"));

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

                    //string _result;

                    //Get Cache
                    if (_HttpCache.Get(UserID.ToString() + name + Lat.ToString() + Lon.ToString() + radius.ToString() + from + to, out _ViewEventsList) == true)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, ViewEventsPreList.GetEventsList(_ViewEventsList).ToPagedList(Pageindex - 1, Pagesize).ToList(), "EventList", _ViewEventsList.Count));
                    }

                    List<ViewEventsPreList> dbEventsList = new List<ViewEventsPreList>();

                    if (UserID > 0)
                    {
                        dbEventsList = DbEventListCheck(UserID, id, name, type);
                    }
                    else
                    {
                        dbEventsList = DbEventListCheck(-1, id, name, type);
                    }

                    try
                    {

                        if (!String.IsNullOrEmpty(name))            //If searching by typing in the search box
                        {
                            radius = 0;
                            Lat = 0;
                            Lon = 0;
                        }

                        #region "Get SeatGeek Events by Lat/lon/fromdate/todate/radius"
                        #region SearchString
                        string _concat = SGAuth() + "&";

                        //Default login time lat/lon if lat/lon not provided 
                        if (Lat != null && Lon != null && Lat != 0 && Lon != 0)
                        {
                            _concat = _concat + "lat=" + Lat + "&lon=" + Lon + "&";
                        }
                        //else
                        //{
                        //    _concat = _concat + "lat=" + _Users.DeviceLat + "&lon=" + _Users.DeviceLong + "&";
                        //}

                        //Default is 25mi if radius not provided
                        if (radius != null && radius != 0)
                        {
                            _concat = _concat + "range=" + radius + "mi&";
                        }
                        //else
                        //{
                        //    _concat = _concat + "range=25mi&";
                        //}

                        //set from-date if avaialble
                        if (from != null && from != "")
                        {
                            string _stfrom = DateTime.ParseExact(from, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None).ToString("yyyy-MM-dd");
                            _concat = _concat + "datetime_local.gte=" + _stfrom + "&";
                        }

                        //Default is 6 month from the current date if To-Date not provided
                        if (to != null && to != "")
                        {
                            string _stto = DateTime.ParseExact(to, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None).ToString("yyyy-MM-dd");
                            _concat = _concat + "datetime_local.lte=" + _stto + "&";
                        }
                        else
                        {
                            string _stto = DateTime.Now.AddDays(180).ToString("MM/dd/yyyy");
                            _concat = _concat + "datetime_local.lte=" + _stto + "&";
                        }

                        #endregion

                        _ViewEventsList = new List<ViewEventsPreList>();

                        var strEncoded = HttpUtility.UrlEncode(name);

                        if (type == EventSearchType.Venue.ToString())
                        {
                            _concat = _concat + "venue.name=" + strEncoded + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&per_page=100&page=1";
                        }
                        else if (type == EventSearchType.Artist.ToString())
                        {
                            _concat = _concat + "q=" + strEncoded + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&per_page=100&page=1";
                        }
                        else
                        {
                            _concat = _concat + "q=" + strEncoded + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&per_page=100&page=1";
                        }

                        #region "Commented by Mukesh - 13 Aug 2018"
                        //var _Get_Events_ByLat = GetSeatGeakEvent(_concat);

                        //if (_Get_Events_ByLat != null)
                        //{
                        //    if (_Get_Events_ByLat.events != null && _Get_Events_ByLat.events.Count > 0)
                        //    {

                        //        foreach (SeatGeek3.Event _Event in _Get_Events_ByLat.events.OrderBy(x => x.datetime_local).ToList())
                        //        {
                        //            //if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _Event.title))
                        //            //{
                        //            if (!db.ArtistsNotLatin.Any(x => x.ArtistName == _Event.title))
                        //            {
                        //                bool anyLatin = false;
                        //                if (_Event.performers.Count() > 0)
                        //                {
                        //                    //anyLatin = true;

                        //                    foreach (var p in _Event.performers)
                        //                    {
                        //                        try
                        //                        {
                        //                            if (db.Artists.Any(x => x.ArtistName == p.name))
                        //                            {
                        //                                //if (_ArtistsRepo.Repository.AsQueryable().Any(x => x.ArtistName == p.name))
                        //                                //{
                        //                                anyLatin = true;
                        //                                break;
                        //                            }
                        //                            //else if (!_ArtistsRepo.Repository.AsQueryable().Any(x => x.ArtistName == p.name))
                        //                            //{
                        //                            else if (!db.Artists.Any(x => x.ArtistName == p.name))
                        //                            {
                        //                                anyLatin = CheckSeatGeekLatin(p.name, _unitOfWork);
                        //                            }
                        //                            else
                        //                            { }
                        //                        }
                        //                        catch (Exception ex)
                        //                        {

                        //                        }
                        //                    }
                        //                }

                        //                if (anyLatin || _Event.performers.Count() == 0)
                        //                {
                        //                    var localtime = Convert.ToDateTime(_Event.datetime_local);

                        //                    if (localtime > DateTime.Now)
                        //                    {
                        //                        var ddate = Convert.ToDateTime(_Event.datetime_local).ToString("d");

                        //                        if (!dbEventsList.Any(x => x.TourDateID == _Event.id || x.Datetime_Local == ddate))
                        //                        {
                        //                            var localDateString = localtime.ToString("MM/dd");
                        //                            var eventName = _Event.title;
                        //                            if (eventName.Length > 38)
                        //                                eventName = eventName.Substring(0, 38) + "...";

                        //                            _ViewEventsList.Add(new ViewEventsPreList
                        //                            {
                        //                                TourDateID = _Event.id,
                        //                                ArtistName = eventName,
                        //                                Datetime_dt = Convert.ToDateTime(ddate),
                        //                                VenuID = _Event.venue.id,
                        //                                VenueName = _Event.venue.name,
                        //                                OnTour = true,
                        //                                ImageURL = "",
                        //                                BannerImage_URL = "",
                        //                                ArtistID = 0,
                        //                                IsDeleted = false

                        //                            });
                        //                        }
                        //                    }
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                        #endregion


                        #endregion

                        if (_ViewEventsList != null && _ViewEventsList.Count() > 50)
                            _ViewEventsList = _ViewEventsList.Take(50).ToList();

                        if (dbEventsList != null && dbEventsList.Count() > 0)
                        {
                            _ViewEventsList.InsertRange(0, dbEventsList);        //   .Union(_ViewDBLookup).OrderBy(x=>x.TourID.GetInt()).GroupBy(x => x.TourID);
                        }

                        if (_ViewEventsList.Count() > 0)
                        {
                            if (radius != null && radius > 0 && Lat != null && Lat != 0 && Lon != null && Lon != 0) //using the filter
                                _ViewEventsList.RemoveAll(p => CalculateDistance.distance(p.VenueLat.GetDouble(), p.VenueLon.GetDouble(), Lat.GetDouble(), Lon.GetDouble(), _Users.UserLanguage == EUserLanguage.EN.ToString() ? CalculateDistance.Measure.M : CalculateDistance.Measure.K) > radius.GetDouble());

                            if (_ViewEventsList.Count() > 0)
                            {
                                DateTime? fromDate = null;
                                DateTime? toDate = null;

                                if (!String.IsNullOrEmpty(to))
                                    toDate = Convert.ToDateTime(to);

                                if (!String.IsNullOrEmpty(from))
                                    fromDate = Convert.ToDateTime(from);

                                if (fromDate == null && toDate != null)
                                    fromDate = DateTime.Now;
                                else if (fromDate != null && toDate == null)
                                    toDate = DateTime.MaxValue;

                                if (fromDate != null && toDate != null)
                                {
                                    string strFromDate = fromDate.Value.ToString();
                                    string strToDate = toDate.Value.ToString();
                                    _ViewEventsList.RemoveAll(x => Convert.ToDateTime(x.Datetime_Local) < fromDate || Convert.ToDateTime(x.Datetime_Local) > toDate);
                                }
                                if (type == "Artist")
                                {
                                    _ViewEventsList = _ViewEventsList.ToList();
                                }
                                else
                                {
                                    _ViewEventsList = _ViewEventsList.OrderBy(p => Convert.ToDateTime(p.Datetime_Local)).ToList();
                                }
                            }
                        }



                        //_ViewEventsList = _ViewEventsList.OrderBy(x => Convert.ToDateTime(x.Datetime_Local)).ToList();

                        //Save cache
                        if (_ViewEventsList.Count > 0)
                        {
                            if (UserID > 0)
                            {
                                _HttpCache.Set(UserID.ToString() + name + Lat.ToString() + Lon.ToString() + radius.ToString() + from + to, _ViewEventsList, 24);
                            }
                        }

                        var _listing = ViewEventsPreList.GetEventsList(_ViewEventsList);


                        if (_listing.Count > 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _listing.ToPagedList(Pageindex - 1, Pagesize).ToList(), "EventList", _listing.Count));
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "We have found no results. Please try again.", "EventList"));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog3(ex, Request);
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "EventList"));
                    }
                    //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewEventsList, "EventList"));
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }
        }



        public SeatGeek3.Get_Events_ByLat GetSeatGeakEvent(string _concat)
        {
            string _result;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.seatgeek.com/2/events" + _concat);

            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                _result = streamReader.ReadToEnd();
            }

            //_result = "";
            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
            dynamic obj = null;

            // deserializing 
            return JsonConvert.DeserializeObject<SeatGeek3.Get_Events_ByLat>(_result);
        }


        [HttpGet]
        [Route("api/v2/Events/GetEventByID")]
        public async Task<HttpResponseMessage> GetEventByID(string TourID, int UserID)
        {
            try
            {
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                Models.Users _Users = null;

                if (UserID > 0)
                {
                    _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);
                }

                //if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "EventsDetail"));
                //}

                string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];

                #region "Commented Code"
                //string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
                //string _Instagram_access_token = ConfigurationManager.AppSettings["instagram.access_token"].ToString();

                //string _Eventful_app_key = ConfigurationManager.AppSettings["Eventful_app_key"].ToString();

                //string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();
                //string _SeatGeek_client_secret = ConfigurationManager.AppSettings["SeatGeek_client_secret"].ToString();

                //string _Spotify_Country = ConfigurationManager.AppSettings["Spotify_Country"].ToString();

                //string _result;

                //string _MusicGraph_ID = null;                
                //Models.Venue _VenuEntity = null;


                //Models.Artists _Artists = null;
                //Models.TourPerformers _TourPerformers = null;

                //Models.ArtistGenre _ArtistGenre = null;
                //Models.Genre _Genre = null;
                #endregion

                Models.TourDate _TourDateEntity = null;

                //_unitOfWork.StartTransaction();
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);

                #region "Commented Code"
                //GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                //GenericRepository<ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<ArtistPhotos>(_unitOfWork);
                //GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(_unitOfWork);

                //GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                //GenericRepository<TourPerformers> _TourPerformersRepo = new GenericRepository<TourPerformers>(_unitOfWork);

                //GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(_unitOfWork);
                //GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(_unitOfWork);
                #endregion

                var serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
                //dynamic obj = null;

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
                        //_unitOfWork.RollBack();
                        if ((!String.IsNullOrEmpty(UserID.ToString()) && UserID > 0))
                        {
                            if (_Users.RecordStatus != RecordStatus.Active.ToString())
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, GetTourDetail_ByID(_TourDateEntity, UserID), "EventsDetail"));
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, GetTourDetail_ByID(_TourDateEntity, -1), "EventsDetail"));
                        }
                        //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, GetTourDetail_ByID(_TourDateEntity, UserID), "EventsDetail"));
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, GetTourDetail_ByID(_TourDateEntity, UserID), "EventsDetail"));

                #region "Commented Code - 9 Nov 2018 "
                //try
                //{
                //    if (_TourDateEntity == null)
                //    {
                //        #region "Commented by Mukesh - Nov05
                //        //#region "Get SeatGeek Event By ID"

                //        //var _Get_Events_ByID = await SeatGeek_GetEventByID_Asyn(_TourDateEntity == null ? TourID : _TourDateEntity.SeatGeek_TourID);//await for the function to be completed.

                //        //if (_Get_Events_ByID != null)
                //        //{
                //        //    #region "Add Venu Information"

                //        //    SeatGeek4.Venue _Venue = _Get_Events_ByID.venue;

                //        //    _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                //        //                   where (A.SeatGeek_VenuID == _Venue.id.ToString())
                //        //                   select A).FirstOrDefault();


                //        //    //search the venu using fuzzy searching
                //        //    if (_VenuEntity == null)
                //        //    {

                //        //        string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ&\s]";
                //        //        var search = Regex.Replace(_Venue.name.ToLower(), pattern, "");

                //        //        var venueByName = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 60 && x.RecordStatus == RecordStatus.Eventful.ToString() && x.VenueName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                //        //        if (venueByName == null || venueByName.Count() == 0)
                //        //        {
                //        //            var allEventfulVenues = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 80 && x.RecordStatus == RecordStatus.Eventful.ToString()).ToList();
                //        //            foreach (var a in allEventfulVenues)
                //        //            {
                //        //                var eventfulVenue = Regex.Replace(a.VenueName, pattern, "");
                //        //                if (eventfulVenue.ToLower() == search.ToLower())
                //        //                {
                //        //                    _VenuEntity = a;
                //        //                    break;
                //        //                }
                //        //                if (search.Length > 11 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(2, search.Length - 2)))
                //        //                {
                //        //                    _VenuEntity = a;
                //        //                    break;
                //        //                }
                //        //                if (search.Length > 20 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(5, search.Length - 5)))
                //        //                {
                //        //                    _VenuEntity = a;
                //        //                    break;
                //        //                }
                //        //                if (search.Length > 40 && eventfulVenue.ToLower().Contains(search.ToLower().Substring(10, search.Length - 10)))
                //        //                {
                //        //                    _VenuEntity = a;
                //        //                    break;
                //        //                }
                //        //            }
                //        //        }
                //        //        else
                //        //        {
                //        //            _VenuEntity = venueByName.First();
                //        //        }
                //        //    }

                //        //    if (_VenuEntity == null)
                //        //    {
                //        //        _VenuEntity = new Venue();
                //        //        _VenuEntity.SeatGeek_VenuID = _Venue.id.ToString();
                //        //        _VenuEntity.VenueName = _Venue.name;
                //        //        _VenuEntity.Extended_Address = _Venue.extended_address;
                //        //        _VenuEntity.VenueCountry = _Venue.country;
                //        //        _VenuEntity.Display_Location = _Venue.display_location;
                //        //        _VenuEntity.Slug = _Venue.slug;
                //        //        _VenuEntity.VenueState = _Venue.state;
                //        //        _VenuEntity.Postal_Code = _Venue.postal_code;
                //        //        _VenuEntity.VenueCity = _Venue.city;
                //        //        _VenuEntity.Address = _Venue.address;
                //        //        _VenuEntity.Timezone = _Venue.timezone;

                //        //        if (_Venue.location != null)
                //        //        {
                //        //            _VenuEntity.VenueLat = _Venue.location.lat;
                //        //            _VenuEntity.VenueLong = _Venue.location.lon;
                //        //        }

                //        //        _VenuEntity.CreatedDate = DateTime.Now;
                //        //        _VenuEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                //        //        _VenueRepo.Repository.Add(_VenuEntity);
                //        //    }
                //        //    #endregion

                //        //    //loop through to get Artist Information
                //        //    #region "Get Artists/Instagram Information"
                //        //    List<SeatGeek4.Performer> _Performers = new List<SeatGeek4.Performer>();
                //        //    SeatGeek4.Performer _PerformersChk = null;

                //        //    _Performers = _Get_Events_ByID.performers.Where(p => p.type != "theater" && p.type == "band").ToList();
                //        //    _Performers = _Performers.OrderByDescending(p => p.primary).ToList();
                //        //    _PerformersChk = _Performers.Where(p => p.primary == true).FirstOrDefault();

                //        //    #region "Data from SeatGeek API"
                //        //    foreach (SeatGeek4.Performer _perfomer in _Performers)
                //        //    {
                //        //        _MusicGraph_ID = null;

                //        //        #region "Get Artist Info Using Name  (MusicGraph)"
                //        //        try
                //        //        {
                //        //            var _Search_ByName = await Spotify_SearchArtist(_perfomer.name.Trim());//await for the function to be completed

                //        //            if (_Search_ByName != null)
                //        //            {
                //        //                foreach (MusicGraph.Datum _Datum in _Search_ByName)
                //        //                {
                //        //                    if (RemoveDiacritics(_Datum.name.ToLower()) == _perfomer.name.ToLower())
                //        //                    {
                //        //                        _MusicGraph_ID = _Datum.id;
                //        //                        _Artists = _ArtistsRepo.Repository.Get(p => p.Spotify_ID == _MusicGraph_ID);

                //        //                        #region "Add New"
                //        //                        if (_Artists == null)
                //        //                        {
                //        //                            bool isLatin = false;

                //        //                            isLatin = CheckMusicGraphLatin(_Datum.id, _unitOfWork);
                //        //                            if (!isLatin)
                //        //                                isLatin = CheckSeatGeekLatin(_Datum.name, _unitOfWork);
                //        //                            if (!isLatin)
                //        //                                isLatin = CheckLastResortSpotifyGenre(_Datum.spotify_id);


                //        //                            if (!isLatin)
                //        //                            {
                //        //                                _unitOfWork.RollBack();
                //        //                                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "Its not a latin artist event", "EventsDetail"));
                //        //                            }

                //        //                            if (isLatin)
                //        //                            {
                //        //                                _Artists = new Artists();

                //        //                                _Artists.ArtistName = _Datum.name;
                //        //                                _Artists.Main_Genre = _Datum.main_genre;
                //        //                                _Artists.Seatgeek_ID = _perfomer.id.ToString();
                //        //                                _Artists.Spotify_ID = !String.IsNullOrEmpty(_Datum.id) ? _Datum.id : _Datum.name;

                //        //                                _Artists.RecordStatus = RecordStatus.MusicGraph.ToString();
                //        //                                _Artists.CreatedDate = DateTime.Now;
                //        //                                _Artists.ModifiedDate = DateTime.Now;

                //        //                                _Artists.OnTour = _perfomer.has_upcoming_events == true ? true : false;

                //        //                                _ArtistsRepo.Repository.Add(_Artists);

                //        //                                GetProfileImageFromSpotifyFeed(_Artists);

                //        //                                #region "Loop through the Genre"
                //        //                                if (_perfomer.genres != null && _perfomer.genres.Count > 0)
                //        //                                {
                //        //                                    foreach (SeatGeek4.Genre _Ev in _perfomer.genres)
                //        //                                    {
                //        //                                        _Genre = _GenreRepo.Repository.Get(p => p.Name == _Ev.name.Trim());

                //        //                                        if (_Genre == null)
                //        //                                        {
                //        //                                            _Genre = new Genre();
                //        //                                            _Genre.Name = _Ev.name;
                //        //                                            _Genre.CreatedDate = DateTime.Now;
                //        //                                            _Genre.RecordStatus = RecordStatus.SeatGeek.ToString();
                //        //                                            _GenreRepo.Repository.Add(_Genre);
                //        //                                        }

                //        //                                        var check = !_ArtistGenreRepo.Repository.AsQueryable().Any(x => x.ArtistID == _Artists.ArtistID && x.GenreID == _Genre.GenreID);

                //        //                                        if (check)
                //        //                                        {
                //        //                                            _ArtistGenre = new ArtistGenre();
                //        //                                            _ArtistGenre.ArtistID = _Artists.ArtistID;
                //        //                                            _ArtistGenre.Slug = _Ev.slug;
                //        //                                            _ArtistGenre.Primary = _Ev.primary;
                //        //                                            _ArtistGenre.Name = _Ev.name;
                //        //                                            _ArtistGenre.CreatedDate = DateTime.Now;
                //        //                                            _ArtistGenre.RecordStatus = RecordStatus.SeatGeek.ToString();

                //        //                                            _ArtistGenreRepo.Repository.Add(_ArtistGenre);
                //        //                                        }
                //        //                                    }
                //        //                                }
                //        //                                #endregion

                //        //                                await Task.Factory.StartNew(() =>
                //        //                                {
                //        //                                    if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                //        //                                    {
                //        //                                        #region "Get Artist Picture "
                //        //                                        try
                //        //                                        {
                //        //                                            if (_perfomer.images != null)
                //        //                                            {
                //        //                                                if (_perfomer.images.huge != null)
                //        //                                                {
                //        //                                                    bool _ArtistPic = ArtistProfilePicture_Asyn(UserID, _Artists, _unitOfWork, true, _perfomer.images.huge);
                //        //                                                }
                //        //                                            }
                //        //                                        }
                //        //                                        catch (Exception)
                //        //                                        {

                //        //                                        }
                //        //                                        #endregion
                //        //                                    }

                //        //                                    #region "Get Similar Artists (dont need this block while just updating the records)"
                //        //                                    Task<List<MusicGraph.Datum>> _GetSimilarArtists_ByID = Spotify_GetSimilarArtistByID(_Artists.Spotify_ID);
                //        //                                    #endregion

                //        //                                    _GetSimilarArtists_ByID.Wait();

                //        //                                    #region "Eventful API Implementation"
                //        //                                    Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(UserID, _Artists, _unitOfWork, true);
                //        //                                    #endregion

                //        //                                    #region "Instagram Api Implementation"
                //        //                                    //Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, _ArtistMatrics_ByID.Result, _unitOfWork, true, _TourDateEntity);
                //        //                                    #endregion

                //        //                                    #region "Spotify Api Implementation
                //        //                                    Task<Dictionary<string, object>> _Spotify = Spotify_GetSongInfo_Asyn(UserID, _Artists, _unitOfWork, true);
                //        //                                    #endregion
                //        //                                });
                //        //                            }
                //        //                        }
                //        //                        #endregion

                //        //                        #region "Update Existing"
                //        //                        else
                //        //                        {
                //        //                            _Artists.ModifiedDate = DateTime.Now;
                //        //                            _Artists.OnTour = _perfomer.has_upcoming_events == true ? true : false;
                //        //                            await Task.Factory.StartNew(() =>
                //        //                            {
                //        //                                if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                //        //                                {
                //        //                                    #region "Get Artist Picture "
                //        //                                    try
                //        //                                    {
                //        //                                        if (_perfomer.images != null)
                //        //                                        {
                //        //                                            if (_perfomer.images.huge != null)
                //        //                                            {
                //        //                                                bool _ArtistPic = ArtistProfilePicture_Asyn(UserID, _Artists, _unitOfWork, true, _perfomer.images.huge);
                //        //                                            }
                //        //                                        }
                //        //                                    }
                //        //                                    catch (Exception ex)
                //        //                                    {

                //        //                                    }
                //        //                                    #endregion
                //        //                                }

                //        //                                #region "Instagram Api Implementation"
                //        //                                Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, null, _unitOfWork, false, _TourDateEntity);
                //        //                                #endregion

                //        //                                #region "Eventful API Implementation"
                //        //                                Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(UserID, _Artists, _unitOfWork, false);
                //        //                                #endregion

                //        //                                #region "Spotify Api Implementation
                //        //                                Task<Dictionary<string, object>> _Spotify = Spotify_GetSongInfo_Asyn(UserID, _Artists, _unitOfWork, false);
                //        //                                #endregion
                //        //                            });
                //        //                        }
                //        //                        #endregion   
                //        //                        if (_perfomer.primary == true || _PerformersChk == null)
                //        //                        {
                //        //                            _PerformersChk = _perfomer;
                //        //                            #region "Add/Update Tour Information"

                //        //                            DateTime _datetime_local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);


                //        //                            _TourDateEntity = (from A in _VenueRepo.Repository.GetAll(p => p.VenueID == _VenuEntity.VenueID)
                //        //                                               join B in _TourDateRepo.Repository.GetAll(p =>
                //        //                                                                               (p.SeatGeek_TourID == _Get_Events_ByID.id.ToString() && p.ArtistID == _Artists.ArtistID)
                //        //                                                                            || (DbFunctions.TruncateTime(p.Datetime_Local).Value.Month == _datetime_local.Month
                //        //                                                                                && DbFunctions.TruncateTime(p.Datetime_Local).Value.Year == _datetime_local.Year
                //        //                                                                                && DbFunctions.TruncateTime(p.Datetime_Local).Value.Day == _datetime_local.Day
                //        //                                                                                && p.ArtistID == _Artists.ArtistID
                //        //                                                                                && p.RecordStatus == RecordStatus.Eventful.ToString())
                //        //                                                                            ) on A.VenueID equals B.VenueID
                //        //                                               where B.ArtistID == _Artists.ArtistID
                //        //                                               select B).FirstOrDefault();

                //        //                            if (_TourDateEntity == null)
                //        //                            {
                //        //                                _TourDateEntity = new TourDate();

                //        //                                _TourDateEntity.SeatGeek_TourID = _Get_Events_ByID.id.ToString();
                //        //                                _TourDateEntity.ArtistID = _Artists.ArtistID;
                //        //                                _TourDateEntity.VenueID = _VenuEntity.VenueID;

                //        //                                _TourDateEntity.EventID = null;
                //        //                                _TourDateEntity.Score = _Get_Events_ByID.score;

                //        //                                _TourDateEntity.Announce_Date = Convert.ToDateTime(_Get_Events_ByID.announce_date);
                //        //                                _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Events_ByID.visible_until_utc);
                //        //                                _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Events_ByID.datetime_utc);
                //        //                                _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);

                //        //                                _TourDateEntity.CreatedDate = DateTime.Now;
                //        //                                _TourDateEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                //        //                                if (!String.IsNullOrEmpty(_Get_Events_ByID.url))
                //        //                                    _TourDateEntity.TicketURL = _Get_Events_ByID.url;
                //        //                                else if (_Get_Events_ByID.venue != null && !String.IsNullOrEmpty(_Get_Events_ByID.venue.url))
                //        //                                    _TourDateEntity.TicketURL = _Get_Events_ByID.venue.url;
                //        //                                else
                //        //                                    _TourDateEntity.TicketURL = "https://seatgeek.com/";

                //        //                                _TourDateRepo.Repository.Add(_TourDateEntity);
                //        //                            }
                //        //                            else
                //        //                            {
                //        //                                _TourDateEntity.SeatGeek_TourID = _Get_Events_ByID.id.ToString();
                //        //                                _TourDateEntity.ArtistID = _Artists.ArtistID;
                //        //                                _TourDateEntity.VenueID = _VenuEntity.VenueID;
                //        //                                _TourDateEntity.Score = _Get_Events_ByID.score;

                //        //                                _TourDateEntity.Announce_Date = Convert.ToDateTime(_Get_Events_ByID.announce_date);
                //        //                                _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Events_ByID.visible_until_utc);
                //        //                                _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Events_ByID.datetime_utc);

                //        //                                _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);

                //        //                                _TourDateEntity.ModifiedDate = DateTime.Now;
                //        //                                _TourDateEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                //        //                                if (!String.IsNullOrEmpty(_Get_Events_ByID.url))
                //        //                                    _TourDateEntity.TicketURL = _Get_Events_ByID.url;
                //        //                                else if (_Get_Events_ByID.venue != null && !String.IsNullOrEmpty(_Get_Events_ByID.venue.url))
                //        //                                    _TourDateEntity.TicketURL = _Get_Events_ByID.venue.url;
                //        //                                else
                //        //                                    _TourDateEntity.TicketURL = "https://seatgeek.com/";

                //        //                                _TourDateRepo.Repository.Update(_TourDateEntity);
                //        //                            }
                //        //                            #endregion
                //        //                        }
                //        //                        else
                //        //                        {
                //        //                            #region "Save Other Artist in Tour Perfromer"
                //        //                            _TourPerformers = _TourPerformersRepo.Repository.Get(p => p.ArtistID == _Artists.ArtistID && p.TourDateID == _TourDateEntity.TourDateID);

                //        //                            if (_TourPerformers == null)
                //        //                            {
                //        //                                _TourPerformers = new TourPerformers();

                //        //                                _TourPerformers.TourDateID = _TourDateEntity.TourDateID;
                //        //                                _TourPerformers.ArtistID = _Artists.ArtistID;
                //        //                                _TourPerformers.CreatedDate = DateTime.Now;
                //        //                                _TourPerformers.RecordStatus = RecordStatus.MusicGraph.ToString();

                //        //                                _TourPerformersRepo.Repository.Add(_TourPerformers);
                //        //                            }
                //        //                            #endregion
                //        //                        }
                //        //                        _ArtistsRepo.Repository.Update(_Artists);
                //        //                        break;
                //        //                    }
                //        //                }
                //        //            }
                //        //        }
                //        //        catch (Exception ex)
                //        //        {

                //        //        }

                //        //        #endregion

                //        //        if (_MusicGraph_ID == null)
                //        //        {
                //        //            #region "use this block if Artist not found in (MusicGrapgh)"
                //        //            try
                //        //            {
                //        //                _Artists = _ArtistsRepo.Repository.Get(p => p.Seatgeek_ID == _perfomer.id.ToString());

                //        //                #region "Add New"
                //        //                if (_Artists == null)
                //        //                {
                //        //                    bool isLatin = false;

                //        //                    if (!isLatin)
                //        //                        isLatin = CheckSeatGeekLatin(_perfomer.name, _unitOfWork);

                //        //                    if (isLatin)
                //        //                    {

                //        //                        _Artists = new Artists();

                //        //                        _Artists.ArtistName = _perfomer.name;//  Regex.Replace(_perfomer.name, "[^A-Za-z0-9 _]", "");
                //        //                        _Artists.Gender = null;
                //        //                        _Artists.Decade = null;

                //        //                        if (_perfomer.genres != null)
                //        //                        {
                //        //                            _Artists.Main_Genre = _perfomer.genres[0].name;
                //        //                        }

                //        //                        _Artists.Seatgeek_ID = _perfomer.id.ToString();

                //        //                        _Artists.Musicgraph_ID = _perfomer.name;
                //        //                        _Artists.Artist_Ref_ID = null;
                //        //                        _Artists.Musicbrainz_ID = null;
                //        //                        _Artists.Spotify_ID = null;
                //        //                        _Artists.Youtube_ID = null;
                //        //                        _Artists.Alternate_Names = null;

                //        //                        _Artists.RecordStatus = RecordStatus.SeatGeek.ToString();
                //        //                        _Artists.CreatedDate = DateTime.Now;
                //        //                        _Artists.ModifiedDate = DateTime.Now;

                //        //                        _Artists.OnTour = _perfomer.has_upcoming_events == true ? true : false;

                //        //                        _ArtistsRepo.Repository.Add(_Artists);

                //        //                        GetProfileImageFromSpotifyFeed(_Artists);

                //        //                        _unitOfWork.Commit();
                //        //                        _unitOfWork.StartTransaction();

                //        //                        #region "Loop through the Genre"
                //        //                        if (_perfomer.genres != null && _perfomer.genres.Count > 0)
                //        //                        {
                //        //                            foreach (SeatGeek4.Genre _Ev in _perfomer.genres)
                //        //                            {
                //        //                                _Genre = _GenreRepo.Repository.Get(p => p.Name == _Ev.name.Trim());

                //        //                                if (_Genre == null)
                //        //                                {
                //        //                                    _Genre = new Genre();
                //        //                                    _Genre.Name = _Ev.name;
                //        //                                    _Genre.CreatedDate = DateTime.Now;
                //        //                                    _Genre.RecordStatus = RecordStatus.SeatGeek.ToString();
                //        //                                    _GenreRepo.Repository.Add(_Genre);

                //        //                                }

                //        //                                var check = !_ArtistGenreRepo.Repository.AsQueryable().Any(x => x.ArtistID == _Artists.ArtistID && x.GenreID == _Genre.GenreID);

                //        //                                if (check)
                //        //                                {
                //        //                                    _ArtistGenre = new ArtistGenre();
                //        //                                    _ArtistGenre.GenreID = _Genre.GenreID;
                //        //                                    _ArtistGenre.ArtistID = _Artists.ArtistID;
                //        //                                    _ArtistGenre.Slug = _Ev.slug;
                //        //                                    _ArtistGenre.Primary = _Ev.primary;
                //        //                                    _ArtistGenre.Name = _Ev.name;
                //        //                                    _ArtistGenre.CreatedDate = DateTime.Now;
                //        //                                    _ArtistGenre.RecordStatus = RecordStatus.SeatGeek.ToString();

                //        //                                    _ArtistGenreRepo.Repository.Add(_ArtistGenre);
                //        //                                }
                //        //                            }
                //        //                        }
                //        //                        #endregion

                //        //                        await Task.Factory.StartNew(() =>
                //        //                        {
                //        //                            if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                //        //                            {
                //        //                                #region "Get Artist Picture "
                //        //                                try
                //        //                                {
                //        //                                    if (_perfomer.images != null)
                //        //                                    {
                //        //                                        if (_perfomer.images.huge != null)
                //        //                                        {
                //        //                                            bool _ArtistPic = ArtistProfilePicture_Asyn(UserID, _Artists, _unitOfWork, true, _perfomer.images.huge);
                //        //                                        }
                //        //                                    }
                //        //                                }
                //        //                                catch (Exception ex)
                //        //                                {

                //        //                                }
                //        //                                #endregion
                //        //                            }

                //        //                            #region "Instagram Api Implementation"
                //        //                            Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, null, _unitOfWork, true, _TourDateEntity);
                //        //                            #endregion

                //        //                            #region "Eventful API Implementation"
                //        //                            Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(UserID, _Artists, _unitOfWork, true);
                //        //                            #endregion

                //        //                        });
                //        //                    }
                //        //                }
                //        //                #endregion

                //        //                #region "Update Existing"
                //        //                else
                //        //                {
                //        //                    _Artists.ModifiedDate = DateTime.Now;
                //        //                    _Artists.OnTour = _perfomer.has_upcoming_events == true ? true : false;

                //        //                    await Task.Factory.StartNew(() =>
                //        //                    {
                //        //                        if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                //        //                        {
                //        //                            #region "Get Artist Picture "
                //        //                            try
                //        //                            {
                //        //                                if (_perfomer.images != null)
                //        //                                {
                //        //                                    if (_perfomer.images.huge != null)
                //        //                                    {
                //        //                                        bool _ArtistPic = ArtistProfilePicture_Asyn(UserID, _Artists, _unitOfWork, true, _perfomer.images.huge);
                //        //                                    }
                //        //                                }
                //        //                            }
                //        //                            catch (Exception ex)
                //        //                            {

                //        //                            }
                //        //                            #endregion
                //        //                        }
                //        //                        #region "Instagram Api Implementation"
                //        //                        Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, null, _unitOfWork, false, _TourDateEntity);
                //        //                        #endregion

                //        //                        #region "Eventful API Implementation"
                //        //                        Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(UserID, _Artists, _unitOfWork, false);
                //        //                        #endregion
                //        //                    });
                //        //                }
                //        //                #endregion

                //        //                if (_perfomer.primary == true || _PerformersChk == null)
                //        //                {
                //        //                    _PerformersChk = _perfomer;
                //        //                    #region "Add/Update Tour Information"
                //        //                    //Entering Tour records
                //        //                    //_TourDateEntity = _TourDateRepo.Repository.Get(p => p.SeatGeek_TourID == _Get_Events_ByID.id.ToString() && p.ArtistID == _Artists.ArtistID);
                //        //                    DateTime _datetime_local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);

                //        //                    _TourDateEntity = (from A in _VenueRepo.Repository.GetAll(p => p.VenueID == _VenuEntity.VenueID)
                //        //                                       join B in _TourDateRepo.Repository.GetAll(p =>
                //        //                                                                       (p.SeatGeek_TourID == _Get_Events_ByID.id.ToString() && p.ArtistID == _Artists.ArtistID)
                //        //                                                                    || (DbFunctions.TruncateTime(p.Datetime_Local).Value.Month == _datetime_local.Month
                //        //                                                                        && DbFunctions.TruncateTime(p.Datetime_Local).Value.Year == _datetime_local.Year
                //        //                                                                        && DbFunctions.TruncateTime(p.Datetime_Local).Value.Day == _datetime_local.Day)
                //        //                                                                        && p.ArtistID == _Artists.ArtistID
                //        //                                                                        && p.RecordStatus == RecordStatus.Eventful.ToString()
                //        //                                                                    ) on A.VenueID equals B.VenueID
                //        //                                       select B).FirstOrDefault();

                //        //                    if (_TourDateEntity == null)
                //        //                    {
                //        //                        _TourDateEntity = new TourDate();

                //        //                        _TourDateEntity.SeatGeek_TourID = _Get_Events_ByID.id.ToString();
                //        //                        _TourDateEntity.ArtistID = _Artists.ArtistID;
                //        //                        _TourDateEntity.VenueID = _VenuEntity.VenueID;

                //        //                        _TourDateEntity.EventID = null;
                //        //                        _TourDateEntity.Score = _Get_Events_ByID.score;

                //        //                        _TourDateEntity.Announce_Date = Convert.ToDateTime(_Get_Events_ByID.announce_date);
                //        //                        _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Events_ByID.visible_until_utc);
                //        //                        _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Events_ByID.datetime_utc);
                //        //                        _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);

                //        //                        _TourDateEntity.CreatedDate = DateTime.Now;
                //        //                        _TourDateEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                //        //                        if (!String.IsNullOrEmpty(_Get_Events_ByID.url))
                //        //                            _TourDateEntity.TicketURL = _Get_Events_ByID.url;
                //        //                        else if (_Get_Events_ByID.venue != null && !String.IsNullOrEmpty(_Get_Events_ByID.venue.url))
                //        //                            _TourDateEntity.TicketURL = _Get_Events_ByID.venue.url;
                //        //                        else
                //        //                            _TourDateEntity.TicketURL = "https://seatgeek.com/";

                //        //                        _TourDateRepo.Repository.Add(_TourDateEntity);
                //        //                    }
                //        //                    else
                //        //                    {

                //        //                        _TourDateEntity.SeatGeek_TourID = _Get_Events_ByID.id.ToString();
                //        //                        _TourDateEntity.ArtistID = _Artists.ArtistID;
                //        //                        _TourDateEntity.VenueID = _VenuEntity.VenueID;
                //        //                        _TourDateEntity.Score = _Get_Events_ByID.score;

                //        //                        _TourDateEntity.Announce_Date = Convert.ToDateTime(_Get_Events_ByID.announce_date);
                //        //                        _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Events_ByID.visible_until_utc);
                //        //                        _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Events_ByID.datetime_utc);

                //        //                        _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Events_ByID.datetime_local);

                //        //                        _TourDateEntity.ModifiedDate = DateTime.Now;

                //        //                        if (!String.IsNullOrEmpty(_Get_Events_ByID.url))
                //        //                            _TourDateEntity.TicketURL = _Get_Events_ByID.url;
                //        //                        else if (_Get_Events_ByID.venue != null && !String.IsNullOrEmpty(_Get_Events_ByID.venue.url))
                //        //                            _TourDateEntity.TicketURL = _Get_Events_ByID.venue.url;
                //        //                        else
                //        //                            _TourDateEntity.TicketURL = "https://seatgeek.com/";

                //        //                        _TourDateRepo.Repository.Update(_TourDateEntity);

                //        //                    }
                //        //                    #endregion
                //        //                }
                //        //                else
                //        //                {
                //        //                    #region "Save Other Artist in Tour Perfromer"
                //        //                    _TourPerformers = _TourPerformersRepo.Repository.Get(p => p.ArtistID == _Artists.ArtistID && p.TourDateID == _TourDateEntity.TourDateID);

                //        //                    if (_TourPerformers == null)
                //        //                    {
                //        //                        _TourPerformers = new TourPerformers();

                //        //                        _TourPerformers.TourDateID = _TourDateEntity.TourDateID;
                //        //                        _TourPerformers.ArtistID = _Artists.ArtistID;
                //        //                        _TourPerformers.CreatedDate = DateTime.Now;
                //        //                        _TourPerformers.RecordStatus = RecordStatus.MusicGraph.ToString();

                //        //                        _TourPerformersRepo.Repository.Add(_TourPerformers);
                //        //                    }
                //        //                    #endregion
                //        //                }


                //        //                _ArtistsRepo.Repository.Update(_Artists);

                //        //            }
                //        //            catch (Exception ex)
                //        //            {

                //        //            }
                //        //            #endregion
                //        //        }
                //        //    }
                //        //    #endregion

                //        //    #endregion

                //        //}
                //        //#endregion
                //        #endregion

                //        // Added on NOv05
                //        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "Event not found", "EventsDetail"));
                //    }
                //    else
                //    {
                //        await Task.Factory.StartNew(() =>
                //        {
                //            #region "Instagram Api Implementation"
                //            Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(UserID, _Artists, null, _unitOfWork, false, _TourDateEntity);
                //            #endregion

                //            _Instagram.Wait();
                //        });

                //        #region "EventFul"
                //        if (_TourDateEntity.Eventful_TourID != null)
                //        {

                //        }
                //        #endregion

                //        #region "SeetGeek"
                //        if (_TourDateEntity.SeatGeek_TourID != null)
                //        {

                //        }
                //        #endregion

                //    }

                //    _unitOfWork.Commit();

                //    if (_TourDateEntity != null)
                //    {
                //        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, GetTourDetail_ByID(_TourDateEntity, UserID), "EventsDetail"));
                //    }
                //    else
                //    {
                //        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "Event not found", "EventsDetail"));
                //    }
                //}
                //catch (Exception ex)
                //{
                //    LogHelper.CreateLog3(ex, Request);
                //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "EventsDetail"));
                //}
                #endregion
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
            GenericRepository<TicketingUsers> _TicketingUserRepo = new GenericRepository<TicketingUsers>(_unitOfWork);

            GenericRepository<TicketingEventTickets> _TicketingRepo = new GenericRepository<TicketingEventTickets>(_unitOfWork);
            GenericRepository<TicketingEventTicketsSummary> _TicketingEventSummary = new GenericRepository<TicketingEventTicketsSummary>(_unitOfWork);
            GenericRepository<TicketingEventsNew> _TicketingEventNewRepo = new GenericRepository<TicketingEventsNew>(_unitOfWork);

            Models.UserTourDate _UserTourDate = null;
            Models.Artists _Artist = null;

            var _users = _UsersRepo.Repository.GetById(UserID);
            if (_users == null)
            {
                _users = new Users();
                _users.UserID = 0;
            }

            _Artist = _ArtistsRepo.Repository.Get(p => p.ArtistID == _TourDate.ArtistID);

            _ViewEventDetail.TourID = _TourDate.TourDateID;
            _ViewEventDetail.ArtistID = _Artist.ArtistID;
            _ViewEventDetail.ArtistName = _Artist.ArtistName;

            if (_users != null)
            {
                _ViewEventDetail.About = _Artist.AboutES != null && _users.UserLanguage == EUserLanguage.ES.ToString() ? _Artist.AboutES : _Artist.About;
            }
            else
            {
                _ViewEventDetail.About = string.Empty;
            }

            //if (_ViewEventDetail.About == null)
            //    _ViewEventDetail.About = "";

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
            try
            {
                _ViewEventDetail.Date_Local = _TourDate.Datetime_Local.ToString().Substring(0, 10).Trim();
                _ViewEventDetail.Time_Local = _TourDate.Datetime_Local.ToString().Substring(10).Trim();
            }
            catch (Exception) { }
            if (_users != null && _users.UserID != 0)
            {
                _UserTourDate = _UserTourDateRepo.Repository.Get(p => p.TourDateID == _TourDate.TourDateID && p.UserID == UserID && p.RecordStatus != RecordStatus.Deleted.ToString());
            }
            else
            {
                _UserTourDate = _UserTourDateRepo.Repository.Get(p => p.TourDateID == _TourDate.TourDateID && p.RecordStatus != RecordStatus.Deleted.ToString());
            }
            if (_UserTourDate == null && _users.UserID != 0)
            {
                _ViewEventDetail.IsTracking = false;
            }
            else
            {
                _ViewEventDetail.IsTracking = true;
            }


            if (!String.IsNullOrEmpty(_TourDate.TicketURL))
            {
                _ViewEventDetail.TicketURL = _TourDate.TicketURL;
            }
            else
            {
                _ViewEventDetail.TicketURL = "http://appserver.musikaapp.com/TicketEventCheckout.aspx";
            }


            _ViewEventDetail.Event_Name = _TourDate.EventName;


            Models.UserGoing _UserGoing = null;
            if (_users != null && _users.UserID != 0)
            {
                _UserGoing = _UserGoingRepo.Repository.Get(p => p.TourDateID == _TourDate.TourDateID && p.UserID == UserID);
            }
            else
            {
                _UserGoing = _UserGoingRepo.Repository.Get(p => p.TourDateID == _TourDate.TourDateID);
            }

            if (_UserGoing != null && _users.UserID != 0)
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

            //var _lst = (from A1 in _UserFriendsRepo.Repository.GetAll(p => p.UserID == UserID)
            //            join A in _UserGoingRepo.Repository.GetAll(p => p.TourDateID == _TourDate.TourDateID && p.RecordStatus == EUserGoing.Going.ToString()) on A1.Matched_UserID equals A.UserID
            //            join B in _UsersRepo.Repository.GetAll() on A.UserID equals B.UserID
            //            select new ViewEventUsers
            //            {
            //                ThumbnailURL = B.ThumbnailURL ?? "",
            //                UserID = A.UserID.Value,
            //                UserName = B.UserName.ToString(),
            //                Email = B.Email ?? "",
            //                CreatedDate = Convert.ToDateTime(A.CreatedDate).ToString("d")
            //            }).OrderByDescending(p => p.CreatedDate).ToList();
            //who else is going
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
                        }).DistinctBy(y => y.UserID).OrderByDescending(p => p.CreatedDate).ToList();
            // Get Users Going For EventList
            //List<ViewEventUsers> lstViewEventUsers = new List<ViewEventUsers>();
            //DataSet ds1 = new DataSet();
            //ds1 = new SpRepository().GetUsersGoingForEventList(_TourDate.TourDateID, UserID);
            //if (ds1.Tables[0].Rows.Count > 0)
            //{
            //    for (int i = 0; i < ds1.Tables[0].Rows.Count; i++)
            //    {
            //        ViewEventUsers viewEventUsers = new ViewEventUsers();
            //        viewEventUsers.ThumbnailURL = ds1.Tables[0].Rows[i]["ThumbnailURL"].ToString();
            //        viewEventUsers.UserID = Convert.ToInt32(ds1.Tables[0].Rows[i]["UserID"].ToString());
            //        viewEventUsers.UserName = ds1.Tables[0].Rows[i]["UserName"].ToString();
            //        viewEventUsers.Email = ds1.Tables[0].Rows[i]["Email"].ToString();
            //        viewEventUsers.CreatedDate = "";//Convert.ToDateTime(ds1.Tables[0].Rows[i]["CreatedDate"].ToString()).ToString("d");
            //        lstViewEventUsers.Add(viewEventUsers);
            //    }
            //}

            //who else is going
            _ViewEventDetail.UsersGoing = _lst.ToList();
            // _ViewEventDetail.UsersGoing = lstViewEventUsers;

            _ViewEventDetail.NoOfUserGoing = _lst.Count();
            //  _ViewEventDetail.NoOfUserGoing = lstViewEventUsers.Count();

            //Get Similar Artists
            _ViewEventDetail.ArtistRelated = (from A in _ArtistRelatedRepo.Repository.GetAll(p => p.ArtistID == _Artist.ArtistID)
                                              select new viewEventRelated
                                              {
                                                  Musicgraph_ID = A.Musicgraph_ID,
                                                  RelatedArtistName = A.RelatedArtistName,
                                                  Similarity = A.Similarity
                                              }).ToList();

            // Added by Mukesh
            int? eventID;
            MusikaEntities db = new MusikaEntities();
            List<TourDate> lstSummary = new List<TourDate>();
            eventID = db.TourDate.Where(t => t.TourDateID == _TourDate.TourDateID).FirstOrDefault().TicketingEventID;
            //eventID = _TourDateRepo.Repository.GetAll().Where(t => t.TourDateID == _TourDate.TourDateID).FirstOrDefault().TicketingEventID;
            if (!String.IsNullOrEmpty(eventID.ToString()))
            {
                //lstSummary = _TourDateRepo.Repository.GetAll().Where(p => p.TicketingEventID == eventID).ToList();
                lstSummary = db.TourDate.Where(p => p.TicketingEventID == eventID).ToList();
            }

            List<Ticket> lstTicket = new List<Ticket>();
            if (lstSummary.Count > 0)
            {
                for (int i = 0; i < lstSummary.Count; i++)
                {
                    Ticket tickets = new Ticket();
                    tickets.EventId = lstSummary[i].TicketingEventID ?? default(int);

                    TicketingEventTicketsSummary summary = new TicketingEventTicketsSummary();
                    summary = _TicketingEventSummary.Repository.GetAll().Where(t => t.EventID == tickets.EventId).ToList().FirstOrDefault();

                    if (summary != null)
                    {
                        tickets.CountryId = summary.CountryId;
                        tickets.Currency = summary.Currency;
                        tickets.RefundPolicy = summary.RefundPolicy;
                        tickets.EventId = summary.EventID;
                        tickets.ServiceFee = string.IsNullOrEmpty(summary.ServiceFee) ? "15%" : summary.ServiceFee;
                        tickets.Tax = string.IsNullOrEmpty(summary.Tax) ? "18%" : summary.Tax;
                    }
                    else
                    {
                        tickets.CountryId = 1;
                        tickets.Currency = "$";
                        tickets.RefundPolicy = "Yes";
                        tickets.ServiceFee = "15%";
                        tickets.Tax = "18%";
                        tickets.EventId = tickets.EventId = eventID ?? default(int);
                    }
                    List<TicketingEventTicketsSummary> lstTicketData = new List<TicketingEventTicketsSummary>();
                    List<TicketData> lstData = new List<TicketData>();

                    DataSet ds = new DataSet();
                    ds = new SpRepository().GetTicketBalanceSummary(tickets.EventId);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                        {
                            TicketData data = new TicketData();
                            DataRow dr = ds.Tables[0].Rows[j];
                            //if (dr["Balance"].ToString() != "0")
                            //{
                            if ((Convert.ToDateTime(dr["PackageStartDate"]).AddHours(12) <= DateTime.UtcNow.Date.AddHours(12)) && Convert.ToDateTime(dr["PackageEndDate"]).AddHours(12) >= (DateTime.UtcNow.Date.AddHours(12)))
                            {
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
                            else if ((Convert.ToDateTime(dr["PackageStartDate"]).AddHours(12) > DateTime.UtcNow.Date.AddHours(12)) && Convert.ToDateTime(dr["PackageEndDate"]).AddHours(12) >= (DateTime.UtcNow.Date.AddHours(12)))
                            {
                                if (lstData.Count == 0)
                                {
                                    tickets.RefundPolicy = "Coming Soon"; //considering this field to show expiry and coming soon message
                                }

                            }
                            else if (Convert.ToDateTime(dr["PackageEndDate"]).AddHours(12) < (DateTime.UtcNow.Date.AddHours(12)))
                            {
                                if (lstData.Count == 0)
                                {
                                    tickets.RefundPolicy = "Ticket Expired"; //considering this field to show expiry and coming soon message
                                }
                            }
                            // }
                        }
                    }

                    #region "Unused Code"
                    //lstTicketData = _TicketingEventSummary.Repository.GetAll().Where(t => t.EventID == tickets.EventId).ToList();

                    //if (lstTicketData.Count > 0)
                    //{
                    //    for (int k = 0; k < lstTicketData.Count; k++)
                    //    {
                    //        TicketData data = new TicketData();
                    //        data.EventId = eventID ?? 0;
                    //        data.Price = lstTicketData[k].Cost;
                    //        data.Quantity = lstTicketData[k].Quantity;
                    //        //data.TicketCategory = lstTicketData[i].TicketCategory;
                    //        //data.TicketType = lstTicketData[i].TicketType;
                    //        data.TicketCategory = lstTicketData[k].TicketCategory;
                    //        data.TicketType = lstTicketData[k].TicketType;
                    //        lstData.Add(data);
                    //    }
                    //}
                    #endregion

                    tickets.lstTicketData = lstData;
                    lstTicket.Add(tickets);
                }
            }

            TicketingEventsNew ticketingNew = _TicketingEventNewRepo.Repository.GetAll().Where(p => p.EventID == eventID).FirstOrDefault();


            try
            {
                if (ticketingNew != null)
                {
                    _ViewEventDetail.Date_Local = Convert.ToDateTime(ticketingNew.StartDate).ToString("d");
                    _ViewEventDetail.Time_Local = Convert.ToDateTime(ticketingNew.StartTime).ToString("t");
                }
            }
            catch (Exception)
            { }



            if (ticketingNew != null)
            {
                _ViewEventDetail.Event_Description = Regex.Replace(ticketingNew.EventDescription, "<.*?>", String.Empty).Replace("\n", "").Replace("\t", "");
                _ViewEventDetail.EventDescriptionNew = ticketingNew.EventDescription.Replace("\n", "").Replace("\t", "");
                _ViewEventDetail.Event_Name = Regex.Replace(_TourDate.EventName, "<.*?>", String.Empty).Replace("\n", "").Replace("\t", "");
                ticketingNew.OrganizerName = _TicketingUserRepo.Repository.GetById((int)ticketingNew.CreatedBy).UserName;
                _ViewEventDetail.Organizer_Name = ticketingNew.OrganizerName;
                _ViewEventDetail.Organizer_Description = Regex.Replace(ticketingNew.OrganizerDescription, "<.*?>", String.Empty).Replace("\n", "").Replace("\t", "");
                _ViewEventDetail.OrganizerDescriptionNew = ticketingNew.OrganizerDescription.Replace("\n", "").Replace("\t", "");
            }
            else
            {
                _ViewEventDetail.Organizer_Name = "Musika";
                _ViewEventDetail.Organizer_Description = "Musika Event Organizer";

            }
            _ViewEventDetail.lstTicket = lstTicket;
            return _ViewEventDetail;
        }


        [HttpPost]
        [Route("api/v2/Events/UpdateIsGoing")]
        public HttpResponseMessage UpdateIsGoing(InputUpdateisGoing input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);

                Models.Users _Users = null;
                Models.UserGoing _UserGoing = null;

                _Users = _UsersRepo.Repository.Get(p => p.UserID == input.UserID);

                if (_Users == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "IsGoing"));
                }


                //User Going
                if (input.UserGoing.ToString() == EUserGoing.Going.ToString())
                {
                    _UserGoing = _UserGoingRepo.Repository.Get(p => p.TourDateID == input.TourID && p.UserID == input.UserID);
                    if (_UserGoing == null)
                    {
                        _UserGoing = new UserGoing();
                        _UserGoing.UserID = input.UserID;
                        _UserGoing.TourDateID = input.TourID;
                        _UserGoing.CreatedDate = DateTime.Now;
                        _UserGoing.RecordStatus = input.UserGoing.ToString();

                        _UserGoingRepo.Repository.Add(_UserGoing);
                    }
                    else
                    {
                        _UserGoing.RecordStatus = input.UserGoing.ToString();
                        _UserGoing.ModifiedDate = DateTime.Now;

                        _UserGoingRepo.Repository.Update(_UserGoing);
                    }
                }

                //User Not Going
                if (input.UserGoing.ToString() == EUserGoing.NotGoing.ToString())
                {
                    _UserGoing = _UserGoingRepo.Repository.Get(p => p.TourDateID == input.TourID && p.UserID == input.UserID);
                    if (_UserGoing == null)
                    {
                        _UserGoing = new UserGoing();
                        _UserGoing.UserID = input.UserID;
                        _UserGoing.TourDateID = input.TourID;
                        _UserGoing.CreatedDate = DateTime.Now;
                        _UserGoing.RecordStatus = input.UserGoing.ToString();

                        _UserGoingRepo.Repository.Add(_UserGoing);
                    }
                    else
                    {
                        _UserGoing.RecordStatus = input.UserGoing.ToString();
                        _UserGoing.ModifiedDate = DateTime.Now;

                        _UserGoingRepo.Repository.Update(_UserGoing);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, input.UserGoing.ToString(), "IsGoing"));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "IsGoing"));
            }
        }


        [HttpGet]
        [Route("api/v2/Events/GetPeopleGoing")]
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
                                          }).DistinctBy(y => y.UserID).OrderByDescending(p => p.CreatedDate).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewPeopleGoing, "PeopleGiong"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "PeopleGiong"));
            }
        }


        [HttpPost]
        [Route("api/v2/Events/UpdateTrackEvent")]
        public HttpResponseMessage UpdateTrackEvent(InputTrackEvent input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

                Models.Users _Users = null;
                Models.TourDate _TourDate = null;
                Models.UserTourDate _UserTourDate = null;

                _TourDate = _TourDateRepo.Repository.Get(p => p.TourDateID == input.TourID);
                if (_TourDate == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.EventNotDound, "TrackEvent"));
                }

                _UserTourDate = _UserTourDateRepo.Repository.Get(p => p.TourDateID == input.TourID && p.UserID == input.UserID);

                //Tracking
                if (input.TrackEvent.ToString() == ETracking.Tracking.ToString())
                {
                    if (_UserTourDate == null)
                    {
                        _UserTourDate = new UserTourDate();
                        _UserTourDate.TourDateID = _TourDate.TourDateID;
                        _UserTourDate.UserID = input.UserID;
                        _UserTourDate.CreatedDate = DateTime.Now;
                        _UserTourDate.RecordStatus = RecordStatus.Active.ToString();

                        _UserTourDateRepo.Repository.Add(_UserTourDate);
                    }
                }
                else
                {
                    if (_UserTourDate != null)
                    {
                        _UserTourDateRepo.Repository.DeletePermanent(_UserTourDate.UserTourDateID);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, input.TrackEvent.ToString(), "TrackEvent"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "TrackEvent"));
            }
        }


        #region "Venue"

        [HttpGet]
        [Route("api/v2/Venue/GetVenueDetail")]
        public HttpResponseMessage GetVenueDetail(Int32 VenueID, Int32 UserID, Int16 Pageindex, Int16 Pagesize)
        {
            try
            {
                ViewVenueDetail _ViewVenueDetail = new ViewVenueDetail();

                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);


                Models.Artists _Artist = null;
                Models.TourDate _TourDate = null;
                Models.Users _Users = null;
                Models.Venue _Venue = null;

                if (!String.IsNullOrEmpty(UserID.ToString()))
                {
                    _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);
                }
                else
                {
                    _Users = null;
                }

                //if (_Users == null)
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "VenueDetail"));
                //}


                _Venue = _VenueRepo.Repository.Get(p => p.VenueID == VenueID);

                if (_Venue == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.VenueNotFound, "VenueDetail"));
                }

                _ViewVenueDetail.VenueID = _Venue.VenueID;
                _ViewVenueDetail.VenueName = _Venue.VenueName ?? "";
                _ViewVenueDetail.ImageURL = _Venue.ImageURL ?? WebConfigurationManager.AppSettings["WebPath"].ToString() + "/Content/Upload/Default-Venue.jpg";
                _ViewVenueDetail.Extended_Address = _Venue.Extended_Address ?? "";
                _ViewVenueDetail.Display_Location = _Venue.Display_Location ?? "";
                _ViewVenueDetail.Slug = _Venue.Slug ?? "";
                _ViewVenueDetail.Postal_Code = _Venue.Postal_Code ?? "";
                _ViewVenueDetail.Address = _Venue.Address ?? "";
                _ViewVenueDetail.Timezone = _Venue.Timezone ?? "";
                _ViewVenueDetail.VenueCity = _Venue.VenueCity ?? "";
                _ViewVenueDetail.VenueState = _Venue.VenueState ?? "";
                _ViewVenueDetail.VenueCountry = _Venue.VenueCountry ?? "";
                _ViewVenueDetail.VenueLat = _Venue.VenueLat ?? 0;
                _ViewVenueDetail.VenueLong = _Venue.VenueLong ?? 0;

                //Upcoming Events
                _ViewVenueDetail.UpcomingEvents = (from A in _TourDateRepo.Repository.GetAll(p => p.VenueID == _Venue.VenueID)
                                                   join B in _ArtistsRepo.Repository.GetAll() on A.ArtistID equals B.ArtistID
                                                   where !A.IsDeleted && (A.Visible_Until_utc >= DateTime.UtcNow &&
                                                           A.Visible_Until_utc <= DateTime.UtcNow.AddDays(180))
                                                   orderby A.Visible_Until_utc
                                                   select new ViewVenueTours
                                                   {
                                                       TourDateID = A.TourDateID,
                                                       ArtistID = A.ArtistID,
                                                       ArtistName = B.ArtistName,
                                                       ImageURL = B.ImageURL ?? "",
                                                       BannerImage_URL = B.BannerImage_URL ?? "",
                                                       Date_Local = Convert.ToDateTime(A.Datetime_Local).ToString("d"),
                                                       Time_Local = Convert.ToDateTime(A.Datetime_Local).ToString("t")
                                                   }).ToPagedList(Pageindex - 1, Pagesize).ToList();


                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewVenueDetail, "VenueDetail"));

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "VenueDetail"));
            }
        }


        [HttpGet]
        [Route("api/v2/Venue/GetYourPlans")]
        public HttpResponseMessage GetYourPlans(Int32 UserID, Int16 Pageindex, Int16 Pagesize)
        {
            try
            {

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                ViewYourTicketPlans _ViewYourPlansDetail = new ViewYourTicketPlans();

                DataSet dsUser = new DataSet();
                dsUser = new SpRepository().GetUserByID(UserID);
                if (dsUser.Tables[0].Rows.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "YourPlans"));
                }
                int _userid = Convert.ToInt32(dsUser.Tables[0].Rows[0]["UserId"]);
                _ViewYourPlansDetail.UserID = Convert.ToInt32(dsUser.Tables[0].Rows[0]["UserId"]);
                _ViewYourPlansDetail.UserName = dsUser.Tables[0].Rows[0]["UserName"].ToString();
                _ViewYourPlansDetail.Email = dsUser.Tables[0].Rows[0]["Email"].ToString();
                if (String.IsNullOrEmpty(dsUser.Tables[0].Rows[0]["ThumbnailURL"].ToString()))
                {
                    _ViewYourPlansDetail.ThumbnailURL = ConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                }
                else
                {
                    _ViewYourPlansDetail.ThumbnailURL = dsUser.Tables[0].Rows[0]["ThumbnailURL"].ToString();
                }

                if (String.IsNullOrEmpty(dsUser.Tables[0].Rows[0]["ThumbnailURL"].ToString()))
                {
                    _ViewYourPlansDetail.ImageURL = ConfigurationManager.AppSettings["SiteImgURL"] + "/default-user.png";
                }
                else
                {
                    _ViewYourPlansDetail.ImageURL = dsUser.Tables[0].Rows[0]["ThumbnailURL"].ToString();
                }


                //Upcoming Events

                List<ViewYourTicketPlanlst> lstPlans = new List<ViewYourTicketPlanlst>();

                DataSet dsLive = new DataSet();
                dsLive = new SpRepository().SpTicketingLiveEventListByUserId(_userid);
                if (dsLive.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsLive.Tables[0].Rows.Count; i++)
                    {
                        DataRow dr = dsLive.Tables[0].Rows[i];
                        // ViewYourplanlst plan = new ViewYourplanlst();
                        ViewYourTicketPlanlst plan = new ViewYourTicketPlanlst();
                        plan.TourDateID = Convert.ToInt32(dr["TourDateID"].ToString());
                        plan.ArtistID = Convert.ToInt32(dr["ArtistID"].ToString());
                        plan.IsApproved = true;
                        plan.ArtistName = Convert.ToString(dr["ArtistName"].ToString()) ?? "";
                        plan.ImageURL = Convert.ToString(dr["ImageURL"].ToString()) ?? "";
                        plan.BannerImage_URL = Convert.ToString(dr["BannerImage_URL"].ToString()) ?? "";
                        plan.Datetime_Local = Convert.ToDateTime(dr["Datetime_Local"].ToString());
                        plan.Date_Local = Convert.ToDateTime(dr["Date_Local"].ToString()).ToString("d");
                        plan.Time_Local = Convert.ToDateTime(dr["Time_Local"].ToString()).ToString("t");
                        plan.VenueID = Convert.ToInt32(dr["VenueID"].ToString());
                        plan.VenueName = Convert.ToString(dr["VenueName"].ToString()) ?? "";
                        plan.Extended_Address = Convert.ToString(dr["Extended_Address"].ToString()) ?? "";
                        plan.Display_Location = Convert.ToString(dr["Display_Location"].ToString()) ?? "";
                        plan.Slug = Convert.ToString(dr["Slug"].ToString()) ?? "";
                        plan.Postal_Code = Convert.ToString(dr["Postal_Code"].ToString()) ?? "";
                        plan.Address = Convert.ToString(dr["Address"].ToString()) ?? "";
                        plan.Timezone = Convert.ToString(dr["Timezone"].ToString()) ?? "";
                        plan.VenueCity = Convert.ToString(dr["VenueCity"].ToString()) ?? "";
                        plan.VenueState = Convert.ToString(dr["VenueState"].ToString()) ?? "";
                        plan.VenueCountry = Convert.ToString(dr["VenueCountry"].ToString()) ?? "";
                        plan.VenueLat = Convert.ToDecimal(dr["VenueLat"].ToString());
                        plan.VenueLong = Convert.ToDecimal(dr["VenueLong"].ToString());

                        lstPlans.Add(plan);
                    }
                }

                // Get Ticketing App Events                
                DataSet ds = new DataSet();
                ds = new SpRepository().SpGetEventDetailsByUserId(_userid);

                // Get Ticketing App Events                
                DataSet dsGoing = new DataSet();
                dsGoing = new SpRepository().UserGoingTicketingEventByUserId(_userid);


                //ViewYourplanlst
                List<ViewYourTicketPlanlst> lstPlans2 = ds.Tables[0].AsEnumerable().Select(
                            dataRow => new ViewYourTicketPlanlst
                            {
                                TourDateID = dataRow.Field<int>("TourDateID"),
                                IsApproved = dataRow.Field<bool>("IsApproved"),
                                ArtistID = dataRow.Field<int>("ArtistID"),
                                ArtistName = dataRow.Field<string>("ArtistName"),
                                ImageURL = dataRow.Field<string>("ImageURL") == null ? "" : dataRow.Field<string>("ImageURL"),
                                BannerImage_URL = dataRow.Field<string>("BannerImage_URL") == null ? "" : dataRow.Field<string>("BannerImage_URL"),

                                Datetime_Local = dataRow.Field<DateTime>("Tour_UtcDate") == System.DateTime.MinValue ? DateTime.MinValue : dataRow.Field<DateTime>("Tour_UtcDate"),
                                Date_Local = (dataRow.Field<DateTime>("Tour_UtcDate") == System.DateTime.MinValue ? DateTime.MinValue : dataRow.Field<DateTime>("Tour_UtcDate")).ToShortDateString(),

                                VenueID = dataRow.Field<int>("VenueID"),
                                VenueName = dataRow.Field<string>("VenueName"),
                                Extended_Address = dataRow.Field<string>("Extended_Address"),
                                Display_Location = dataRow.Field<string>("Display_Location") == null ? "" : dataRow.Field<string>("Display_Location"),
                                Slug = dataRow.Field<string>("Slug") == null ? "" : dataRow.Field<string>("Slug"),
                                Postal_Code = dataRow.Field<string>("Postal_Code") == null ? "" : dataRow.Field<string>("Postal_Code"),
                                Address = dataRow.Field<string>("Address") == null ? "" : dataRow.Field<string>("Address"),
                                Timezone = dataRow.Field<string>("Timezone") == null ? "" : dataRow.Field<string>("Timezone"),
                                VenueCity = dataRow.Field<string>("VenueCity") == null ? "" : dataRow.Field<string>("VenueCity"),
                                VenueState = dataRow.Field<string>("VenueState") == null ? "" : dataRow.Field<string>("VenueState"),
                                VenueCountry = dataRow.Field<string>("VenueCountry") == null ? "" : dataRow.Field<string>("VenueCountry"),
                                VenueLat = dataRow.Field<decimal>("VenueLat"),
                                VenueLong = dataRow.Field<decimal>("VenueLong"),
                                tickets = GetTickets(Convert.ToInt32(dataRow.Field<int>("TourDateID")), _userid),
                                //QRCodeImage = dataRow.Field<string>("QRCodeImage") == null ? "23.111.138.246/Content/QRCodeImages/c59b60ad-ada8-4e5b-98dc-81b5ff25e0b2.jpg" : dataRow.Field<string>("QRCodeImage"),
                                ////TicketId = dataRow.Field<Guid>("TicketId")
                                //TicketId = dataRow.Field<string>("TicketSerialNumber")
                            }).ToList();
                //}).Where(p => p.QRCodeImage != null).Where(p => p.TicketId != null).ToList();


                //ViewYourplanlst
                List<ViewYourTicketPlanlst> lstPlanGoing = dsGoing.Tables[0].AsEnumerable().Select(
                            dataRow => new ViewYourTicketPlanlst
                            {
                                TourDateID = dataRow.Field<int>("TourDateID"),
                                ArtistID = dataRow.Field<int>("ArtistID"),
                                ArtistName = dataRow.Field<string>("ArtistName"),
                                ImageURL = dataRow.Field<string>("ImageURL") == null ? "" : dataRow.Field<string>("ImageURL"),
                                BannerImage_URL = dataRow.Field<string>("BannerImage_URL") == null ? "" : dataRow.Field<string>("BannerImage_URL"),

                                Datetime_Local = dataRow.Field<DateTime>("Tour_UtcDate") == System.DateTime.MinValue ? DateTime.MinValue : dataRow.Field<DateTime>("Tour_UtcDate"),
                                Date_Local = (dataRow.Field<DateTime>("Tour_UtcDate") == System.DateTime.MinValue ? DateTime.MinValue : dataRow.Field<DateTime>("Tour_UtcDate")).ToShortDateString(),

                                VenueID = dataRow.Field<int>("VenueID"),
                                VenueName = dataRow.Field<string>("VenueName"),
                                Extended_Address = dataRow.Field<string>("Extended_Address"),
                                Display_Location = dataRow.Field<string>("Display_Location") == null ? "" : dataRow.Field<string>("Display_Location"),
                                Slug = dataRow.Field<string>("Slug") == null ? "" : dataRow.Field<string>("Slug"),
                                Postal_Code = dataRow.Field<string>("Postal_Code") == null ? "" : dataRow.Field<string>("Postal_Code"),
                                Address = dataRow.Field<string>("Address") == null ? "" : dataRow.Field<string>("Address"),
                                Timezone = dataRow.Field<string>("Timezone") == null ? "" : dataRow.Field<string>("Timezone"),
                                VenueCity = dataRow.Field<string>("VenueCity") == null ? "" : dataRow.Field<string>("VenueCity"),
                                VenueState = dataRow.Field<string>("VenueState") == null ? "" : dataRow.Field<string>("VenueState"),
                                VenueCountry = dataRow.Field<string>("VenueCountry") == null ? "" : dataRow.Field<string>("VenueCountry"),
                                VenueLat = dataRow.Field<decimal>("VenueLat"),
                                VenueLong = dataRow.Field<decimal>("VenueLong"),
                                tickets = new List<Tickets>(),
                            }).ToList();


                List<ViewYourTicketPlanlst> lstPlanstmp = new List<ViewYourTicketPlanlst>();
                lstPlanstmp.AddRange(lstPlans);
                lstPlanstmp.AddRange(lstPlans2);
                //lstPlanstmp.AddRange(lstPlans2.Where(x => !lstPlanGoing.Contains(x)).ToList());
                lstPlanstmp.AddRange(lstPlanGoing);
                lstPlanstmp = lstPlanstmp.Where(p => p.Datetime_Local >= DateTime.Now).Distinct().ToList();

                _ViewYourPlansDetail.Plans = lstPlanstmp.OrderBy(p => Convert.ToDateTime(p.Datetime_Local)).ToList();
                // _ViewYourPlansDetail.GoingPlan= lstPlanGoing.OrderBy(p => Convert.ToDateTime(p.Datetime_Local)).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewYourPlansDetail, "YourPlans"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "YourPlans"));
            }
        }
        private List<Musika.Models.API.View.Tickets> GetTickets(int tourDateID, int userid)
        {
            DataSet ds = new SpRepository().GetTicketsDetaislByTourDateID(tourDateID, userid);
            List<Musika.Models.API.View.Tickets> lstTickets = new List<Musika.Models.API.View.Tickets>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                Musika.Models.API.View.Tickets ticket = new Musika.Models.API.View.Tickets();
                ticket.TicketId = Convert.ToString(ds.Tables[0].Rows[i][0].ToString());
                ticket.QRCodeImage = Convert.ToString(ds.Tables[0].Rows[i][1].ToString());
                lstTickets.Add(ticket);
            }
            return lstTickets;
        }

        [HttpGet]
        [Route("api/v2/Venue/GetYourFriendPlans")]
        public HttpResponseMessage GetYourFriendPlans(Int32 UserID, Int16 Pageindex, Int16 Pagesize)
        {
            try
            {
                List<ViewYourFreiendplanlst> _ViewYourFriendPlans = new List<ViewYourFreiendplanlst>();
                List<ViewFriendPlans> _ViewFriendPlans = new List<ViewFriendPlans>();

                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                GenericRepository<UserFriends> _UserFriendsRepo = new GenericRepository<UserFriends>(_unitOfWork);

                //Models.Artists _Artist = null;
                //Models.TourDate _TourDate = null;
                //Models.Venue _Venue = null;

                Models.Users _Users = null;

                //_Users = _UsersRepo.Repository.GetById(UserID);
                DataSet dsUser = new DataSet();
                dsUser = new SpRepository().GetUserByID(UserID);
                if (dsUser.Tables[0].Rows.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "FriendPlans"));
                }
                //int _userid = Convert.ToInt32(dsUser.Tables[0].Rows[0]["UserId"]);

                //if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "FriendPlans"));
                //}
                DataSet dsgoingCout = new SpRepository().SpGetUserGoingCount(UserID);
                for (int i = 0; i < dsgoingCout.Tables[0].Rows.Count; i++)
                {
                    DataSet dsFriendPlan = new SpRepository().SpViewYourFriendsPlan(Convert.ToInt32(dsgoingCout.Tables[0].Rows[i]["TourID"].ToString()));
                    if (dsFriendPlan.Tables[0].Rows.Count > 0)
                    {
                        for (int j = 0; j < dsFriendPlan.Tables[0].Rows.Count; j++)
                        {
                            ViewYourFreiendplanlst temp = new ViewYourFreiendplanlst();

                            DataRow dr = dsFriendPlan.Tables[0].Rows[j];
                            temp.TourDateID = Convert.ToInt32(dr["TourDateID"].ToString());
                            temp.IsApproved = true;
                            temp.ArtistID = Convert.ToInt32(dr["ArtistID"].ToString());
                            temp.ArtistName = dr["ArtistName"].ToString();
                            temp.ImageURL = dr["ImageURL"].ToString() ?? "";
                            temp.BannerImage_URL = dr["BannerImage_URL"].ToString() ?? "";
                            temp.VenueID = Convert.ToInt32(dr["VenueID"]);
                            temp.VenueName = dr["VenueName"].ToString();
                            temp.Going = new List<ViewFriendPlans>();
                            //temp.GoingCount = dr["GiongCount"].ToString() > 3 ? A.GiongCount - 3 : 0,
                            if (!String.IsNullOrEmpty(dr["Datetime_Local"].ToString()))
                            {
                                temp.Date_Local = DateTime.Parse(dr["Datetime_Local"].ToString()).ToString("d");
                            }
                            DataSet dsFriendPlanGoing = new SpRepository().SpViewYourFriendsGoingPlan(UserID, temp.TourDateID);
                            if (dsFriendPlanGoing.Tables[0].Rows.Count > 0)
                            {
                                for (int k = 0; k < dsFriendPlanGoing.Tables[0].Rows.Count; k++)
                                {
                                    ViewFriendPlans ViewFriendPlans = new ViewFriendPlans();
                                    DataRow drgoing = dsFriendPlanGoing.Tables[0].Rows[k];
                                    ViewFriendPlans.Email = drgoing["Email"].ToString() ?? "";
                                    ViewFriendPlans.ImageURL = drgoing["ImageURL"].ToString() ?? "";
                                    ViewFriendPlans.ThumbnailURL = drgoing["ThumbnailURL"].ToString() ?? "";
                                    ViewFriendPlans.UserID = Convert.ToInt64(drgoing["UserID"].ToString());
                                    ViewFriendPlans.UserName = drgoing["UserName"].ToString();
                                    temp.Going.Add(ViewFriendPlans);
                                }
                            }
                            _ViewYourFriendPlans.Add(temp);
                        }

                    }

                }
                DataSet dsgoingCoutnew = new SpRepository().SpViewYourFriendsPlanNew(UserID);
                for (int i = 0; i < dsgoingCoutnew.Tables[0].Rows.Count; i++)
                {

                    ViewYourFreiendplanlst temp = new ViewYourFreiendplanlst();

                    DataRow dr = dsgoingCoutnew.Tables[0].Rows[i];
                    if (!string.IsNullOrEmpty(dr["TourDateID"].ToString()))
                    {
                        temp.TourDateID = Convert.ToInt32(dr["TourDateID"].ToString());
                        temp.IsApproved = Convert.ToBoolean(dr["IsApproved"]);
                        temp.ArtistID = Convert.ToInt32(dr["ArtistID"].ToString());
                        temp.ArtistName = dr["ArtistName"].ToString();
                        temp.ImageURL = dr["ImageURL"].ToString() ?? "";
                        temp.BannerImage_URL = dr["BannerImage_URL"].ToString() ?? "";
                        temp.VenueID = Convert.ToInt32(dr["VenueID"]);
                        temp.VenueName = dr["VenueName"].ToString();
                        temp.Date_Local = DateTime.Parse(dr["StartDate"].ToString()).ToString("d");
                        temp.Going = new List<ViewFriendPlans>();
                        //_ViewYourFriendPlans.Add(temp);
                        DataSet dsFriendPlannew = new SpRepository().SpViewYourFriendsGoingPlanNew(UserID, Convert.ToInt32(dsgoingCoutnew.Tables[0].Rows[i]["TourDateID"].ToString()));
                        if (dsFriendPlannew.Tables[0].Rows.Count > 0)
                        {
                            for (int k = 0; k < dsFriendPlannew.Tables[0].Rows.Count; k++)
                            {
                                ViewFriendPlans ViewFriendPlans = new ViewFriendPlans();
                                DataRow drgoing = dsFriendPlannew.Tables[0].Rows[k];
                                ViewFriendPlans.Email = drgoing["Email"].ToString() ?? "";
                                ViewFriendPlans.ImageURL = drgoing["ImageURL"].ToString() ?? "";
                                ViewFriendPlans.ThumbnailURL = drgoing["ThumbnailURL"].ToString() ?? "";
                                ViewFriendPlans.UserID = Convert.ToInt64(drgoing["UserID"].ToString());
                                ViewFriendPlans.UserName = drgoing["UserName"].ToString();

                                temp.Going.Add(ViewFriendPlans);
                            }
                        }
                        int index = _ViewYourFriendPlans.FindIndex(item => item.TourDateID == temp.TourDateID);
                        if (index < 0)
                        {
                            _ViewYourFriendPlans.Add(temp);
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewYourFriendPlans, "FriendPlans"));

            }

            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "FriendPlans"));
            }
        }


        [HttpGet]
        [Route("api/v2/Venue/GetDiscoverDetail")]
        public HttpResponseMessage GetDiscoverDetail(Int32 UserID, double Lat, double Lon)
        {
            try
            {
                ViewDiscoverDetail _ViewDiscoverDetail = new ViewDiscoverDetail();
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                Models.Users _Users = null;

                //if (UserID > 0)
                //{
                //    _Users = _UsersRepo.Repository.GetById(UserID);
                //}
                //else
                //{
                //    _Users = null;
                //}

                //if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "DiscoverDetail"));
                //}

                DataSet ds;
                SpRepository _sp = new SpRepository();
                if (UserID > 0)
                {
                    ds = _sp.SpGetDiscoverDetail(UserID, Lat, Lon, 12);
                }
                else
                {
                    ds = _sp.SpGetDiscoverDetailWithOutUserId(Lat, Lon, 12);
                }

                if (ds != null)
                {

                    //Most Popular
                    if (ds.Tables.Count >= 1)
                    {
                        if (ds.Tables[0].Rows.Count != 0)
                        {
                            _ViewDiscoverDetail.MostPopular = General.DTtoList<ViewDiscoverlst>(ds.Tables[0]);
                        }
                        else
                        {
                            _ViewDiscoverDetail.MostPopular = new List<ViewDiscoverlst>();
                        }
                    }

                    //Recommended
                    if (ds.Tables.Count >= 2)
                    {
                        if (ds.Tables[1].Rows.Count != 0)
                        {
                            _ViewDiscoverDetail.Recommended = General.DTtoList<ViewDiscoverlst>(ds.Tables[1]);
                        }
                        else
                        {
                            _ViewDiscoverDetail.Recommended = new List<ViewDiscoverlst>();
                        }
                    }
                    else
                    {
                        _ViewDiscoverDetail.Recommended = new List<ViewDiscoverlst>();
                    }

                    //Hot Events
                    if (ds.Tables.Count >= 3)
                    {
                        if (ds.Tables[2].Rows.Count != 0)
                        {
                            _ViewDiscoverDetail.HotNewTour = General.DTtoList<ViewDiscoverlst>(ds.Tables[2]);
                        }
                        else
                        {
                            _ViewDiscoverDetail.HotNewTour = new List<ViewDiscoverlst>();
                        }
                    }
                    else
                    {
                        _ViewDiscoverDetail.HotNewTour = new List<ViewDiscoverlst>();
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewDiscoverDetail, "DiscoverDetail"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "DiscoverDetail"));
            }
        }

        #endregion


        #region "Third party API"

        [HttpGet]
        [Route("api/v2/thirdpartyapi/v2/GetSpotifyAccessToken")]
        public HttpResponseMessage GetSpotifyAccessToken()
        {
            try
            {
                ClientCredentialsAuth _ClientCredentialsAuth = new ClientCredentialsAuth();
                _ClientCredentialsAuth.ClientId = WebConfigurationManager.AppSettings["Spotify_client_id"].ToString();
                _ClientCredentialsAuth.ClientSecret = WebConfigurationManager.AppSettings["Spotify_client_secret"].ToString();
                _ClientCredentialsAuth.Scope = Scope.Streaming;

                //_ClientCredentialsAuth.Scope = Scope.UserLibrarayRead;
                Token _Token = _ClientCredentialsAuth.DoAuth();
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _Token, "Spotify"));

            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.Message));
            }
        }

        public string RetSpotifyAccessToken()
        {
            try
            {
                ClientCredentialsAuth _ClientCredentialsAuth = new ClientCredentialsAuth();
                _ClientCredentialsAuth.ClientId = WebConfigurationManager.AppSettings["Spotify_client_id"].ToString();
                _ClientCredentialsAuth.ClientSecret = WebConfigurationManager.AppSettings["Spotify_client_secret"].ToString();
                _ClientCredentialsAuth.Scope = Scope.Streaming;

                //_ClientCredentialsAuth.Scope = Scope.UserLibrarayRead;
                Token _Token = _ClientCredentialsAuth.DoAuth();

                return _Token.AccessToken;
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return "";
            }
        }

        #region "Music Graph"

        private Task<MusicGraph.Search_ByName> MusicGrapgh_GetArtistByName_Asyn(string vMusicgraph_Name)
        {
            string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
            HttpWebRequest httpWebRequest;
            HttpWebResponse httpResponse;
            string _result;

            //LogHelper.CreateLog("MusicGrapgh_GetArtistByName_Asyn (" + vMusicgraph_Name + ")");

            return Task.Factory.StartNew(() =>
            {
                try
                {
                    httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/v2/artist/search?api_key=" + _MusicGrapgh_api_key + "&name=" + vMusicgraph_Name.Trim());
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }

                    // deserializing 
                    return JsonConvert.DeserializeObject<MusicGraph.Search_ByName>(_result);
                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    LogHelper.CreateLog("MusicGrapgh_GetArtistByName_Asyn " + ex.Message);
                    return null;
                }
            }
         );
        }

        private Task<MusicGraph1.Search_ByID> MusicGrapgh_GetArtistByID_Asyn(string vMusicgraph_ID)
        {

            string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();

            //LogHelper.CreateLog("MusicGrapgh_GetArtistByID_Asyn (" + vMusicgraph_ID + ")");

            return Task.Factory.StartNew(() =>
            {
                try
                {

                    HttpWebRequest httpWebRequest;
                    HttpWebResponse httpResponse;
                    string _result;
                    MusicGraph1.Search_ByID _Search_ByID = null;

                    httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/v2/artist/" + vMusicgraph_ID + "?api_key=" + _MusicGrapgh_api_key);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }

                    // deserializing 
                    _Search_ByID = JsonConvert.DeserializeObject<MusicGraph1.Search_ByID>(_result);

                    return _Search_ByID;

                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    LogHelper.CreateLog("MusicGrapgh_GetArtistByID_Asyn " + ex.Message);
                    return null;
                }
            });
        }

        private async Task<Dictionary<string, object>> MusicGrapgh_GetSimilarArtists_Asyn(string vMusicgraph_ID, Int32 vArtistID, IUnitOfWork vUnitOfWork)
        {

            string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
            GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(vUnitOfWork);
            GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(vUnitOfWork);

            Dictionary<string, object> list = new Dictionary<string, object>();

            MusicGraph5.GetSimilarArtists_ByID _GetSimilarArtists_ByID = await Task.Factory.StartNew(() =>
            {
                try
                {
                    HttpWebRequest httpWebRequest;
                    HttpWebResponse httpResponse = null;
                    string _result;

                    httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/v2/artist/" + vMusicgraph_ID + "/similar?api_key=" + _MusicGrapgh_api_key);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    for (int i = 0; i <= 1; i++)
                    {
                        try
                        {
                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            if (httpResponse != null)
                                break;
                        }
                        catch (Exception e)
                        {
                            if (i == 1)
                            {
                                LogHelper.CreateLog("MusicGrapgh_GetSimilarArtists_Asyn " + e.Message);
                                return null;
                            }

                            Thread.Sleep(2000);
                        }
                    }

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }


                    // deserializing 
                    return JsonConvert.DeserializeObject<MusicGraph5.GetSimilarArtists_ByID>(_result);

                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    LogHelper.CreateLog("MusicGrapgh_GetSimilarArtists_Asyn " + ex.Message);
                    return null;
                }
            });

            list.Add("MusicGraphArtist", _GetSimilarArtists_ByID);

            return list;
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

                httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/v2/artist/" + vMusicgraph_ID + "/biography?api_key=" + _MusicGrapgh_api_key);
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
                    }

                    if (data == null || String.IsNullOrEmpty(bioNoTags))
                    {
                        data = job.SelectToken("data.artist_bio_nlp");
                        if (data != null)
                        {
                            bioWithTags = data.Value<string>();
                            if (!String.IsNullOrEmpty(bioWithTags))
                            {
                                string input = bioWithTags;
                                string regex = "\\[\\S*_?\\S*\\]\\b|\\[\\S*_?\\S*\\]";//"(\\[.*\\])";
                                bioWithTags = Regex.Replace(input, regex, "");
                                return bioWithTags;
                            }
                        }

                    }
                    else
                        return bioNoTags;

                }
            }
            catch (Exception)
            { }

            return "";
        }

        private Task<MusicGraph2.ArtistMatrics_ByID> MusicGrapgh_GetArtistMatrics_Asyn(string vMusicgraph_ID, Int32 vArtistID, IUnitOfWork vUnitOfWork)
        {

            string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();

            return Task.Factory.StartNew(() =>
            {
                try
                {
                    HttpWebRequest httpWebRequest;
                    HttpWebResponse httpResponse;
                    string _result;

                    httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/v2/artist/" + vMusicgraph_ID + "/metrics?api_key=" + _MusicGrapgh_api_key);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }

                    // deserializing 
                    return JsonConvert.DeserializeObject<MusicGraph2.ArtistMatrics_ByID>(_result);
                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog3(ex, Request);
                    LogHelper.CreateLog("MusicGrapgh_GetArtistMatrics_Asyn " + ex.Message);
                    return null;
                }
            });

        }

        #endregion

        #region "Instagram"

        private void GetImagesFromInstagramFeed_Depricated(Artists artist)
        {
            GenericRepository<ArtistPhotos> _ArtistPhotosRepository = new GenericRepository<ArtistPhotos>(_unitOfWork);
            int photoLikeRequirement = 0;
            int totalPhotosAllowed = 30;

            try
            {
                //List<ArtistPhotos> artistTourPhotos = new List<ArtistPhotos>();
                var currentDate = DateTime.Now;

                var artistTourPhotos = _ArtistPhotosRepository.Repository.GetAll(x => x.ArtistID == artist.ArtistID && x.RecordStatus == RecordStatus.Active.ToString()).ToList();
                //var artistTourPhotos = _ArtistPhotosRepository.Repository.AsQueryable().Where(p=>p.ArtistID == artist.ArtistID).ToList();
                var oldRecord = DateTime.Now.AddDays(-7);



                foreach (var tourphoto in artistTourPhotos)
                {
                    if (tourphoto.ModifiedDate < oldRecord)
                    {
                        tourphoto.RecordStatus = RecordStatus.InActive.ToString();
                        tourphoto.ModifiedDate = currentDate;
                        //_ArtistPhotosRepository.Repository.Update(tourphoto);
                    }
                }

                int count = artistTourPhotos.Count(x => x.RecordStatus == RecordStatus.Active.ToString());

                if (count < totalPhotosAllowed)
                {

                    try
                    {
                        string name = artist.ArtistName;


                        name = name.Replace(" ", "");
                        string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ\s]";
                        string cleanName = Regex.Replace(name, pattern, "");
                        //x.ArtistName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                        //_ArtistsRepo.Repository.AsQueryable().Where(x => x.ArtistName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).Select


                        HttpWebRequest httpWebRequest;
                        HttpWebResponse httpResponse;
                        string _result;

                        if (String.IsNullOrEmpty(artist.Instagram_Url))
                        {
                            httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.instagram.com/" + cleanName + "/media/");
                            photoLikeRequirement = ConfigurationManager.AppSettings["MinLikesInstagramCalc"].GetInt();
                        }
                        else
                        {
                            httpWebRequest = (HttpWebRequest)WebRequest.Create(artist.Instagram_Url + "/media/");
                            photoLikeRequirement = ConfigurationManager.AppSettings["MinLikesInstagramUrl"].GetInt();
                        }


                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "GET";

                        httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            _result = streamReader.ReadToEnd();
                        }

                        JObject joResponse = JObject.Parse(_result);
                        JArray items = (JArray)joResponse["items"];

                        count = totalPhotosAllowed - count;

                        foreach (var i in items)
                        {
                            int imagelikes = i["likes"].Value<int>("count");
                            string imageId = i.Value<string>("id");
                            if (imagelikes > photoLikeRequirement)
                            {
                                var artistRecord = artistTourPhotos.FirstOrDefault(x => x.InstagramPostId == imageId);

                                if (artistRecord == null)
                                {
                                    ArtistPhotos tp = new ArtistPhotos();
                                    tp.ImageThumbnailUrl = i["images"]["thumbnail"].Value<string>("url");
                                    tp.ImageUrl = i["images"]["standard_resolution"].Value<string>("url");

                                    try
                                    {
                                        //tp.ImageThumbnailUrl = tp.ImageThumbnailUrl.Remove(tp.ImageThumbnailUrl.LastIndexOf('?'));
                                        tp.ImageThumbnailUrl = tp.ImageThumbnailUrl.Replace("?", "");
                                    }
                                    catch
                                    { }

                                    try
                                    {
                                        // tp.ImageUrl = tp.ImageUrl.Remove(tp.ImageUrl.LastIndexOf('?'));
                                        tp.ImageUrl = tp.ImageUrl.Replace("?", "");
                                    }
                                    catch { }

                                    tp.InstagramPostId = i.Value<string>("id");
                                    tp.ArtistID = artist.ArtistID;
                                    tp.HashTagName = cleanName;
                                    tp.CreatedDate = DateTime.Now;
                                    tp.ModifiedDate = DateTime.Now;
                                    tp.RecordStatus = RecordStatus.Active.ToString();
                                    _ArtistPhotosRepository.Repository.Add(tp);

                                    count--;
                                }
                                else
                                {
                                    if (artistRecord.RecordStatus == RecordStatus.InActive.ToString())
                                    {
                                        artistRecord.RecordStatus = RecordStatus.Active.ToString();
                                        artistRecord.ModifiedDate = DateTime.Now;
                                        _ArtistPhotosRepository.Repository.Update(artistRecord);

                                        count--;
                                    }
                                }

                                if (count == 0)
                                    break;
                            }
                        }
                    }
                    catch (Exception)
                    { }

                    foreach (var a in artistTourPhotos)
                    {
                        //    if (a.RecordStatus == RecordStatus.InActive.ToString() && a.ModifiedDate == currentDate)
                        //    {
                        //        _ArtistPhotosRepository.Repository.Update(a);
                        //    }
                        //    else 
                        if (a.RecordStatus == RecordStatus.InActive.ToString())
                        {
                            _ArtistPhotosRepository.Repository.DeletePermanent(a.PhotoID);
                        }
                    }


                    //JObject ojObject = (JObject)joResponse["items"];
                    //JArray imgs = (JArray)items["images"];
                    //JArray likes = (JArray)items["likes"];


                    // deserializing 
                    //return JsonConvert.DeserializeObject<MusicGraph5.GetSimilarArtists_ByID>(_result);
                }
            }
            catch (Exception ex)
            {
                //  LogHelper.CreateLog("MusicGrapgh_GetSimilarArtists_Asyn " + ex.Message);
                //  return null;
            }
        }

        private async Task<Dictionary<string, object>> GetImagesFromInstagramFeed(Artists artist)
        {
            GenericRepository<ArtistPhotos> _ArtistPhotosRepository = new GenericRepository<ArtistPhotos>(_unitOfWork);
            int photoLikeRequirement = 0;
            int totalPhotosAllowed = 30;
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                //List<ArtistPhotos> artistTourPhotos = new List<ArtistPhotos>();
                var currentDate = DateTime.Now;

                var artistTourPhotos = _ArtistPhotosRepository.Repository.GetAll(x => x.ArtistID == artist.ArtistID && x.RecordStatus == RecordStatus.Active.ToString()).ToList();
                //var artistTourPhotos = _ArtistPhotosRepository.Repository.AsQueryable().Where(p=>p.ArtistID == artist.ArtistID).ToList();
                var oldRecord = DateTime.Now.AddDays(-7);

                //foreach (var tourphoto in artistTourPhotos)
                //{
                //    if (tourphoto.ModifiedDate < oldRecord)
                //    {
                //        tourphoto.RecordStatus = RecordStatus.InActive.ToString();
                //        tourphoto.ModifiedDate = currentDate;
                //        //_ArtistPhotosRepository.Repository.Update(tourphoto);
                //    }
                //}

                int count = artistTourPhotos.Count(x => x.RecordStatus == RecordStatus.Active.ToString());

                if (count < totalPhotosAllowed)
                {
                    result = await Task.Factory.StartNew(() =>
                    {
                        #region "Get Data From Api"
                        string apiResult = string.Empty;
                        try
                        {
                            string name = artist.ArtistName;

                            name = name.Replace(" ", "");
                            string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ\s]";
                            string cleanName = Regex.Replace(name, pattern, "");
                            //x.ArtistName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                            //_ArtistsRepo.Repository.AsQueryable().Where(x => x.ArtistName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).Select

                            HttpWebRequest httpWebRequest;
                            HttpWebResponse httpResponse;
                            if (String.IsNullOrEmpty(artist.Instagram_Url))
                            {
                                //httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.instagram.com/" + cleanName + "/media/");
                                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.instagram.com//explore/tags/" + cleanName + "/?__a=1");
                                photoLikeRequirement = ConfigurationManager.AppSettings["MinLikesInstagramCalc"].GetInt();
                            }
                            else
                            {
                                //httpWebRequest = (HttpWebRequest)WebRequest.Create(artist.Instagram_Url + "/media/");
                                httpWebRequest = (HttpWebRequest)WebRequest.Create(artist.Instagram_Url.Replace("instagram.com", "instagram.com/explore/tags") + "/?__a=1");
                                photoLikeRequirement = ConfigurationManager.AppSettings["MinLikesInstagramUrl"].GetInt();
                            }

                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "GET";

                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                apiResult = streamReader.ReadToEnd();
                            }

                            count = totalPhotosAllowed - count;
                            result.Add("Count", count);
                            result.Add("ApiResponse", apiResult);
                        }
                        catch (Exception ex)
                        { }
                        return result;
                        #endregion
                    });
                }
            }
            catch (Exception ex)
            {
                //  LogHelper.CreateLog("MusicGrapgh_GetSimilarArtists_Asyn " + ex.Message);
                //  return null;
            }
            return result;
        }

        private void GetInstagramHashTagPhotos()
        {
            /*var httpWebRequest = httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.instagram.com/v1/tags/musikaFake/media/recent?access_token=" + vArtists.Instagram_ID + "/?access_token=" + _Instagram_access_token);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string _result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                _result = streamReader.ReadToEnd();
            }
            */
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

        #region "SeatGeek"

        private string SGAuth()
        {
            return "?client_id=" + WebConfigurationManager.AppSettings["SeatGeek_client_id"] + "&client_secret=" + WebConfigurationManager.AppSettings["SeatGeek_client_secret"];
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
            });
        }


        private async Task<bool> SeatGeek_CheckLatinGenre_Asyn(string artistName, IUnitOfWork vUnitOfWork)
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


            bool isLatin = false;

            SeatGeek.Get_Performers _Get_PerformersRet = await Task.Factory.StartNew(() =>
            {
                try
                {
                    #region "Get SeatGeek Performers (Dont use this while updating the records)"

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
                    // deserializing 
                    return _Get_Performers;
                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog("SeatGeek_CheckLatinGenre_Asyn " + ex.Message);
                    return null;
                }
            }
            #endregion

             );
            return isLatin;
        }


        private async Task<Dictionary<string, object>> SeatGeek_GetEventByArtistName_Asyn(Int32 vUserID, Artists vArtists, IUnitOfWork vUnitOfWork, bool vNew, bool vScan = false)
        {
            string strThumbnailURLfordb = null;
            string strIamgeURLfordb = null;
            string strTempImageSave = null;


            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
            dynamic obj = null;

            string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();

            string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

            string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
            string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];

            HttpWebRequest httpWebRequest;
            HttpWebResponse httpResponse;
            string _result;
            SeatGeek.Get_Performers _Get_Performers = null;

            string _Performer_ID = null;
            string _strEvent = null;

            Dictionary<string, object> response = new Dictionary<string, object>();

            #region "Task 1 to get SeatGeek Detail using Artist Name"
            if (vArtists.Seatgeek_ID == null)
            {
                SeatGeek.Get_Performers _Get_PerformersRet = await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        #region "Get SeatGeek Performers (Dont use this while updating the records)"

                        string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ\s]";
                        string cleanName = Regex.Replace(vArtists.ArtistName, pattern, "");

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
                        #endregion


                        if (_Get_Performers != null)
                        {
                            if (_Get_Performers.performers.Count > 0)
                            {

                                SeatGeek.Performer _Performer = new SeatGeek.Performer();

                                _Performer = (from A in _Get_Performers.performers
                                              where DiceCoefficientExtensions.DiceCoefficient(A.name.ToLower(), vArtists.ArtistName.Trim()) >= _FuzzySearchCri && CheckSeatGeekLatin(A.name, _unitOfWork)
                                              select A).OrderByDescending(p => p.score).FirstOrDefault();


                                if (_Performer == null)
                                {
                                    _Performer = _Get_Performers.performers[0];
                                }

                                if (_Performer != null)
                                {
                                    vArtists.OnTour = _Performer.has_upcoming_events == true ? true : false;
                                    _Performer_ID = _Performer.id.ToString();
                                    vArtists.Seatgeek_ID = _Performer_ID;

                                    if (_Performer.taxonomies != null && _Performer.taxonomies.Count > 0)
                                    {
                                        foreach (SeatGeek.Taxonomy _Taxonomy in _Performer.taxonomies)
                                        {
                                            _strEvent = _strEvent + _Taxonomy.name + " ,";
                                        }
                                    }

                                    if (_strEvent.ToString() == "") _strEvent = vArtists.ArtistName + " Event";

                                }
                            }
                        }
                        // deserializing 
                        return _Get_Performers;

                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("SeatGeek_GetEventByArtistName_Asyn (Task 1) " + ex.Message);
                        return null;
                    }
                });
                response.Add("SeatGeekDetail", _Get_PerformersRet);
            }
            #endregion

            #region  "Task2 if above task completed successfully"

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    #region "Get SeatGeek Performer Tours/Events"

                    //_Events.SeatGeek_ID
                    string seatid = vArtists.Seatgeek_ID;
                    if (String.IsNullOrEmpty(vArtists.Seatgeek_ID))
                        seatid = "0";

                    httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.seatgeek.com/2/events" + SGAuth() + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&performers.id=" + seatid + "&page=1&per_page=100");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }

                    // deserializing 
                    //var _Get_Performers_Events = JsonConvert.DeserializeObject<SeatGeek2.Get_Performers_Events>(_result);

                    dynamic _Get_Performers_Events = serializer.Deserialize(_result, typeof(object));

                    response.Add("SeatGeekPerformer", _Get_Performers_Events);

                    if (vNew == false)
                    {
                        if (String.IsNullOrEmpty(vArtists.ImageURL))
                            GetProfileImageFromSpotifyFeed(vArtists);
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    LogHelper.CreateLog("SeatGeek_GetEventByArtistName_Asyn (Task 2 - Update) " + ex.Message);
                }
            });
            #endregion

            response.Add("vNew", vNew);

            return response;
        }

        #endregion

        #region "Eventful"

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

                    // var _Search_Artist = JsonConvert.DeserializeObject<EventFull.Search_Performer>(_result);

                    if (_Search_Artist != null)
                    {
                        if (_Search_Artist.performers != null && String.IsNullOrEmpty(vArtists.About))
                        {
                            if (Convert.ToInt16(_Search_Artist.total_items) > 1)
                            {
                                vArtists.About = _Search_Artist.performers.performer[0].short_bio;
                                // _Events.EventFul_ID = _Search_Artist.performers.performer[0].id;
                                vArtists.Eventful_ID = _Search_Artist.performers.performer[0].id;
                            }
                            else
                            {
                                vArtists.About = _Search_Artist.performers.performer.short_bio;
                                // _Events.EventFul_ID = _Search_Artist.performers.performer.id;
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
                        //var _Get_Performer_Events = JsonConvert.DeserializeObject<EventFull2.Get_Performer_Events>(_result);
                        dynamic Get_Performer_Events = serializer.Deserialize(_result, typeof(object));
                        return Get_Performer_Events;

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        LogHelper.CreateLog("EventFul_GetEventInfo_Asyn (Task 2) " + ex.Message + " API : " +
                           "http://api.eventful.com/json/performers/get?app_key=" + _Eventful_app_key + "&id=" + vArtists.Eventful_ID + "&show_events=true&image_sizes=large");
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
                                    //var _Get_Event_ByID = JsonConvert.DeserializeObject<EventFull3.Get_Event_ByID>(_result);
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


                //if (_Get_Performer_Events != null)
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}

            }
            #endregion

            response.Add("Artist", vArtists);

            return response;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="spotifyID"></param>
        /// <returns></returns>


        #endregion

        #region "Spotify"

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

        //New API's for spotify
        private async Task<List<MusicGraph.Datum>> Spotify_SearchArtist(string _q)
        {
            try
            {
                List<MusicGraph.Datum> _artistlst = new List<MusicGraph.Datum>();

                string _Spotify_Country = ConfigurationManager.AppSettings["Spotify_Country"].ToString();

                //Task.Factory.StartNew(() =>
                {
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



                //);



            }
            catch (Exception ex)
            {
                LogHelper.CreateLog(ex);
                return null;
            }

        }


        private async Task<MusicGraph.Datum> Spotify_GetArtistByID(string ID)
        {
            try
            {
                MusicGraph.Datum _artistlst = new MusicGraph.Datum();

                string _Spotify_Country = ConfigurationManager.AppSettings["Spotify_Country"].ToString();

                //Task.Factory.StartNew(() =>
                {
                    Artist _artist = await Artist.GetArtist(ID);
                    if (_artist.Id == null)
                        throw new Exception("Artist not found");

                    //foreach (var item in _lst.Items)
                    {

                        string _Genre = "";
                        bool _Islatin = false;

                        if (_artist.Genres != null && _artist.Genres.Count > 0)
                        {
                            _Genre = _artist.Genres[0];

                            foreach (var _gen in _artist.Genres)
                            {
                                if (_GenreFilter.Any(x => x.ToLower() == _gen.ToString()))
                                {
                                    _Islatin = true;
                                    break;
                                }
                            }
                        }

                        _artistlst.id = _artist.Id;
                        _artistlst.name = _artist.Name;
                        _artistlst.main_genre = _Genre;
                        _artistlst.IsLatin = _Islatin;
                    }
                    return _artistlst;
                }

                //);
            }
            catch (Exception ex)
            {
                await LogHelper.CreateLog(ex);
                throw new Exception("Artist not found");
                return null;
            }
        }

        private async Task<List<MusicGraph.Datum>> Spotify_GetSimilarArtistByID(string ID)
        {
            try
            {
                List<MusicGraph.Datum> _artistlst = new List<MusicGraph.Datum>();
                string _Spotify_Country = ConfigurationManager.AppSettings["Spotify_Country"].ToString();
                //Task.Factory.StartNew(() =>
                {
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
                //);
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog(ex);
                return null;
            }
        }

        #endregion

        #region "Profile Picture"

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
                        //string imageresizenametmp = ResizeImage.ScaleImage(tempfilePath + strpTempImageSave, tempfilePath, "_S_" + strpTempImageSave, 1430, 1430);
                        string imageresizenametmp = ResizeImage.ScaleImage(tempfilePath + strpTempImageSave, tempfilePath, "_S_" + strpTempImageSave, 650, 650);

                        //Banner Image (event listing)
                        //imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_B_" + strpTempImageSave, 0, 100, 1428, 476);
                        imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_B_" + strpTempImageSave, 0, 100, 640, 270);
                        strpBannerURLfordb = _SiteURL + "/Artists/" + vArtists.ArtistID + "/" + imageresizename;

                        //Image  (used in artist detail screen)
                        //imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + imageresizenametmp, newFilePath, "_A_" + strpTempImageSave, 1428, 689);
                        //imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_A_" + strpTempImageSave, 0, 0, 1428, 689);
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


        private bool SpotifyProfilePicture(Int32 vUserID, Artists vArtists, GenericRepository<Artists> _artistrepo, bool vNew, string Url)
        {
            try
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

                    #region "Commented Code"
                    //Scale up the image 
                    //string imageresizenametmp = ResizeImage.ScaleImage(tempfilePath + strpTempImageSave, tempfilePath, "_S_" + strpTempImageSave, 1430, 1430);
                    /*string imageresizenametmp = ResizeImage.ScaleImage(tempfilePath + strpTempImageSave, tempfilePath, "_S_" + strpTempImageSave, 650, 650);

                    //Banner Image (event listing)
                    //imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_B_" + strpTempImageSave, 0, 100, 1428, 476);
                    imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_B_" + strpTempImageSave, 0, 100, 640, 270);
                    strpBannerURLfordb = _SiteURL + "/Artists/" + vArtists.ArtistID + "/" + imageresizename;

                    //Image  (used in artist detail screen)
                    //imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + imageresizenametmp, newFilePath, "_A_" + strpTempImageSave, 1428, 689);
                    //imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_A_" + strpTempImageSave, 0, 0, 1428, 689);
                    imageresizename = ResizeImage.CropImage(tempfilePath + imageresizenametmp, newFilePath, "_A_" + strpTempImageSave, 0, 50, 640, 360);
                    strpIamgeURLfordb = _SiteURL + "/Artists/" + vArtists.ArtistID + "/" + imageresizename;
                    */
                    #endregion

                    vArtists.ImageURL = Url;
                    vArtists.ThumbnailURL = strpThumbnailURLfordb;
                    vArtists.BannerImage_URL = Url;
                    _artistrepo.Repository.Update(vArtists);
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog("ArtistProfilePicture_Asyn" + ex.Message);
            }
            return true;
        }

        #endregion

        #endregion

        #region ADs
        //[Route("api/v2/Musika/GetBannerList/{UserID}")]
        //[HttpGet]
        //public HttpResponseMessage GetBannerList(int UserID)
        //{
        //    var _Users = new Users();
        //    try
        //    {
        //        GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
        //        if (!String.IsNullOrEmpty(UserID.ToString()))
        //        {
        //            _Users = _UsersRepo.Repository.Get(x => x.UserID == UserID);
        //        }
        //        else
        //        {
        //            _Users = null;
        //        }

        //        string CityName = "";
        //        if (_Users != null)
        //        {
        //            string Lat = "";
        //            if (_Users.DeviceLat != null)
        //            {
        //                Lat = Convert.ToString(_Users.DeviceLat);
        //            }
        //            string Log = "";
        //            if (_Users.DeviceLat != null)
        //            {
        //                Log = Convert.ToString(_Users.DeviceLong);
        //            }
        //            CityName = ReverseGeoCode(Lat, Log);
        //        }
        //        GenericRepository<Ads> _AdsRepo = new GenericRepository<Ads>(_unitOfWork);
        //        var _AdsList = _AdsRepo.Repository.GetAll(t => t.City == CityName && t.Recordstatus == RecordStatus.Active.ToString()).Select(x => new { x.AdId, x.ImageURL, x.LinkURL });
        //        //var _AdsList = _AdsRepo.Repository.GetAll(t => t.City == "Lahore" && t.Recordstatus == RecordStatus.Active.ToString()).Select(x => new { x.AdId, x.ImageURL, x.LinkURL });

        //        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _AdsList, "BannerList"));
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
        //    }
        //}

        [Route("api/v2/Musika/GetBannerList/{UserID}")]
        [HttpGet]
        public HttpResponseMessage GetBannerList(int UserID)
        {
            var _Users = new Users();
            try
            {
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
                if (!String.IsNullOrEmpty(UserID.ToString()))
                {
                    _Users = _UsersRepo.Repository.Get(x => x.UserID == UserID);
                }
                else
                {
                    _Users = null;
                }

                string CityName = "";
                if (_Users != null)
                {
                    string Lat = "";
                    if (_Users.DeviceLat != null)
                    {
                        Lat = Convert.ToString(_Users.DeviceLat);
                    }
                    string Log = "";
                    if (_Users.DeviceLat != null)
                    {
                        Log = Convert.ToString(_Users.DeviceLong);
                    }
                    CityName = ReverseGeoCode(Lat, Log);
                }
                GenericRepository<Ads> _AdsRepo = new GenericRepository<Ads>(_unitOfWork);
                if (_Users != null)
                {
                    var _AdsList = _AdsRepo.Repository.GetAll(t => t.City == CityName && t.Recordstatus == RecordStatus.Active.ToString()).Select(x => new { x.AdId, x.ImageURL, x.LinkURL });
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _AdsList, "BannerList"));
                }
                else
                {
                    var _AdsList = _AdsRepo.Repository.GetAll(t => t.Recordstatus == RecordStatus.Active.ToString()).Select(x => new { x.AdId, x.ImageURL, x.LinkURL });
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _AdsList, "BannerList"));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }

        public string ReverseGeoCode(string lat, string lng)
        {
            try
            {
                //&key=" + APIkey;
                //string APIKey = "AIzaSyDQTpXj82d8UpCi97wzo_nKXL7nYrd4G70";//ConfigurationManager.AppSettings["GoogleAppID"].ToString();
                string APIKey = "AIzaSyDCeTiDNn6CjjcBG1iOxOFCcHiwZET6iNc";
                string coordinate = lat + "," + lng;
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load("https://maps.googleapis.com/maps/api/geocode/xml?latlng=" + coordinate + "&key=" + APIKey);

                XmlNodeList xNodelst = xDoc.GetElementsByTagName("result");
                XmlNode xNode = xNodelst.Item(0);
                string FullAddress = xNode.SelectSingleNode("formatted_address").InnerText;
                string city = "";

                try
                {
                    if (xNode.SelectSingleNode("address_component[1]/type").InnerText == "country")
                    {
                        city = xNode.SelectSingleNode("address_component[1]/long_name").InnerText;
                    }


                    if (xNode.SelectSingleNode("address_component[2]/type").InnerText == "country" && city == "")
                    {
                        city = xNode.SelectSingleNode("address_component[2]/long_name").InnerText;
                    }

                    if (xNode.SelectSingleNode("address_component[3]/type").InnerText == "country" && city == "")
                    {
                        city = xNode.SelectSingleNode("address_component[3]/long_name").InnerText;
                    }

                    if (xNode.SelectSingleNode("address_component[4]/type").InnerText == "country" && city == "")
                    {
                        city = xNode.SelectSingleNode("address_component[4]/long_name").InnerText;
                    }

                    if (xNode.SelectSingleNode("address_component[5]/type").InnerText == "country" && city == "")
                    {
                        city = xNode.SelectSingleNode("address_component[5]/long_name").InnerText;
                    }
                    if (xNode.SelectSingleNode("address_component[6]/type").InnerText == "country" && city == "")
                    {
                        city = xNode.SelectSingleNode("address_component[6]/long_name").InnerText;
                    }
                    if (xNode.SelectSingleNode("address_component[7]/type").InnerText == "country" && city == "")
                    {
                        city = xNode.SelectSingleNode("address_component[7]/long_name").InnerText;
                    }
                    if (xNode.SelectSingleNode("address_component[8]/type").InnerText == "country" && city == "")
                    {
                        city = xNode.SelectSingleNode("address_component[8]/long_name").InnerText;
                    }
                    if (xNode.SelectSingleNode("address_component[9]/type").InnerText == "country" && city == "")
                    {
                        city = xNode.SelectSingleNode("address_component[9]/long_name").InnerText;
                    }
                }
                catch
                {

                }

                #region "Commented Code"
                //string code = xNode.SelectSingleNode("address_component[5]/short_name").InnerText;
                //string code1 = xNode.SelectSingleNode("address_component[5]/long_name").InnerText;


                //string Number = xNode.SelectSingleNode("address_component[1]/long_name").InnerText;
                //string Street = xNode.SelectSingleNode("address_component[2]/long_name").InnerText;
                //string Village = xNode.SelectSingleNode("address_component[3]/long_name").InnerText;
                //string Area = xNode.SelectSingleNode("address_component[4]/long_name").InnerText;
                //string County = xNode.SelectSingleNode("address_component[5]/long_name").InnerText;
                //string State = xNode.SelectSingleNode("address_component[6]/long_name").InnerText;
                //string Zip = xNode.SelectSingleNode("address_component[8]/long_name").InnerText;
                //string Country = xNode.SelectSingleNode("address_component[7]/long_name").InnerText;

                //dynamic res = new { FullAddress = FullAddress, ShortAddress = city + ", " + code }; ;
                #endregion

                return city;

            }
            catch (Exception ex)
            {
                return "";
                //throw;
            }
        }


        #endregion

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

        #region "Send Push Notification"

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


        [AllowAnonymous]
        [Route("api/v2/Musika/SendNotification/{fromUserId}/{toUserId}/{message}/{tourDateId}/{artistName}/{venueName}")]
        [HttpGet]
        public HttpResponseMessage SendNotification(int fromUserId, int toUserId, string message, int tourDateId, string artistName, string venueName, string type)
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

                _UsersTo = _UsersRepo.Repository.Get(p => p.UserID == toUserId);

                //_UsersTo.UserLanguage
                _UserSettings = _UserSettingsRepo.Repository.Get(p => p.UserID == _UsersTo.UserID && p.SettingKey == EUserSettings.Musika.ToString());

                PushNotifications pNoty = new PushNotifications();

                var deviceList = _UserDevicesRepo.Repository.GetAll(x => x.UserId == toUserId);

                if (tourDateId > 0)
                    _TourEntity = _TourDateRepo.Repository.GetById(tourDateId);

                if (_UserSettings.SettingValue == false)
                {
                    LogHelper.CreateLog2(message + " - NOTIFICATION OFF - ToUserID : " + toUserId.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                }

                //for multiple devices
                if (deviceList != null && deviceList.Count > 0 && _UserSettings.SettingValue == true && _UsersTo.RecordStatus == RecordStatus.Active.ToString() && _TourEntity != null)
                {
                    bool updateCount = false;

                    message = SetMessageLang(_UsersTo.UserLanguage, _TourEntity.Tour_Utcdate, artistName, venueName);

                    foreach (var d in deviceList)
                    {
                        if (string.IsNullOrEmpty(d.DeviceToken))
                        {
                            LogHelper.CreateLog2(message + " - Device Token Not found - ToUserID : " + toUserId.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                        }

                        if (d.DeviceType == "IOS" && !string.IsNullOrEmpty(d.DeviceToken))
                        {
                            LogHelper.CreateLog2(message + " - IOS - ToUserID : " + toUserId.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                            //pNoty.SendNotification_IOS(d.DeviceToken, message,type);
                            //pNoty.SendNotification_IOS(d.DeviceToken, message, Numerics.GetInt(_UserSettings.NotificationCount + 1));
                            updateCount = true;
                        }
                        else if (d.DeviceType == "Android" && !string.IsNullOrEmpty(d.DeviceToken))
                        {
                            LogHelper.CreateLog2(message + " - ANDROID - ToUserID : " + toUserId.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                            //pNoty.SendNotification_Android(d.DeviceToken, message,type);
                            updateCount = true;
                        }
                    }

                    if (updateCount)
                    {
                        _UserSettings.NotificationCount += 1;
                        _UserSettingsRepo.Repository.Update(_UserSettings);
                    }
                }

                // for single device
                if (deviceList.Count == 0 && _UserSettings.SettingValue == true && _UsersTo.RecordStatus == RecordStatus.Active.ToString() && _TourEntity != null)
                {
                    bool updateCount = false;

                    message = SetMessageLang(_UsersTo.UserLanguage, _TourEntity.Tour_Utcdate, artistName, venueName);

                    if (string.IsNullOrEmpty(_UsersTo.DeviceToken))
                    {
                        LogHelper.CreateLog2(message + " - Device Token Not found - ToUserID : " + toUserId.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                    }

                    if (_UsersTo.DeviceType == "IOS" && !string.IsNullOrEmpty(_UsersTo.DeviceToken))
                    {
                        LogHelper.CreateLog2(message + " - IOS - ToUserID : " + toUserId.ToString() + " devicetoken= " + _UsersTo.DeviceToken, Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                        //pNoty.SendNotification_IOS(_UsersTo.DeviceToken, message,type);
                        //pNoty.SendNotification_IOS(_UsersTo.DeviceToken, message, Numerics.GetInt(_UserSettings.NotificationCount + 1));
                        _UserSettings.NotificationCount += 1;
                        _UserSettingsRepo.Repository.Update(_UserSettings);
                    }
                    else if (_UsersTo.DeviceType == "Android" && !string.IsNullOrEmpty(_UsersTo.DeviceToken))
                    {
                        LogHelper.CreateLog2(message + " - ANDROID - ToUserID : " + toUserId.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                        //pNoty.SendNotification_Android(_UsersTo.DeviceToken, message,type);
                        _UserSettings.NotificationCount += 1;
                        _UserSettingsRepo.Repository.Update(_UserSettings);
                    }
                }

                #region "Commented Code"
                /*
                if (_UserSettings.SettingValue == true)
                {
                    if (_UsersTo.DeviceType == "IOS")
                    {
                        pNoty.SendNotification_IOS(_UsersTo.DeviceToken, message, Numerics.GetInt(_UserSettings.NotificationCount + 1));
                        _UserSettings.NotificationCount += 1;
                        _UserSettingsRepo.Repository.Update(_UserSettings);
                    }
                    else if (_UsersTo.DeviceType == "Android")
                    {
                        pNoty.SendNotification_Android(_UsersTo.DeviceToken, message);
                        _UserSettings.NotificationCount += 1;
                        _UserSettingsRepo.Repository.Update(_UserSettings);
                    }
                }
                */
                #endregion

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, null));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message));
            }
        }

        #region "New Notification To be Sent Weekly"
        public string SetMessageLangNew(string lang)    // DateTime? tourDate)
        {

            var message = "";

            if (lang == EUserLanguage.EN.ToString())
                message = NotificationResponse.TwoDayEnglish;
            else
                message = NotificationResponse.TwoDaySpanish;

            #region "Commented Code"
            //if (tourDate.HasValue)
            //{
            //    var dayOfTheWeek = tourDate.Value.DayOfWeek.ToString();
            //    var isTwoDays = tourDate.Value > DateTime.UtcNow.AddDays(1);


            //    if (isTwoDays)
            //    {
            //        if (lang == EUserLanguage.EN.ToString())
            //            message = NotificationResponse.TwoDayEnglish;
            //        else
            //            message = NotificationResponse.TwoDaySpanish;
            //    }
            //    else
            //    {
            //        if (lang == EUserLanguage.EN.ToString())
            //            message = NotificationResponse.OneDayEnglish;
            //        else
            //            message = NotificationResponse.OneDaySpanish;
            //    }


            //    if (!String.IsNullOrEmpty(message))
            //    {
            //        if (message.Contains("[day_of_week]"))
            //            message = message.Replace("[day_of_week]", dayOfTheWeek);

            //        //if (message.Contains("[artist_name]"))
            //        //    message = message.Replace("[artist_name]", artistName);

            //        //if (message.Contains("[venue_name]"))
            //        //    message = message.Replace("[venue_name]", venueName);
            //    }
            //}
            #endregion

            return message;
        }

        public static DateTime GetMonday(DateTime time)
        {
            //if (time.DayOfWeek != DayOfWeek.Monday)
            if (time.DayOfWeek != DayOfWeek.Monday)
                return time.Subtract(new TimeSpan((int)time.DayOfWeek - 1, 0, 0, 0));

            return time;
        }

        public void SendNotificationNew()
        {
            MusikaEntities db = new MusikaEntities();
            List<Users> lstUsers = new List<Users>();

            IUnitOfWork _unitOfWork;
            HttpCache _Cache = new HttpCache();

            string type = string.Empty;
            string message = string.Empty;

            _unitOfWork = new UnitOfWork();

            GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);
            GenericRepository<Artists> _ArtistRepo = new GenericRepository<Artists>(_unitOfWork);
            GenericRepository<UserSettings> _UserSettingsRepo = new GenericRepository<UserSettings>(_unitOfWork);
            GenericRepository<UserDevices> _UserDevicesRepo = new GenericRepository<UserDevices>(_unitOfWork);
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);

            List<Artists> lstArtists = new List<Artists>();
            DateTime dt = GetMonday(DateTime.Now);
            lstArtists = _ArtistRepo.Repository.GetAll(u => u.RecordStatus == "Active").Where(t => t.CreatedDate >= dt).ToList();

            #region "Commented Code"
            //lstArtists = _ArtistRepo.Repository.GetAll(u => u.RecordStatus == "Active").ToList();

            //string artists = string.Empty;
            //artists += "Newly Added Artists : ";
            //foreach (Artists str in lstArtists)
            //{
            //    artists += str.ArtistName + ",";
            //}
            #endregion

            string events = string.Empty;
            List<TourDate> lstEvents = new List<TourDate>();
            lstEvents = _TourDateRepo.Repository.GetAll(u => u.RecordStatus == "Active").Where(t => t.CreatedDate >= dt).ToList();

            #region "Commented Code"
            //lstEvents = _TourDateRepo.Repository.GetAll(u => u.RecordStatus == "Active").ToList();
            //events += "Newly Added Events : ";
            //foreach (TourDate str in lstEvents)
            //{
            //    events += str.EventName + ",";
            //}
            #endregion

            string finalMessage = string.Empty;
            if (lstArtists.Count > 0 || lstEvents.Count > 0)
            {
                finalMessage += "There are some updates in Musika App";
            }

            #region "Commented Code"
            //finalMessage += artists + "-" + events;

            //if (finalMessage.Length > 1)
            //{
            //    finalMessage += "There are some updates in Musika App";
            //}
            #endregion

            //Users entity;
            lstUsers = _UsersRepo.Repository.GetAll(u => u.RecordStatus == "Active");

            //lstUsers = _UsersRepo.Repository.GetAll(u => u.RecordStatus == "Active").Where(t=>t.UserID==7985).ToList();  // IOS
            //lstUsers = _UsersRepo.Repository.GetAll(u => u.RecordStatus == "Active").Where(t => t.UserID == 8207).ToList(); // Android

            if (lstUsers.Count > 0)
            {
                try
                {
                    Users _UsersTo = null;
                    UserSettings _UserSettings = null;
                    TourDate _TourEntity = null;

                    for (int i = 0; i < lstUsers.Count; i++)
                    {
                        int uid = lstUsers[i].UserID;
                        _UsersTo = _UsersRepo.Repository.Get(p => p.UserID == uid);

                        //_UsersTo.UserLanguage
                        _UserSettings = _UserSettingsRepo.Repository.Get(p => p.UserID == _UsersTo.UserID && p.SettingKey == EUserSettings.Musika.ToString());

                        type = lstUsers[i].DeviceType;

                        PushNotifications pNoty = new PushNotifications();

                        var deviceList = _UserDevicesRepo.Repository.GetAll(x => x.UserId == uid);

                        _TourEntity = null;
                        //if (tourDateId > 0)
                        //    _TourEntity = _TourDateRepo.Repository.GetById(tourDateId);

                        if (_UserSettings.SettingValue == false)
                        {
                            LogHelper.CreateLog2(message + " - NOTIFICATION OFF - ToUserID : " + uid.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                        }

                        //for multiple devices
                        if (deviceList != null && deviceList.Count > 0 && _UserSettings.SettingValue == true && _UsersTo.RecordStatus == RecordStatus.Active.ToString()) // && _TourEntity != null)
                        {
                            bool updateCount = false;

                            //message = SetMessageLangNew(_UsersTo.UserLanguage); //, _TourEntity.Tour_Utcdate);

                            foreach (var d in deviceList)
                            {
                                if (string.IsNullOrEmpty(d.DeviceToken))
                                {
                                    LogHelper.CreateLog2(finalMessage + " - Device Token Not found - ToUserID : " + lstUsers[i].UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                                }

                                if (d.DeviceType == "IOS" && !string.IsNullOrEmpty(d.DeviceToken))
                                {
                                    LogHelper.CreateLog2(finalMessage + " - IOS - ToUserID : " + lstUsers[i].UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                                    pNoty.SendNotification_IOS(d.DeviceToken, finalMessage, type);
                                    updateCount = true;
                                }
                                else if (d.DeviceType == "Android" && !string.IsNullOrEmpty(d.DeviceToken))
                                {
                                    LogHelper.CreateLog2(message + " - ANDROID - ToUserID : " + lstUsers[i].UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                                    pNoty.SendNotification_Android(d.DeviceToken, finalMessage, type);
                                    updateCount = true;
                                }
                            }
                            if (updateCount)
                            {
                                _UserSettings.NotificationCount += 1;
                                _UserSettingsRepo.Repository.Update(_UserSettings);
                            }
                        }

                        // for single device
                        if (deviceList.Count == 1 && _UserSettings.SettingValue == true && _UsersTo.RecordStatus == RecordStatus.Active.ToString() && _TourEntity != null)
                        {
                            message = SetMessageLangNew(_UsersTo.UserLanguage);  //, _TourEntity.Tour_Utcdate);

                            if (string.IsNullOrEmpty(_UsersTo.DeviceToken))
                            {
                                LogHelper.CreateLog2(message + " - Device Token Not found - ToUserID : " + lstUsers[i].UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                            }

                            if (_UsersTo.DeviceType == "IOS" && !string.IsNullOrEmpty(_UsersTo.DeviceToken))
                            {
                                LogHelper.CreateLog2(message + " - IOS - ToUserID : " + lstUsers[i].UserID.ToString() + " devicetoken= " + _UsersTo.DeviceToken, Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                                pNoty.SendNotification_IOS(_UsersTo.DeviceToken, message, type);
                                _UserSettings.NotificationCount += 1;
                                _UserSettingsRepo.Repository.Update(_UserSettings);
                            }
                            else if (_UsersTo.DeviceType == "Android" && !string.IsNullOrEmpty(_UsersTo.DeviceToken))
                            {
                                LogHelper.CreateLog2(message + " - ANDROID - ToUserID : " + lstUsers[i].UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                                pNoty.SendNotification_Android(_UsersTo.DeviceToken, message, type);
                                _UserSettings.NotificationCount += 1;
                                _UserSettingsRepo.Repository.Update(_UserSettings);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        #region "New Updated Code"
        public HttpResponseMessage SendNotificationWeekly(int fromUserId, string message)
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

                        message = SetMessageLang(_UsersTo.UserLanguage, _TourEntity.Tour_Utcdate, "", null);

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
                                    pNoty.SendNotification_IOS(d.DeviceToken, message, "");
                                    updateCount = true;
                                }
                                else if (d.DeviceType == "Android" && !string.IsNullOrEmpty(d.DeviceToken))
                                {
                                    LogHelper.CreateLog2(message + " - ANDROID - ToUserID : " + _UsersTo.UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                                    pNoty.SendNotification_Android(d.DeviceToken, message, "");
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

                        message = SetMessageLang(_UsersTo.UserLanguage, _TourEntity.Tour_Utcdate, "", null);

                        if (string.IsNullOrEmpty(_UsersTo.DeviceToken))
                        {
                            LogHelper.CreateLog2(message + " - Device Token Not found - ToUserID : " + _UsersTo.UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);
                        }

                        if (_UsersTo.DeviceType == "IOS" && !string.IsNullOrEmpty(_UsersTo.DeviceToken))
                        {
                            LogHelper.CreateLog2(message + " - IOS - ToUserID : " + _UsersTo.UserID.ToString() + " devicetoken= " + _UsersTo.DeviceToken, Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                            pNoty.SendNotification_IOS(_UsersTo.DeviceToken, message, "");

                            //pNoty.SendNotification_IOS(_UsersTo.DeviceToken, message, Numerics.GetInt(_UserSettings.NotificationCount + 1));
                            _UserSettings.NotificationCount += 1;
                            _UserSettingsRepo.Repository.Update(_UserSettings);
                        }
                        else if (_UsersTo.DeviceType == "Android" && !string.IsNullOrEmpty(_UsersTo.DeviceToken))
                        {
                            LogHelper.CreateLog2(message + " - ANDROID - ToUserID : " + _UsersTo.UserID.ToString(), Musika.Library.Utilities.LogHelper.ErrorType.Notification);

                            pNoty.SendNotification_Android(_UsersTo.DeviceToken, message, "");
                            _UserSettings.NotificationCount += 1;
                            _UserSettingsRepo.Repository.Update(_UserSettings);
                        }
                    }

                    if (_UserSettings.SettingValue == true)
                    {
                        if (_UsersTo.DeviceType == "IOS")
                        {
                            #region "Commented Code"
                            //pNoty.SendNotification_IOS(_UsersTo.DeviceToken, message, Numerics.GetInt(_UserSettings.NotificationCount + 1));
                            //_UserSettings.NotificationCount += 1;
                            //_UserSettingsRepo.Repository.Update(_UserSettings);
                            #endregion

                            #region "New Code"
                            pNoty.SendNotification_IOS(_UsersTo.DeviceToken, message, "");
                            _UserSettings.NotificationCount += 1;
                            _UserSettingsRepo.Repository.Update(_UserSettings);
                            #endregion
                        }
                        else if (_UsersTo.DeviceType == "Android")
                        {
                            pNoty.SendNotification_Android(_UsersTo.DeviceToken, message, "");
                            _UserSettings.NotificationCount += 1;
                            _UserSettingsRepo.Repository.Update(_UserSettings);
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
        #endregion


        [AllowAnonymous]
        [Route("api/v2/Musika/SendNotificationWeekly")]
        [HttpPost]
        public HttpResponseMessage SendNotificationWeekly()
        {
            SendNotificationNew();
            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, null));
        }
        #endregion

        #endregion

        #region "Function"
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

        public string CleanName(string name)
        {
            name = name.Replace(" ", "");
            string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ\s]";
            string _st = Regex.Replace(name, pattern, "");

            return Regex.Replace(name, pattern, "");
        }
        #endregion

        #region Perform Operation

        private void EventFul_Asyn_Operation(Int32 vUserID, Artists vArtists, IUnitOfWork vUnitOfWork, Dictionary<string, object> apiResponse, bool vScan = false)
        {
            try
            {
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(vUnitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(vUnitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(vUnitOfWork);
                GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

                Models.Venue _VenuEntity = null;
                Models.TourDate _TourDateEntity = null;
                UserTourDate _UserTourDate = null;
                dynamic _Get_Performer_Events = null;

                if (apiResponse.ContainsKey("SeatGeekDetail"))
                {
                    _Get_Performer_Events = (dynamic)apiResponse["SeatGeekDetail"];
                    if (_Get_Performer_Events.events != null)
                    {
                        if (_Get_Performer_Events.events.@event is IEnumerable)
                        {

                            foreach (dynamic _event in _Get_Performer_Events.events.@event)
                            {
                                dynamic _Get_Event_ByID = (dynamic)apiResponse["GetEventByID_" + _event.id.ToString()];
                                if (_Get_Event_ByID != null)
                                {
                                    //Venu information

                                    #region "Commented Code"
                                    //_VenuEntity = _VenueRepo.Repository.Get(p => (p.Eventful_VenueID == _Get_Event_ByID.id.ToString()) ||
                                    //                                             (p.VenueName.ToLower() == _Get_Event_ByID.venue_name.ToLower())
                                    //        );
                                    #endregion

                                    //using fuzzy searching techinque here
                                    _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                                   where (A.Eventful_VenueID == _Get_Event_ByID.venue_id.ToString())
                                                   select A).FirstOrDefault();

                                    //search the venu using fuzzy searching
                                    if (_VenuEntity == null)
                                    {
                                        #region "Commented Code"
                                        /* _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                                       where (DiceCoefficientExtensions.DiceCoefficient(A.VenueName.ToLower(), _Get_Event_ByID.venue_name.ToLower()) >= _FuzzySearchCri)
                                                       && A.RecordStatus == RecordStatus.SeatGeek.ToString()
                                                       select A).FirstOrDefault();
                                                       */
                                        #endregion

                                        string name = _Get_Event_ByID.venue_name.ToLower();
                                        string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ&\s]";
                                        var search = Regex.Replace(name, pattern, "");

                                        var venueByName = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 60 && x.RecordStatus == RecordStatus.SeatGeek.ToString() && x.VenueName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                                        if (venueByName == null || venueByName.Count() == 0)
                                        {
                                            var allEventfulVenues = _VenueRepo.Repository.AsQueryable().Where(x => x.VenueName.Length < 80 && x.RecordStatus == RecordStatus.SeatGeek.ToString()).ToList();
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
                                        _VenuEntity.Eventful_VenueID = _Get_Event_ByID.venue_id.ToString();
                                        _VenuEntity.SeatGeek_VenuID = null;

                                        _VenuEntity.VenueName = _Get_Event_ByID.venue_name;
                                        _VenuEntity.Extended_Address = _Get_Event_ByID.address;
                                        _VenuEntity.VenueCountry = _Get_Event_ByID.country;
                                        _VenuEntity.Display_Location = "";
                                        _VenuEntity.Slug = "";
                                        _VenuEntity.VenueState = "";
                                        _VenuEntity.Postal_Code = _Get_Event_ByID.postal_code;
                                        _VenuEntity.VenueCity = _Get_Event_ByID.city;
                                        _VenuEntity.Address = _Get_Event_ByID.address;
                                        _VenuEntity.Timezone = "";

                                        _VenuEntity.VenueLat = Convert.ToDecimal(_Get_Event_ByID.latitude);
                                        _VenuEntity.VenueLong = Convert.ToDecimal(_Get_Event_ByID.longitude);

                                        _VenuEntity.CreatedDate = DateTime.Now;
                                        _VenuEntity.RecordStatus = RecordStatus.Eventful.ToString();

                                        _VenueRepo.Repository.Add(_VenuEntity);
                                    }

                                    string _Event_ID = _Get_Event_ByID.id.ToString();
                                    string _start_time = _Get_Event_ByID.start_time;

                                    #region "Commented Code"
                                    //Entering Tour records
                                    /* _TourDateEntity = (from A in _VenueRepo.Repository.GetAll(p => p.VenueID == _VenuEntity.VenueID)
                                                        join B in _TourDateRepo.Repository.GetAll(p =>
                                                                                        (p.Eventful_TourID == _Event_ID && p.ArtistID == vArtists.ArtistID)
                                                                                     || (Convert.ToDateTime(p.Datetime_Local).Month == Convert.ToDateTime(_start_time).Month
                                                                                         && Convert.ToDateTime(p.Datetime_Local).Year == Convert.ToDateTime(_start_time).Year
                                                                                         && Convert.ToDateTime(p.Datetime_Local).Day == Convert.ToDateTime(_start_time).Day
                                                                                         && p.ArtistID == vArtists.ArtistID
                                                                                         && p.RecordStatus == RecordStatus.SeatGeek.ToString())
                                                                                     ) on A.VenueID equals B.VenueID
                                                        where B.ArtistID == vArtists.ArtistID
                                                        select B).FirstOrDefault();
                                                        */
                                    #endregion

                                    if (_TourDateEntity == null)
                                    {
                                        DateTime local = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                        if (local != null)
                                        {
                                            string strLocal = local.ToString("MM/dd/yyyy");
                                            _TourDateEntity = _TourDateRepo.Repository.AsQueryable().Where(x => x.ArtistID == vArtists.ArtistID && (x.Datetime_Local.DateToString() == strLocal || x.Visible_Until_utc.DateToString() == strLocal || x.Tour_Utcdate.DateToString() == strLocal)).FirstOrDefault();
                                        }

                                        if (_TourDateEntity == null)
                                        {
                                            _TourDateEntity = new TourDate();

                                            _TourDateEntity.Eventful_TourID = _Get_Event_ByID.id.ToString();
                                            _TourDateEntity.SeatGeek_TourID = null;

                                            _TourDateEntity.ArtistID = vArtists.ArtistID;
                                            _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                            _TourDateEntity.EventName = _Get_Event_ByID.title;
                                            _TourDateEntity.EventID = null;
                                            _TourDateEntity.Score = 0;

                                            _TourDateEntity.Announce_Date = null;
                                            _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                            _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                            _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Event_ByID.start_time);

                                            _TourDateEntity.CreatedDate = DateTime.Now;
                                            _TourDateEntity.RecordStatus = RecordStatus.Eventful.ToString();

                                            try
                                            {
                                                if (_Get_Event_ByID.url != null)
                                                    _TourDateEntity.TicketURL = _Get_Event_ByID.url;
                                                else if (_Get_Event_ByID.links != null)
                                                    _TourDateEntity.TicketURL = _Get_Event_ByID.links.link[0].url;
                                            }
                                            catch { }

                                            if (String.IsNullOrEmpty(_TourDateEntity.TicketURL))
                                                _TourDateEntity.TicketURL = "http://eventful.com/";


                                            //if(_TourDateRepo.Repository.AsQueryable().Any(x=>x.TourDateID == _TourDateEntity.TourDateID)
                                            _TourDateRepo.Repository.Add(_TourDateEntity);

                                            if (_UserTourDate == null)
                                            {
                                                _UserTourDate = new UserTourDate();
                                                _UserTourDate.TourDateID = _TourDateEntity.TourDateID;
                                                _UserTourDate.UserID = vUserID;
                                                _UserTourDate.CreatedDate = DateTime.Now;
                                                _UserTourDate.RecordStatus = RecordStatus.Active.ToString();

                                                #region "Commented Code"
                                                /*if (_Get_Event_ByID.links != null)
                                                {
                                                    try
                                                    {
                                                        _TourDateEntity.TicketURL = _Get_Event_ByID.links.link[0].url;
                                                    }
                                                    catch { }
                                                }
                                                else
                                                {
                                                    _TourDateEntity.TicketURL = "http://eventful.com/";
                                                }*/
                                                #endregion

                                                _UserTourDateRepo.Repository.Add(_UserTourDate);
                                            }
                                        }
                                    }
                                    #region "Commented Code"
                                    /*SOMETHING HERE USERTOURDATE*/
                                    //else
                                    //{

                                    //    _TourDateEntity.SeatGeek_TourID = _Get_Event_ByID.id.ToString();
                                    //    _TourDateEntity.ArtistID = vArtists.ArtistID;
                                    //    _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                    //    _TourDateEntity.EventName = _Get_Event_ByID.title;
                                    //    _TourDateEntity.EventID = null;
                                    //    _TourDateEntity.Score = 0;

                                    //    _TourDateEntity.Announce_Date = null;
                                    //    _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                    //    _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                    //    _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Event_ByID.start_time);

                                    //    _TourDateEntity.ModifiedDate = DateTime.Now;

                                    //    _TourDateRepo.Repository.Update(_TourDateEntity);

                                    //}
                                    #endregion
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog(ex);
                //throw new Exception(ex.Message, ex);
            }
        }

        private void SeatGeek_Asyn_Operation(Int32 vUserID, Artists vArtists, IUnitOfWork vUnitOfWork, Task<Dictionary<string, object>> apiResponse, bool vScan = false)
        {
            GenericRepository<Artists> _ArtistRepo = new GenericRepository<Artists>(vUnitOfWork);
            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(vUnitOfWork);
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(vUnitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(vUnitOfWork);

            GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(vUnitOfWork);
            GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(vUnitOfWork);
            GenericRepository<Genre> _GenreRepo = new GenericRepository<Genre>(vUnitOfWork);

            string _Performer_ID = null;
            string _strEvent = null;

            Models.Venue _VenuEntity = null;
            Models.TourDate _TourDateEntity = null;
            Models.UserTourDate _UserTourDate = null;
            Models.ArtistGenre _ArtistGenre = null;
            Models.Genre _Genre = null;

            SeatGeek.Get_Performers _Get_Performers = null;
            dynamic _Get_Performers_Events = null;
            bool vNew = Convert.ToBoolean(apiResponse.Result["vNew"]);
            try
            {
                if (vNew)
                {
                    _Get_Performers = (SeatGeek.Get_Performers)apiResponse.Result["SeatGeekDetail"];
                    _Get_Performers_Events = apiResponse.Result["SeatGeekPerformer"];

                    if (_Get_Performers != null)
                    {
                        if (_Get_Performers.performers.Count > 0)
                        {
                            SeatGeek.Performer _Performer = new SeatGeek.Performer();

                            _Performer = (from A in _Get_Performers.performers
                                          where DiceCoefficientExtensions.DiceCoefficient(A.name.ToLower(), vArtists.ArtistName.Trim()) >= _FuzzySearchCri && CheckSeatGeekLatin(A.name, _unitOfWork)
                                          select A).OrderByDescending(p => p.score).FirstOrDefault();

                            if (_Performer != null)
                            {
                                vArtists.OnTour = _Performer.has_upcoming_events == true ? true : false;
                                _Performer_ID = _Performer.id.ToString();
                                vArtists.Seatgeek_ID = _Performer_ID;

                                if (_Performer.taxonomies != null && _Performer.taxonomies.Count > 0)
                                {
                                    foreach (SeatGeek.Taxonomy _Taxonomy in _Performer.taxonomies)
                                    {
                                        _strEvent = _strEvent + _Taxonomy.name + " ,";
                                    }
                                }

                                if (_strEvent.ToString() == "") _strEvent = vArtists.ArtistName + " Event";

                                #region "Loop through the Genre"
                                if (_Performer.genres != null && _Performer.genres.Count > 0)
                                {
                                    foreach (SeatGeek.Genre _Ev in _Performer.genres)
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

                                        var check = !_ArtistGenreRepo.Repository.AsQueryable().Any(x => x.ArtistID == vArtists.ArtistID && x.GenreID == _Genre.GenreID);

                                        if (check)
                                        {
                                            _ArtistGenre = new ArtistGenre();
                                            _ArtistGenre.GenreID = _Genre.GenreID;
                                            _ArtistGenre.ArtistID = vArtists.ArtistID;
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

                                if (String.IsNullOrEmpty(vArtists.ImageURL))
                                    GetProfileImageFromSpotifyFeed(vArtists);

                                if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                                {
                                    #region "Get Artist Picture "
                                    try
                                    {
                                        if (_Performer.images != null)
                                        {
                                            if (_Performer.images.huge != null)
                                            {
                                                bool _ArtistPic = ArtistProfilePicture_Asyn(vUserID, vArtists, vUnitOfWork, true, _Performer.images.huge);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    #endregion
                                }
                            }
                        }
                        else
                        {
                            vArtists.OnTour = false;
                        }
                    }
                    else
                    {
                        vArtists.OnTour = false;
                    }

                    if (_Get_Performers_Events != null)
                    {
                        if (_Get_Performers_Events.events.Count > 0)
                        {
                            #region "loop through the events"
                            //foreach (SeatGeek2.Event _Ev in _Get_Performers_Events.events)
                            foreach (dynamic _Ev in _Get_Performers_Events.events)
                            {
                                //SeatGeek2.Venue _Venue = _Ev.venue;
                                dynamic _Venue = _Ev.venue;

                                //Venu information
                                //_VenuEntity = _VenueRepo.Repository.Get(p => p.SeatGeek_VenuID == _Venue.id.ToString());

                                string _VenueID = _Venue.id.ToString();
                                string _VenueName = _Venue.name.ToLower();

                                _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                               where (A.SeatGeek_VenuID == _VenueID)
                                               select A).FirstOrDefault();

                                //search the venu using fuzzy searching
                                if (_VenuEntity == null)
                                {
                                    #region "Commented Code"
                                    /*_VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                                   where (DiceCoefficientExtensions.DiceCoefficient(A.VenueName.ToLower(), _VenueName) >= _FuzzySearchCri)
                                                   && A.RecordStatus == RecordStatus.Eventful.ToString()
                                                   select A).FirstOrDefault();
                                                   */
                                    #endregion

                                    string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ&\s]";
                                    var search = Regex.Replace(_VenueName, pattern, "");

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

                                    #region "Commented Code"
                                    //try
                                    //{
                                    //    if (_Venue.url != "")
                                    //    {
                                    //        strTempImageSave = ResizeImage.Download_Image(_Venue.url);

                                    //        //---New Image path
                                    //        string newFilePath = _SiteRoot + @"\" + "Venu" + @"\" ;
                                    //        Helper.CreateDirectories(_SiteRoot + @"\" + "Venu" + @"\" );

                                    //        string imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_V_" + strTempImageSave, _Imagethumbsize, _Imagethumbsize);
                                    //        strIamgeURLfordb = _SiteURL + "/Venu/" + imageresizename;

                                    //        _Events.ImageURL = strIamgeURLfordb;
                                    //    }
                                    //}
                                    //catch(Exception ex) { }
                                    #endregion

                                    _VenuEntity.CreatedDate = DateTime.Now;
                                    _VenuEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                    _VenueRepo.Repository.Add(_VenuEntity);
                                }

                                //Entering Tour records
                                // _TourDateEntity = _TourDateRepo.Repository.Get(p => p.SeatGeek_TourID == _Ev.id.ToString() );
                                string _Id = _Ev.id.ToString();
                                string _datetime_local = _Ev.datetime_local;

                                DateTime _datetime_local2 = Convert.ToDateTime(_datetime_local);
                                _TourDateEntity = (from A in _VenueRepo.Repository.GetAll(p => p.VenueID == _VenuEntity.VenueID)
                                                   join B in _TourDateRepo.Repository.GetAll(p =>
                                                                                   (p.SeatGeek_TourID == _Id && p.ArtistID == vArtists.ArtistID)
                                                                                || (DbFunctions.TruncateTime(p.Datetime_Local).Value.Month == _datetime_local2.Month
                                                                                    && DbFunctions.TruncateTime(p.Datetime_Local).Value.Year == _datetime_local2.Year
                                                                                    && DbFunctions.TruncateTime(p.Datetime_Local).Value.Day == _datetime_local2.Day
                                                                                    && p.ArtistID == vArtists.ArtistID
                                                                                    && p.RecordStatus == RecordStatus.Eventful.ToString())
                                                                                ) on A.VenueID equals B.VenueID
                                                   where B.ArtistID == vArtists.ArtistID
                                                   select B).FirstOrDefault();


                                if (_TourDateEntity == null)
                                {
                                    _TourDateEntity = new TourDate();

                                    _TourDateEntity.SeatGeek_TourID = _Ev.id.ToString();
                                    _TourDateEntity.ArtistID = vArtists.ArtistID;
                                    _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                    _TourDateEntity.EventName = _Ev.title;

                                    _TourDateEntity.EventID = null;
                                    _TourDateEntity.Score = _Ev.score;

                                    _TourDateEntity.Announce_Date = Convert.ToDateTime(_Ev.announce_date);
                                    _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Ev.visible_until_utc);
                                    _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Ev.datetime_utc);
                                    _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Ev.datetime_local);

                                    _TourDateEntity.CreatedDate = DateTime.Now;
                                    _TourDateEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                    if (!String.IsNullOrEmpty(_Ev.url))
                                        _TourDateEntity.TicketURL = _Ev.url;
                                    else if (_Ev.venue != null && !String.IsNullOrEmpty(_Ev.venue.url))
                                        _TourDateEntity.TicketURL = _Ev.venue.url;
                                    else
                                        _TourDateEntity.TicketURL = "https://seatgeek.com/";

                                    _TourDateRepo.Repository.Add(_TourDateEntity);

                                    if (vScan == true)
                                    {
                                        _UserTourDate = new UserTourDate();
                                        _UserTourDate.TourDateID = _TourDateEntity.TourDateID;
                                        _UserTourDate.UserID = vUserID;
                                        _UserTourDate.CreatedDate = DateTime.Now;
                                        _UserTourDate.RecordStatus = RecordStatus.Active.ToString();

                                        _UserTourDateRepo.Repository.Add(_UserTourDate);
                                    }
                                }
                                else
                                {
                                    _TourDateEntity.SeatGeek_TourID = _Ev.id.ToString();
                                    _TourDateEntity.ArtistID = vArtists.ArtistID;
                                    _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                    _TourDateEntity.Score = _Ev.score;
                                    _TourDateEntity.EventName = _Ev.title;

                                    _TourDateEntity.Announce_Date = Convert.ToDateTime(_Ev.announce_date);
                                    _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Ev.visible_until_utc);
                                    _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Ev.datetime_utc);

                                    _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Ev.datetime_local);

                                    _TourDateEntity.ModifiedDate = DateTime.Now;

                                    if (!String.IsNullOrEmpty(_Ev.url))
                                        _TourDateEntity.TicketURL = _Ev.url;
                                    else if (_Ev.venue != null && !String.IsNullOrEmpty(_Ev.venue.url))
                                        _TourDateEntity.TicketURL = _Ev.venue.url;
                                    else
                                        _TourDateEntity.TicketURL = "https://seatgeek.com/";

                                    _TourDateRepo.Repository.Update(_TourDateEntity);

                                    if (vScan == true)
                                    {
                                        _UserTourDate = _UserTourDateRepo.Repository.Get(p => p.UserID == vUserID && p.TourDateID == _TourDateEntity.TourDateID);

                                        if (_UserTourDate == null)
                                        {
                                            _UserTourDate = new UserTourDate();
                                            _UserTourDate.TourDateID = _TourDateEntity.TourDateID;
                                            _UserTourDate.UserID = vUserID;
                                            _UserTourDate.CreatedDate = DateTime.Now;
                                            _UserTourDate.RecordStatus = RecordStatus.Active.ToString();

                                            _UserTourDateRepo.Repository.Add(_UserTourDate);
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }//update part 
                else
                {
                    if (apiResponse.Result.ContainsKey("SeatGeekDetail"))
                    {
                        _Get_Performers = (SeatGeek.Get_Performers)apiResponse.Result["SeatGeekDetail"];
                    }
                    _Get_Performers_Events = apiResponse.Result["SeatGeekPerformer"];

                    //SeatGeek Update Task 1 Operation

                    if (vArtists.Seatgeek_ID == null)
                    {
                        if (_Get_Performers != null)
                        {
                            if (_Get_Performers.performers.Count > 0)
                            {
                                SeatGeek.Performer _Performer = new SeatGeek.Performer();

                                _Performer = (from A in _Get_Performers.performers
                                              where DiceCoefficientExtensions.DiceCoefficient(A.name.ToLower(), vArtists.ArtistName.Trim()) >= _FuzzySearchCri && CheckSeatGeekLatin(A.name, _unitOfWork)
                                              select A).OrderByDescending(p => p.score).FirstOrDefault();

                                if (_Performer == null)
                                {
                                    _Performer = _Get_Performers.performers[0];
                                }

                                if (_Performer != null)
                                {
                                    vArtists.OnTour = _Performer.has_upcoming_events == true ? true : false;
                                    _Performer_ID = _Performer.id.ToString();
                                    vArtists.Seatgeek_ID = _Performer_ID;

                                    if (_Performer.taxonomies != null && _Performer.taxonomies.Count > 0)
                                    {
                                        foreach (SeatGeek.Taxonomy _Taxonomy in _Performer.taxonomies)
                                        {
                                            _strEvent = _strEvent + _Taxonomy.name + " ,";
                                        }
                                    }

                                    if (_strEvent.ToString() == "") _strEvent = vArtists.ArtistName + " Event";

                                    #region "Loop through the Genre"
                                    if (_Performer.genres != null && _Performer.genres.Count > 0)
                                    {
                                        foreach (SeatGeek.Genre _Ev in _Performer.genres)
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
                                            var check = !_ArtistGenreRepo.Repository.AsQueryable().Any(x => x.ArtistID == vArtists.ArtistID && x.GenreID == _Genre.GenreID);

                                            if (check)
                                            {
                                                _ArtistGenre = new ArtistGenre();
                                                _ArtistGenre.GenreID = _Genre.GenreID;
                                                _ArtistGenre.ArtistID = vArtists.ArtistID;
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

                                    if (String.IsNullOrEmpty(vArtists.ImageURL))
                                        GetProfileImageFromSpotifyFeed(vArtists);

                                    if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                                    {
                                        #region "Get Artist Picture "
                                        try
                                        {
                                            if (_Performer.images != null)
                                            {
                                                if (_Performer.images.huge != null)
                                                {
                                                    bool _ArtistPic = ArtistProfilePicture_Asyn(vUserID, vArtists, vUnitOfWork, true, _Performer.images.huge);
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                        #endregion
                                    }
                                }
                            }
                            else
                            {
                                vArtists.OnTour = false;
                            }
                        }
                        else
                        {
                            vArtists.OnTour = false;
                        }
                    }

                    //SeatGeek Update Task 2 Operation
                    if (_Get_Performers_Events != null)
                    {
                        if (_Get_Performers_Events.events.Count > 0)
                        {
                            if (WebConfigurationManager.AppSettings["SeatGeek.ArtistPicture"].ToString() == "True")
                            {
                                #region "Get Artist Picture "
                                try
                                {

                                    if (_Get_Performers_Events.events[0].performers[0].image != null)
                                    {
                                        if (_Get_Performers_Events.events[0].performers[0].image != null)
                                        {
                                            Task<bool> _ArtistPic = ArtistProfilePicture_Asyn(vUserID, vArtists, vUnitOfWork, true, _Get_Performers_Events.events[0].performers[0].image);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                #endregion
                            }
                            #region "Loop through the Events"
                            //foreach (SeatGeek2.Event _Ev in _Get_Performers_Events.events)
                            foreach (dynamic _Ev in _Get_Performers_Events.events)
                            {
                                //SeatGeek2.Venue _Venue = _Ev.venue;
                                dynamic _Venue = _Ev.venue;

                                string _VenueID = _Venue.id.ToString();
                                string _VenueName = _Venue.name.ToLower();

                                //Venu information
                                // _VenuEntity = _VenueRepo.Repository.Get(p => p.SeatGeek_VenuID == _Venue.id.ToString());

                                _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                               where (A.SeatGeek_VenuID == _VenueID)
                                               select A).FirstOrDefault();

                                //search the venu using fuzzy searching
                                if (_VenuEntity == null)
                                {
                                    /*_VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                                   where (DiceCoefficientExtensions.DiceCoefficient(A.VenueName.ToLower(), _VenueName) >= _FuzzySearchCri)
                                                   && A.RecordStatus == RecordStatus.Eventful.ToString()
                                                   select A).FirstOrDefault();
                                                   */

                                    string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ&\s]";
                                    var search = Regex.Replace(_VenueName, pattern, "");

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

                                    #region "Commented Code"
                                    //try
                                    //{
                                    //    if (_Venue.url != "")
                                    //    {
                                    //        strTempImageSave = ResizeImage.Download_Image(_Venue.url);

                                    //        //---New Image path
                                    //        string newFilePath = _SiteRoot + @"\" + "Venu" + @"\" ;
                                    //        Helper.CreateDirectories(_SiteRoot + @"\" + "Venu" + @"\" );

                                    //        string imageresizename = ResizeImage.Resize_Image_Thumb(tempfilePath + strTempImageSave, newFilePath, "_V_" + strTempImageSave, _Imagethumbsize, _Imagethumbsize);
                                    //        strIamgeURLfordb = _SiteURL + "/Venu/" + imageresizename;

                                    //        _Events.ImageURL = strIamgeURLfordb;
                                    //    }
                                    //}
                                    //catch(Exception ex) { }
                                    #endregion

                                    _VenuEntity.CreatedDate = DateTime.Now;
                                    _VenuEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                    _VenueRepo.Repository.Add(_VenuEntity);
                                }

                                //Entering Tour records
                                //_TourDateEntity = _TourDateRepo.Repository.Get(p => p.SeatGeek_TourID == _Ev.id.ToString());
                                string _Id = _Ev.id.ToString();
                                string _datetime_local = _Ev.datetime_local;

                                DateTime _datetime_local2 = Convert.ToDateTime(_datetime_local);
                                _TourDateEntity = (from A in _VenueRepo.Repository.GetAll(p => p.VenueID == _VenuEntity.VenueID)
                                                   join B in _TourDateRepo.Repository.GetAll(p =>
                                                                                   (p.SeatGeek_TourID == _Id && p.ArtistID == vArtists.ArtistID)
                                                                                || (DbFunctions.TruncateTime(p.Datetime_Local).Value.Month == _datetime_local2.Month
                                                                                    && DbFunctions.TruncateTime(p.Datetime_Local).Value.Year == _datetime_local2.Year
                                                                                    && DbFunctions.TruncateTime(p.Datetime_Local).Value.Day == _datetime_local2.Day
                                                                                    && p.ArtistID == vArtists.ArtistID
                                                                                    && p.RecordStatus == RecordStatus.Eventful.ToString())
                                                                                ) on A.VenueID equals B.VenueID
                                                   where B.ArtistID == vArtists.ArtistID
                                                   select B).FirstOrDefault();


                                if (_TourDateEntity == null)
                                {
                                    _TourDateEntity = new TourDate();

                                    _TourDateEntity.SeatGeek_TourID = _Ev.id.ToString();
                                    _TourDateEntity.ArtistID = vArtists.ArtistID;
                                    _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                    _TourDateEntity.EventName = _Ev.title;
                                    _TourDateEntity.EventID = null;
                                    _TourDateEntity.Score = _Ev.score;

                                    _TourDateEntity.Announce_Date = Convert.ToDateTime(_Ev.announce_date);
                                    _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Ev.visible_until_utc);
                                    _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Ev.datetime_utc);
                                    _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Ev.datetime_local);

                                    _TourDateEntity.CreatedDate = DateTime.Now;
                                    _TourDateEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                    if (!String.IsNullOrEmpty(_Ev.url))
                                        _TourDateEntity.TicketURL = _Ev.url;
                                    else if (_Ev.venue != null && !String.IsNullOrEmpty(_Ev.venue.url))
                                        _TourDateEntity.TicketURL = _Ev.venue.url;
                                    else
                                        _TourDateEntity.TicketURL = "https://seatgeek.com/";

                                    _TourDateRepo.Repository.Add(_TourDateEntity);
                                }
                                else
                                {
                                    _TourDateEntity.SeatGeek_TourID = _Ev.id.ToString();
                                    _TourDateEntity.ArtistID = vArtists.ArtistID;
                                    _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                    _TourDateEntity.Score = _Ev.score;
                                    _TourDateEntity.EventName = _Ev.title;

                                    _TourDateEntity.Announce_Date = Convert.ToDateTime(_Ev.announce_date);
                                    _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Ev.visible_until_utc);
                                    _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Ev.datetime_utc);

                                    _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Ev.datetime_local);

                                    if (!String.IsNullOrEmpty(_Ev.url))
                                        _TourDateEntity.TicketURL = _Ev.url;
                                    else if (_Ev.venue != null && !String.IsNullOrEmpty(_Ev.venue.url))
                                        _TourDateEntity.TicketURL = _Ev.venue.url;
                                    else
                                        _TourDateEntity.TicketURL = "https://seatgeek.com/";

                                    _TourDateEntity.ModifiedDate = DateTime.Now;

                                    _TourDateRepo.Repository.Update(_TourDateEntity);
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog(ex);
                // throw new Exception(ex.Message, ex);
            }
        }

        private void Instagram_Asyn_Operation(Int32 vUserID, Artists vArtists, MusicGraph2.ArtistMatrics_ByID vArtistMatrics_ByID, IUnitOfWork vUnitOfWork, TourDate vTour, Task<Dictionary<string, object>> result)
        {
            string strIamgeURLfordb = null;
            string _Instagram_access_token = ConfigurationManager.AppSettings["instagram.access_token"].ToString();
            GenericRepository<TourPhoto> _TourPhotosRepo = new GenericRepository<TourPhoto>(vUnitOfWork);
            GenericRepository<Artists> _ArtistRepo = new GenericRepository<Artists>(vUnitOfWork);
            try
            {
                if (Convert.ToBoolean(result.Result["vNew"]))
                {
                    Instagram4.Tags_Images _User_TagPictures = (Instagram4.Tags_Images)result.Result["Tags_Images"];
                    if (_User_TagPictures != null)
                    {
                        if (_User_TagPictures.data.Count > 0)
                        {
                            List<Instagram4.Datum> _Data = _User_TagPictures.data.Where(p => p.type == "image").ToList();

                            int _chk = 0;
                            int _count = 0;

                            if (_Data.Count > 29)
                            {
                                _count = 29;
                            }
                            else
                            {
                                _count = _Data.Count;
                            }

                            foreach (Instagram4.Datum _d in _Data)
                            {
                                try
                                {
                                    Instagram4.Images _image = _d.images;

                                    if (_image.standard_resolution.url != "")
                                    {
                                        Models.TourPhoto _tourPhotos = new TourPhoto();

                                        strIamgeURLfordb = _image.standard_resolution.url;
                                        strIamgeURLfordb = strIamgeURLfordb.Remove(strIamgeURLfordb.LastIndexOf('?'));
                                        _tourPhotos.ImageURL = strIamgeURLfordb;// urll.Remove(urll.IndexOf('?'), urll.Length);
                                        _tourPhotos.ImageThumbnailURL = _image.thumbnail.url.Remove(_image.thumbnail.url.LastIndexOf('?'));
                                        _tourPhotos.CreatedDate = DateTime.Now;
                                        //_tourPhotos.ArtistID = vArtists.ArtistID;
                                        _tourPhotos.RecordStatus = RecordStatus.Instagram.ToString();
                                        _tourPhotos.TourDateId = vTour.TourDateID;
                                        _TourPhotosRepo.Repository.Add(_tourPhotos);
                                    }

                                    _chk += 1;
                                    if (_chk > _count)
                                    {
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.CreateLog("Instagram_GetPictures_Asyn (Recent Picture (Hash Tag) - Lopp) " + ex.Message);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Instagram4.Tags_Images _User_TagPictures = (Instagram4.Tags_Images)result.Result["Tags_Images"];
                    if (_User_TagPictures != null)
                    {
                        if (_User_TagPictures.data.Count > 0)
                        {
                            List<Instagram4.Datum> _Data = _User_TagPictures.data.Where(p => p.type == "image").ToList();

                            int _chk = 0;
                            int _count = 0;

                            if (_Data.Count > 29)
                            {
                                _count = 29;
                            }
                            else
                            {
                                _count = _Data.Count;
                            }
                            foreach (Instagram4.Datum _d in _Data)
                            {
                                try
                                {
                                    Instagram4.Images _image = _d.images;

                                    if (_image.standard_resolution.url != "")
                                    {
                                        Models.TourPhoto _tourPhotos = new TourPhoto();

                                        strIamgeURLfordb = _image.standard_resolution.url;
                                        strIamgeURLfordb = strIamgeURLfordb.Remove(strIamgeURLfordb.LastIndexOf('?'));
                                        _tourPhotos.ImageURL = strIamgeURLfordb;// urll.Remove(urll.IndexOf('?'), urll.Length);
                                        _tourPhotos.ImageThumbnailURL = _image.thumbnail.url.Remove(_image.thumbnail.url.LastIndexOf('?'));
                                        _tourPhotos.CreatedDate = DateTime.Now;
                                        //_tourPhotos.ArtistID = vArtists.ArtistID;
                                        _tourPhotos.RecordStatus = RecordStatus.Instagram.ToString();
                                        _tourPhotos.TourDateId = vTour.TourDateID;
                                        _TourPhotosRepo.Repository.Add(_tourPhotos);
                                    }
                                    _chk += 1;
                                    if (_chk > _count)
                                    {
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.CreateLog("Instagram_GetPictures_Asyn (Recent Picture (Hash Tag) - Lopp) " + ex.Message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog(ex);
                throw new Exception(ex.Message, ex);
            }
        }

        public void ImagesFromInstagram_Asyn_Operation(Artists vArtists, Task<Dictionary<string, object>> IstagramImgs)
        {
            GenericRepository<ArtistPhotos> _ArtistPhotosRepository = new GenericRepository<ArtistPhotos>(_unitOfWork);
            GenericRepository<Artists> _ArtistRepo = new GenericRepository<Artists>(_unitOfWork);
            int photoLikeRequirement = 0;
            int totalPhotosAllowed = 30;
            try
            {
                if (IstagramImgs != null)
                {
                    if (IstagramImgs.Result.ContainsKey("ApiResponse") && IstagramImgs.Result.ContainsKey("Count"))
                    {
                        string name = vArtists.ArtistName;
                        name = name.Replace(" ", "");
                        string pattern = @"[^a-zA-Z0-9áéíñóúüÁÉÍÑÓÚÜ\s]";
                        string cleanName = Regex.Replace(name, pattern, "");

                        var artistTourPhotos = _ArtistPhotosRepository.Repository.GetAll(x => x.ArtistID == vArtists.ArtistID && x.RecordStatus == RecordStatus.Active.ToString()).ToList();
                        int count = Convert.ToInt32(IstagramImgs.Result["Count"]);
                        string apiResponse = IstagramImgs.Result["ApiResponse"].ToString();
                        JObject joResponse = JObject.Parse(apiResponse);

                        JArray items = null;
                        int _flag = 0;
                        try
                        {
                            items = (JArray)joResponse["user"]["media"]["nodes"];
                        }
                        catch
                        {
                            items = (JArray)joResponse["graphql"]["hashtag"]["edge_hashtag_to_media"]["edges"];
                            _flag = 2;
                        }

                        #region "_flag=0"
                        if (_flag == 0)
                        {
                            count = totalPhotosAllowed - count;

                            foreach (var i in items)
                            {
                                int imagelikes = i["likes"].Value<int>("count");
                                string imageId = i.Value<string>("id");
                                if (imagelikes > photoLikeRequirement)
                                {
                                    var artistRecord = artistTourPhotos.FirstOrDefault(x => x.InstagramPostId == imageId);

                                    if (artistRecord == null)
                                    {
                                        ArtistPhotos tp = new ArtistPhotos();
                                        tp.ImageThumbnailUrl = i["thumbnail_resources"].ElementAt(0).Value<string>("src");

                                        try
                                        {
                                            tp.ImageUrl = i["thumbnail_resources"].ElementAt(3).Value<string>("src");
                                        }
                                        catch
                                        {
                                            tp.ImageUrl = i["thumbnail_resources"].ElementAt(0).Value<string>("src");
                                        }

                                        try
                                        {
                                            //tp.ImageThumbnailUrl = tp.ImageThumbnailUrl.Remove(tp.ImageThumbnailUrl.LastIndexOf('?'));
                                            tp.ImageThumbnailUrl = tp.ImageThumbnailUrl.Replace("", "");
                                        }
                                        catch
                                        { }

                                        //try
                                        //{
                                        //    // tp.ImageUrl = tp.ImageUrl.Remove(tp.ImageUrl.LastIndexOf('?'));
                                        //    tp.ImageUrl = tp.ImageUrl.Replace("s150x150", "s320x320");
                                        //}
                                        //catch { }

                                        tp.InstagramPostId = i.Value<string>("id");
                                        tp.ArtistID = vArtists.ArtistID;
                                        tp.HashTagName = cleanName;
                                        tp.CreatedDate = DateTime.Now;
                                        tp.ModifiedDate = DateTime.Now;
                                        tp.RecordStatus = RecordStatus.Active.ToString();
                                        _ArtistPhotosRepository.Repository.Add(tp);

                                        count--;
                                    }
                                    else
                                    {
                                        if (artistRecord.RecordStatus == RecordStatus.InActive.ToString())
                                        {
                                            artistRecord.RecordStatus = RecordStatus.Active.ToString();
                                            artistRecord.ModifiedDate = DateTime.Now;
                                            _ArtistPhotosRepository.Repository.Update(artistRecord);

                                            count--;
                                        }
                                    }

                                    if (count == 0)
                                        break;
                                }
                            }
                        }
                        #endregion

                        #region "_flag=1"
                        else if (_flag == 1)
                        {
                            count = totalPhotosAllowed - count;

                            foreach (var i in items)
                            {
                                int imagelikes = i["node"]["edge_liked_by"].Value<int>("count");
                                string imageId = i["node"].Value<string>("id");
                                if (imagelikes > photoLikeRequirement)
                                {
                                    var artistRecord = artistTourPhotos.FirstOrDefault(x => x.InstagramPostId == imageId);

                                    if (artistRecord == null)
                                    {
                                        ArtistPhotos tp = new ArtistPhotos();
                                        tp.ImageThumbnailUrl = i["node"]["thumbnail_resources"].ElementAt(0).Value<string>("src");

                                        try
                                        {
                                            tp.ImageUrl = i["node"]["thumbnail_resources"].ElementAt(3).Value<string>("src");
                                        }
                                        catch
                                        {
                                            tp.ImageUrl = i["node"]["thumbnail_resources"].ElementAt(0).Value<string>("src");
                                        }

                                        try
                                        {
                                            //tp.ImageThumbnailUrl = tp.ImageThumbnailUrl.Remove(tp.ImageThumbnailUrl.LastIndexOf('?'));
                                            tp.ImageThumbnailUrl = tp.ImageThumbnailUrl.Replace("", "");
                                        }
                                        catch
                                        { }

                                        //try
                                        //{
                                        //    // tp.ImageUrl = tp.ImageUrl.Remove(tp.ImageUrl.LastIndexOf('?'));
                                        //    tp.ImageUrl = tp.ImageUrl.Replace("s150x150", "s320x320");
                                        //}
                                        //catch { }

                                        tp.InstagramPostId = i["node"].Value<string>("id");
                                        tp.ArtistID = vArtists.ArtistID;
                                        tp.HashTagName = cleanName;
                                        tp.CreatedDate = DateTime.Now;
                                        tp.ModifiedDate = DateTime.Now;
                                        tp.RecordStatus = RecordStatus.Active.ToString();
                                        _ArtistPhotosRepository.Repository.Add(tp);

                                        count--;
                                    }
                                    else
                                    {
                                        if (artistRecord.RecordStatus == RecordStatus.InActive.ToString())
                                        {
                                            artistRecord.RecordStatus = RecordStatus.Active.ToString();
                                            artistRecord.ModifiedDate = DateTime.Now;
                                            _ArtistPhotosRepository.Repository.Update(artistRecord);

                                            count--;
                                        }
                                    }

                                    if (count == 0)
                                        break;
                                }
                            }
                        }
                        #endregion

                        #region "_flag=2"
                        else if (_flag == 2)
                        {
                            count = totalPhotosAllowed - count;

                            foreach (var i in items)
                            {
                                int imagelikes = i["node"]["edge_liked_by"].Value<int>("count");
                                string imageId = i["node"].Value<string>("id");
                                if (imagelikes > photoLikeRequirement)
                                {
                                    var artistRecord = artistTourPhotos.FirstOrDefault(x => x.InstagramPostId == imageId);

                                    if (artistRecord == null)
                                    {
                                        ArtistPhotos tp = new ArtistPhotos();
                                        tp.ImageThumbnailUrl = i["node"]["thumbnail_resources"].ElementAt(0).Value<string>("src");

                                        try
                                        {
                                            tp.ImageUrl = i["node"]["thumbnail_resources"].ElementAt(3).Value<string>("src");
                                        }
                                        catch
                                        {
                                            tp.ImageUrl = i["node"]["thumbnail_resources"].ElementAt(0).Value<string>("src");
                                        }

                                        try
                                        {
                                            //tp.ImageThumbnailUrl = tp.ImageThumbnailUrl.Remove(tp.ImageThumbnailUrl.LastIndexOf('?'));
                                            tp.ImageThumbnailUrl = tp.ImageThumbnailUrl.Replace("", "");
                                        }
                                        catch
                                        { }

                                        //try
                                        //{
                                        //    // tp.ImageUrl = tp.ImageUrl.Remove(tp.ImageUrl.LastIndexOf('?'));
                                        //    tp.ImageUrl = tp.ImageUrl.Replace("s150x150", "s320x320");
                                        //}
                                        //catch { }

                                        tp.InstagramPostId = i["node"].Value<string>("id");
                                        tp.ArtistID = vArtists.ArtistID;
                                        tp.HashTagName = cleanName;
                                        tp.CreatedDate = DateTime.Now;
                                        tp.ModifiedDate = DateTime.Now;
                                        tp.RecordStatus = RecordStatus.Active.ToString();
                                        _ArtistPhotosRepository.Repository.Add(tp);

                                        count--;
                                    }
                                    else
                                    {
                                        if (artistRecord.RecordStatus == RecordStatus.InActive.ToString())
                                        {
                                            artistRecord.RecordStatus = RecordStatus.Active.ToString();
                                            artistRecord.ModifiedDate = DateTime.Now;
                                            _ArtistPhotosRepository.Repository.Update(artistRecord);

                                            count--;
                                        }
                                    }

                                    if (count == 0)
                                        break;
                                }
                            }
                        }
                        #endregion

                        foreach (var a in artistTourPhotos)
                        {
                            //    if (a.RecordStatus == RecordStatus.InActive.ToString() && a.ModifiedDate == currentDate)
                            //    {
                            //        _ArtistPhotosRepository.Repository.Update(a);
                            //    }
                            //    else 
                            if (a.RecordStatus == RecordStatus.InActive.ToString())
                            {
                                _ArtistPhotosRepository.Repository.DeletePermanent(a.PhotoID);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog(ex);
                //throw new Exception(ex.Message, ex);
            }
        }

        public void SpotifyImage_Asyn_Operation(Artists vArtists, Task<string> ImageUrl)
        {
            GenericRepository<Artists> _ArtistRepository = new GenericRepository<Artists>(_unitOfWork);
            try
            {
                if (ImageUrl != null)
                {
                    if (!string.IsNullOrEmpty(ImageUrl.Result))
                    {
                        bool _ArtistPic = SpotifyProfilePicture(0, vArtists, _ArtistRepository, true, ImageUrl.Result);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
            }
        }

        public void Spotify_GetSongInfo_Asyn_Operation(Int32 vUserID, Artists vArtists, IUnitOfWork vUnitOfWork, Task<Dictionary<string, object>> apiResponse)
        {
            //bool vNew = Convert.ToBoolean(apiResponse.Result["vNew"]);
            GenericRepository<Artists> _ArtistRepository = new GenericRepository<Artists>(_unitOfWork);
            try
            {
                if (apiResponse.Result.ContainsKey("Task"))
                {
                    List<Track> _task = (List<Track>)apiResponse.Result["Task"];
                    if (_task != null && _task.Count > 0)
                    {
                        _task = _task.Where(p => p.PreviewUrl != null).OrderByDescending(t => t.Popularity).ToList();

                        if (_task.Count > 2)
                        {
                            vArtists.Spotify_Url = _task[2].PreviewUrl;
                            vArtists.Spotify_URL_Name = _task[2].Name;
                        }
                        else
                        {
                            vArtists.Spotify_Url = _task[0].PreviewUrl;
                            vArtists.Spotify_URL_Name = _task[0].Name;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog(ex.Message);
            }
        }

        public void Spotify_GetSimilarArtistByID_Operation(Artists vArtists, IUnitOfWork vUnitOfWork, List<MusicGraph.Datum> apiResponse)
        {
            try
            {
                GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(vUnitOfWork);
                GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(vUnitOfWork);

                Models.ArtistRelated _ArtistRelatedEntity = null;

                List<MusicGraph.Datum> _GetSimilarArtists_ByID = apiResponse;
                if (_GetSimilarArtists_ByID != null)
                {

                    if (_GetSimilarArtists_ByID.Count > 0)
                    {
                        var _similarlst = (from A in _GetSimilarArtists_ByID
                                           select A).OrderByDescending(p => p.id).ToList();

                        var _Artistrelated = _ArtistRelatedRepo.Repository.GetAll(p => p.ArtistID == vArtists.ArtistID).Select(p => p.RelatedID).ToList();

                        if (_Artistrelated != null )
                        {
                            if (_Artistrelated.Count > 0)
                            {
                                _ArtistRelatedRepo.Repository.DeletePermanent(_Artistrelated);
                            }
                        }

                        foreach (MusicGraph.Datum _related in _similarlst)
                        {
                            if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _related.name))
                            {
                                bool isLatin = false;

                                //isLatin = CheckMusicGraphLatin(_related.id, _unitOfWork);

                                if (!isLatin)
                                {
                                    Task<bool> _mytastk = SeatGeek_CheckLatinGenre_Asyn(_related.name, _unitOfWork);
                                    isLatin = _mytastk.Result;
                                }
                                if (!isLatin)
                                {
                                    isLatin = CheckLastResortSpotifyGenre(_related.id);
                                }

                                if (isLatin)
                                {
                                    string _Musicgraph_ID = !String.IsNullOrEmpty(_related.id) ? _related.id : _related.name;

                                    _ArtistRelatedEntity = new ArtistRelated();
                                    _ArtistRelatedEntity.Musicgraph_ID = _Musicgraph_ID;
                                    _ArtistRelatedEntity.RelatedArtistName = _related.name;
                                    _ArtistRelatedEntity.Similarity = 0;

                                    _ArtistRelatedEntity.ArtistID = vArtists.ArtistID;
                                    _ArtistRelatedEntity.CreatedDate = DateTime.Now;
                                    _ArtistRelatedEntity.RecordStatus = "Spotify";

                                    _ArtistRelatedRepo.Repository.Add(_ArtistRelatedEntity);
                                }
                                else
                                {
                                    if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _related.name))
                                        _ArtistNotLatinRepo.Repository.Add(new ArtistsNotLatin() { ArtistName = _related.name });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog(ex.Message);
            }
        }

        #endregion

        [HttpGet]
        [Route("api/v2/Venue/GetYourTicketingPlans")]
        public HttpResponseMessage GetYourTicketingPlans(Int32 UserID, Int16 Pageindex, Int16 Pagesize)
        {
            try
            {
                ViewYourPlans _ViewYourPlansDetail = new ViewYourPlans();

                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<UserGoing> _UserGoingRepo = new GenericRepository<UserGoing>(_unitOfWork);
                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                //Models.Artists _Artist = null;
                //Models.TourDate _TourDate = null;
                Models.Users _Users = null;
                //Models.Venue _Venue = null;

                _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "YourPlans"));
                }

                _ViewYourPlansDetail.UserID = _Users.UserID;
                _ViewYourPlansDetail.UserName = _Users.UserName;
                _ViewYourPlansDetail.Email = _Users.Email;
                _ViewYourPlansDetail.ThumbnailURL = _Users.ThumbnailURL;
                _ViewYourPlansDetail.ImageURL = _Users.ImageURL;

                //Upcoming Events
                _ViewYourPlansDetail.Plans = (from A in _UserGoingRepo.Repository.GetAll(p => p.UserID == UserID && p.RecordStatus == EUserGoing.Going.ToString())
                                              join B in _TourDateRepo.Repository.GetAll(p => p.Tour_Utcdate >= DateTime.UtcNow && !p.IsDeleted) on A.TourDateID equals B.TourDateID
                                              join C in _ArtistsRepo.Repository.GetAll() on B.ArtistID equals C.ArtistID
                                              join D in _VenueRepo.Repository.GetAll() on B.VenueID equals D.VenueID
                                              orderby B.Visible_Until_utc descending
                                              select new ViewYourplanlst
                                              {
                                                  TourDateID = B.TourDateID,
                                                  ArtistID = B.ArtistID,
                                                  ArtistName = C.ArtistName ?? "",
                                                  ImageURL = C.ImageURL ?? "",
                                                  BannerImage_URL = C.BannerImage_URL ?? "",
                                                  Datetime_Local = B.Datetime_Local.Value,
                                                  Date_Local = Convert.ToDateTime(B.Datetime_Local).ToString("d"),
                                                  Time_Local = Convert.ToDateTime(B.Datetime_Local).ToString("t"),
                                                  VenueID = B.VenueID,
                                                  VenueName = D.VenueName ?? "",
                                                  Extended_Address = D.Extended_Address ?? "",
                                                  Display_Location = D.Display_Location ?? "",
                                                  Slug = D.Slug ?? "",
                                                  Postal_Code = D.Postal_Code ?? "",
                                                  Address = D.Address ?? "",
                                                  Timezone = D.Timezone ?? "",
                                                  VenueCity = D.VenueCity ?? "",
                                                  VenueState = D.VenueState ?? "",
                                                  VenueCountry = D.VenueCountry ?? "",
                                                  VenueLat = D.VenueLat,
                                                  VenueLong = D.VenueLong,
                                                  QRCodeImage = string.Empty,
                                                  //TicketId = string.Empty
                                              }).OrderBy(p => p.Datetime_Local).ToPagedList(Pageindex - 1, Pagesize).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _ViewYourPlansDetail, "YourPlans"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "YourPlans"));
            }
        }

        #region "Coupon"
        [HttpGet]
        [Route("api/v2/Coupon/ValidateCouponCode")]
        public HttpResponseMessage ValidateCouponCode(string name, string category)
        {
            GenericRepository<Coupons> _Coupons = new GenericRepository<Coupons>(_unitOfWork);
            Musika.Models.CouponsModel coupons = new CouponsModel();
            Models.EventReponse tmpResponse = new EventReponse();

            var _list = (from A1 in _Coupons.Repository.GetAll()
                         .Where(p => p.CouponCode == name && p.TicketCategory == category)
                         select new Coupons
                         {
                             Id = A1.Id,
                             EventName = A1.EventName,
                             CouponCode = A1.CouponCode,
                             Discount = A1.Discount,
                             ExpiryDate = A1.ExpiryDate,
                             CreateOn = A1.CreateOn,
                             CreatedBy = A1.CreatedBy,
                             Status = A1.Status,
                             TicketCategory = A1.TicketCategory
                         }).ToList();

            if (_list.Count > 0)
            {
                coupons.Id = _list[0].Id;
                coupons.EventName = _list[0].EventName;
                coupons.CouponCode = _list[0].CouponCode;
                coupons.Discount = _list[0].Discount ?? 0;
                coupons.ExpiryDate = _list[0].ExpiryDate ?? Convert.ToDateTime("1/1/1900");
                coupons.CreateOn = _list[0].CreateOn ?? Convert.ToDateTime("1/1/1900");
                coupons.CreatedBy = _list[0].CreatedBy ?? -1;
                coupons.Status = _list[0].Status.Value == false ? 0 : 1;
                coupons.TicketCategory = _list[0].TicketCategory;

                if (coupons.ExpiryDate < DateTime.Now)
                {
                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Coupon is Expired";
                }
                else if (coupons.Status == 1)
                {
                    tmpResponse.ResponseId = 400;
                    tmpResponse.ReturnMessage = "Coupon is Already Used";
                }
                else
                {
                    new SpRepository().UpdateCoupon(coupons.CouponCode);
                    tmpResponse.ResponseId = 200;
                    tmpResponse.ReturnMessage = "Success";
                }
                coupons.MessageResponse = tmpResponse;

                return Request.CreateResponse(HttpStatusCode.OK, coupons);
            }
            else
            {
                tmpResponse.ResponseId = 400;
                tmpResponse.ReturnMessage = "Invalid Coupon";
                coupons.MessageResponse = tmpResponse;
                return Request.CreateResponse(HttpStatusCode.OK, coupons);
            }
        }
        #endregion
    }
}