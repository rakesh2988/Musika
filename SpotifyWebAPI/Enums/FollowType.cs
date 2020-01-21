using System;

namespace SpotifyWebAPI.Web.Enums
{
    [Flags]
    public enum FollowType
    {
        [String("artist")]
        Artist = 1,

        [String("user")]
        User = 2
    }
}