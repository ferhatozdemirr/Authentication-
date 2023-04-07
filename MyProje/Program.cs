using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MyProje.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddDbContext<DatabaseContext>(opts =>
{
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    //opts.UseLazyLoadingProxies();
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opts =>
    {
        opts.Cookie.Name = ".HotelDemo.auth";  //istedi�ini yazabilirsin buraya �ok �nemli de�il
        opts.ExpireTimeSpan = TimeSpan.FromDays(7);  //Cokie s�resi 
        opts.SlidingExpiration = false;             //cookinin s�resi uzas�n m� ? hay�r false
        opts.LoginPath = "/Account/Login";           //Login oldugunda gidece�i sayfa
        opts.LogoutPath = "/Account/Logout";        //Logout oldugunda gidilecek sayfa
        opts.AccessDeniedPath = "/Home/AccessDenied";  //yetkisi olmad���nda gidece�i sayfa

    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
