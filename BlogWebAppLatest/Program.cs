using BlogWebApp.ExceptionHandling;
using BlogWebApp.HelperClass;
using BlogWebApp.Hubs;
using BlogWebApp.MiddlewareExtensions;
using BlogWebApp.Models.IdentityModel;
using BlogWebApp.SubscribeTableDependencies;
using BlogWebApp.ViewModel;
using BlogWebAppLatest.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SignalRYoutube.Repos;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

//var notificationConnectionString = builder.Configuration.GetConnectionString("NotificationConnection") ?? throw new InvalidOperationException("Connection string 'NotificationConnection' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(notificationConnectionString),
//    ServiceLifetime.Singleton);

// Register ApplicationDbContext as scoped for other services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<User, Role>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders()
     .AddSignInManager()
    .AddRoles<Role>();

// builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

//DI
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<NotificationHub>();
builder.Services.AddScoped<INotificationService, NotificationService>();
//builder.Services.AddScoped<SubscribeNotificationTableDependency>();
builder.Services.AddSingleton<SubscribeNotificationTableDependency>();

builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

//register the global exxception handler
//builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Session
//builder.Services.AddDistributedMemoryCache();
//builder.Services.AddSession();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/Home/Error");
    //app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//app.UseExceptionHandler("/Home/Error");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
//app.UseRedirectMiddleware();

app.UseAuthorization();
app.MapHub<NotificationHub>("/notificationHub");

//app.UseExceptionHandler("/Home/Error");

#pragma warning disable ASP0014 // Suggest using top level route registrations
app.UseEndpoints(endpoints =>
{
    // Map the default MVC controller route
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Blog}/{action=Index}/{id?}");

    // Map Razor Pages
    endpoints.MapRazorPages();

    // Add additional route for another controller
    endpoints.MapControllerRoute(
        name: "custom",
        pattern: "custom/{controller}/{action}/{id?}",
        defaults: new { controller = "Custom", action = "Index" });

    // Map fallback route for error page
    endpoints.MapFallbackToController("Error", "Dashboard");

});
#pragma warning restore ASP0014 // Suggest using top level route registrations
app.UseSqlTableDependency<SubscribeNotificationTableDependency>(connectionString);

app.Run();
