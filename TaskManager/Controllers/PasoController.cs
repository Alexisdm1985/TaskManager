using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Entidades;
using TaskManager.Models;
using TaskManager.Servicios;

namespace TaskManager.Controllers
{
    [Route("api/pasos")]
    public class PasoController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IServicioUsuarios servicioUsuarios;

        public PasoController(ApplicationDbContext dbContext, IServicioUsuarios servicioUsuarios)
        {
            this.dbContext = dbContext;
            this.servicioUsuarios = servicioUsuarios;
        }

        [HttpPost("{idTarea:int}")]
        public async Task<ActionResult<PasoTarea>> AgregarPaso(int idTarea, [FromBody] CrearPasoTareaDTO pasoTareaDTO)
        {
            var usuarioId = servicioUsuarios.ObtenerIdUsuarioAutentificado();
            
            var tarea = await dbContext.Tareas.FirstOrDefaultAsync(t => t.Id == idTarea);
            if (tarea is null)
            {
                return NotFound();
            }

            if (tarea.UsuarioCreadorId != usuarioId)
            {
                return Forbid();
            }

            // Logica para el ordenamiento en el front
            var ordenMayor = 0;

            var tareaTienePasos = await dbContext.PasoTareas.AnyAsync(p => p.TareaId == idTarea);
            
            if (tareaTienePasos)
            {
                ordenMayor = await dbContext.PasoTareas.Where(p => p.TareaId == idTarea)
                    .Select(p => p.Orden)
                    .MaxAsync();
            }

            // Creacion nueva tarea
            var nuevoPaso = new PasoTarea();
            nuevoPaso.Descripcion = pasoTareaDTO.Descripcion;
            nuevoPaso.Realizado = pasoTareaDTO.Realizado;
            nuevoPaso.TareaId = idTarea;
            nuevoPaso.Orden = ordenMayor + 1;

            dbContext.Add(nuevoPaso);
            await dbContext.SaveChangesAsync();

            return nuevoPaso;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarPaso(Guid id,[FromBody] CrearPasoTareaDTO pasoActualizado)
        {
            var usuarioId = servicioUsuarios.ObtenerIdUsuarioAutentificado();

            var paso = await dbContext.PasoTareas
                .Include(paso => paso.Tarea)
                .FirstOrDefaultAsync(paso => paso.Id == id);

            if (paso is null)
            {
                return NotFound();
            }

            if (paso.Tarea.UsuarioCreadorId != usuarioId)
            {
                return Forbid();
            }

            paso.Descripcion = pasoActualizado.Descripcion;
            paso.Realizado = pasoActualizado.Realizado;

            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarPaso(Guid id)
        {
            var usuarioId = servicioUsuarios.ObtenerIdUsuarioAutentificado();

            var paso = await dbContext.PasoTareas
                .Include(p => p.Tarea)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (paso is null)
            {
                return NotFound();
            }

            if (paso.Tarea.UsuarioCreadorId != usuarioId)
            {
                return Forbid();
            }

            dbContext.Remove(paso);
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        // Ordena los pasos
        [HttpPost("ordenar/{tareaId:int}")]
        public async Task<IActionResult> OrdenarPasos(int tareaId, [FromBody] Guid[] ids)
        {
            var usuarioId = servicioUsuarios.ObtenerIdUsuarioAutentificado();

            var tarea = await dbContext.Tareas.FirstOrDefaultAsync(t => t.Id == tareaId && t.UsuarioCreadorId == usuarioId);
            if (tarea is null)
            {
                return NotFound();
            }

            // De aqui se obtiene los ids de los pasos de la tarea encontrada
            var pasos = await dbContext.PasoTareas.Where(p => p.TareaId == tareaId).ToListAsync();
            var pasosIds = pasos.Select(pasos => pasos.Id);

            // Validacion: De los ids del front, retorname todos los que no esten en pasosIds.
            var idsPasosNoPertenecenAUsuario = ids.Except(pasosIds).ToList();

            if (idsPasosNoPertenecenAUsuario.Any())
            {
                return BadRequest("No todos los pasos estan presentes");
            }

            var pasosDiccionario = pasos.ToDictionary(p => p.Id);

            for (int i = 0; i < ids.Length; i++)
            {
                var pasoId = ids[i];
                var paso = pasosDiccionario[pasoId];

                paso.Orden = i + 1;
            }

            await dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
