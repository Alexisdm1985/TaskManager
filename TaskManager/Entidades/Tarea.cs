using System.ComponentModel.DataAnnotations;

namespace TaskManager.Entidades
{
    public class Tarea
    {
        public int Id { get; set; }

        [StringLength(250)]
        [Required]
        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public int Orden { get; set; }

        public DateTime FechaCreacion { get; set; }

        public List<PasoTarea> PasoTareas { get; set; }

        public List<ArchivoAdjunto> ArchivosAdjuntos { get; set; }
    }
}
