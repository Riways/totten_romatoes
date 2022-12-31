using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Text.Json;
using totten_romatoes.Server.Data;
using totten_romatoes.Server.Services;
using totten_romatoes.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("ru-RU"),
            };

builder.Configuration.AddJsonFile(Path.Combine(Environment.CurrentDirectory, "config", "secrets.json"));

// Add services to the container.
var connectionString = builder.Configuration["DefaultConnection"];
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
        options.SignIn.RequireConfirmedAccount = false; 
        options.SignIn.RequireConfirmedEmail= false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityServer()
    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>(options =>
    {
        options.IdentityResources["openid"].UserClaims.Add("name");
        options.ApiResources.Single().UserClaims.Add("name");
        options.IdentityResources["openid"].UserClaims.Add("role");
        options.ApiResources.Single().UserClaims.Add("role");
    });

builder.Services.AddAuthentication(options =>
{
    options.RequireAuthenticatedSignIn= false;
})
    .AddIdentityServerJwt()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["google_client_id"];
        options.ClientSecret = builder.Configuration["google_client_secret"];
    })
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = builder.Configuration["microsoft_client_id"];
        options.ClientSecret = builder.Configuration["microsoft_client_secret"];
    });

builder.Services.AddTransient<IDropboxService, DropboxService>();
builder.Services.AddTransient<IImageService, ImageService>();
builder.Services.AddTransient<IReviewService,ReviewService>();
builder.Services.AddTransient<ISubjectService,SubjectService>();
builder.Services.AddTransient<IUserService, UserService>();

builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddRazorPages();

builder.Services.AddLocalization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});


app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
