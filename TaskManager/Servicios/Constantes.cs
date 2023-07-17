using Microsoft.AspNetCore.Mvc.Rendering;

namespace TaskManager.Servicios
{
    public class Constantes
    {
        public const string ROL_ADMIN = "admin";

        public static readonly SelectListItem[] CulturasUISoportadas = new SelectListItem[]
        {
            new SelectListItem {Value = "es", Text = "Espaniol"},
            new SelectListItem {Value = "en", Text = "English"}
        };
    }
}
