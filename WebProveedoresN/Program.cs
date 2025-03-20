using Microsoft.AspNetCore.Authentication.Cookies;
using WebProveedoresN.Interfaces;
using WebProveedoresN.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation(); // Para ver los cambios en tiempo real con el NUGET Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) // Para autenticación con cookies
    .AddCookie(options =>
    {
        options.LoginPath = "/Access/Login";
        options.ExpireTimeSpan = System.TimeSpan.FromMinutes(30);
        options.AccessDeniedPath = "/Home/Privacy";
    });
//Registrar IHttpContextAccessor
builder.Services.AddHttpContextAccessor();
//Registrar IIPService
builder.Services.AddScoped<IIPService, IPService>();

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

app.UseAuthentication(); // Para autenticación con cookies

app.UseAuthorization();

//app.UseSession(); // Para usar sesiones

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Access}/{action=Login}/{id?}");

app.Run();
