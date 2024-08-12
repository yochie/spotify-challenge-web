using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SpotifyPlaylisterApp.Areas.Identity.Data;
using SpotifyPlaylisterApp.Models;

namespace SpotifyPlaylisterApp.Authorization;

public class PlaylistDetailsAuthorizationHandler : 
    AuthorizationHandler<PlaylistOwnerRequirement, Playlist>
{
    private readonly UserManager<SpotifyPlaylisterUser> _userManager;

    public PlaylistDetailsAuthorizationHandler(UserManager<SpotifyPlaylisterUser> userManager){
        _userManager = userManager;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   PlaylistOwnerRequirement requirement,
                                                   Playlist playlist)
    {
        if(playlist is null || context.User is null){
            return Task.CompletedTask;
        }
        string userId = _userManager.GetUserId(context.User) ?? "";
        if (userId == ""){
            return Task.CompletedTask;
        }

        if (playlist.SpotifyPlaylisterUsers.Select(u => u.Id).Contains(userId))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class PlaylistOwnerRequirement : IAuthorizationRequirement { }