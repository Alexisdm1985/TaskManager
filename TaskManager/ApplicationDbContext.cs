using Microsoft.EntityFrameworkCore;
using TaskManager.Entidades;

namespace TaskManager
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }        

        public DbSet<Tarea> Tareas { get; set; }
        public DbSet<PasoTarea> PasoTareas { get; set; }
        public DbSet<ArchivoAdjunto> ArchivosAdjuntos { get; set; }
    }
}


// Crear nueva entidad: DbSet<Modelo> NombreEntidad {get; set;}