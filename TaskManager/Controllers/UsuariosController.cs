using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Login()
        {
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
    }
}

// SignInManager trae propiedades y metodos propios tales como para
// login como: PasswordSignInAsync()
// Al parecer es mas facil usar el HttpContext.SignOutAsync() para desloguear;