using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        // IdentityUser es la clase para Usuarios configurada en programs.cs
        // Si tuviera otra clase para usuarios iria aqui luego de UserManager
        public UsuariosController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(RegistroViewModel modeloRegistro)
        {
            if (!ModelState.IsValid)
            {
                return View(modeloRegistro);
            }

            // "Crea" un usuario con identity
            var usuario = new IdentityUser()
            {
                UserName = modeloRegistro.Email,
                Email = modeloRegistro.Email
            };

            // Registra/Guarda el usuario de Identity en la base de datos
            var resultado = await userManager.CreateAsync(usuario, password: modeloRegistro.Password);

            // Si se registra correctamente, el usuario es logueado
            if (resultado.Succeeded)
            {
                await signInManager.SignInAsync(usuario, isPersistent: true);
                return RedirectToAction("Index", "Home");

            }
            else
            {
                foreach (var error in resultado.Errors)
                {
                    // Agrega los errores al modelo
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(modeloRegistro);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string mensajeError = null)
        {
            if (mensajeError != null)
            {
                ViewData["mensajeError"] = mensajeError;
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel modeloRegistro)
        {
            if (!ModelState.IsValid)
            {
                return View(modeloRegistro);
            }

            // PasswordSignInAsync: intenta loguear con una combinacion de username y password 
            var resultadoLogueo = await signInManager.PasswordSignInAsync(
                modeloRegistro.Email,
                modeloRegistro.Password,
                isPersistent: true,
                lockoutOnFailure: false);

            if (resultadoLogueo.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Email o Password incorrecto.");
                return View(modeloRegistro);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public ChallengeResult LoginExterno(string proveedor, string urlRetorno = null)
        {
            // Luego de la autentificacion regresa a este URL
            var urlRedireccion = Url.Action("RegistrarUsuarioExterno",
                values: new { urlRetorno });

            var externalAuthProperties = signInManager
                .ConfigureExternalAuthenticationProperties(proveedor, urlRedireccion);

            return new ChallengeResult(proveedor, externalAuthProperties);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RegistrarUsuarioExterno(string urlRetorno = null,
            string remoteError = null)
        {
            /////-----------------------------------------------------------------------------
            /// SECCION VALIDACIONES:
            /// si trae error y si es posible obtener la informacion de login

            var mensajeVista = "";

            // Si SignInManager da error al autentificar con el proveedor
            if (remoteError is not null)
            {
                mensajeVista = $"Error del proveedor externo: {remoteError}";
                return RedirectToAction("Login", routeValues: new { mensajeVista });
            }

            var loginInfo = await signInManager.GetExternalLoginInfoAsync();
            if (loginInfo is null)
            {
                mensajeVista = "Error cargando la data de login externo";
                return RedirectToAction("Login", routeValues: new { mensajeVista });
            }

            //// -----------------------------------------------------------------------------
            /// SECCION LOGUEO
            /// Ahora con las credenciales en "loginInfo", intentamos loguear al usuario externamente
            /// en el caso de que ya existiese en nuestra DB, si no, entonces se crea como nuevo usuario.

            urlRetorno = urlRetorno ?? Url.Content("~/");

            var resultadoLoginExerno = await signInManager.ExternalLoginSignInAsync(
                loginInfo.LoginProvider,
                loginInfo.ProviderKey,
                isPersistent: true,
                bypassTwoFactor: true);

            // Si el Login es exitoso
            if (resultadoLoginExerno.Succeeded)
            {
                return LocalRedirect(urlRetorno);
            }

            //// -------------------------------------------------------------
            /// SECCION REGISTRO USUARIO

            bool loginInfoHasEmailClaim = loginInfo.Principal.HasClaim(claim => claim.Type == ClaimTypes.Email) ? true : false;
            if (!loginInfoHasEmailClaim)
            {
                mensajeVista = "Error leyendo el Email del usuario del proveedor";
                return RedirectToAction("Login", routeValues: new { mensajeVista });
            }

            var email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email);
            var usuario = new IdentityUser { Email = email, UserName = email };

            var resultadoCreacionUsuario = await userManager.CreateAsync(usuario);

            if (!resultadoCreacionUsuario.Succeeded)
            {
                mensajeVista = resultadoCreacionUsuario.Errors.First().Description;
                return RedirectToAction("Login", routeValues: new { mensajeVista });
            }

            // Una vez creado el usuario, se agrega un login externo al usuario
            // guardando la informacion del proveedor en nuestra DB, para
            // loguear al nuevo usuario usando el proveedor externo como autenticacion en nuestra app.
            var resultadoAgregarLogin = await userManager.AddLoginAsync(usuario, loginInfo);

            if (!resultadoAgregarLogin.Succeeded)
            {
                mensajeVista = $"Ha ocurrido un error agregando el Login Externo: {resultadoAgregarLogin.Errors.First().Description}";
                return RedirectToAction("Login", routeValues: new { mensajeVista });
            }

            // Procede a loguear
            await signInManager.SignInAsync(usuario, isPersistent: true, loginInfo.LoginProvider);
            return LocalRedirect(urlRetorno);
        }   
    }
}

// SignInManager trae propiedades y metodos propios tales como para
// login como: PasswordSignInAsync()
// Al parecer es mas facil usar el HttpContext.SignOutAsync() para desloguear;
// Claims, contienen credenciales de usuarios o applicaciones externas y viene en System.Security.Claims;

//// --- Conexion cuenta externa Microsoft (Azure)
/// Registrar nuestra app en Azure = https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/ApplicationsListBlade
/// Copiar el client Id que nos da Azure (Application (client) ID)
/// Crear un "secreto" en Azure certificates and secrets y nos copiamos el valor (no el secret id)
/// Microsoft dispone de un Framework para OAuth2, por lo que instalamos "AspNetCore.Authentication.MicrosoftAccount" con NuGet.
/// https://www.udemy.com/course/aprende-aspnet-core-mvc-haciendo-proyectos-desde-cero/learn/lecture/34630098#overview