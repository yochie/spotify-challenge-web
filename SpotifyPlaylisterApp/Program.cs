using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SpotifyPlaylistApp.Data;
using SpotifyPlaylisterApp;
using SpotifyPlaylisterApp.Requests;
using Microsoft.AspNetCore.Identity;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
if (builder.Environment.IsDevelopment()){
    builder.Services.AddDbContext<SpotifyPlaylistAppContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("SpotifyPlaylistAppContext") ?? throw new InvalidOperationException("Connection string 'SpotifyPlaylistAppContext' not found.")));
} else {
    builder.Services.AddDbContext<SpotifyPlaylistAppContext> (options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("ProductionSpotifyPlaylisterAppContext")));
}

builder.Services.AddDefaultIdentity<IdentityUser>(
        options => options.SignIn.RequireConfirmedAccount = true
    )
    .AddEntityFrameworkStores<SpotifyPlaylistAppContext>();
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


IConfigurationRoot config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("settings.json")
    .Build();
Settings settings = config.GetRequiredSection("Settings").Get<Settings>() ?? throw new Exception("Bad config");

//need to implement new auth class so that it uses authorization code flow
// builder.Services.AddHttpClient(SpotifyAuthorizationCodeAuthentifier.httpClientName, client => {
//     client.BaseAddress = new Uri(settings.AuthAPIAddress);
// });
builder.Services.AddHttpClient(SpotifyClientCredentialAuthentifier.httpClientName, client => {
    client.BaseAddress = new Uri(settings.AuthAPIAddress);
});
builder.Services.AddHttpClient(SpotifyClient.httpClientName, client => {
    client.BaseAddress = new Uri(settings.DataAPIAddress);
});
builder.Services.AddScoped<IAuthenticationProvider>(provider =>
    new SpotifyClientCredentialAuthentifier(
        settings.AuthAPIAddress,
        settings.ClientID,
        settings.Secret,
        provider.GetService<IHttpClientFactory>()!
    )
);
builder.Services.AddScoped<ISpotifyClient>(provider => 
    new SpotifyClient(
        provider.GetService<IAuthenticationProvider>()!,
        settings.DataAPIAddress,
        provider.GetService<IHttpClientFactory>()!
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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

