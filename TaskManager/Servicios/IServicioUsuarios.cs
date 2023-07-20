using System.Security.Claims;

namespace TaskManager.Servicios
{
    public interface IServicioUsuarios
    {
        string ObtenerIdUsuarioAutentificado();
    }

    public class ServicioUsuarios : IServicioUsuarios
    {
        private HttpContext httpContext;

        public ServicioUsuarios(IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
        }

        public string ObtenerIdUsuarioAutentificado()
        {

            if (httpContext.User.Identity.IsAuthenticated)
            {
                // "NameIdentifier" corresponde el tipo de Claim ID.
                var idClaim = httpContext.User.Claims
                    .Where(claim => claim.Type == ClaimTypes.NameIdentifier)
                    .FirstOrDefault();

                return idClaim.Value;
            }
            else
            {
                throw new Exception("El usuario no se encuentra autentificado.");
            }

        }
    }
}
