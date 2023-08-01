using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using TaskManager.Models;

namespace TaskManager.Servicios
{
    public class AlmacenadorArchivosAzure : IAlmacenadorArchivos
    {
        private string azureConnectionString;

        public AlmacenadorArchivosAzure(IConfiguration configuration)
        {
            this.azureConnectionString = configuration.GetConnectionString("AzureStorage");
        }

        public async Task<AlmacenarArchivoResultado[]> Almacenar(string contenedor, IEnumerable<IFormFile> archivos)
        {
            // Instanciar el cliente de AzureStorage
            var cliente = new BlobContainerClient(azureConnectionString, contenedor);

            // Crear cliente
            await cliente.CreateIfNotExistsAsync();

            // Agregar politica de acceso
            cliente.SetAccessPolicy(PublicAccessType.Blob);

            // Por cada archivo se crea y se guarda el objeto AlmacenarArchivoResultado
            var tareas = archivos.Select(async archivo =>
            {
                var nombeOriginal = Path.GetFileName(archivo.FileName);
                var extension = Path.GetExtension(archivo.FileName);

                // Para que dos clientes puedan subir un archivo del mismo nombre
                // se usa Guid en el nombre del archivo para Azure.
                var nombreArchivo = $"{Guid.NewGuid()}{extension}";

                // Creacion archivo blob
                var blob = cliente.GetBlobClient(nombreArchivo);

                // Se asigna el tipo de archivo atravez del header
                var blobHttpHeaders = new BlobHttpHeaders();
                blobHttpHeaders.ContentType = archivo.ContentType;

                // Sube el archivo a Azure
                await blob.UploadAsync(archivo.OpenReadStream(), blobHttpHeaders);

                // Finalmente se guarda en "tareas" los archivos guardados en azure 
                // con el nombre y url.
                return new AlmacenarArchivoResultado
                {
                    Url = blob.Uri.ToString(),
                    Titulo = nombeOriginal
                };
            });

            // "WhenAll" cuando todas las "tareas" esten completas, ejecuta las acciones del codigo.
            var resultados = await Task.WhenAll(tareas);
            return resultados;
        }

        public async Task Borrar(string ruta, string contenedor)
        {
            if (string.IsNullOrEmpty(ruta))
            {
                return;
            }

            // Instancia y creacion de cliente
            var cliente = new BlobContainerClient(azureConnectionString, contenedor);
            await cliente.CreateIfNotExistsAsync();

            // Obtencion del archivo de azure
            var nombreArchivo = Path.GetFileName(ruta);
            var blob = cliente.GetBlobClient(nombreArchivo);

            // Eliminar
            await blob.DeleteIfExistsAsync();
        }
    }
}


/// Recordar que contenedor no es mas que el nombre o ruta de la carpeta en el que se guardara el/los archivos