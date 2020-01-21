using Musika.Enums;
using Musika.Library.CacheProvider;
using Musika.Library.JsonConverter;
using Musika.Library.Search;
using Musika.Library.Utilities;
using Musika.Models;
using Musika.Models.API.Input;
using Musika.Models.API.View;
using Musika.Repository.GRepository;
using Musika.Repository.Interface;
using Musika.Repository.UnitofWork;
using MusikaDataService.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyWebAPI;
using SpotifyWebAPI.Web.Enums;
using SpotifyWebAPI.Web.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Configuration;
using System.Web.Script.Serialization;


namespace MusikaDataService
{

    // ArtistName
    // Serach Event List
    // SearchVenue

    public partial class Service1 : ServiceBase
    {
        private readonly IUnitOfWork _unitOfWork;
        Timer timer = new Timer(); // name space(using System.Timers;) 
        MusikaEntities db = new MusikaEntities();
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
                                    "Pachanga","Pagode","Pambiche","Pasillo","Payada","Plena","Porro","Pop in spanish","Punto guajiro","Ranchera","Rasin","Reggaeton","Rondalla","Rumba","Salsa","Samba",
                                    "Sertanejo","Seis","Son","Son jalisciense","Son Jarocho","Son montuno","Songo","Tango",
                                    "Tango Tejano","Timba","Tonada","Trío romántico","Tropicália","Tropicalia","Twoubadou",
                                    "Vallenato","Vals criollo","World","Zouk"};
        double _FuzzySearchCri = 0.33;
        public Service1()
        {
            InitializeComponent();
            _unitOfWork = new UnitOfWork();
            var data = PagingforArtist();
            List<MusicGraph.ArtistList> artl = GetArtistByName(data);
            var eventdata = GetEventsSearchResultforcache();
            GetEventByIDAsync(eventdata);
        }

        protected override void OnStart(string[] args)
        {

            WriteToFile("Service is started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = (24 * 60 * 60 * 1000);//number in miliseconds  
            timer.Enabled = true;

            // Get Artists List and Save to Local Database

        }

        #region "Get Artist Name"
        public List<MusicGraph.ArtistList> GetArtistByName(RootObject SeatGeekArtist)
        {
            //changed
            try
            {
                List<Artists> lstArtist = new List<Artists>();
                HttpCache _HttpCache = new HttpCache();
                List<MusicGraph.ArtistList> _ArtistList = new List<MusicGraph.ArtistList>();
                GenericRepository<Musika.Models.ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<Musika.Models.ArtistsNotLatin>(_unitOfWork);
                List<MusicGraph.ArtistList> _ArtistListDB = new List<MusicGraph.ArtistList>();

                Task<List<MusicGraph.Datum>> _MasterList = GetArtists(1, 0, "a");
                bool isLatin = false;
                int pageSize = 20;//_MasterList.meta.per_page;
                int pageIndex = 0;
                string _result = "";
                string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
                int count = SeatGeekArtist.performers.Count;
                int artistlitscount = count - pageSize;
                foreach (var val in SeatGeekArtist.performers)
                {
                    try
                    {

                        pageIndex = 0;
                        var artistName = val.name.Replace("#", "");
                        //if (artistName.Contains(" "))
                        //{
                        //    artistName = artistName.Replace("The ", "");
                        //    var firstSpaceIndex = artistName.IndexOf(" ");
                        //    artistName = artistName.Substring(0, firstSpaceIndex);
                        //    pageSize = 20;
                        //}
                        //else
                        //{
                        //    pageSize = 10;
                        //}
                        var list = GetArtists(pageSize, pageIndex, artistName);
                        if (list.Result != null & list.Result.Count > 0)
                        {
                            _MasterList.Result.Clear();

                            _MasterList.Result.AddRange(list.Result);

                            ImportData(ref _ArtistList, _ArtistNotLatinRepo, _ArtistListDB, ref pageIndex, _MasterList, ref isLatin);
                            //}
                        }
                        else
                        {
                            // break;
                        }

                    }
                    catch (Exception ex)
                    {
                        WriteToFile("GetArtistByName " + ex.StackTrace + " Message=" + ex.Message);
                        count = 0;
                    }
                }


                //// deserializing 
                //var _Search_ByName = JsonConvert.DeserializeObject<MusicGraph.Search_ByName>(_result);



                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, Response.InvalidArtistName, "Artist"));
                return _ArtistList;
            }
            catch (Exception ex)
            {
                WriteToFile("GetArtistByName " + ex.StackTrace + " Message=" + ex.Message);
                //LogHelper.CreateLog3(ex, Request);
                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Exception, ex.StackTrace, "Artist"));
                return null;
            }


        }


        private bool SeatGeek_CheckLatinGenre_Asyn(string artistName, IUnitOfWork vUnitOfWork)
        {

            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });

            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(vUnitOfWork);
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(vUnitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(vUnitOfWork);

            GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(vUnitOfWork);
            GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(vUnitOfWork);
            GenericRepository<Musika.Models.Genre> _GenreRepo = new GenericRepository<Musika.Models.Genre>(vUnitOfWork);

            string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();

            string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

            string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
            string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];

            HttpWebRequest httpWebRequest;
            HttpWebResponse httpResponse;
            string _result;
            SeatGeek.Get_Performers _Get_Performers = null;


            bool isLatin = false;


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

            }
            catch (Exception ex)
            {
                WriteToFile("SeatGeek_CheckLatinGenre_Asyn " + ex.StackTrace + " Message=" + ex.Message);
                return false;
            }
            #endregion


            return isLatin;
        }

        private void ImportData(ref List<MusicGraph.ArtistList> _ArtistList, GenericRepository<Musika.Models.ArtistsNotLatin> _ArtistNotLatinRepo, List<MusicGraph.ArtistList> _ArtistListDB, ref int pageIndex, Task<List<MusicGraph.Datum>> _Search_ByName, ref bool isLatin)
        {
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
            Artists _Artists = null;
            string _MG_Artist_ID = null;
            string spotifyID = "";

            foreach (var _datum in _Search_ByName.Result)
            {
                if (_datum.name.Length < 200)
                {
                    try
                    {
                        if (_datum.IsLatin)
                        {

                        }
                        SeatGeek4.Performer performer = new SeatGeek4.Performer();
                        //_unitOfWork.StartTransaction();
                        myArtistName _name = new myArtistName();
                        _name.Name = _datum.name;

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

                                spotifyID = _datum.id;

                                _MG_Artist_ID = _datum.id;
                                //  break;
                            }
                        }

                        if (!isLatin)
                        {
                            try
                            {
                                isLatin = SeatGeek_CheckLatinGenre_Asyn(_name.Name, _unitOfWork);

                                if (!isLatin)
                                {
                                    isLatin = CheckLastResortSpotifyGenre(spotifyID);

                                    if (!isLatin)
                                    {
                                        if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _name.Name))
                                        {
                                            // _ArtistNotLatinRepo.Repository.Add(new ArtistsNotLatin() { ArtistName = _name.Name });
                                            db.ArtistsNotLatin.Add(new ArtistsNotLatin() { ArtistName = _name.Name });
                                            db.SaveChanges();
                                            //   _unitOfWork.Commit();
                                            //_unitOfWork.StartTransaction();
                                        }

                                        _MG_Artist_ID = null;
                                        //  _unitOfWork.RollBack();
                                        //continue;
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                WriteToFile("ImportData " + ex.StackTrace + " Message=" + ex.Message);
                                throw ex;
                            }

                        }


                        #region "Add New Rec"
                        //Start- Saving New Data 
                        if (_Artists == null)
                        {
                            isLatin = false;

                            //if (!isLatin)
                            //    isLatin = CheckMusicGraphLatin(_Datum.id, _unitOfWork);
                            if (!isLatin)
                                isLatin = CheckSeatGeekLatin(_datum.name, _unitOfWork);
                            if (!isLatin)
                                isLatin = CheckLastResortSpotifyGenre(_datum.id);

                            if (!isLatin)
                            {
                                //_unitOfWork.RollBack();
                                //return;
                            }
                            if (isLatin)
                            {
                                //   _unitOfWork.StartTransaction();
                                // _Artists.OnTour = _performer.has_upcoming_events == true ? true : false;

                                _Artists = new Artists();
                                _Artists.ArtistName = _datum.name;// Regex.Replace(_Search_ByID.data.name, "[^A-Za-z0-9 _]", "");

                                // _Artists.Gender = _Search_ByID.data.gender;
                                // _Artists.Decade = _Search_ByID.data.decade;

                                _Artists.Main_Genre = _datum.main_genre;

                                //_Artists.Musicgraph_ID = !String.IsNullOrEmpty(_Search_ByID.data.id) ? _Search_ByID.data.id : _Search_ByID.data.name;
                                //_Artists.Artist_Ref_ID = _Search_ByID.data.artist_ref_id;
                                //_Artists.Musicbrainz_ID = _Search_ByID.data.musicbrainz_id;

                                _Artists.Spotify_ID = _datum.id;

                                //_Artists.Youtube_ID = _Search_ByID.data.youtube_id;
                                //_Artists.Alternate_Names = _Search_ByID.data.alternate_names != null && _Search_ByID.data.alternate_names.Count > 0 ? _Search_ByID.data.alternate_names[0] : "";

                                _Artists.RecordStatus = RecordStatus.Spotify.ToString();
                                _Artists.CreatedDate = DateTime.Now;
                                _Artists.ModifiedDate = DateTime.Now;
                                //    _Artists.ImageURL = _datum.Image;
                                //_ArtistsRepo.Repository.Add(_Artists);

                                #region "SeatGeek Api Implementation"
                                SeatGeek.Performer checkSeatID = CheckExistenceOfSeatGeekArtistName(_Artists, _unitOfWork, true, true);
                                Musika.Models.Artists _existArtist = new Musika.Models.Artists();
                                #endregion
                                if (checkSeatID != null)
                                {
                                    string seatGeekID = checkSeatID.id.ToString();
                                    _existArtist = db.Artists.Where(s => s.Seatgeek_ID == seatGeekID).SingleOrDefault();
                                }
                                if (_existArtist == null)
                                {
                                    db.Artists.Add(_Artists);
                                    db.SaveChanges();
                                    //_unitOfWork.Commit();

                                    #region "Get profile picture"

                                    string _ImageFromSpotify = GetProfileImageFromSpotifyFeed(_Artists);

                                    SpotifyImage_Asyn_Operation(_Artists, _ImageFromSpotify);

                                    #endregion

                                    #region "Get Similar Artists (dont need this block while just updating the records)"
                                    //Task<Dictionary<string,object>> _GetSimilarArtists_ByID = MusicGrapgh_GetSimilarArtists_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);
                                    Task<List<MusicGraph.Datum>> _GetSimilarArtists_ByID = Spotify_GetSimilarArtistByID(_Artists.Spotify_ID);
                                    #endregion

                                    #region "SeatGeek Api Implementation"
                                    Task<Dictionary<string, object>> _seatGeek = SeatGeek_GetEventByArtistName_Asyn(0, _Artists, _unitOfWork, true, true);
                                    _seatGeek.Wait();
                                    #endregion

                                    #region "Eventful API Implementation"
                                    Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(0, _Artists, _unitOfWork, true);
                                    _Eventful.Wait();
                                    #endregion

                                    #region "Instagram HashTag Images"
                                    Task<Dictionary<string, object>> _ImagesFromInstagram = GetImagesFromInstagramFeed(_Artists);
                                    #endregion


                                    #region "Spotify Api Implementation
                                    Task<Dictionary<string, object>> _Spotify = Spotify_GetSongInfo_Asyn(0, _Artists, _unitOfWork, true);
                                    #endregion

                                    _GetSimilarArtists_ByID.Wait();


                                    //_Instagram.Wait();
                                    //_Spotify.Wait();

                                    //                 _unitOfWork.StartTransaction();

                                    if (_TourDateRepo.Repository.GetAll(x => x.ArtistID == _Artists.ArtistID && x.Datetime_Local >= DateTime.Now).Count > 0)
                                    {
                                        _Artists.OnTour = true;
                                    }

                                    //string MusicGraphBio = "";
                                    //if (!String.IsNullOrEmpty(_Artists.Musicgraph_ID))
                                    //    MusicGraphBio = GetMusicGraphBio(_Artists.Musicgraph_ID);
                                    //if (!String.IsNullOrEmpty(MusicGraphBio))
                                    //    _Artists.About = MusicGraphBio;

                                    SeatGeek_Asyn_Operation(0, _Artists, _unitOfWork, _seatGeek);
                                    EventFul_Asyn_Operation(0, _Artists, _unitOfWork, _Eventful.Result, false);
                                    //Instagram_Asyn_Operation(input.UserID, _Artists, null, _unitOfWork, null, _Instagram);
                                    ImagesFromInstagram_Asyn_Operation(_Artists, _ImagesFromInstagram);
                                    Spotify_GetSongInfo_Asyn_Operation(0, _Artists, _unitOfWork, _Spotify);


                                    SpotifyImage_Asyn_Operation(_Artists, _ImageFromSpotify);

                                    Spotify_GetSimilarArtistByID_Operation(_Artists, _unitOfWork, _GetSimilarArtists_ByID.Result);


                                    if (String.IsNullOrEmpty(_Artists.ImageURL))
                                    {
                                        if (!string.IsNullOrEmpty(_ImageFromSpotify))
                                        {
                                            _Artists.ImageURL = _ImageFromSpotify;
                                            _Artists.ThumbnailURL = _ImageFromSpotify;
                                            _Artists.BannerImage_URL = _ImageFromSpotify;
                                        }
                                    }

                                    //_ArtistsRepo.Repository.Update(_Artists);
                                    db.Entry(_Artists).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                //   _unitOfWork.Commit();
                            }//End- Saving New Data 
                        }
                        else
                        {
                            _Artists.ModifiedDate = DateTime.Now;
                            // _Artists.OnTour = _performer.has_upcoming_events == true ? true : false;
                            string _ImageFromSpotify = GetProfileImageFromSpotifyFeed(_Artists);

                            SpotifyImage_Asyn_Operation(_Artists, _ImageFromSpotify);

                            #endregion

                            #region "Get Similar Artists (dont need this block while just updating the records)"
                            //Task<Dictionary<string,object>> _GetSimilarArtists_ByID = MusicGrapgh_GetSimilarArtists_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);
                            Task<List<MusicGraph.Datum>> _GetSimilarArtists_ByID = Spotify_GetSimilarArtistByID(_Artists.Spotify_ID);
                            #endregion

                            #region "SeatGeek Api Implementation"
                            Task<Dictionary<string, object>> _seatGeek = SeatGeek_GetEventByArtistName_Asyn(0, _Artists, _unitOfWork, true, true);
                            _seatGeek.Wait();
                            #endregion

                            #region "Eventful API Implementation"
                            Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(0, _Artists, _unitOfWork, true);
                            _Eventful.Wait();
                            #endregion

                            #region "Instagram HashTag Images"
                            Task<Dictionary<string, object>> _ImagesFromInstagram = GetImagesFromInstagramFeed(_Artists);
                            #endregion


                            #region "Spotify Api Implementation
                            Task<Dictionary<string, object>> _Spotify = Spotify_GetSongInfo_Asyn(0, _Artists, _unitOfWork, true);
                            #endregion

                            _GetSimilarArtists_ByID.Wait();


                            //_Instagram.Wait();
                            //_Spotify.Wait();

                            //                 _unitOfWork.StartTransaction();

                            if (_TourDateRepo.Repository.GetAll(x => x.ArtistID == _Artists.ArtistID && x.Datetime_Local >= DateTime.Now).Count > 0)
                            {
                                _Artists.OnTour = true;
                            }

                            //string MusicGraphBio = "";
                            //if (!String.IsNullOrEmpty(_Artists.Musicgraph_ID))
                            //    MusicGraphBio = GetMusicGraphBio(_Artists.Musicgraph_ID);
                            //if (!String.IsNullOrEmpty(MusicGraphBio))
                            //    _Artists.About = MusicGraphBio;

                            SeatGeek_Asyn_Operation(0, _Artists, _unitOfWork, _seatGeek);
                            EventFul_Asyn_Operation(0, _Artists, _unitOfWork, _Eventful.Result, false);
                            //Instagram_Asyn_Operation(input.UserID, _Artists, null, _unitOfWork, null, _Instagram);
                            ImagesFromInstagram_Asyn_Operation(_Artists, _ImagesFromInstagram);
                            Spotify_GetSongInfo_Asyn_Operation(0, _Artists, _unitOfWork, _Spotify);


                            SpotifyImage_Asyn_Operation(_Artists, _ImageFromSpotify);

                            Spotify_GetSimilarArtistByID_Operation(_Artists, _unitOfWork, _GetSimilarArtists_ByID.Result);


                            if (String.IsNullOrEmpty(_Artists.ImageURL))
                            {
                                if (!string.IsNullOrEmpty(_ImageFromSpotify))
                                {
                                    _Artists.ImageURL = _ImageFromSpotify;
                                    _Artists.ThumbnailURL = _ImageFromSpotify;
                                    _Artists.BannerImage_URL = _ImageFromSpotify;
                                }
                            }

                            //_ArtistsRepo.Repository.Update(_Artists);
                            db.Entry(_Artists).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }

                        #endregion

                    }
                    catch (Exception ex)
                    {
                        //((System.Data.Entity.Validation.DbEntityValidationException)ex).EntityValidationErrors.

                        WriteToFile("ImportData " + ex.StackTrace + " Message=" + ex.Message);
                    }
                }
            }

        }

        public void Spotify_GetSimilarArtistByID_Operation(Artists vArtists, IUnitOfWork vUnitOfWork, List<MusicGraph.Datum> apiResponse)
        {
            try
            {
                GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(vUnitOfWork);
                GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(vUnitOfWork);

                ArtistRelated _ArtistRelatedEntity = null;

                List<MusicGraph.Datum> _GetSimilarArtists_ByID = apiResponse;
                if (_GetSimilarArtists_ByID != null)
                {

                    if (_GetSimilarArtists_ByID.Count > 0)
                    {
                        var _similarlst = (from A in _GetSimilarArtists_ByID
                                           select A).OrderByDescending(p => p.id).ToList();


                        var _Artistrelated = _ArtistRelatedRepo.Repository.GetAll(p => p.ArtistID == vArtists.ArtistID).Select(p => p.RelatedID).ToList();

                        if (_Artistrelated != null)
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
                                    isLatin = SeatGeek_CheckLatinGenre_Asyn(_related.name, _unitOfWork);
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
                                    _ArtistRelatedEntity.RecordStatus = RecordStatus.Spotify.ToString();

                                    //_ArtistRelatedRepo.Repository.Add(_ArtistRelatedEntity);
                                    db.ArtistRelated.Add(_ArtistRelatedEntity);
                                    db.SaveChanges();
                                }
                                else
                                {

                                    if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _related.name))
                                    {
                                        //_ArtistNotLatinRepo.Repository.Add(new ArtistsNotLatin() { ArtistName = _related.name });
                                        db.ArtistsNotLatin.Add(new ArtistsNotLatin() { ArtistName = _related.name });
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToFile("Spotify_GetSimilarArtistByID_Operation " + ex.StackTrace + " Message=" + ex.Message);
            }
        }

        public void SpotifyImage_Asyn_Operation(Artists vArtists, string ImageUrl)
        {
            GenericRepository<Artists> _ArtistRepository = new GenericRepository<Artists>(_unitOfWork);
            try
            {
                if (!string.IsNullOrEmpty(ImageUrl))
                {
                    bool _ArtistPic = SpotifyProfilePicture(0, vArtists, _ArtistRepository, true, ImageUrl);
                }
            }
            catch (Exception ex)
            {
                WriteToFile("SpotifyImage_Asyn_Operation " + ex.StackTrace + " Message=" + ex.Message);

            }
        }

        private Task<List<MusicGraph.Datum>> GetArtists(int pageSize, int pageIndex, string val)
        {
            Task<List<MusicGraph.Datum>> _Search_ByName = Task.Run(async () => await Spotify_SearchArtist(val, pageSize, pageIndex));
            _Search_ByName.Wait();
            return _Search_ByName;
        }
        private void SeatGeek_Asyn_Operation(Int32 vUserID, Artists vArtists, IUnitOfWork vUnitOfWork, Task<Dictionary<string, object>> apiResponse, bool vScan = false)
        {
            GenericRepository<Artists> _ArtistRepo = new GenericRepository<Artists>(vUnitOfWork);
            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(vUnitOfWork);
            GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(vUnitOfWork);
            GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(vUnitOfWork);

            GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(vUnitOfWork);
            GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(vUnitOfWork);
            GenericRepository<Musika.Models.Genre> _GenreRepo = new GenericRepository<Musika.Models.Genre>(vUnitOfWork);

            string _Performer_ID = null;
            string _strEvent = null;

            Venue _VenuEntity = null;
            TourDate _TourDateEntity = null;
            UserTourDate _UserTourDate = null;
            ArtistGenre _ArtistGenre = null;
            Musika.Models.Genre _Genre = null;

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
                                            _Genre = new Musika.Models.Genre();
                                            _Genre.Name = _Ev.name;
                                            _Genre.CreatedDate = DateTime.Now;
                                            _Genre.RecordStatus = RecordStatus.SeatGeek.ToString();
                                            //_GenreRepo.Repository.Add(_Genre);
                                            db.Genre.Add(_Genre);
                                            db.SaveChanges();

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

                                            // _ArtistGenreRepo.Repository.Add(_ArtistGenre);
                                            db.ArtistGenre.Add(_ArtistGenre);
                                            db.SaveChanges();
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
                                        WriteToFile("SeatGeek_Asyn_Operation " + ex.StackTrace + " Message=" + ex.Message);
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

                                    _VenuEntity.CreatedDate = DateTime.Now;
                                    _VenuEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                    //_VenueRepo.Repository.Add(_VenuEntity);
                                    db.Venue.Add(_VenuEntity);
                                    db.SaveChanges();

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


                                    //_TourDateRepo.Repository.Add(_TourDateEntity);
                                    db.TourDate.Add(_TourDateEntity);
                                    db.SaveChanges();

                                    //if (vScan == true)
                                    //{
                                    //    _UserTourDate = new UserTourDate();
                                    //    _UserTourDate.TourDateID = _TourDateEntity.TourDateID;
                                    //    _UserTourDate.UserID = vUserID;
                                    //    _UserTourDate.CreatedDate = DateTime.Now;
                                    //    _UserTourDate.RecordStatus = RecordStatus.Active.ToString();

                                    //    //_UserTourDateRepo.Repository.Add(_UserTourDate);
                                    //    db.UserTourDate.Add(_UserTourDate);
                                    //    db.SaveChanges();
                                    //}

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
                                    _TourDateEntity.IsDeleted = false;
                                    _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Ev.datetime_local);

                                    _TourDateEntity.ModifiedDate = DateTime.Now;

                                    if (!String.IsNullOrEmpty(_Ev.url))
                                        _TourDateEntity.TicketURL = _Ev.url;
                                    else if (_Ev.venue != null && !String.IsNullOrEmpty(_Ev.venue.url))
                                        _TourDateEntity.TicketURL = _Ev.venue.url;
                                    else
                                        _TourDateEntity.TicketURL = "https://seatgeek.com/";


                                    //_TourDateRepo.Repository.Update(_TourDateEntity);
                                    db.Entry(_TourDateEntity).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();


                                    //if (vScan == true)
                                    //{
                                    //    _UserTourDate = _UserTourDateRepo.Repository.Get(p => p.UserID == vUserID && p.TourDateID == _TourDateEntity.TourDateID);

                                    //    if (_UserTourDate == null)
                                    //    {
                                    //        _UserTourDate = new UserTourDate();
                                    //        _UserTourDate.TourDateID = _TourDateEntity.TourDateID;
                                    //        _UserTourDate.UserID = vUserID;
                                    //        _UserTourDate.CreatedDate = DateTime.Now;
                                    //        _UserTourDate.RecordStatus = RecordStatus.Active.ToString();

                                    //       // _UserTourDateRepo.Repository.Add(_UserTourDate);
                                    //        db.UserTourDate.Add(_UserTourDate);
                                    //        db.SaveChanges();
                                    //    }
                                    //}

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
                                                _Genre = new Musika.Models.Genre();
                                                _Genre.Name = _Ev.name;
                                                _Genre.CreatedDate = DateTime.Now;
                                                _Genre.RecordStatus = RecordStatus.SeatGeek.ToString();
                                                //_GenreRepo.Repository.Add(_Genre);
                                                db.Genre.Add(_Genre);
                                                db.SaveChanges();

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

                                                //_ArtistGenreRepo.Repository.Add(_ArtistGenre);
                                                db.ArtistGenre.Add(_ArtistGenre);
                                                db.SaveChanges();
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
                                            WriteToFile("SeatGeek_Asyn_Operation " + ex.StackTrace + " Message=" + ex.Message);
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
                                    WriteToFile("SeatGeek_Asyn_Operation " + ex.StackTrace + " Message=" + ex.Message);
                                }
                                #endregion
                            }


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

                                    _VenuEntity.CreatedDate = DateTime.Now;
                                    _VenuEntity.RecordStatus = RecordStatus.SeatGeek.ToString();

                                    //_VenueRepo.Repository.Add(_VenuEntity);
                                    db.Venue.Add(_VenuEntity);
                                    db.SaveChanges();

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


                                    //_TourDateRepo.Repository.Add(_TourDateEntity);
                                    db.TourDate.Add(_TourDateEntity);
                                    db.SaveChanges();
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

                                    // _TourDateRepo.Repository.Update(_TourDateEntity);
                                    db.Entry(_TourDateEntity).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();

                                }



                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToFile("SeatGeek_Asyn_Operation " + ex.StackTrace + " Message=" + ex.Message);

            }
        }





        public bool ImportVenue(SeatGeek3.Get_Venues venue)
        {
            Venue _VenuEntity = null;
            GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
            try
            {
                foreach (var val in venue.venues)
                {

                    _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                   where (A.SeatGeek_VenuID == val.id.ToString())
                                   select A).FirstOrDefault();
                    if (_VenuEntity == null)
                    {
                        _VenuEntity = new Venue();
                        _VenuEntity.Eventful_VenueID = "";// _Get_Event_ByID.venue_id.ToString();
                        _VenuEntity.SeatGeek_VenuID = val.id.ToString();

                        _VenuEntity.VenueName = val.name;
                        _VenuEntity.Extended_Address = val.address;
                        _VenuEntity.VenueCountry = val.country;
                        _VenuEntity.Display_Location = val.display_location;
                        _VenuEntity.Slug = val.slug;
                        _VenuEntity.VenueState = val.state;
                        _VenuEntity.Postal_Code = val.postal_code;
                        _VenuEntity.VenueCity = val.city;
                        _VenuEntity.Address = val.address;
                        _VenuEntity.Timezone = val.timezone;

                        _VenuEntity.VenueLat = Convert.ToDecimal(val.location.lat);
                        _VenuEntity.VenueLong = Convert.ToDecimal(val.location.lon);

                        _VenuEntity.CreatedDate = DateTime.Now;
                        _VenuEntity.RecordStatus = RecordStatus.Eventful.ToString();

                        //_VenueRepo.Repository.Add(_VenuEntity);
                        db.Venue.Add(_VenuEntity);
                        db.SaveChanges();

                    }
                }
            }
            catch (Exception ex)
            {
                WriteToFile("ImportVenue " + ex.StackTrace + " Message=" + ex.Message);
            }
            return true;
        }

        public async Task<bool> GetEventByIDAsync(SeatGeekEventResponse.RootObject EventTourData)
        {
            try
            {

                // string strThumbnailURLfordb = null;
                // string strIamgeURLfordb = null;
                // string strTempImageSave = null;

                string tempfilePath = WebConfigurationManager.AppSettings["SiteImgPath"].ToString() + @"\" + "TempImages" + @"\";

                string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];


                string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
                string _Instagram_access_token = ConfigurationManager.AppSettings["instagram.access_token"].ToString();

                string _Eventful_app_key = ConfigurationManager.AppSettings["Eventful_app_key"].ToString();

                string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();
                string _SeatGeek_client_secret = ConfigurationManager.AppSettings["SeatGeek_client_secret"].ToString();

                string _Spotify_Country = ConfigurationManager.AppSettings["Spotify_Country"].ToString();

                //string _result;

                string _MusicGraph_ID = null;
                // string _Performer_ID = null;
                // string _strEvent = null;

                Venue _VenuEntity = null;
                TourDate _TourDateEntity = null;
                //ArtistRelated _ArtistRelatedEntity = null;
                Artists _Artists = null;
                TourPerformers _TourPerformers = null;

                ArtistGenre _ArtistGenre = null;
                Musika.Models.Genre _Genre = null;

                // string _MG_Artist_ID = null;

                //_unitOfWork.StartTransaction();

                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);
                GenericRepository<ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<ArtistPhotos>(_unitOfWork);
                GenericRepository<ArtistRelated> _ArtistRelatedRepo = new GenericRepository<ArtistRelated>(_unitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(_unitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(_unitOfWork);
                GenericRepository<TourPerformers> _TourPerformersRepo = new GenericRepository<TourPerformers>(_unitOfWork);
                GenericRepository<ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<ArtistsNotLatin>(_unitOfWork);
                GenericRepository<ArtistGenre> _ArtistGenreRepo = new GenericRepository<ArtistGenre>(_unitOfWork);
                GenericRepository<Musika.Models.Genre> _GenreRepo = new GenericRepository<Musika.Models.Genre>(_unitOfWork);


                var serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
                dynamic obj = null;

                foreach (var val in EventTourData.events)
                {
                    //Check Tour Date in Local DB
                    //try
                    //{
                    //    Int64 _TourID = Convert.ToInt64(val.id);
                    //    _TourDateEntity = _TourDateRepo.Repository.Get(p => p.TourDateID == _TourID);
                    //}
                    //catch { }


                    if (_TourDateEntity == null)
                    {
                        _TourDateEntity = _TourDateRepo.Repository.Get(p => p.SeatGeek_TourID == val.id.ToString());
                    }


                    if (_TourDateEntity != null)
                    {
                        TimeSpan span = DateTime.Now - Convert.ToDateTime(_TourDateEntity.ModifiedDate);
                        if (span.Hours < 24 && span.Days == 0)
                        {
                            //_unitOfWork.RollBack();
                            // GetTourDetail_ByID(_TourDateEntity);
                            //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success,  "EventsDetail"));
                        }
                    }
                    try
                    {
                        if (_TourDateEntity == null)
                        {
                            #region "Get SeatGeek Event By ID"
                            var _Get_Events_ByID = await SeatGeek_GetEventByID_Asyn(_TourDateEntity == null ? val.id.ToString() : _TourDateEntity.SeatGeek_TourID);//await for the function to be completed.
                            if (_Get_Events_ByID != null)
                            {
                                #region "Add Venu Information"

                                SeatGeek4.Venue _Venue = _Get_Events_ByID.venue;

                                //Venu information
                                //_VenuEntity = _VenueRepo.Repository.Get(p => p.SeatGeek_VenuID == _Venue.id.ToString());

                                _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                               where (A.SeatGeek_VenuID == _Venue.id.ToString())
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

                                    // _VenueRepo.Repository.Add(_VenuEntity);
                                    db.Venue.Add(_VenuEntity);
                                    db.SaveChanges();

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
                                        //var _Search_ByName = await MusicGrapgh_GetArtistByName_Asyn(_perfomer.name.Trim());//await for the function to be completed
                                        var _Search_ByName = await Spotify_SearchArtist(_perfomer.name.Trim(), 5, 1);//await for the function to be completed

                                        if (_Search_ByName.Count > 0 && _Search_ByName != null)// && _Search_ByName.status.message == "Success")
                                        {
                                            bool isLatin = false;
                                            foreach (MusicGraph.Datum _Datum in _Search_ByName)
                                            {
                                                if (RemoveDiacritics(_Datum.name.ToLower()) == _perfomer.name.ToLower())
                                                {
                                                    _MusicGraph_ID = _Datum.id;
                                                    _Artists = _ArtistsRepo.Repository.Get(p => p.Spotify_ID == _MusicGraph_ID);
                                                    #region "Add New"
                                                    if (_Artists == null)
                                                    {
                                                        if (!isLatin)
                                                            isLatin = CheckSeatGeekLatin(_Datum.name, _unitOfWork);
                                                        if (!isLatin)
                                                            isLatin = CheckLastResortSpotifyGenre(_Datum.spotify_id);

                                                        if (!isLatin)
                                                        {
                                                            //_unitOfWork.RollBack();
                                                            // return false;
                                                            //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "Its not a latin artist event", "EventsDetail"));
                                                        }

                                                        if (isLatin)
                                                        {
                                                            _Artists = new Artists();

                                                            _Artists.ArtistName = _Datum.name; // Regex.Replace(_Datum.name, "[^A-Za-z0-9 _]", "");
                                                            _Artists.Gender = _Datum.gender;
                                                            _Artists.Decade = _Datum.decade;
                                                            _Artists.Main_Genre = _Datum.main_genre;

                                                            _Artists.Seatgeek_ID = _perfomer.id.ToString();

                                                            _Artists.Spotify_ID = !String.IsNullOrEmpty(_Datum.id) ? _Datum.id : _Datum.name;
                                                            _Artists.Artist_Ref_ID = _Datum.artist_ref_id;
                                                            _Artists.Musicbrainz_ID = _Datum.musicbrainz_id;
                                                            _Artists.Spotify_ID = _Datum.spotify_id;
                                                            _Artists.Youtube_ID = _Datum.youtube_id;
                                                            _Artists.Alternate_Names = _Datum.alternate_names != null && _Datum.alternate_names.Count > 0 ? _Datum.alternate_names[0] : "";

                                                            _Artists.RecordStatus = RecordStatus.MusicGraph.ToString();
                                                            _Artists.CreatedDate = DateTime.Now;
                                                            _Artists.ModifiedDate = DateTime.Now;

                                                            _Artists.OnTour = _perfomer.has_upcoming_events == true ? true : false;

                                                            //_ArtistsRepo.Repository.Add(_Artists);
                                                            db.Artists.Add(_Artists);
                                                            db.SaveChanges();

                                                            GetProfileImageFromSpotifyFeed(_Artists);

                                                            #region "Loop through the Genre"
                                                            if (_perfomer.genres != null && _perfomer.genres.Count > 0)
                                                            {
                                                                foreach (SeatGeek4.Genre _Ev in _perfomer.genres)
                                                                {
                                                                    _Genre = _GenreRepo.Repository.Get(p => p.Name == _Ev.name.Trim());

                                                                    if (_Genre == null)
                                                                    {
                                                                        _Genre = new Musika.Models.Genre();
                                                                        _Genre.Name = _Ev.name;
                                                                        _Genre.CreatedDate = DateTime.Now;
                                                                        _Genre.RecordStatus = RecordStatus.SeatGeek.ToString();
                                                                        //_GenreRepo.Repository.Add(_Genre);
                                                                        db.Genre.Add(_Genre);
                                                                        db.SaveChanges();

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

                                                                        //_ArtistGenreRepo.Repository.Add(_ArtistGenre);
                                                                        db.ArtistGenre.Add(_ArtistGenre);
                                                                        db.SaveChanges();
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
                                                                                bool _ArtistPic = ArtistProfilePicture_Asyn(0, _Artists, _unitOfWork, true, _perfomer.images.huge);
                                                                            }
                                                                        }
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        WriteToFile("GetEventByIDAsync-6 " + ex.StackTrace + " Message=" + ex.Message);
                                                                    }
                                                                    #endregion

                                                                }

                                                                #region "Get Similar Artists (dont need this block while just updating the records)"
                                                                //Task<Dictionary<string, object>> _GetSimilarArtists_ByID = MusicGrapgh_GetSimilarArtists_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);
                                                                #endregion
                                                                Task<List<MusicGraph.Datum>> _GetSimilarArtists_ByID = Spotify_GetSimilarArtistByID(_Artists.Spotify_ID);
                                                                _GetSimilarArtists_ByID.Wait();

                                                                #region "Get Artist Matrics (dont need this block while just updating the records)"
                                                                //Task<MusicGraph2.ArtistMatrics_ByID> _ArtistMatrics_ByID = MusicGrapgh_GetArtistMatrics_Asyn(_Artists.Musicgraph_ID, _Artists.ArtistID, _unitOfWork);

                                                                //_ArtistMatrics_ByID.Wait(); // wait for the function to complete

                                                                ////Get Instagram ID from the MusicGraph matrcis 
                                                                //if (_ArtistMatrics_ByID.Result.data.instagram != null && _ArtistMatrics_ByID.Result.data.instagram.url != null && _ArtistMatrics_ByID.Result.data.instagram.url != "")
                                                                //{
                                                                //    _Artists.Instagram_Url = _ArtistMatrics_ByID.Result.data.instagram.url;
                                                                //    string _instaGram_ID = _ArtistMatrics_ByID.Result.data.instagram.url.Replace("http:", "https:").Replace("https://instagram.com/", "").Replace("/", "");
                                                                //    _Artists.Instagram_ID = _instaGram_ID;
                                                                //}

                                                                #endregion



                                                                #region "Eventful API Implementation"
                                                                Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(0, _Artists, _unitOfWork, true);
                                                                #endregion



                                                                //#region "Instagram Api Implementation"
                                                                //Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(0, _Artists, _ArtistMatrics_ByID.Result, _unitOfWork, true, _TourDateEntity);
                                                                //#endregion

                                                                #region "Spotify Api Implementation
                                                                Task<Dictionary<string, object>> _Spotify = Spotify_GetSongInfo_Asyn(0, _Artists, _unitOfWork, true);
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
                                                                            bool _ArtistPic = ArtistProfilePicture_Asyn(0, _Artists, _unitOfWork, true, _perfomer.images.huge);
                                                                        }
                                                                    }
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    WriteToFile("GetEventByIDAsync-5 " + ex.StackTrace + " Message=" + ex.Message);
                                                                }
                                                                #endregion
                                                            }

                                                            #region "Instagram Api Implementation"
                                                            Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(0, _Artists, null, _unitOfWork, false, _TourDateEntity);
                                                            #endregion

                                                            #region "Eventful API Implementation"
                                                            Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(0, _Artists, _unitOfWork, false);
                                                            #endregion

                                                            #region "Spotify Api Implementation
                                                            Task<Dictionary<string, object>> _Spotify = Spotify_GetSongInfo_Asyn(0, _Artists, _unitOfWork, false);
                                                            #endregion



                                                        });
                                                    }
                                                    #endregion


                                                    //_TourDateEntity.TicketURL = "8";
                                                    //_TourDateRepo.Repository.Update(_TourDateEntity);
                                                    //_unitOfWork.Commit();
                                                    if (isLatin)
                                                    {
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
                                                                                                                && DbFunctions.TruncateTime(p.Datetime_Local).Value.Day == _datetime_local.Day
                                                                                                                && p.ArtistID == _Artists.ArtistID
                                                                                                                && p.RecordStatus == RecordStatus.Eventful.ToString())
                                                                                                            ) on A.VenueID equals B.VenueID
                                                                               where B.ArtistID == _Artists.ArtistID
                                                                               select B).FirstOrDefault();


                                                            //_TourDateEntity.TicketURL = "8";
                                                            //_TourDateRepo.Repository.Update(_TourDateEntity);
                                                            //_unitOfWork.Commit();

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


                                                                //_TourDateRepo.Repository.Add(_TourDateEntity);
                                                                db.TourDate.Add(_TourDateEntity);
                                                                db.SaveChanges();
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

                                                                // _TourDateRepo.Repository.Update(_TourDateEntity);
                                                                db.Entry(_TourDateEntity).State = System.Data.Entity.EntityState.Modified;
                                                                db.SaveChanges();

                                                            }
                                                            #endregion
                                                        }
                                                        else
                                                        {
                                                            #region "Save Other Artist in Tour Perfromer"
                                                            _TourPerformers = _TourPerformersRepo.Repository.Get(p => p.ArtistID == _Artists.ArtistID && p.TourDateID == _TourDateEntity.TourDateID);

                                                            if (_TourPerformers == null)
                                                            {
                                                                _TourPerformers = new TourPerformers();

                                                                _TourPerformers.TourDateID = _TourDateEntity.TourDateID;
                                                                _TourPerformers.ArtistID = _Artists.ArtistID;
                                                                _TourPerformers.CreatedDate = DateTime.Now;
                                                                _TourPerformers.RecordStatus = RecordStatus.MusicGraph.ToString();

                                                                // _TourPerformersRepo.Repository.Add(_TourPerformers);
                                                                db.TourPerformers.Add(_TourPerformers);
                                                                db.SaveChanges();
                                                            }
                                                            #endregion
                                                        }

                                                        //string MusicGraphBio = "";
                                                        //if (!String.IsNullOrEmpty(_Artists.Musicgraph_ID))
                                                        //    MusicGraphBio = GetMusicGraphBio(_Artists.Musicgraph_ID);
                                                        //if (!String.IsNullOrEmpty(MusicGraphBio))
                                                        //    _Artists.About = MusicGraphBio;

                                                        // _ArtistsRepo.Repository.Update(_Artists);
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        WriteToFile("GetEventByIDAsync-4 " + ex.StackTrace + " Message=" + ex.Message);
                                    }



                                    #endregion

                                    //if Artist not found in Music Graph (Save Direct)
                                    if (string.IsNullOrEmpty(_MusicGraph_ID))// == null)
                                    {
                                        #region "use this block if Artist not found in (MusicGrapgh)"
                                        try
                                        {
                                            _Artists = _ArtistsRepo.Repository.Get(p => p.Seatgeek_ID == _perfomer.id.ToString());
                                            bool isLatin = false;
                                            #region "Add New"
                                            if (_Artists == null)
                                            {


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

                                                    //_ArtistsRepo.Repository.Add(_Artists);
                                                    db.Artists.Add(_Artists);
                                                    db.SaveChanges();

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
                                                                _Genre = new Musika.Models.Genre();
                                                                _Genre.Name = _Ev.name;
                                                                _Genre.CreatedDate = DateTime.Now;
                                                                _Genre.RecordStatus = RecordStatus.SeatGeek.ToString();
                                                                // _GenreRepo.Repository.Add(_Genre);
                                                                db.Genre.Add(_Genre);
                                                                db.SaveChanges();

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

                                                                //_ArtistGenreRepo.Repository.Add(_ArtistGenre);
                                                                db.ArtistGenre.Add(_ArtistGenre);
                                                                db.SaveChanges();
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
                                                                        bool _ArtistPic = ArtistProfilePicture_Asyn(0, _Artists, _unitOfWork, true, _perfomer.images.huge);
                                                                    }
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                WriteToFile("GetEventByIDAsync-3 " + ex.StackTrace + " Message=" + ex.Message);
                                                            }
                                                            #endregion
                                                        }

                                                        #region "Instagram Api Implementation"
                                                        Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(0, _Artists, null, _unitOfWork, true, _TourDateEntity);
                                                        #endregion

                                                        #region "Eventful API Implementation"
                                                        Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(0, _Artists, _unitOfWork, true);
                                                        #endregion

                                                    });
                                                }
                                                else
                                                {

                                                    if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == _perfomer.name))
                                                    {
                                                        // _ArtistNotLatinRepo.Repository.Add(new ArtistsNotLatin() { ArtistName = _perfomer.name });
                                                        db.ArtistsNotLatin.Add(new ArtistsNotLatin() { ArtistName = _perfomer.name });
                                                        db.SaveChanges();
                                                    }
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
                                                                    bool _ArtistPic = ArtistProfilePicture_Asyn(0, _Artists, _unitOfWork, true, _perfomer.images.huge);
                                                                }
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }
                                                        #endregion

                                                    }
                                                    #region "Instagram Api Implementation"
                                                    Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(0, _Artists, null, _unitOfWork, false, _TourDateEntity);
                                                    #endregion

                                                    #region "Eventful API Implementation"
                                                    Task<Dictionary<string, object>> _Eventful = EventFul_GetEventInfo_Asyn(0, _Artists, _unitOfWork, false);
                                                    #endregion
                                                });
                                            }
                                            #endregion
                                            if (isLatin)
                                            {
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

                                                        //_TourDateRepo.Repository.Add(_TourDateEntity);
                                                        db.TourDate.Add(_TourDateEntity);
                                                        db.SaveChanges();
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

                                                        //_TourDateRepo.Repository.Update(_TourDateEntity);
                                                        db.Entry(_TourDateEntity).State = System.Data.Entity.EntityState.Modified;
                                                        db.SaveChanges();

                                                    }
                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region "Save Other Artist in Tour Perfromer"
                                                    _TourPerformers = _TourPerformersRepo.Repository.Get(p => p.ArtistID == _Artists.ArtistID && p.TourDateID == _TourDateEntity.TourDateID);

                                                    if (_TourPerformers == null)
                                                    {
                                                        _TourPerformers = new TourPerformers();

                                                        _TourPerformers.TourDateID = _TourDateEntity.TourDateID;
                                                        _TourPerformers.ArtistID = _Artists.ArtistID;
                                                        _TourPerformers.CreatedDate = DateTime.Now;
                                                        _TourPerformers.RecordStatus = RecordStatus.MusicGraph.ToString();

                                                        // _TourPerformersRepo.Repository.Add(_TourPerformers);
                                                        db.TourPerformers.Add(_TourPerformers);
                                                        db.SaveChanges();
                                                    }
                                                    #endregion
                                                }

                                                string MusicGraphBio = "";
                                                if (!String.IsNullOrEmpty(_Artists.Musicgraph_ID))
                                                    MusicGraphBio = GetMusicGraphBio(_Artists.Musicgraph_ID);
                                                if (!String.IsNullOrEmpty(MusicGraphBio))
                                                    _Artists.About = MusicGraphBio;

                                                //_ArtistsRepo.Repository.Update(_Artists);
                                                db.Entry(_Artists).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            WriteToFile("GetEventByIDAsync -2" + ex.StackTrace + " Message=" + ex.Message);
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
                                Task<Dictionary<string, object>> _Instagram = Instagram_GetPictures_Asyn(0, _Artists, null, _unitOfWork, false, _TourDateEntity);
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
                    }
                    catch (Exception ex)
                    {
                        WriteToFile("GetEventByIDAsync-1 " + ex.StackTrace + " Message=" + ex.Message);

                    }
                }
            }
            catch (Exception ex)
            {
                WriteToFile("GetEventByIDAsync " + ex.StackTrace + " Message=" + ex.Message);
            }

            return true;
        }


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


        #region "Spotify"
        private async Task<Dictionary<string, object>> Spotify_GetSongInfo_Asyn(Int32 vUserID, Artists vArtists, IUnitOfWork vUnitOfWork, bool vNew)
        {
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
                    WriteToFile("Spotify_GetSongInfo_Asyn " + ex.StackTrace + " Message=" + ex.Message);
                    //LogHelper.CreateLog("Spotify_GetSongInfo_Asyn " + ex.StackTrace);
                }
            });

            return dic;
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
                WriteToFile("Spotify_GetArtistByID " + ex.StackTrace + " Message=" + ex.Message);
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
                            IsLatin = _Islatin,
                            Image = item.Images.Count > 0 ? item.Images.FirstOrDefault().Url : string.Empty
                        });

                    }

                    return _artistlst;
                }



                //);



            }
            catch (Exception ex)
            {
                WriteToFile("Spotify_GetSimilarArtistByID " + ex.StackTrace + " Message=" + ex.Message);
                return null;
            }

        }

        #endregion


        #region "Third party API"





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
                WriteToFile("RetSpotifyAccessToken " + ex.StackTrace + " Message=" + ex.Message);
                return "";
            }


        }



        #region "Music Graph"

        //private Task<MusicGraph.Search_ByName> MusicGrapgh_GetArtistByName_Asyn(string vMusicgraph_Name)
        //{

        //    //string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
        //    string _Eventful_app_key = ConfigurationManager.AppSettings["Eventful_app_key"].ToString();
        //    HttpWebRequest httpWebRequest;
        //    HttpWebResponse httpResponse;
        //    string _result;

        //    //LogHelper.CreateLog("MusicGrapgh_GetArtistByName_Asyn (" + vMusicgraph_Name + ")");

        //    return Task.Factory.StartNew(() =>
        //    {
        //        try
        //        {
        //            //httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/v2/artist/search?api_key=" + _MusicGrapgh_api_key + "&name=" + vMusicgraph_Name.Trim());
        //            //httpWebRequest.ContentType = "application/json";
        //            //httpWebRequest.Method = "GET";
        //            httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.eventful.com/json/performers/search?app_key=" + _Eventful_app_key + "&keywords=" + vMusicgraph_Name.Trim());
        //            httpWebRequest.ContentType = "application/json";
        //            httpWebRequest.Method = "GET";

        //            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        //            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        //            {
        //                _result = streamReader.ReadToEnd();
        //            }


        //            // deserializing 
        //            return JsonConvert.DeserializeObject<MusicGraph.Search_ByName>(_result);

        //        }
        //        catch (Exception ex)
        //        {
        //            WriteToFile("MusicGrapgh_GetArtistByName_Asyn " + ex.StackTrace);
        //            return null;
        //        }
        //    }


        //        );


        //}

        //private Task<MusicGraph1.Search_ByID> MusicGrapgh_GetArtistByID_Asyn(string vMusicgraph_ID)
        //{

        //    string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();

        //    //LogHelper.CreateLog("MusicGrapgh_GetArtistByID_Asyn (" + vMusicgraph_ID + ")");

        //    return Task.Factory.StartNew(() =>
        //    {
        //        try
        //        {

        //            HttpWebRequest httpWebRequest;
        //            HttpWebResponse httpResponse;
        //            string _result;

        //            MusicGraph1.Search_ByID _Search_ByID = null;

        //            httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/v2/artist/" + vMusicgraph_ID + "?api_key=" + _MusicGrapgh_api_key);
        //            httpWebRequest.ContentType = "application/json";
        //            httpWebRequest.Method = "GET";

        //            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        //            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        //            {
        //                _result = streamReader.ReadToEnd();
        //            }

        //            // deserializing 
        //            _Search_ByID = JsonConvert.DeserializeObject<MusicGraph1.Search_ByID>(_result);

        //            return _Search_ByID;

        //        }
        //        catch (Exception ex)
        //        {
        //            WriteToFile("MusicGrapgh_GetArtistByID_Asyn " + ex.StackTrace);
        //            return null;
        //        }
        //    }


        //        );


        //}

        private async Task<Dictionary<string, object>> MusicGrapgh_GetSimilarArtists_Asyn(string vMusicgraph_ID, Int32 vArtistID, IUnitOfWork vUnitOfWork)
        {

            string _MusicGrapgh_api_key = ConfigurationManager.AppSettings["MusicGrapgh_api_key"].ToString();
            GenericRepository<Musika.Models.ArtistRelated> _ArtistRelatedRepo = new GenericRepository<Musika.Models.ArtistRelated>(vUnitOfWork);
            GenericRepository<Musika.Models.ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<Musika.Models.ArtistsNotLatin>(vUnitOfWork);

            Dictionary<string, object> list = new Dictionary<string, object>();

            MusicGraph5.GetSimilarArtists_ByID _GetSimilarArtists_ByID = await Task.Factory.StartNew(() =>
            {

                try
                {

                    //HttpWebRequest httpWebRequest;
                    //HttpWebResponse httpResponse = null;
                    //string _result;

                    //httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/v2/artist/" + vMusicgraph_ID + "/similar?api_key=" + _MusicGrapgh_api_key);
                    //httpWebRequest.ContentType = "application/json";
                    //httpWebRequest.Method = "GET";
                    string spotifyUrl = "https://api.spotify.com/v1/artists/" + vMusicgraph_ID + "/related-artists";
                    HttpWebRequest httpWebRequest;
                    HttpWebResponse httpResponse = null;
                    string _result;


                    httpWebRequest = (HttpWebRequest)WebRequest.Create(spotifyUrl);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";
                    httpWebRequest.Headers.Add("Authorization", "Bearer " + RetSpotifyAccessToken());

                    //httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    //using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    //{
                    //    _result = streamReader.ReadToEnd();
                    //}

                    for (int i = 0; i <= 1; i++)
                    {
                        try
                        {
                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                            if (httpResponse != null)
                                break;
                        }
                        catch (Exception ex)
                        {
                            if (i == 1)
                            {
                                WriteToFile("MusicGrapgh_GetSimilarArtists_Asyn " + ex.StackTrace + " Message=" + ex.Message);
                                return null;
                            }

                            System.Threading.Thread.Sleep(2000);
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
                    WriteToFile("MusicGrapgh_GetSimilarArtists_Asyn " + ex.StackTrace + " Message=" + ex.Message);
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
            catch (Exception ex)
            {
                WriteToFile("GetMusicGraphBio " + ex.StackTrace + " Message=" + ex.Message);
            }

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
                    WriteToFile("MusicGrapgh_GetArtistMatrics_Asyn " + ex.StackTrace + " Message=" + ex.Message);
                    return null;
                }
            });

        }

        #endregion

        #region "Instagram"


        private void GetImagesFromInstagramFeed_Depricated(Artists artist)
        {
            GenericRepository<Musika.Models.ArtistPhotos> _ArtistPhotosRepository = new GenericRepository<Musika.Models.ArtistPhotos>(_unitOfWork);
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
                                    Musika.Models.ArtistPhotos tp = new Musika.Models.ArtistPhotos();
                                    tp.ImageThumbnailUrl = i["images"]["thumbnail"].Value<string>("url");
                                    tp.ImageUrl = i["images"]["standard_resolution"].Value<string>("url");

                                    try
                                    {
                                        //tp.ImageThumbnailUrl = tp.ImageThumbnailUrl.Remove(tp.ImageThumbnailUrl.LastIndexOf('?'));
                                        tp.ImageThumbnailUrl = tp.ImageThumbnailUrl.Replace("?", "");
                                    }
                                    catch (Exception ex)
                                    {
                                        WriteToFile("GetImagesFromInstagramFeed_Depricated " + ex.StackTrace + " Message=" + ex.Message);
                                    }

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
                                    // _ArtistPhotosRepository.Repository.Add(tp);
                                    db.ArtistPhotos.Add(tp);
                                    db.SaveChanges();

                                    count--;
                                }
                                else
                                {
                                    if (artistRecord.RecordStatus == RecordStatus.InActive.ToString())
                                    {
                                        artistRecord.RecordStatus = RecordStatus.Active.ToString();
                                        artistRecord.ModifiedDate = DateTime.Now;
                                        //_ArtistPhotosRepository.Repository.Update(artistRecord);
                                        db.Entry(artistRecord).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();

                                        count--;
                                    }
                                }

                                if (count == 0)
                                    break;

                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        WriteToFile("GetImagesFromInstagramFeed_Depricated " + ex.StackTrace + " Message=" + ex.Message);
                    }

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
                WriteToFile("GetImagesFromInstagramFeed_Depricated " + ex.StackTrace + " Message=" + ex.Message);
                //  return null;
            }


        }

        private async Task<Dictionary<string, object>> GetImagesFromInstagramFeed(Artists artist)
        {
            GenericRepository<Musika.Models.ArtistPhotos> _ArtistPhotosRepository = new GenericRepository<Musika.Models.ArtistPhotos>(_unitOfWork);
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
                                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.instagram.com/explore/tags/" + cleanName + "/?__a=1");
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
                        {
                            WriteToFile("GetImagesFromInstagramFeed1 " + ex.StackTrace + " Message=" + ex.Message);
                        }
                        return result;
                        #endregion
                    });
                }
            }
            catch (Exception ex)
            {
                WriteToFile("GetImagesFromInstagramFeed " + ex.StackTrace + " Message=" + ex.Message);
                //  return null;
            }
            return result;

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
                httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.musicgraph.com/api/v2/artist/" + mId + "/profile?api_key=" + _MusicGrapgh_api_key);
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
            catch (Exception ex)
            {
                WriteToFile("CheckMusicGraphLatin " + ex.StackTrace + " Message=" + ex.Message);
            }


            return isLatin;
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

        private async Task<Dictionary<string, object>> Instagram_GetPictures_Asyn(Int32 vUserID, Artists vArtists, MusicGraph2.ArtistMatrics_ByID vArtistMatrics_ByID, IUnitOfWork vUnitOfWork, bool vNew, Musika.Models.TourDate vTour)
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
            GenericRepository<Musika.Models.ArtistPhotos> _ArtistPhotosRepo = new GenericRepository<Musika.Models.ArtistPhotos>(vUnitOfWork);
            GenericRepository<Musika.Models.TourPhoto> _TourPhotosRepo = new GenericRepository<Musika.Models.TourPhoto>(vUnitOfWork);
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
                        WriteToFile("Instagram_GetPictures_Asyn " + ex.StackTrace + " Message=" + ex.Message);
                        //  LogHelper.CreateLog3(ex, Request);
                        //  LogHelper.CreateLog("Instagram_GetPictures_Asyn (Task 1) " + ex.StackTrace);
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
                        WriteToFile("Instagram_GetPictures_Asyn-2 " + ex.StackTrace + " Message=" + ex.Message);
                        // LogHelper.CreateLog3(ex, Request);
                        //  LogHelper.CreateLog("Instagram_GetPictures_Asyn (Task 2) " + ex.StackTrace);
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
                        WriteToFile("Instagram_GetPictures_Asyn-3" + ex.StackTrace + " Message=" + ex.Message);
                        // LogHelper.CreateLog3(ex, Request);
                        //  LogHelper.CreateLog("Instagram_GetPictures_Asyn (Profile Picture ) " + ex.StackTrace);
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
                        WriteToFile("Instagram_GetPictures_Asyn-4 " + ex.StackTrace + " Message=" + ex.Message);
                        // LogHelper.CreateLog("Instagram_GetPictures_Asyn (Recent Picture - Update) " + ex.StackTrace);
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
                        WriteToFile("Instagram_GetPictures_Asyn-5=> " + ex.StackTrace + " Message=" + ex.Message);
                        //  LogHelper.CreateLog("Instagram_GetPictures_Asyn (Profile Picture - Update) " + ex.StackTrace);
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
                        WriteToFile("Instagram_GetPictures_Asyn-6 " + ex.StackTrace + " Message=" + ex.Message);
                        // LogHelper.CreateLog("Instagram_GetPictures_Asyn (Recent Picture - Upadte) " + ex.StackTrace);
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


            // LogHelper.CreateLog("SeatGeek_GetEventByID_Asyn (" + vSeatGeekID + ")");

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
                    WriteToFile("SeatGeek_GetEventByID_Asyn " + ex.StackTrace + " Message=" + ex.Message);
                    //  LogHelper.CreateLog3(ex, Request);
                    // LogHelper.CreateLog("SeatGeek_GetEventByID_Asyn " + ex.StackTrace);
                    return null;
                }
            }


                );


        }


        private async Task<Dictionary<string, object>> SeatGeek_GetEventByArtistName_Asyn(Int32 vUserID, Artists vArtists, IUnitOfWork vUnitOfWork, bool vNew, bool vScan = false)
        {
            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });

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
                        WriteToFile("SeatGeek_GetEventByArtistName_Asyn " + ex.StackTrace + " Message=" + ex.Message);
                        // LogHelper.CreateLog("SeatGeek_GetEventByArtistName_Asyn (Task 1) " + ex.StackTrace);
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
                    WriteToFile("SeatGeek_GetEventByArtistName_Asyn-1 " + ex.StackTrace + " Message=" + ex.Message);
                    //LogHelper.CreateLog("SeatGeek_GetEventByArtistName_Asyn (Task 2 - Update) " + ex.StackTrace);
                }
            });
            #endregion
            response.Add("vNew", vNew);

            return response;
        }


        private SeatGeek.Performer CheckExistenceOfSeatGeekArtistName(Artists vArtists, IUnitOfWork vUnitOfWork, bool vNew, bool vScan = false)
        {
            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });

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
            SeatGeek.Performer _Performer = new SeatGeek.Performer();

            Dictionary<string, object> response = new Dictionary<string, object>();
            #region "Task 1 to get SeatGeek Detail using Artist Name"
            if (vArtists.Seatgeek_ID == null)
            {
                // SeatGeek.Get_Performers _Get_PerformersRet = Task.Factory.StartNew(() =>
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



                                _Performer = (from A in _Get_Performers.performers
                                              where DiceCoefficientExtensions.DiceCoefficient(A.name.ToLower(), vArtists.ArtistName.Trim()) >= _FuzzySearchCri && CheckSeatGeekLatin(A.name, _unitOfWork)
                                              select A).OrderByDescending(p => p.score).FirstOrDefault();


                                if (_Performer == null)
                                {
                                    _Performer = _Get_Performers.performers[0];
                                }

                                if (_Performer != null)
                                {
                                    //vArtists.OnTour = _Performer.has_upcoming_events == true ? true : false;
                                    // _Performer_ID = _Performer.id.ToString();
                                    //vArtists.Seatgeek_ID = _Performer_ID;

                                    //if (_Performer.taxonomies != null && _Performer.taxonomies.Count > 0)
                                    //{
                                    //    foreach (SeatGeek.Taxonomy _Taxonomy in _Performer.taxonomies)
                                    //    {
                                    //        _strEvent = _strEvent + _Taxonomy.name + " ,";
                                    //    }
                                    //}

                                    //if (_strEvent.ToString() == "") _strEvent = vArtists.ArtistName + " Event";

                                }
                            }
                        }
                        // deserializing 
                        //return _Performer;

                    }
                    catch (Exception ex)
                    {
                        WriteToFile("SeatGeek_GetEventByArtistName_Asyn " + ex.StackTrace + " Message=" + ex.Message);
                        // LogHelper.CreateLog("SeatGeek_GetEventByArtistName_Asyn (Task 1) " + ex.StackTrace);
                        return null;
                    }
                };

                //response.Add("SeatGeekDetail", _Get_PerformersRet);
            }
            #endregion
            return _Performer;

        }
        #endregion

        #region "Eventful"

        private async Task<Dictionary<string, object>> EventFul_GetEventInfo_Asyn(Int32 vUserID, Artists vArtists, IUnitOfWork vUnitOfWork, bool vNew)
        {

            GenericRepository<Musika.Models.Venue> _VenueRepo = new GenericRepository<Musika.Models.Venue>(vUnitOfWork);
            GenericRepository<Musika.Models.TourDate> _TourDateRepo = new GenericRepository<Musika.Models.TourDate>(vUnitOfWork);
            GenericRepository<Musika.Models.UserTourDate> _UserTourDateRepo = new GenericRepository<Musika.Models.UserTourDate>(_unitOfWork);
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
                        WriteToFile("EventFul_GetEventInfo_Asyn-1 " + ex.StackTrace + " Message=" + ex.Message);
                        // LogHelper.CreateLog("EventFul_GetEventInfo_Asyn (Task 1) " + ex.StackTrace);
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
                        WriteToFile("EventFul_GetEventInfo_Asyn-2 " + ex.StackTrace + " Message=" + ex.Message);
                        // LogHelper.CreateLog("EventFul_GetEventInfo_Asyn (Task 2) " + ex.StackTrace + " API : " +
                        //  "http://api.eventful.com/json/performers/get?app_key=" + _Eventful_app_key + "&id=" + //vArtists.Eventful_ID + "&show_events=true&image_sizes=large");
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
                                    WriteToFile("EventFul_GetEventInfo_Asyn-3 " + ex.StackTrace + " Message=" + ex.Message);
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
            catch (Exception ex)
            {
                WriteToFile("CheckLastResortSpotifyGenre " + ex.StackTrace + " Message=" + ex.Message);
                return false;
            }
        }

        private string GetProfileImageFromSpotifyFeed(Artists artist)
        {
            GenericRepository<Artists> _ArtistRepository = new GenericRepository<Artists>(_unitOfWork);
            string _response = string.Empty;

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
                WriteToFile("GetProfileImageFromSpotifyFeed " + ex.StackTrace + " Message=" + ex.Message);
                //  LogHelper.CreateLog("MusicGrapgh_GetSimilarArtists_Asyn " + ex.StackTrace);
                //  return null;
            }
            return imageUrl;
        }



        //New API's for spotify
        private async Task<List<MusicGraph.Datum>> Spotify_SearchArtist(string _q, int PageSize, int PageIndex)
        {
            try
            {

                List<MusicGraph.Datum> _artistlst = new List<MusicGraph.Datum>();

                string _Spotify_Country = ConfigurationManager.AppSettings["Spotify_Country"].ToString();
                //RootObject _MasterList = GetArtistsSearchResultforcache(1, 1);
                //Page<Artist> _lst = await Artist.Search(_q, "", "", "", "", PageSize, PageIndex);
                //int pageSize = PageSize;//_MasterList.meta.per_page;
                //int pageIndex = 0;

                //int count = _lst.Total;
                ////_MasterList = new RootObject();
                //int artistlitscount = count - pageSize;

                //for (int i = 0; i < artistlitscount; i++)
                //{
                //    try
                //    {
                //        pageIndex = pageIndex + 1;
                //        artistlitscount = artistlitscount - pageSize;
                //        Page<Artist> lst = await Artist.Search(_q, "", "", "", "", pageSize, pageIndex);
                //        if (lst != null)
                //        {

                //            _lst.Items.AddRange(lst.Items);
                //        }
                //        else
                //        {
                //            //return _MasterList;
                //        }

                //    }
                //    catch (Exception ex)
                //    {
                //        WriteToFile("PagingforArtist " + ex.StackTrace + " Message=" + ex.Message);
                //        count = 0;
                //    }

                //}


                Page<Artist> _lst = await Artist.Search(_q, "", "", "", "", PageSize, PageIndex);

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
                        IsLatin = _Islatin,
                        Image = item.Images.Count > 0 ? item.Images.FirstOrDefault().Url : string.Empty
                    });
                }

                return _artistlst;
            }
            catch (Exception ex)
            {
                WriteToFile("Spotify_SearchArtist " + ex.StackTrace + " Message=" + ex.Message);
                //LogHelper.CreateLog(ex);
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
                WriteToFile("ArtistProfilePicture_Asyn " + ex.StackTrace + " Message=" + ex.Message);
                // LogHelper.CreateLog("ArtistProfilePicture_Asyn" + ex.StackTrace);
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

                    vArtists.ImageURL = Url;
                    vArtists.ThumbnailURL = strpThumbnailURLfordb;
                    vArtists.BannerImage_URL = Url;
                    //_artistrepo.Repository.Update(vArtists);
                    db.Entry(vArtists).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                WriteToFile("SpotifyProfilePicture " + ex.StackTrace + " Message=" + ex.Message);
                //  LogHelper.CreateLog("ArtistProfilePicture_Asyn" + ex.StackTrace);
            }

            return true;
        }

        #endregion


        #endregion
        public bool CheckSeatGeekLatin(string artistName, IUnitOfWork vUnitOfWork)
        {
            bool isLatin = false;

            try
            {
                GenericRepository<Musika.Models.ArtistsNotLatin> _ArtistNotLatinRepo = new GenericRepository<Musika.Models.ArtistsNotLatin>(vUnitOfWork);

                //if (!_ArtistNotLatinRepo.Repository.AsQueryable().Any(x => x.ArtistName == artistName))
                {

                    var serializer = new JavaScriptSerializer();
                    serializer.RegisterConverters(new[] { new DynamicJsonConverter() });

                    GenericRepository<Musika.Models.Venue> _VenueRepo = new GenericRepository<Musika.Models.Venue>(vUnitOfWork);
                    GenericRepository<Musika.Models.TourDate> _TourDateRepo = new GenericRepository<Musika.Models.TourDate>(vUnitOfWork);
                    GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(vUnitOfWork);

                    GenericRepository<Musika.Models.UserTourDate> _UserTourDateRepo = new GenericRepository<Musika.Models.UserTourDate>(vUnitOfWork);
                    GenericRepository<Musika.Models.ArtistGenre> _ArtistGenreRepo = new GenericRepository<Musika.Models.ArtistGenre>(vUnitOfWork);
                    GenericRepository<Musika.Models.Genre> _GenreRepo = new GenericRepository<Musika.Models.Genre>(vUnitOfWork);

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
            catch (Exception ex)
            {
                WriteToFile("CheckSeatGeekLatin " + ex.StackTrace + " Message=" + ex.Message);
            }


            return isLatin;
        }

        #region "seatgeek api for events"

        public SeatGeekEventResponse.RootObject getEventList(int PageSize, int pageIndex)
        {
            string _result = string.Empty;
            SeatGeekEventResponse.RootObject art = new SeatGeekEventResponse.RootObject();
            try
            {
                string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();
                string _SeatGeek_client_secret = ConfigurationManager.AppSettings["SeatGeek_client_secret"].ToString();

                try
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                           | SecurityProtocolType.Tls11
                           | SecurityProtocolType.Tls12
                           | SecurityProtocolType.Ssl3;
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.seatgeek.com/2/events?&per_page=" + PageSize + "&page=" + pageIndex + "&client_id=" + _SeatGeek_client_id + "&client_secret=" + _SeatGeek_client_secret + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3000000&taxonomies.id=3010200&taxonomies.id=3010300");

                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                        art = JsonConvert.DeserializeObject<SeatGeekEventResponse.RootObject>(_result);
                        return art;
                    }

                }
                catch (Exception ex)
                {
                    WriteToFile("getEventList " + ex.StackTrace + " Message=" + ex.Message);
                }

                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, JsonConvert.DeserializeObject(_result), "Events"));
            }
            catch (Exception ex)
            {
                WriteToFile("getEventList-1 " + ex.StackTrace + " Message=" + ex.Message);
            }
            return art;
            //return JsonConvert.DeserializeObject<SeatGeek3.Get_Events_ByLat>(_result);
        }

        public SeatGeek3.Get_Venues getVenueList(int pageIndex, int PageSize)
        {
            string _result = string.Empty;
            try
            {
                string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();
                string _SeatGeek_client_secret = ConfigurationManager.AppSettings["SeatGeek_client_secret"].ToString();

                try
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                           | SecurityProtocolType.Tls11
                           | SecurityProtocolType.Tls12
                           | SecurityProtocolType.Ssl3;
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.seatgeek.com/2/venues?per_page=" + PageSize + "&page=" + pageIndex + "&taxonomies.id= 2000000&taxonomies.id=2010000&taxonomies.id= 3010100&taxonomies.id=3010200&taxonomies.id=3010300&client_id=" + _SeatGeek_client_id + "&client_secret=" + _SeatGeek_client_secret);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }

                }
                catch (Exception ex)
                {
                    WriteToFile("getVenueList " + ex.StackTrace + " Message=" + ex.Message);
                }

                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, JsonConvert.DeserializeObject(_result), "Events"));
            }
            catch (Exception ex)
            {
                WriteToFile("getVenueList-1 " + ex.StackTrace + " Message=" + ex.Message);
            }
            return JsonConvert.DeserializeObject<SeatGeek3.Get_Venues>(_result);
        }

        public SeatGeekEventResponse.RootObject GetEventsSearchResultforcache()
        {
            List<Performer> lstArtist = new List<Performer>();
            SeatGeekEventResponse.RootObject _MasterList = getEventList(1, 1);
            int pageSize = 1000;//_MasterList.meta.per_page;
            int pageIndex = 0;
            //RootObject _Search_ByName = GetArtistsSearchResultforcache(pageSize, pageIndex);

            //if (_Search_ByName != null)
            //{

            int count = _MasterList.meta.total;
            //_MasterList = new RootObject();
            int artistlitscount = count - pageSize;
            for (int i = 0; i < artistlitscount; i++)
            {
                try
                {
                    pageIndex = pageIndex + 1;
                    artistlitscount = artistlitscount - pageSize;
                    var list = getEventList(pageSize, pageIndex);
                    if (list != null)
                    {
                        _MasterList.events.AddRange(list.events);
                    }
                    else
                    {
                        return _MasterList;
                    }

                }
                catch (Exception ex)
                {
                    WriteToFile("PagingforArtist " + ex.StackTrace + " Message=" + ex.Message);
                    count = 0;
                }

            }

            //  _MasterList.performers.AddRange(list.performers);
            // }
            return _MasterList;

            //int count = 0;
            //int pageSize = 100;
            //int pageIndex = 1;
            //var eventList = getEventList(pageIndex, pageSize);
            //var masterList = eventList;

            //if (eventList != null)
            //{
            //    count = eventList.events.Count();
            //    while (count > 0)
            //    {
            //        pageIndex = pageIndex + 1;
            //        var list = getEventList(pageIndex, pageSize);
            //        try
            //        {
            //            count = list.events.Count();
            //        }
            //        catch (Exception ex)
            //        {
            //            count = 0;
            //        }
            //        masterList.events.AddRange(list.events);
            //    }
            //}



        }


        public SeatGeek3.Get_Venues GetVenue()
        {
            //int count = 0;
            //int pageSize = 100;
            //int pageIndex = 1;
            //var eventList = getVenueList(pageIndex, pageSize);
            //var masterList = eventList;

            //if (eventList != null)
            //{
            //    count = eventList.venues.Count();
            //    for (int i = 0; i < artistlitscount; i++)
            //    {
            //        try
            //        {
            //            pageIndex = pageIndex + 1;
            //            var list = getVenueList(pageIndex, pageSize);
            //            try
            //            {
            //                count = list.venues.Count();
            //            }

            //            catch (Exception ex)
            //            {
            //                WriteToFile("GetVenueSearchResultforcache " + ex.StackTrace);
            //                count = 0;
            //            }
            //            masterList.venues.AddRange(list.venues);
            //        }
            //}
            //}


            List<Venue> lstArtist = new List<Venue>();
            SeatGeek3.Get_Venues _MasterList = getVenueList(1, 1);
            int pageSize = 1000;//_MasterList.meta.per_page;
            int pageIndex = 0;


            int count = _MasterList.meta.total;

            int artistlitscount = count - pageSize;
            for (int i = 0; i < artistlitscount; i++)
            {
                try
                {
                    pageIndex = pageIndex + 1;
                    artistlitscount = artistlitscount - pageSize;
                    var list = getVenueList(pageSize, pageIndex);
                    if (list != null)
                    {
                        _MasterList.venues.AddRange(list.venues);
                    }
                    else
                    {
                        return _MasterList;
                    }

                }
                catch (Exception ex)
                {
                    WriteToFile("PagingforArtist " + ex.StackTrace + " Message=" + ex.Message);
                    count = 0;
                }

            }
            return _MasterList;
        }

        public string GetVenuesSearchResultforcache()
        {
            string _result = string.Empty;
            try
            {
                string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();
                string _SeatGeek_client_secret = ConfigurationManager.AppSettings["SeatGeek_client_secret"].ToString();

                try
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                           | SecurityProtocolType.Tls11
                           | SecurityProtocolType.Tls12
                           | SecurityProtocolType.Ssl3;
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.seatgeek.com/2/venues?client_id=" + _SeatGeek_client_id + "&client_secret=" + _SeatGeek_client_secret);

                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                    }

                }
                catch (Exception ex)
                {
                    WriteToFile("GetVenuesSearchResultforcache " + ex.StackTrace + " Message=" + ex.Message);
                }

                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, JsonConvert.DeserializeObject(_result), "Events"));
            }
            catch (Exception ex)
            {
                WriteToFile("GetVenuesSearchResultforcache-1 " + ex.StackTrace + " Message=" + ex.Message);
            }
            return _result;

        }
        public RootObject PagingforArtist()
        {
            List<Performer> lstArtist = new List<Performer>();
            RootObject _MasterList = GetArtistsSearchResultforcache(1, 1);
            int pageSize = 2000;//_MasterList.meta.per_page;
            int pageIndex = 0;
            //RootObject _Search_ByName = GetArtistsSearchResultforcache(pageSize, pageIndex);

            //if (_Search_ByName != null)
            //{

            int count = _MasterList.meta.total;
            //_MasterList = new RootObject();
            int artistlitscount = count - pageSize;
            for (int i = 0; i < artistlitscount; i++)
            {
                try
                {
                    pageIndex = pageIndex + 1;
                    artistlitscount = artistlitscount - pageSize;
                    var list = GetArtistsSearchResultforcache(pageSize, pageIndex);
                    if (list != null)
                    {
                        var l = list.performers.Where(p => p.type != "theater" && p.type == "band").ToList();
                        _MasterList.performers.AddRange(l);
                    }
                    else
                    {
                        return _MasterList;
                    }

                }
                catch (Exception ex)
                {
                    WriteToFile("PagingforArtist " + ex.StackTrace + " Message=" + ex.Message);
                    count = 0;
                }

            }

            //  _MasterList.performers.AddRange(list.performers);
            // }
            return _MasterList;
        }
        public RootObject GetArtistsSearchResultforcache(int pagesize, int pageindex)
        {
            string _result = string.Empty;
            RootObject art = new RootObject();
            //List<MusikaDataService.Models.Artists1> art = new List<MusikaDataService.Models.Artists1>();
            try
            {
                string _SeatGeek_client_id = ConfigurationManager.AppSettings["SeatGeek_client_id"].ToString();
                string _SeatGeek_client_secret = ConfigurationManager.AppSettings["SeatGeek_client_secret"].ToString();
                try
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                           | SecurityProtocolType.Tls11
                           | SecurityProtocolType.Tls12
                           | SecurityProtocolType.Ssl3;
                    //var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.seatgeek.com/2/performers?client_id=" + _SeatGeek_client_id + "&client_secret=" + _SeatGeek_client_secret);
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.seatgeek.com/2/performers" + SGAuth() + "&per_page=" + pagesize + "&page=" + pageindex + "&taxonomies.id=2000000&taxonomies.id=2010000&taxonomies.id=3010100&taxonomies.id=3010200&taxonomies.id=3010300");

                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _result = streamReader.ReadToEnd();
                        art = JsonConvert.DeserializeObject<RootObject>(_result);
                        return art;
                    }

                }
                catch (Exception ex)
                {
                    WriteToFile("GetArtistsSearchResultforcache " + ex.StackTrace + " Message=" + ex.Message);
                }

                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, JsonConvert.DeserializeObject(_result), "Events"));
            }
            catch (Exception ex)
            {
                WriteToFile("GetArtistsSearchResultforcache-1 " + ex.StackTrace + " Message=" + ex.Message);
            }
            return art;

        }
        #endregion
        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            WriteToFile("Service is recall at " + DateTime.Now);
        }

        private void EventFul_Asyn_Operation(Int32 vUserID, Artists vArtists, IUnitOfWork vUnitOfWork, Dictionary<string, object> apiResponse, bool vScan = false)
        {
            try
            {
                GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(vUnitOfWork);
                GenericRepository<Venue> _VenueRepo = new GenericRepository<Venue>(vUnitOfWork);
                GenericRepository<TourDate> _TourDateRepo = new GenericRepository<TourDate>(vUnitOfWork);
                GenericRepository<UserTourDate> _UserTourDateRepo = new GenericRepository<UserTourDate>(_unitOfWork);

                Venue _VenuEntity = null;
                TourDate _TourDateEntity = null;
                // UserTourDate _UserTourDate = null;
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
                                //TourDate _TourDateEntity = null;
                                dynamic _Get_Event_ByID = (dynamic)apiResponse["GetEventByID_" + _event.id.ToString()];
                                if (_Get_Event_ByID != null)
                                {
                                    //Venu information
                                    //_VenuEntity = _VenueRepo.Repository.Get(p => (p.Eventful_VenueID == _Get_Event_ByID.id.ToString()) ||
                                    //                                             (p.VenueName.ToLower() == _Get_Event_ByID.venue_name.ToLower())
                                    //        );


                                    //using fuzzy searching techinque here
                                    _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                                   where (A.Eventful_VenueID == _Get_Event_ByID.venue_id.ToString())
                                                   select A).FirstOrDefault();


                                    //search the venu using fuzzy searching
                                    if (_VenuEntity == null)
                                    {
                                        /* _VenuEntity = (from A in _VenueRepo.Repository.GetAll()
                                                       where (DiceCoefficientExtensions.DiceCoefficient(A.VenueName.ToLower(), _Get_Event_ByID.venue_name.ToLower()) >= _FuzzySearchCri)
                                                       && A.RecordStatus == RecordStatus.SeatGeek.ToString()
                                                       select A).FirstOrDefault();
                                                       */

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
                                        db.Venue.Add(_VenuEntity);
                                        db.SaveChanges();
                                        //_VenueRepo.Repository.Add(_VenuEntity);

                                    }

                                    string _Event_ID = _Get_Event_ByID.id.ToString();
                                    string _start_time = _Get_Event_ByID.start_time;


                                    //Entering Tour records
                                    //_TourDateEntity = (from A in _VenueRepo.Repository.GetAll(p => p.VenueID == _VenuEntity.VenueID)
                                    //                    join B in _TourDateRepo.Repository.GetAll(p =>
                                    //                                                    (p.Eventful_TourID == _Event_ID && p.ArtistID == vArtists.ArtistID)
                                    //                                                 || (Convert.ToDateTime(p.Datetime_Local).Month == Convert.ToDateTime(_start_time).Month
                                    //                                                     && Convert.ToDateTime(p.Datetime_Local).Year == Convert.ToDateTime(_start_time).Year
                                    //                                                     && Convert.ToDateTime(p.Datetime_Local).Day == Convert.ToDateTime(_start_time).Day
                                    //                                                     && p.ArtistID == vArtists.ArtistID
                                    //                                                     && p.RecordStatus == RecordStatus.SeatGeek.ToString())
                                    //                                                 ) on A.VenueID equals B.VenueID
                                    //                    where B.ArtistID == vArtists.ArtistID
                                    //                    select B).FirstOrDefault();




                                    DateTime _datetime_local2 = Convert.ToDateTime(_start_time);
                                    _TourDateEntity = (from A in _VenueRepo.Repository.GetAll(p => p.VenueID == _VenuEntity.VenueID)
                                                       join B in _TourDateRepo.Repository.GetAll(p =>
                                                                                       (p.Eventful_TourID == _Event_ID && p.ArtistID == vArtists.ArtistID)
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
                                        DateTime local = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                        if (local != null)
                                        {
                                            string strLocal = local.ToString("MM/dd/yyyy");
                                            _TourDateEntity = _TourDateRepo.Repository.AsQueryable().Where(x => x.ArtistID == vArtists.ArtistID && x.VenueID == _VenuEntity.VenueID && (x.Datetime_Local.DateToString() == strLocal || x.Visible_Until_utc.DateToString() == strLocal || x.Tour_Utcdate.DateToString() == strLocal)).FirstOrDefault();
                                        }

                                        if (_TourDateEntity == null)
                                        {
                                            _TourDateEntity = new TourDate();

                                            _TourDateEntity.Eventful_TourID = _Get_Event_ByID.id.ToString();
                                            _TourDateEntity.SeatGeek_TourID = null;

                                            _TourDateEntity.ArtistID = vArtists.ArtistID;
                                            _TourDateEntity.VenueID = _VenuEntity.VenueID;
                                            if (!string.IsNullOrEmpty(_Get_Event_ByID.title.ToString()))
                                            {
                                                if (_Get_Event_ByID.title.ToString().Length >= 200)
                                                {
                                                    _TourDateEntity.EventName = _Get_Event_ByID.title.ToString().Substring(0, 200);
                                                }
                                                else
                                                {
                                                    _TourDateEntity.EventName = _Get_Event_ByID.title;
                                                }
                                            }
                                            else
                                            {
                                                _TourDateEntity.EventName = _Get_Event_ByID.title;
                                            }
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
                                            //_TourDateRepo.Repository.Add(_TourDateEntity);
                                            db.TourDate.Add(_TourDateEntity);
                                            db.SaveChanges();
                                            //if (_UserTourDate == null)
                                            //{
                                            //    _UserTourDate = new UserTourDate();
                                            //    _UserTourDate.TourDateID = _TourDateEntity.TourDateID;
                                            //    _UserTourDate.UserID = vUserID;
                                            //    _UserTourDate.CreatedDate = DateTime.Now;
                                            //    _UserTourDate.RecordStatus = RecordStatus.Active.ToString();

                                            //    /*if (_Get_Event_ByID.links != null)
                                            //    {
                                            //        try
                                            //        {
                                            //            _TourDateEntity.TicketURL = _Get_Event_ByID.links.link[0].url;
                                            //        }
                                            //        catch { }
                                            //    }
                                            //    else
                                            //    {
                                            //        _TourDateEntity.TicketURL = "http://eventful.com/";
                                            //    }*/

                                            //   // _UserTourDateRepo.Repository.Add(_UserTourDate);
                                            //    db.UserTourDate.Add(_UserTourDate);
                                            //    db.SaveChanges();
                                            //}

                                        }
                                    }

                                    /*SOMETHING HERE USERTOURDATE*/

                                    else
                                    {
                                        _TourDateEntity.Eventful_TourID = _Get_Event_ByID.id.ToString();
                                        _TourDateEntity.ArtistID = vArtists.ArtistID;
                                        _TourDateEntity.VenueID = _VenuEntity.VenueID;

                                        //_TourDateEntity.EventName = _Get_Event_ByID.title.ToString().Substring(0, 200);

                                        if (!string.IsNullOrEmpty(_Get_Event_ByID.title.ToString()))
                                        {
                                            if (_Get_Event_ByID.title.ToString().Length >= 200)
                                            {
                                                _TourDateEntity.EventName = _Get_Event_ByID.title.ToString().Substring(0, 200);
                                            }
                                            else
                                            {
                                                _TourDateEntity.EventName = _Get_Event_ByID.title;
                                            }
                                        }
                                        else
                                        {
                                            _TourDateEntity.EventName = _Get_Event_ByID.title;
                                        }


                                        _TourDateEntity.EventID = null;
                                        _TourDateEntity.Score = 0;

                                        _TourDateEntity.Announce_Date = null;
                                        _TourDateEntity.Visible_Until_utc = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                        _TourDateEntity.Tour_Utcdate = Convert.ToDateTime(_Get_Event_ByID.start_time);
                                        _TourDateEntity.Datetime_Local = Convert.ToDateTime(_Get_Event_ByID.start_time);

                                        _TourDateEntity.ModifiedDate = DateTime.Now;

                                        //_TourDateRepo.Repository.Update(_TourDateEntity);


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


                                        _TourDateRepo.Repository.Update(_TourDateEntity);
                                        //db.Entry(_TourDateEntity).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();


                                    }

                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                WriteToFile("EventFul_Asyn_Operation " + ex.StackTrace + " Message=" + ex.Message);
                // LogHelper.CreateLog(ex);
                //throw new Exception(ex.StackTrace, ex);
            }
        }
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message + Environment.NewLine + "-------------------------------");
                }
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
                                        //_ArtistPhotosRepository.Repository.Add(tp);
                                        db.ArtistPhotos.Add(tp);
                                        db.SaveChanges();

                                        count--;
                                    }
                                    else
                                    {
                                        if (artistRecord.RecordStatus == RecordStatus.InActive.ToString())
                                        {
                                            artistRecord.RecordStatus = RecordStatus.Active.ToString();
                                            artistRecord.ModifiedDate = DateTime.Now;
                                            //_ArtistPhotosRepository.Repository.Update(artistRecord);
                                            db.Entry(artistRecord).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();

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
                                        //_ArtistPhotosRepository.Repository.Add(tp);
                                        db.ArtistPhotos.Add(tp);
                                        db.SaveChanges();
                                        count--;
                                    }
                                    else
                                    {
                                        if (artistRecord.RecordStatus == RecordStatus.InActive.ToString())
                                        {
                                            artistRecord.RecordStatus = RecordStatus.Active.ToString();
                                            artistRecord.ModifiedDate = DateTime.Now;
                                            //_ArtistPhotosRepository.Repository.Update(artistRecord);
                                            db.Entry(artistRecord).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();

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
                                        //_ArtistPhotosRepository.Repository.Add(tp);
                                        db.ArtistPhotos.Add(tp);
                                        db.SaveChanges();
                                        count--;
                                    }
                                    else
                                    {
                                        if (artistRecord.RecordStatus == RecordStatus.InActive.ToString())
                                        {
                                            artistRecord.RecordStatus = RecordStatus.Active.ToString();
                                            artistRecord.ModifiedDate = DateTime.Now;
                                            //_ArtistPhotosRepository.Repository.Update(artistRecord);
                                            db.Entry(artistRecord).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();

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
                WriteToFile("ImagesFromInstagram_Asyn_Operation " + ex.StackTrace + " Message=" + ex.Message);
                //throw new Exception(ex.StackTrace, ex);
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
                        else if (_task.Count == 0)
                        {

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
                WriteToFile("Spotify_GetSongInfo_Asyn_Operation " + ex.StackTrace + " Message=" + ex.Message);
            }
        }
    }
}

