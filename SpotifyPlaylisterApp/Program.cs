using Microsoft.EntityFrameworkCore;
using SpotifyPlaylisterApp;
using SpotifyPlaylisterApp.Requests;
using Microsoft.AspNetCore.Identity;
using SpotifyPlaylisterApp.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SpotifyPlaylisterApp.Requests.Auth;
using SpotifyPlaylisterApp.Data;
using SpotifyPlaylisterApp.Pages.MyPlaylists;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using SpotifyPlaylisterApp.Authorization;
using SpotifyPlaylisterApp.Models;
using System.Configuration;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;
var builder = WebApplication.CreateBuilder(args);

IConfigurationRoot config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("settings.json")
    .Build();
Settings settings = config.GetRequiredSection("Settings").Get<Settings>() ?? throw new Exception("Bad config");

// Add services to the container.
builder.Services.AddRazorPages(options => {
//    options.Conventions.AuthorizeFolder("/");
});

builder.Services.AddHttpContextAccessor();

if (builder.Environment.IsDevelopment()){
    builder.Services.AddDbContext<SpotifyPlaylisterAppContext>(options => {
        options.UseSqlite(builder.Configuration.GetConnectionString("SpotifyPlaylistAppContext") 
            ?? throw new InvalidOperationException("Connection string 'SpotifyPlaylistAppContext' not found."));
        options.UseOpenIddict();
    });
} else {
    builder.Services.AddDbContext<SpotifyPlaylisterAppContext> (options => {
        options.UseSqlServer(builder.Configuration.GetConnectionString("ProductionSpotifyPlaylisterAppContext"));
        options.UseOpenIddict();
    });
}

builder.Services.AddDefaultIdentity<SpotifyPlaylisterUser>(
        options => { options.SignIn.RequireConfirmedAccount = true; }
    )
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<SpotifyPlaylisterAppContext>();

builder.Services.AddAuthorization(options => {
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddScoped<IAuthorizationHandler, PlaylistDetailsAuthorizationHandler>();

builder.Services.AddOpenIddict()
    .AddCore(options => {
        options.UseEntityFrameworkCore().UseDbContext<SpotifyPlaylisterAppContext>();
    })
    .AddClient( options => {
        options.AllowAuthorizationCodeFlow();
        options.AllowRefreshTokenFlow();
        options.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
        options.UseAspNetCore()
            .EnableRedirectionEndpointPassthrough()
            .EnableStatusCodePagesIntegration();
            //this causes breakes challenge, but is required for properly deactivating spotify as external auth
            //todo : change challenge to allow this
            //.DisableAutomaticAuthenticationSchemeForwarding();
            
        options.UseSystemNetHttp();
        options.UseWebProviders().AddSpotify(spotifyOptions => {
            spotifyOptions.SetClientId(settings.ClientID)
                .SetClientSecret(settings.Secret)
                .SetRedirectUri(settings.RedirectUri).SetProviderName(Providers.Spotify);
        });
    });

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);

    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.AddHttpClient(SpotifyClientCredentialAuthentifier.httpClientName, client => {
    client.BaseAddress = new Uri(settings.AuthTokenEndPoint);
});
// builder.Services.AddHttpClient(SpotifyOpenIddictAuthentifier.httpClientName, client => {
//     client.BaseAddress = new Uri(settings.AuthCodeEndpoint);
// });
builder.Services.AddHttpClient(LoggedSpotifyClient.httpClientName, client => {
    client.BaseAddress = new Uri(settings.DataAPIAddress);
});

builder.Services.AddTransient<AuthenticatorResolver>(
    provider => {
        return (anonymous) => {
            if (!anonymous){
                return new SpotifyOpenIddictAuthentifier(
                                        provider.GetRequiredService<SpotifyPlaylisterAppContext>(),
                                        provider.GetRequiredService<IHttpContextAccessor>(),
                                        provider.GetRequiredService<OpenIddict.Client.OpenIddictClientService>());
            } else{
                return new SpotifyClientCredentialAuthentifier(
                                        new Uri(settings.AuthTokenEndPoint),
                                        settings.ClientID,
                                        settings.Secret,
                                        provider.GetService<IHttpClientFactory>()!);
            }
        };
    }
);

//for home page public api access using client credentials
builder.Services.AddScoped<IAnonymousSpotifyClient>(provider => 
    new AnonymousSpotifyClient(
        provider.GetRequiredService<AuthenticatorResolver>()(true),
        settings.DataAPIAddress,
        provider.GetRequiredService<IHttpClientFactory>(),
        provider.GetRequiredService<IJsonParser<PlaylistData>>(),
        provider.GetRequiredService<IJsonParser<PlaylistTracksData>>()
    )
);

//for user playlist index
builder.Services.AddScoped<ILoggedSpotifyClient>(provider => 
    new LoggedSpotifyClient(
        provider.GetRequiredService<IHttpContextAccessor>(),
        provider.GetRequiredService<SpotifyPlaylisterAppContext>(),
        provider.GetRequiredService<AuthenticatorResolver>()(false),
        settings.DataAPIAddress,
        settings.Scopes,
        provider.GetRequiredService<IHttpClientFactory>(),
        provider.GetRequiredService<UserManager<SpotifyPlaylisterUser>>(),
        provider.GetRequiredService<IJsonParser<PlaylistIdList>>(),
        provider.GetRequiredService<IJsonParser<PlaylistData>>(),
        provider.GetRequiredService<IJsonParser<PlaylistTracksData>>()
    )
);

//for parsing spotify api json
builder.Services.AddSingleton<IJsonParser<PlaylistData>, PlaylistParser>();
builder.Services.AddSingleton<IJsonParser<PlaylistIdList>, PlaylistIdParser>();
builder.Services.AddSingleton<IJsonParser<PlaylistTracksData>, PlaylistTracksParser>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/StatusCode", "?statusCode={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

namespace SpotifyPlaylisterApp
{
    public delegate IAuthenticationProvider AuthenticatorResolver(bool anonymous);
}