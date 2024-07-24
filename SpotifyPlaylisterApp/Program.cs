using Microsoft.EntityFrameworkCore;
using SpotifyPlaylistApp.Data;
using SpotifyPlaylisterApp;
using SpotifyPlaylisterApp.Requests;
using Microsoft.AspNetCore.Identity;
using SpotifyPlaylisterApp.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SpotifyPlaylisterApp.Requests.auth;
var builder = WebApplication.CreateBuilder(args);

IConfigurationRoot config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("settings.json")
    .Build();
Settings settings = config.GetRequiredSection("Settings").Get<Settings>() ?? throw new Exception("Bad config");

// Add services to the container.
builder.Services.AddRazorPages(options => {
    options.Conventions.AuthorizeFolder("/");
});

if (builder.Environment.IsDevelopment()){
    builder.Services.AddDbContext<SpotifyPlaylistAppContext>(options => {
        options.UseSqlite(builder.Configuration.GetConnectionString("SpotifyPlaylistAppContext") 
            ?? throw new InvalidOperationException("Connection string 'SpotifyPlaylistAppContext' not found."));
        options.UseOpenIddict();
    });
} else {
    builder.Services.AddDbContext<SpotifyPlaylistAppContext> (options => {
        options.UseSqlServer(builder.Configuration.GetConnectionString("ProductionSpotifyPlaylisterAppContext"));
        options.UseOpenIddict();
    });
}

builder.Services.AddDefaultIdentity<SpotifyPlaylisterUser>(
        options => options.SignIn.RequireConfirmedAccount = true
    )
    .AddEntityFrameworkStores<SpotifyPlaylistAppContext>();

builder.Services.AddOpenIddict().AddClient( options => {
    options.AllowAuthorizationCodeFlow();
    options.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
    options.UseAspNetCore().EnableRedirectionEndpointPassthrough();
    options.UseWebProviders().AddSpotify(spotifyOptions => {
        spotifyOptions.SetClientId(settings.ClientID)
            .SetClientSecret(settings.Secret)
            .SetRedirectUri(settings.RedirectUri);
    });
}).AddCore(options => {
    options.UseEntityFrameworkCore().UseDbContext<SpotifyPlaylistAppContext>();
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
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});



//need to implement new auth class so that it uses authorization code flow
// builder.Services.AddHttpClient(SpotifyAuthorizationCodeAuthentifier.httpClientName, client => {
//     client.BaseAddress = new Uri(settings.AuthAPIAddress);
// });
builder.Services.AddHttpClient(SpotifyClientCredentialAuthentifier.httpClientName, client => {
    client.BaseAddress = new Uri(settings.AuthTokenEndPoint);
});
builder.Services.AddHttpClient(SpotifyAuthorizationCodeAuthentifier.httpClientName, client => {
    client.BaseAddress = new Uri(settings.AuthCodeEndpoint);
});
builder.Services.AddHttpClient(LoggedSpotifyClient.httpClientName, client => {
    client.BaseAddress = new Uri(settings.DataAPIAddress);
});

builder.Services.AddSingleton<AuthenticatorResolver>(
    provider =>
        (anonymous) => {
            if (!anonymous){
                return new SpotifyAuthorizationCodeAuthentifier(
                                        new Uri(settings.AuthCodeEndpoint),
                                        new Uri(settings.AuthTokenEndPoint),
                                        settings.ClientID,
                                        settings.Secret,
                                        settings.RedirectUri,
                                        provider.GetService<IHttpClientFactory>()!);

            } else{
                return new SpotifyClientCredentialAuthentifier(
                                        new Uri(settings.AuthTokenEndPoint),
                                        settings.ClientID,
                                        settings.Secret,
                                        provider.GetService<IHttpClientFactory>()!);
            }
        }
);

builder.Services.AddScoped<IAnonymousSpotifyClient>(provider => 
    new AnonymousSpotifyClient(
        provider.GetRequiredService<AuthenticatorResolver>()(true),
        settings.DataAPIAddress,
        provider.GetRequiredService<IHttpClientFactory>()
    )
);

builder.Services.AddScoped<ILoggedSpotifyClient>(provider => 
    new LoggedSpotifyClient(
        provider.GetRequiredService<AuthenticatorResolver>()(false),
        settings.DataAPIAddress,
        provider.GetRequiredService<IHttpClientFactory>()
    )
);

builder.Services.AddSingleton<IJsonParser<PlaylistData>, PlaylistParser>();

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