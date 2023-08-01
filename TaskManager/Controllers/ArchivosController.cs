using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Entidades;
using TaskManager.Servicios;

namespace TaskManager.Controllers
{
    [Route("api/archivos")]
    public class ArchivosController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly string contenedor = "archivosadjuntos";

        public ArchivosController(
            ApplicationDbContext dbContext,
            IAlmacenadorArchivos almacenadorArchivos,
            IServicioUsuarios servicioUsuarios)
        {
            this.dbContext = dbContext;
            this.almacenadorArchivos = almacenadorArchivos;
            this.servicioUsuarios = servicioUsuarios;
        }

        [HttpPost("{tareaId:int}")]
        public async Task<ActionResult<IEnumerable<ArchivoAdjunto>>> Post(int tareaId,
            [FromForm] IEnumerable<IFormFile> archivos)
        {
            var usuarioId = servicioUsuarios.ObtenerIdUsuarioAutentificado();

            var tarea = await dbContext.Tareas.FirstOrDefaultAsync(t => t.Id == tareaId);

            if (tarea is null)
            {
                return NotFound();
            }

            if (tarea.UsuarioCreadorId != usuarioId)
            {
                return Forbid();
            }

            var existenArchivosAdjuntos = await dbContext.ArchivosAdjuntos.AnyAsync(file => file.TareaId == tareaId);

            var ordenMayor = 0;

            if (existenArchivosAdjuntos)
            {
                ordenMayor = await dbContext.ArchivosAdjuntos.Where(ar => ar.TareaId == tareaId)
                    .Select(a => a.Orden).MaxAsync();
            }

            var resultados = await almacenadorArchivos.Almacenar(contenedor, archivos);

            var archivosAdjuntos = resultados.Select((resultado, indice) => new ArchivoAdjunto
            {
                TareaId = tareaId,
                FechaCreacion = DateTime.UtcNow,
                Url = resultado.Url,
                Titulo = resultado.Titulo,
                Orden = ordenMayor + indice + 1
            });

            dbContext.AddRange(archivosAdjuntos);
            await dbContext.SaveChangesAsync();

            return archivosAdjuntos.ToList();
        }
    }
}
