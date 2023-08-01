using TaskManager.Models;

namespace TaskManager.Servicios
{
    public class AlmacenadorArchivosLocal : IAlmacenadorArchivos
    {
        private readonly IWebHostEnvironment hostEnv;
        private readonly IHttpContextAccessor httpContextAccessor;

        public AlmacenadorArchivosLocal(IWebHostEnvironment hostEnv,
            IHttpContextAccessor httpContextAccessor)
        {
            this.hostEnv = hostEnv;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<AlmacenarArchivoResultado[]> Almacenar(string contenedor, IEnumerable<IFormFile> archivos)
        {
            var tareas = archivos.Select(async archivo =>
            {
                var nombreArchivoOriginal = Path.GetFileName(archivo.FileName);
                var extension = Path.GetExtension(archivo.FileName);
                var nombreArchivoLocal = $"{Guid.NewGuid()}{extension}";

                // Se configura la carpeta que guardara los archivos
                // combinando la ruta de hostEnv y contenedor.
                // Si no existe el folder entonces se crea.
                string folder = Path.Combine(hostEnv.WebRootPath, contenedor);

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string ruta = Path.Combine(folder, nombreArchivoLocal);

                // Se copia el archivo en la memoria para extraer el contenido
                // y finalmente guardarlo fisicamente en la ruta creada anteriormente.
                using(var ms = new MemoryStream())
                {
                    await archivo.CopyToAsync(ms);
                    var contenido = ms.ToArray();
                    await File.WriteAllBytesAsync(ruta, contenido);
                }

                //
                var url = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}";
                var urlArchivo = Path.Combine(url, contenedor, nombreArchivoLocal).Replace("\\", "/");

                return new AlmacenarArchivoResultado
                {
                    Url = urlArchivo,
                    Titulo = nombreArchivoOriginal
                };

            });

            var resultados = await Task.WhenAll(tareas);
            return resultados;
        }

        public Task Borrar(string ruta, string contenedor)
        {
            if (string.IsNullOrEmpty(ruta))
            {
                return Task.CompletedTask;
            }

            var nombreArchivo = Path.GetFileName(ruta);
            var directorioArchivo = Path.Combine(hostEnv.WebRootPath, contenedor, nombreArchivo);

            if (File.Exists(directorioArchivo))
            {
                File.Delete(directorioArchivo);
            }

            return Task.CompletedTask;
        }
    }
}
