using Musika.Library.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Musika.Areas.Admin.Controllers
{
    public class SiteAdminController : Controller
    {
        // GET: Admin/SiteAdmin
        [HttpPost]
        public AdminResponse updateEvent()
        {

            //var file = HttpContext.Current.Request.Files.Count > 0 ?
            //  HttpContext.Current.Request.Files[0] : null;


            var httpContext = Request.Form["Address"];

            //if (file != null && file.ContentLength > 0)
            //{
            //    var fileName = Path.GetFileName(file.FileName);

            //    var path = Path.Combine(HttpContext.Current.Server.MapPath("~/uploads"), fileName);
            //}

            //HttpPostedFileBase _HttpPostedFileBase= null;

            //if (data.userfile != null)
            //{
            //    _HttpPostedFileBase =(HttpPostedFileBase)data.userfile;
            //}

            AdminResponse _AdminResponse = new AdminResponse();

            //try
            //{

            //    Models.Artists _Artists = null;
            //    GenericRepository<Artists> _ArtistsRepo = new GenericRepository<Artists>(_unitOfWork);


            //    int ArtistID = Numerics.GetInt(data.ArtistID);
            //    _Artists = _ArtistsRepo.Repository.Get(p => p.ArtistID == ArtistID);

            //    if (_Artists != null)
            //    {
            //        _Artists.ArtistName = (data.ArtistName != null) ? data.ArtistName.ToString() != "" ? data.ArtistName : _Artists.ArtistName : _Artists.ArtistName;
            //        _Artists.Musicgraph_ID = (data.Musicgraph_ID != null) ? data.Musicgraph_ID.ToString() != "" ? data.Musicgraph_ID : _Artists.Musicgraph_ID : _Artists.Musicgraph_ID;
            //        _Artists.About = (data.About != null) ? data.About.ToString() != "" ? data.About : _Artists.About : _Artists.About;
            //        _Artists.Spotify_ID = (data.Spotify_ID != null) ? data.Spotify_ID.ToString() != "" ? data.Spotify_ID : _Artists.Spotify_ID : _Artists.Spotify_ID;
            //        _Artists.Instagram_ID = (data.Instagram_ID != null) ? data.Instagram_ID.ToString() != "" ? data.Instagram_ID : _Artists.Instagram_ID : _Artists.Instagram_ID;
            //        _Artists.OnTour = (data.OnTour != null) ? data.OnTour.ToString() != "" ? data.OnTour : _Artists.OnTour : _Artists.OnTour;


            //        _ArtistsRepo.Repository.Update(_Artists);

            //    _AdminResponse.Status = true;
            //    _AdminResponse.RetMessage = "Artist updated successfully.";
            //    return _AdminResponse;
            //}
            //else
            //{
            //    _AdminResponse.Status = false;
            //    _AdminResponse.RetMessage = "Invalid Artist";
            //    return _AdminResponse;
            //}


            //}
            //catch (Exception ex)
            //{
            _AdminResponse.Status = false;
            // _AdminResponse.RetMessage = ex.Message;
            return _AdminResponse;

            //}
        }

    }
}