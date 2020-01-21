using Newtonsoft.Json;

namespace SpotifyWebAPI.Web.Models
{
    public class FollowedArtists : BasicModel
    {
        [JsonProperty("artists")]
        public CursorPaging<FullArtist> Artists { get; set; }
    }
}