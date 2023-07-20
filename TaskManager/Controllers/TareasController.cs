using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Entidades;
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

        [HttpPost]
        // <ActionResult<Tarea>> quiere decir que puede devolver o Tarea o un ActionResult
        public async Task<ActionResult<Tarea>> Post([FromBody] string titulo)
        {
            var usuarioId = servicioUsuarios.ObtenerIdUsuarioAutentificado();

            // La idea es ver si tiene tareas para el Orden en el que seran mostrados
            // Nueva tarea va al final
            // "usuarioTieneTareas" Retorna un boolean
            var usuarioTieneTareas = await dbContext.Tareas.AnyAsync(tarea => tarea.UsuarioCreadorId == usuarioId);

            var ordenMayorTarea = 0;

            if (usuarioTieneTareas)
            {
                ordenMayorTarea = await dbContext.Tareas.Where(tarea => tarea.UsuarioCreadorId == usuarioId)
                    .Select(tarea => tarea.Orden)
                    .MaxAsync();
            }

            var tarea = new Tarea()
            {
                Titulo = titulo,
                Orden = ordenMayorTarea + 1,
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreadorId = usuarioId
            };

            // Saving data with EF core
            dbContext.Tareas.Add(tarea);
            await dbContext.SaveChangesAsync();

            return tarea;
        }
    }
}
