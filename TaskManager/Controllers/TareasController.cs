using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Entidades;
using TaskManager.Models;
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

        [HttpGet]
        public async Task<List<TareaDTO>> Get()
        {
            var usuarioId = servicioUsuarios.ObtenerIdUsuarioAutentificado();

            // Devuelve un objeto del tipo TareaDTO con los datos necesarios
            // que seran transformados en JSON desde tarea.js para ocuparlos
            var tareas = await dbContext.Tareas
                .Where(tarea => tarea.UsuarioCreadorId == usuarioId)
                .Include(tarea => tarea.PasoTareas)
                .OrderBy(tarea => tarea.Orden)
                .Select(tarea => new TareaDTO
                {
                    Id = tarea.Id,
                    Titulo = tarea.Titulo,
                    TotalPasos = tarea.PasoTareas.Count(),
                    PasosRealizados = tarea.PasoTareas.Where(p => p.Realizado == true).Select(p => p.Realizado).Count()
                })
                .ToListAsync();

            return tareas;
        }

        // Ordena las tareas usando Jquery UI, Identity y EF core.
        [HttpPost]
        [Route("ordenar")]
        public async Task<IActionResult> OrdenarTareas([FromBody] int[] idsTareas)
        {
            // Verificar las tareas del usuario
            var usuarioId = servicioUsuarios.ObtenerIdUsuarioAutentificado();
            var tareas = await dbContext.Tareas.Where(tarea => tarea.UsuarioCreadorId == usuarioId).ToListAsync();

            var tareasIds = tareas.Select(t => t.Id);

            // Validando el idsTareas del front
            var idsTareasNoDelUsuario = idsTareas.Except(tareasIds).ToList();
            if (idsTareasNoDelUsuario.Any())
            {
                return Forbid();
            }

            // Con el diccionario podemos obtener la tarea directamente con
            // el id, que es lo que se hace en el siguiente for()
            var idsTareasDictionary = tareas.ToDictionary(t => t.Id);

            for (int i = 0; i < idsTareas.Length; i++)
            {
                var idTarea = idsTareas[i];
                var tarea = idsTareasDictionary[idTarea];
                tarea.Orden = i + 1;
            }

            // Confirmando los cambios
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Tarea>> Get(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerIdUsuarioAutentificado();

            var tarea = await dbContext.Tareas
                .Include(tarea => tarea.PasoTareas)
                .FirstOrDefaultAsync(t => t.UsuarioCreadorId == usuarioId && t.Id == id);
            if (tarea is null)
            {
                return NotFound();
            }

            return tarea;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Tarea>> Put(int id, [FromBody] TareaEditarDTO tarea)
        {

            var usuarioId = servicioUsuarios.ObtenerIdUsuarioAutentificado();
            var tareaPorEditar = await dbContext.Tareas.FirstOrDefaultAsync(t => t.UsuarioCreadorId == usuarioId && t.Id == tarea.Id);
            if (tareaPorEditar is null)
            {
                return NotFound();
            }

            tareaPorEditar.Titulo = tarea.Titulo;
            tareaPorEditar.Descripcion = tarea.Descripcion;

            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> EliminarTarea(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerIdUsuarioAutentificado();
            var tarea = await dbContext.Tareas.FirstOrDefaultAsync(t => t.UsuarioCreadorId == usuarioId && t.Id == id);
            
            if (tarea is null)
            {
                return NotFound();
            }

            dbContext.Remove(tarea);
            await dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
