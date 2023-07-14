using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManager;

var builder = WebApplication.CreateBuilder(args);

/////// ------------------------------------------------------------------- Filtro politica de usuarios autenticados
/// Esta politica se la pasamos al servicio encargado de controlar la app
/// y quiere decir que para utilizar la app debe estar autentificado.
var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

// Add services to the container.
builder.Services.AddControllersWithViews(opciones =>
{
    opciones.Filters.Add(new AuthorizeFilter(politicaUsuariosAutenticados)); 
});

/////// --------------------------------------------------------------- Servicios
///
// CONFIGURACION SERVICIO DBCONTEXT EF CORE 7
builder.Services.AddDbContext<ApplicationDbContext>(opciones => opciones.UseSqlServer("name = DefaultConnection"));

// Activacion de servicios de autenticacion
// Agrega una autenticacion externa, en este caso de Microsoft
builder.Services.AddAuthentication().AddMicrosoftAccount(opciones =>
{
    opciones.ClientId = builder.Configuration["MicrosoftClientId"];
    opciones.ClientSecret = builder.Configuration["MicrosoftSecretId"];
});

// Activacion servicio para usar Identity (debe estar activo el servicio de autenticacion de arriba)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(opciones =>
{
    // No necesita confirmacion de cuenta para el registro de usuario
    opciones.SignIn.RequireConfirmedAccount = false;

}).AddEntityFrameworkStores<ApplicationDbContext>() // Enlaza Identity con EF Core
.AddDefaultTokenProviders();

// Para usar mis propias clases y enlaces y no los predefinidos por Identity
builder.Services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme,
    opciones =>
    {
        opciones.LoginPath = "/Usuarios/Login";
        opciones.AccessDeniedPath = "/Usuarios/Login";
    });

var app = builder.Build();
////// ------------------------------------------------------------------------------------------------------------------------
///

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

////// ------------------------------------------------------------------------ Midlewares
///
app.UseAuthentication(); // nos permite utilizar la data del usuario autenticado
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
