using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using TaskManager;
using TaskManager.Servicios;

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

    //Configurando "IViewLocalizer" para modificar las vistas segun idioma.
    // LanguageViewLocationExpanderFormat.Suffix quiere decir que los
    // archivos de recursos tendran el sufijo del idioma (.en, .es)
}).AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
.AddDataAnnotationsLocalization(opciones =>
{
    // Esto permite utilizar un unico archivo de recursos para traducir
    // las anotaciones de datos (datos dentro de un formulario como login)
    // y el nombre del archivo de recursos es RecursoCompartido (es como lo mismo que
    // IViewLocalizer pero con las anotaciones de datos.)
    // Esto ya que en login y registro, etc. Comparten la misma informacion entonces
    // para no crear un archivo .resx para cada uno, se crea uno para todos.
    opciones.DataAnnotationLocalizerProvider = (_, factoria) =>
        factoria.Create(typeof(RecursoCompartido));
}).AddJsonOptions(opciones =>
{
    // Esta opciones le dice al serializador JSON que cuando un objeto tenga alguna
    // referencia ciclica, lo ignore. Por ejemplo hay una referencia ciclica entre
    // Tarea y PasoTarea ya que cada una hace referencia a la otra.
    opciones.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
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

// Activa el analisis de la localizacion del usuario, para usar IStringLocalizer
builder.Services.AddLocalization(opciones =>
{
    // ResourcesPath buscar el path donde se encuentran los archivos resources
    // creados para utilizarse dependiendo el idioma del navegador del usuario.
    opciones.ResourcesPath = "Recursos";
});

builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();

// Servicios para manejo de archivos en Azure y Local
builder.Services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosAzure>();
//builder.Services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();


var app = builder.Build();
////// ------------------------------------------------------------------------------------------------------------------------

// Obtiene la localizacion del usuario
app.UseRequestLocalization(opciones =>
{
    opciones.DefaultRequestCulture = new RequestCulture("es");
    opciones.SupportedUICultures = Constantes.CulturasUISoportadas
        .Select(cultura => new CultureInfo(cultura.Value)).ToList();
});

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
