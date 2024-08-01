using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SpotifyPlaylisterApp.Models;

namespace SpotifyPlaylisterApp.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the SpotifyPlaylisterUser class
    public class SpotifyPlaylisterUser : IdentityUser
    {
        [PersonalData]
        public List<Playlist> Playlists {get; set;} = [];

        public string? SpotifyAccessToken {get; set;} 

        public string? SpotifyRefreshToken {get; set;}

        public DateTime? SpotifyAccessTokenExpiration {get; set;}

    }

}
