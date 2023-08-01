using TaskManager.Models;

namespace TaskManager.Servicios
{
    public interface IAlmacenadorArchivos
    {
        Task Borrar(string ruta, string contenedor);
        Task<AlmacenarArchivoResultado[]> Almacenar(
            string contenedor, 
            IEnumerable<IFormFile> archivos);
    }
}


/// IFormFile es el tipo de dato para archivos en dotnet Core
/// string contenedor: es la carpeta donde se guardara el archivo