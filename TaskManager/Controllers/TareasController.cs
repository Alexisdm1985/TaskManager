using Microsoft.AspNetCore.Mvc;
using TaskManager.Servicios;

namespace TaskManager.Controllers
{
    [Route("api/tareas")]
    public class TareasController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IServicioUsuarios servicioUsuarios;

        public TareasController(ApplicationDbContext dbContext,
            IServicioUsuarios servicioUsuarios)
        {
            this.dbContext = dbContext;
            this.servicioUsuarios = servicioUsuarios;
        }
    }
}
