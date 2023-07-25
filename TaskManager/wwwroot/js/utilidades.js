async function mostrarMensajeErrorAPI(respuestaHttp) {
    let mensaje = "";

    if (respuestaHttp.status === 400) {
        mensaje = await respuestaHttp.text();
    } else if (respuestaHttp.status === 404) {
        mensaje = mensajeErrorNotFound;
    } else {
        mensaje = mensajeErrorInesperado;
    }
    modalErrorSweetAlert(mensaje);
}


function modalErrorSweetAlert(mensaje) {
    swal({
        title: "Error...",
        text: mensaje,
        icon: "error"
    });
}

function modalBorrarTarea({ callBackConfirmar, callBackCancelar, titulo }) {
    // Muestra un modal que dependiendo la accion seleccionada
    // dispara ciertas acciones

    swal({
        title: `Esta segur@ de eliminar la tarea - ${titulo}`,
        icon: 'warning',
        buttons: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        focusConfirm: true
    }).then((resultado) => {
        if (resultado) {
            callBackConfirmar();

        } else if (callBackCancelar) {
            // Esto sucede solo si se ha enviado el callback de cancelar
            // y si el usuario no acciono el boton de confirmar eliminar tarea.
            callBackCancelar();
        }
    })
}