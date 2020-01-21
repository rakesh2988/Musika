using Newtonsoft.Json;
using System;

namespace SpotifyWebAPI.Web.Models
{
    public class Snapshot : BasicModel
    {
        [JsonProperty("snapshot_id")]
        public String SnapshotId { get; set; }
    }
}