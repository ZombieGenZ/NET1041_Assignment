using Assignment.Models;
using Assignment.Service;
using Assignment.Utilities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.UseUrls("https://*:443");

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenLocalhost(3000, listenOptions =>
//    {
//        listenOptions.UseHttps();
//    });
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policyBuilder =>
        {
            policyBuilder.SetIsOriginAllowed(origin => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "WebAuth";
    options.DefaultChallengeScheme = "WebAuth";
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? string.Empty;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
    options.Scope.Add("profile");
    options.SignInScheme = "ExternalCookie";
    options.Events.OnCreatingTicket = async context =>
    {
        await Task.CompletedTask;
    };
    options.CallbackPath = "/signin-google";
})
.AddFacebook(options =>
{
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"] ?? string.Empty;
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ?? string.Empty;
    options.SignInScheme = "ExternalCookie";
    options.Fields.Add("picture");
    options.Fields.Add("email");
    options.Events.OnCreatingTicket = async context =>
    {
        await Task.CompletedTask;
    };
    options.CallbackPath = "/signin-facebook";
})
.AddGitHub(options =>
{
    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"] ?? string.Empty;
    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"] ?? string.Empty;
    options.SignInScheme = "ExternalCookie";
    options.Scope.Add("user:email");
    options.Events.OnCreatingTicket = async context =>
    {
        await Task.CompletedTask;
    };
    options.CallbackPath = "/signin-github";
})
.AddDiscord(options =>
{
    options.ClientId = builder.Configuration["Authentication:Discord:ClientId"] ?? string.Empty;
    options.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"] ?? string.Empty;
    options.SignInScheme = "ExternalCookie";
    options.Scope.Add("identify");
    options.Scope.Add("email");
    options.Events.OnCreatingTicket = async context =>
    {
        await Task.CompletedTask;
    };
    options.CallbackPath = "/signin-discord";
})
.AddCookie("WebAuth", options =>
{
    options.Cookie.Name = "Authorization";
    options.LoginPath = "/login";
    options.LogoutPath = "/api/users/logout";
    options.AccessDeniedPath = "/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;

    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
})
.AddCookie("ExternalCookie", options =>
{
    options.Cookie.Name = "ExternalLogin";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthorization(options =>
{
    // Policy cho Web - chỉ chấp nhận Cookie authentication
    options.AddPolicy("WebPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("WebAuth");
        policy.RequireAuthenticatedUser();
    });

    // Policy cho Admin - chấp nhận cả JWT và Cookie nhưng yêu cầu role Admin
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("WebAuth");
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin");
    });

    // Policy cho Shipper - chấp nhận cả JWT và Cookie với role Shipper hoặc Admin
    options.AddPolicy("ShipperPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("WebAuth");
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Shipper", "Admin");
    });

    // Policy cho Customer - chấp nhận cả JWT và Cookie với role Customer, Shipper hoặc Admin
    options.AddPolicy("UserPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("WebAuth");
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Customer", "Shipper", "Admin");
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.Configure<GeminiSetting>(
    builder.Configuration.GetSection("GeminiSetting"));

builder.Services.AddScoped<GeminiApiClient>();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<RealtimeHub>("/realtime-hub");

app.Run();
