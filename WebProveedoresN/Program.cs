using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SW.Services;
using WebProveedoresN.Data;
using WebProveedoresN.Repositories.Implementations;
using WebProveedoresN.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews()
           .AddSessionStateTempDataProvider();
builder.Services.AddSession();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation(); // Para ver los cambios en tiempo real con el NUGET Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation

// Configurar ApplicationDbContext con la cadena de conexión
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSQL1")));

// Configuración de autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Access/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.AccessDeniedPath = "/Home/Privacy";
    });

//Registrar IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

//Registrar IIPService
builder.Services.AddScoped<IIPRepository, IPRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

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
// Configurar la carpeta UploadedFiles para servir archivos estáticos
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "UploadedFiles")),
    RequestPath = "/UploadedFiles"
});
app.UseRouting();

app.UseAuthentication(); // Para autenticación con cookies

app.UseSession(); // Para usar sesiones

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Access}/{action=Login}/{id?}");

app.MapRazorPages();
app.Run();