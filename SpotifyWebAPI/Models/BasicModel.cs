using Newtonsoft.Json;
using System;

namespace SpotifyWebAPI.Web.Models
{
    public abstract class BasicModel
    {
        [JsonProperty("error")]
        public Error Error { get; set; }

        public Boolean HasError()
        {
            return Error != null;
        }
    }
}