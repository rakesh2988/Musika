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

namespace Musika.Controllers.API
{
    public class TicketingAppAPIController : ApiController
    {
        #region "Variables Deceleration"
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpCache _Cache = new HttpCache();

        int _Imagethumbsize = 0;
        int _imageSize = 0;
        bool _ApiLogger = false;
        double _FuzzySearchCri = 0.33;
        #endregion

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

        public TicketingAppAPIController()
        {
            _unitOfWork = new UnitOfWork();
            _Imagethumbsize = Convert.ToInt16(WebConfigurationManager.AppSettings["ImageThumbSize"].ToString());
            _imageSize = Convert.ToInt16(WebConfigurationManager.AppSettings["ImageSize"].ToString());
            _ApiLogger = Convert.ToBoolean(WebConfigurationManager.AppSettings["Logger"].ToString());
        }

        public TicketingAppAPIController(string str)
        {

        }

        #region "Events"
        [HttpGet]
        [Route("api/Ticketing/GetEventsList")]
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

                //GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                //GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                //GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                //GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

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

                DataSet ds = new DataSet();
                List<ViewEventsList> _list = new List<ViewEventsList>();
                ds = new SpRepository().GetEventsListByUserId(UserID);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        ViewEventsList list = new ViewEventsList();
                        DataRow dr = ds.Tables[0].Rows[i];
                        list.TourDateID = Convert.ToInt32(dr[0].ToString());
                        list.ArtistID = Convert.ToInt32(dr[1].ToString());
                        list.ArtistName = Convert.ToString(dr[2].ToString());
                        list.Datetime_dt = Convert.ToDateTime(dr[3].ToString());
                        list.ImageURL = Convert.ToString(dr[4].ToString());
                        list.BannerImage_URL = Convert.ToString(dr[5].ToString());
                        list.OnTour = Convert.ToBoolean(dr[6].ToString());
                        list.VenueName = Convert.ToString(dr[7].ToString());
                        list.VenuID = Convert.ToInt32(dr[8].ToString());
                        _list.Add(list);
                    }
                }

                //_list = _list.OrderBy(p => p.Datetime_Local).ToList();
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
            var artistByName = _ArtistsRepo.Repository.AsQueryable().Where(x => x.ArtistName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).Select(x => new dbEventSearchModel { id = x.ArtistID.ToString(), name = x.ArtistName, type = EventSearchType.Artist.ToString() }).ToList();
            
            var tourDates = venueByName.Union(artistByName).GroupBy(x => x.name).Select(g => g.FirstOrDefault()).ToList();

            return tourDates;
        }

        private List<ViewEventsPreList> DbEventListCheck(Int32 userId, string id, string name, string type)
        {
            List<SeatGeek3.ViewEventLookup> _ViewDBLookup = new List<SeatGeek3.ViewEventLookup>();
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);

            GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

            List<TourDate> dbTourDateList = null;
            List<ViewEventsPreList> dbViewEventsList = new List<ViewEventsPreList>();

            if (type == EventSearchType.Artist.ToString())
            {
                dbTourDateList = _TourDateRepo.Repository.AsQueryable().Where(x => x.ArtistID.ToString() == id)
                    .OrderBy(x => x.Datetime_Local).ToList();
            }
            else if (type == EventSearchType.Venue.ToString())
            {
                dbTourDateList = _TourDateRepo.Repository.AsQueryable().Where(x => x.VenueID.ToString() == id)
                    .OrderBy(x => x.Datetime_Local).ToList();
            }
            else
            {
                dbTourDateList = _TourDateRepo.Repository.AsQueryable().Where(x => x.EventName == name)
                    .OrderBy(x => x.Datetime_Local).ToList();
            }

            foreach (var d in dbTourDateList)
            {
                var _list = (from A in _TourDateRepo.Repository.GetAll(x => x.TourDateID == d.TourDateID).Where(t => t.RecordStatus == null)
                             join B in _ArtistsRepo.Repository.GetAll() on A.ArtistID equals B.ArtistID
                             join C in _VenueRepo.Repository.GetAll() on A.VenueID equals C.VenueID
                             where Helper.GetUTCDateTime(DateTime.Now) < A.Tour_Utcdate
                             select new ViewEventsPreList
                             {
                                 TourDateID = A.TourDateID,
                                 ArtistID = A.ArtistID.Value,
                                 ArtistName = B.ArtistName,
                                 Datetime_dt = A.Datetime_Local,
                                 ImageURL = B.ImageURL,
                                 BannerImage_URL = B.BannerImage_URL,
                                 OnTour = B.OnTour,
                                 VenueName = C.VenueName,
                                 VenuID = C.VenueID,
                                 VenueLat = C.VenueLat,
                                 VenueLon = C.VenueLong,
                                 IsDeleted = A.IsDeleted
                             }).ToList();

                dbViewEventsList.AddRange(_list);
            }
            return dbViewEventsList.OrderBy(x => x.Datetime_Local).ToList();
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
        
        [HttpGet]
        [Route("api/Ticketing/GetEventsSearchResult")]
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

                if (search.Length < 3)
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, "", "Events"));


                _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "Events"));
                }
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

                if (_eventDBLookup == null || _eventDBLookup.Count() < 10)
                {
                    try
                    {
                        //Search for artist name
                        List<dbEventSearchModel> _ArtistList = new List<dbEventSearchModel>();
                        try
                        {

                            var httpMusicRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/v2/artist/suggest?api_key=" + _MusicGrapgh_api_key + "&limit=10&prefix=" + search);
                            httpMusicRequest.ContentType = "application/json";
                            httpMusicRequest.Method = "GET";

                            var httpMusicResponse = (HttpWebResponse)httpMusicRequest.GetResponse();
                            using (var streamReader = new StreamReader(httpMusicResponse.GetResponseStream()))
                            {
                                _result = streamReader.ReadToEnd();
                            }


                            // deserializing 
                            var _Search_ByName = JsonConvert.DeserializeObject<MusicGraph.Search_ByName>(_result);
                            bool isLatin = false;


                            if (_Search_ByName != null)
                            {
                                if (_Search_ByName.data.Count > 0)
                                {
                                    foreach (MusicGraph.Datum _datum in _Search_ByName.data)
                                    {
                                        if (_datum.name.Length < 30)
                                        {
                                            if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _datum.name))
                                            {
                                                //Check to filter Latin artist only
                                                if (_GenreFilter.Contains(_datum.main_genre))
                                                    isLatin = true;
                                                else
                                                {
                                                    //if (!isLatin)
                                                    //    isLatin = CheckMusicGraphLatin(_datum.id, _unitOfWork);

                                                    if (!isLatin)
                                                        isLatin = CheckSeatGeekLatin(_datum.name, _unitOfWork);
                                                    if (!isLatin)
                                                    {
                                                        isLatin = CheckLastResortSpotifyGenre(_datum.spotify_id);
                                                    }
                                                }

                                                if (isLatin)
                                                {
                                                    _ArtistList.Add(new dbEventSearchModel
                                                    {
                                                        id = _datum.id,
                                                        name = _datum.name,
                                                        type = EventSearchType.Artist.ToString()

                                                    });
                                                }
                                                else
                                                {
                                                    if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _datum.name))
                                                        _ArtistNotLatinRepo.Repository.Add(new ArtistsNotLatin() { ArtistName = _datum.name });
                                                }
                                            }
                                        }

                                        isLatin = false;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        { }

                        List<dbEventSearchModel> seatGeekVenueSearch = new List<dbEventSearchModel>();
                        seatGeekVenueSearch = SeatGeekVenueSearch(search);


                        if (seatGeekVenueSearch.Count() < 10)
                        {
                            //var venueWordMatches = new List<string>();
                            foreach (var d in _eventDBLookup)
                            {
                                if (seatGeekVenueSearch.Count() < 15)
                                {
                                    if (d.type == EventSearchType.Venue.ToString())
                                    {
                                        var str = d.name.Split(' ').FirstOrDefault(x => x.ToLower().Contains(search));
                                        if (!String.IsNullOrEmpty(str))
                                        {
                                            seatGeekVenueSearch = seatGeekVenueSearch.Union(SeatGeekVenueSearch(str)).ToList();
                                        }
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        _eventDBLookup = _eventDBLookup.Union(_ArtistList).Union(seatGeekVenueSearch).GroupBy(x => x.name).Select(g => g.FirstOrDefault()).ToList();
                    }
                    catch (Exception)
                    { }
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _eventDBLookup, "Events"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "Events"));
            }
        }

        private string SGAuth()
        {
            return "?client_id=" + WebConfigurationManager.AppSettings["SeatGeek_client_id"] + "&client_secret=" + WebConfigurationManager.AppSettings["SeatGeek_client_secret"];
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
        [Route("api/Ticketing/GetEventsSearchList")]
        public HttpResponseMessage GetEventsSearchList(Int32 UserID, string id, string name, string type, double? Lat, double? Lon, decimal? radius, string from, string to)
        {
            try
            {
                if (UserID == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "UserID required"));
                }

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

                GenericRepository<Users> _UsersRepo = new GenericRepository<Users>(_unitOfWork);

                Models.Users _Users = null;
                HttpCache _HttpCache = new HttpCache();
                //List<SeatGeek3.ViewEventLookup> _ViewEventLookup = new List<SeatGeek3.ViewEventLookup>();
                List<ViewEventsPreList> _ViewEventsList = new List<ViewEventsPreList>();

                _Users = _UsersRepo.Repository.Get(p => p.UserID == UserID);

                if (_Users == null || _Users.RecordStatus != RecordStatus.Active.ToString())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.UserNotFound, "EventList"));
                }

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
                        // Sp -> GetViewEventsPreListWithoutLatLong
                        _list = (from B in _ArtistsRepo.Repository.GetAll()
                                 join A in _TourDateRepo.Repository.GetAll() on B.ArtistID equals A.ArtistID
                                 join C in _VenueRepo.Repository.GetAll() on A.VenueID equals C.VenueID
                                 where Helper.GetUTCDateTime(DateTime.Now) < A.Tour_Utcdate && C.VenueLat.HasValue && C.VenueLong.HasValue && C.VenueLat.Value != 0 && C.VenueLong != 0
                                 select new ViewEventsPreList
                                 {
                                     TourDateID = A.TourDateID,
                                     ArtistID = A.ArtistID.Value,
                                     ArtistName = B.ArtistName,
                                     Datetime_dt = A.Datetime_Local,
                                     ImageURL = B.ImageURL,
                                     BannerImage_URL = B.BannerImage_URL,
                                     OnTour = B.OnTour,
                                     VenueName = C.VenueName,
                                     VenuID = C.VenueID,
                                     VenueLat = C.VenueLat,
                                     VenueLon = C.VenueLong,
                                     IsDeleted = A.IsDeleted,
                                     CityName = C.VenueCity
                                 }).ToList();
                    }
                    else
                    {
                        _list = (from A1 in _UserArtistsRepo.Repository.GetAll(p => p.UserID == UserID)
                                 join A in _TourDateRepo.Repository.GetAll() on A1.ArtistID equals A.ArtistID
                                 join B in _ArtistsRepo.Repository.GetAll() on A.ArtistID equals B.ArtistID
                                 join C in _VenueRepo.Repository.GetAll() on A.VenueID equals C.VenueID
                                 where Helper.GetUTCDateTime(DateTime.Now) < A.Tour_Utcdate && C.VenueLat.HasValue && C.VenueLong.HasValue && C.VenueLat.Value != 0 && C.VenueLong != 0
                                 select new ViewEventsPreList
                                 {
                                     TourDateID = A.TourDateID,
                                     ArtistID = A.ArtistID.Value,
                                     ArtistName = B.ArtistName,
                                     Datetime_dt = A.Datetime_Local,
                                     ImageURL = B.ImageURL,
                                     BannerImage_URL = B.BannerImage_URL,
                                     OnTour = B.OnTour,
                                     VenueName = C.VenueName,
                                     VenuID = C.VenueID,
                                     VenueLat = C.VenueLat,
                                     VenueLon = C.VenueLong,
                                     IsDeleted = A.IsDeleted,
                                     CityName = C.VenueCity
                                 }).ToList();
                    }

                    if (_list.Count() > 0)
                    {
                        if (radius != null && radius > 0 && Lat != null && Lat != 0 && Lon != null && Lon != 0) //using the filter
                            _list.RemoveAll(p => CalculateDistance.distance(p.VenueLat.GetDouble(), p.VenueLon.GetDouble(), Lat.GetDouble(), Lon.GetDouble(), _Users.UserLanguage == EUserLanguage.EN.ToString() ? CalculateDistance.Measure.M : CalculateDistance.Measure.K) > radius.GetDouble());

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
                                _list.RemoveAll(x => Convert.ToDateTime(x.Datetime_Local) < fromDate || Convert.ToDateTime(x.Datetime_Local) > toDate);
                            }

                            #region "Commented Code"
                            //var _returnlist = _list.OrderBy(x => Convert.ToDateTime(x.Datetime_Local)).Select(x => new ViewEventsList
                            //{ ArtistID = x.ArtistID, BannerImage_URL = x.BannerImage_URL, ArtistName = x.ArtistName, Datetime_Local = x.Datetime_Local, ImageURL = x.ImageURL, OnTour = x.OnTour, TourDateID = x.TourDateID, VenueName = x.VenueName,  });


                            //_list = _list.OrderBy(p => Convert.ToDateTime(p.Datetime_Local)).Where(x => x.IsDeleted=false).ToList();
                            #endregion

                            // Added by Mukesh
                            _list = _list.OrderBy(p => Convert.ToDateTime(p.Datetime_Local)).Where(p => p.IsDeleted == false).ToList();
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, ViewEventsPreList.GetEventsList(_list), "EventList"));
                }

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

                string _result;

                //Get Cache
                if (_HttpCache.Get(UserID.ToString() + name + Lat.ToString() + Lon.ToString() + radius.ToString() + from + to, out _ViewEventsList) == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, ViewEventsPreList.GetEventsList(_ViewEventsList), "EventList"));
                }

                List<ViewEventsPreList> dbEventsList = new List<ViewEventsPreList>();
                dbEventsList = DbEventListCheck(UserID, id, name, type);

                try
                {

                    if (!String.IsNullOrEmpty(name)) //If searching by typing in the search box
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
                        string _stto = DateTime.ParseExact(DateTime.Now.AddDays(180).ToString("MM/dd/yyyy"), "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None).ToString("yyyy-MM-dd");
                        _concat = _concat + "datetime_local.lte=" + _stto + "&";
                    }

                    #endregion

                    _ViewEventsList = new List<ViewEventsPreList>();


                    //List<dbEventSearchModel> dbLookupList = new List<dbEventSearchModel>();

                    //if (key != "")
                    //dbLookupList = dbLookup[key] as List<dbEventSearchModel>;
                    // else
                    //     dbLookupList.Add(new dbEventSearchModel { id = 0, name = search });

                    var strEncoded = HttpUtility.UrlEncode(name);
                    //var strEncoded = HttpUtility.UrlEncode(name);

                    if (type == EventSearchType.Venue.ToString())
                    {
                        _concat = _concat + "venue.name=" + strEncoded + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&per_page=100&page=1";
                    }
                    else if (type == EventSearchType.Artist.ToString())
                    {
                        //name = name.ToLower().Replace(" ", "-");

                        _concat = _concat + "q=" + strEncoded + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&per_page=100&page=1";
                    }
                    else
                    {
                        _concat = _concat + "q=" + strEncoded + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300&per_page=100&page=1";
                    }

                    //LogHelper.CreateLog(_concat);

                    #region "Commented by Mukesh - 13 Aug 2018"
                    var _Get_Events_ByLat = GetSeatGeakEvent(_concat);

                    if (_Get_Events_ByLat != null)
                    {
                        if (_Get_Events_ByLat.events != null && _Get_Events_ByLat.events.Count > 0)
                        {

                            foreach (SeatGeek3.Event _Event in _Get_Events_ByLat.events.OrderBy(x => x.datetime_local).ToList())
                            {
                                if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _Event.title))
                                {
                                    bool anyLatin = false;
                                    if (_Event.performers.Count() > 0)
                                    {
                                        //anyLatin = true;

                                        foreach (var p in _Event.performers)
                                        {
                                            try
                                            {
                                                if (_ArtistsRepo.Repository.AsQueryable().Any(x => x.ArtistName == p.name))
                                                {
                                                    anyLatin = true;
                                                    break;
                                                }
                                                else if (!_ArtistsRepo.Repository.AsQueryable().Any(x => x.ArtistName == p.name))
                                                {
                                                    anyLatin = CheckSeatGeekLatin(p.name, _unitOfWork);
                                                }
                                                else
                                                { }
                                            }
                                            catch (Exception ex)
                                            {

                                            }

                                        }
                                    }

                                    if (anyLatin || _Event.performers.Count() == 0)
                                    {
                                        var localtime = Convert.ToDateTime(_Event.datetime_local);

                                        if (localtime > DateTime.Now)
                                        {
                                            var ddate = Convert.ToDateTime(_Event.datetime_local).ToString("d");

                                            if (!dbEventsList.Any(x => x.TourDateID == _Event.id || x.Datetime_Local == ddate))
                                            {
                                                var localDateString = localtime.ToString("MM/dd");
                                                var eventName = _Event.title;
                                                if (eventName.Length > 38)
                                                    eventName = eventName.Substring(0, 38) + "...";

                                                _ViewEventsList.Add(new ViewEventsPreList
                                                {
                                                    TourDateID = _Event.id,
                                                    ArtistName = eventName,
                                                    Datetime_dt =Convert.ToDateTime(ddate),
                                                    VenuID = _Event.venue.id,
                                                    VenueName = _Event.venue.name,
                                                    OnTour = true,
                                                    ImageURL = "",
                                                    BannerImage_URL = "",
                                                    ArtistID = 0,
                                                    IsDeleted = false

                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    #endregion


                    #endregion


                    if (_ViewEventsList != null && _ViewEventsList.Count() > 50)
                        _ViewEventsList = _ViewEventsList.Take(50).ToList();

                    //_ViewEventLookup = new List<SeatGeek3.ViewEventLookup>();

                    if (dbEventsList != null && dbEventsList.Count() > 0)
                    {
                        _ViewEventsList.InsertRange(0, dbEventsList);//   .Union(_ViewDBLookup).OrderBy(x=>x.TourID.GetInt()).GroupBy(x => x.TourID);

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

                            /*var _returnlist = _list.OrderBy(x => Convert.ToDateTime(x.Datetime_Local)).Select(x => new ViewEventsList
                            { ArtistID = x.ArtistID, BannerImage_URL = x.BannerImage_URL, ArtistName = x.ArtistName, Datetime_Local = x.Datetime_Local, ImageURL = x.ImageURL, OnTour = x.OnTour, TourDateID = x.TourDateID, VenueName = x.VenueName,  });*/

                            _ViewEventsList = _ViewEventsList.OrderBy(p => Convert.ToDateTime(p.Datetime_Local)).ToList();

                        }
                    }


                    //_ViewEventsList = _ViewEventsList.OrderBy(x => Convert.ToDateTime(x.Datetime_Local)).ToList();
                    //Save cache
                    if (_ViewEventsList.Count > 0)
                    {
                        _HttpCache.Set(UserID.ToString() + name + Lat.ToString() + Lon.ToString() + radius.ToString() + from + to, _ViewEventsList, 24);
                    }

                    var _listing = ViewEventsPreList.GetEventsList(_ViewEventsList);

                    if (_listing.Count > 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _listing, "EventList"));
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

            //LogHelper.CreateLog(_result);

            //_result = "";
            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
            dynamic obj = null;

            // deserializing 
            return JsonConvert.DeserializeObject<SeatGeek3.Get_Events_ByLat>(_result);
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
        
        
        [HttpGet]
        [Route("api/Ticketing/GetEventByID")]
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
                            #region "Add Venue Information"

                            SeatGeek4.Venue _Venue = _Get_Events_ByID.venue;

                            _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                           where (A.SeatGeek_VenuID == _Venue.id.ToString())
                                           select A).FirstOrDefault();

                            //search the venue using fuzzy searching
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

                                                    //if (!isLatin)
                                                    //    isLatin = CheckMusicGraphLatin(_Datum.id, _unitOfWork);
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

                                                        await GetProfileImageFromSpotifyFeed(_Artists);

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
                                                                catch (Exception)
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

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.InvalidSearchCriteria, "EventsDetail"));
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "EventsDetail"));
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

            _ViewEventDetail.Event_Description = string.Empty;
            _ViewEventDetail.Organizer_Name = string.Empty;
            _ViewEventDetail.Organizer_Description = string.Empty;

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


        [HttpPost]
        [Route("api/Ticketing/UpdateIsGoing")]
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
        
        #region "Get Number of Peoples Going"
        [HttpGet]
        [Route("api/Ticketing/GetPeopleGoing")]
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
                                           select A).OrderByDescending(p => p.id).Take(5).ToList();

                        var _Artistrelated = _ArtistRelatedRepo.Repository.GetAll(p => p.ArtistID == vArtists.ArtistID).Select(p => p.RelatedID).ToList();

                        if (_Artistrelated != null)
                        {
                            _ArtistRelatedRepo.Repository.DeletePermanent(_Artistrelated);
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
                                    isLatin = CheckLastResortSpotifyGenre(_related.spotify_id);
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
                                    _ArtistRelatedEntity.RecordStatus = RecordStatus.MusicGraph.ToString();

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

        [HttpPost]
        [Route("api/Ticketing/UpdateTrackEvent")]
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
        #endregion

        #region "Insert/Update Event"
        [AllowAnonymous]
        [HttpPost]
        [Route("api/Ticketing/updateEvent")]
        public AdminResponse updateEvent()
        {
            _unitOfWork.StartTransaction();

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

                    _unitOfWork.Commit();

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

                    _unitOfWork.Commit();

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
        #endregion

        #region "Get User Events"
        [AllowAnonymous]
        [Route("TicketingAppAPI/GetUserEvents")]
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
                    _list = _list.Where(p => p.UserName.IndexOf(sUserName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (!string.IsNullOrEmpty(sEmail))
                {
                    _list = _list.Where(p => p.Email.IndexOf(sEmail.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (!string.IsNullOrEmpty(sArtistName))
                {
                    _list = _list.Where(p => p.ArtistName.IndexOf(sArtistName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (!string.IsNullOrEmpty(sVenueName))
                {
                    _list = _list.Where(p => p.VenueName.IndexOf(sVenueName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
                }

                ////Sorting
                //if (!string.IsNullOrEmpty(sortColumn))
                //{
                //    _list = _list.OrderBy("" + sortColumn + " " + sortOrder);
                //}

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
        #endregion

       
    }
}
