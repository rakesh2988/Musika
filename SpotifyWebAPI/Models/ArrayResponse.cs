using System.Collections.Generic;

namespace SpotifyWebAPI.Web.Models
{
    public class ListResponse<T> : BasicModel
    {
        public List<T> List { get; set; }
    }
}