let inputArchivoTarea = document.getElementById('archivoATarea');

function manejarClickAgregarArchivoAdjunto() {
    inputArchivoTarea.click();
}

async function manejarOnChangeArchivosAdjuntos(e) {

    const idTarea = tareaEditarVM.id;
    const archivos = e.target.files;
    const archivosArreglo = Array.from(archivos);

    const formData = new FormData();

    for (var i = 0; i < archivosArreglo.length; i++) {

        formData.append("archivos", archivosArreglo[i]);
    }

    const response = await fetch(`${urlArchivos}/${idTarea}`, {
        body: formData,
        method: 'POST'
    });

    if (!response.ok) {
        mostrarMensajeErrorAPI(response);
        return;
    }

    const data = await response.json();
    console.log(data);

    // Limpia la seleccion del input archivo
    inputArchivoTarea.value = null;
}

function prepararArchivosAdjuntos(archivosAdjuntos) {
    
    archivosAdjuntos.forEach(archivo => {

        let fechaCreacion = archivo.fechaCreacion;

        // Si la fecha no viene en formato UTC
        if (archivo.fechaCreacion.indexOf('Z') === -1) {
            fechaCreacion += 'Z';
        }

        const fechaCreacionDT = new Date(fechaCreacion);

        archivo.publicado = fechaCreacionDT.toLocaleString();

        tareaEditarVM.archivosAdjuntos.push(new archivoAdjuntoViewModel({...archivo, modoEdicion: false}));
    })
}