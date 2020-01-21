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
    [RoutePrefix("api/v3")]
    public class APIV3Controller : ApiController
    {
        MusikaEntities db = new MusikaEntities();

        [HttpGet]
        [Route("Venue/GetYourFriendPlans")]
        public HttpResponseMessage GetYourFriendPlans(Int32 UserID, Int16 Pageindex, Int16 Pagesize)
        {
            try
            {
                var users = (from A in db.UserFriends
                             join B in db.UserGoing on A.Matched_UserID equals B.UserID into AB
                             from B in AB.DefaultIfEmpty()
                             join C in db.TourDate on B.TourDateID equals C.TourDateID into BD
                             from C in BD.DefaultIfEmpty()
                             where A.UserID == UserID && A.Matched_UserID != 0 && B.RecordStatus == "Going" && C.TicketingEventID == null
                             select B.TourDateID).ToList();

                int RemainingRecord = Pagesize;
                var _list = (from A in db.TourDate
                             join B in db.Artists on A.ArtistID equals B.ArtistID into AB
                             from B in AB.DefaultIfEmpty()
                             join C in db.Venue on A.VenueID equals C.VenueID into AC
                             from C in AC.DefaultIfEmpty()
                             where users.Contains(A.TourDateID) && A.Tour_Utcdate > DateTime.Now && A.IsDeleted == false
                             select new ViewYourFreiendplanlst()
                             {
                                 TourDateID = A.TourDateID,
                                 ArtistID = A.ArtistID,
                                 ArtistName = B.ArtistName,
                                 ImageURL = B.ImageURL,
                                 BannerImage_URL = B.BannerImage_URL,
                                 Datetime_Local = A.Datetime_Local.Value,
                                 VenueName = C.VenueName,
                                 Going = (from G in db.UserFriends
                                          join H in db.Users on G.Matched_UserID equals H.UserID into GH
                                          from H in GH.DefaultIfEmpty()
                                          join I in db.UserGoing on H.UserID equals I.UserID into HI
                                          from I in HI.DefaultIfEmpty()
                                          where G.UserID == UserID && I.TourDateID == A.TourDateID && H.RecordStatus == "Going"
                                          select new ViewFriendPlans()
                                          {
                                              Email = H.Email,
                                              ImageURL = H.ImageURL,
                                              ThumbnailURL = H.ThumbnailURL,
                                              UserID = H.UserID,
                                              UserName = H.UserName
                                          }).ToList()
                             }).OrderBy(x => x.ArtistID).Skip(Pageindex * Pagesize).Take(Pagesize).ToList();

                RemainingRecord = RemainingRecord - _list.Count;

                var goingList = (from A in db.UserFriends
                                 join B in db.UserGoing on A.Matched_UserID equals B.UserID into AB
                                 from B in AB.DefaultIfEmpty()
                                 where A.UserID == UserID && A.Matched_UserID > 0
                                 && B.RecordStatus == "Going"
                                 select new
                                 {
                                     TourDateID = B.TourDateID
                                 }).Distinct().ToList();

                List<int?> userlist1 = new List<int?>();
                foreach (var item in goingList)
                {
                    userlist1.Add(Convert.ToInt32(item.TourDateID));
                }

                if (RemainingRecord > 0)
                {
                    var list2 = (from A in db.TicketingEventTicketConfirmation
                                 join B in db.TicketingEventsNew on A.EventID equals B.EventID into AB
                                 from B in AB.DefaultIfEmpty()
                                 join C in db.Artists on B.ArtistId equals C.ArtistID into BC
                                 from C in BC.DefaultIfEmpty()
                                 join D in db.Venue on B.VenueName equals D.VenueName into CD
                                 from D in CD.DefaultIfEmpty()
                                 join E in db.TourDate on B.EventID equals E.TicketingEventID into DE
                                 from E in DE.DefaultIfEmpty()
                                 where A.EventID > 0 && userlist1.Contains(A.TourDateID)
                                 select new ViewYourFreiendplanlst()
                                 {
                                     TourDateID = E.TourDateID,
                                     //  EventID = A.EventID,
                                     ArtistID = B.ArtistId,
                                     ArtistName = C.ArtistName,
                                     ImageURL = C.ImageURL,
                                     BannerImage_URL = C.BannerImage_URL,
                                     //StartDate = B.StartDate,
                                     //StartTime = B.StartTime,
                                     VenueID = D.VenueID,
                                     VenueName = D.VenueName,
                                     Datetime_Local = B.StartDate.Value
                                 }).Distinct().OrderBy(x => x.ArtistID).Skip(0).Take(RemainingRecord).ToList();


                    foreach (var item in list2)
                    {

                        item.Going = (from G in db.UserFriends
                                      join H in db.Users on G.Matched_UserID equals H.UserID into GH
                                      from H in GH.DefaultIfEmpty()
                                      join I in db.UserGoing on H.UserID equals I.UserID into HI
                                      from I in HI.DefaultIfEmpty()
                                      where G.UserID == UserID && I.TourDateID == item.TourDateID && I.RecordStatus == "Going" && G.Matched_UserID > 0
                                      select new ViewFriendPlans()
                                      {
                                          Email = H.Email,
                                          ImageURL = H.ImageURL,
                                          ThumbnailURL = H.ThumbnailURL,
                                          UserID = H.UserID,
                                          UserName = H.UserName
                                      }).Distinct().ToList();
                        item.GoingCount = item.Going.Count();
                        item.Date_Local = item.Datetime_Local.ToString("d");
                    }
                    _list.AddRange(list2);
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, _list, "FriendPlans"));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, ex.Message, "FriendPlans"));
            }
        }

    }
}
