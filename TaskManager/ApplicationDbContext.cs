using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Entidades;

namespace TaskManager
{
    public class ApplicationDbContext : IdentityDbContext
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
// Antes esta clase heredaba de DbContext, pero para ocupar Identity con EF Core,
// se instala una version de Identity que ya trae EF Core, por lo que se cambia DbContext por IdentityDbContext
// ya que Identity Tambien trae sus propias tablas de usuarios.