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