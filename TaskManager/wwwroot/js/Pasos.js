﻿function manejarClickAgregarPaso() {

    // Condicion para crear un nuevo paso
    const indice = tareaEditarVM.pasos().findIndex(paso => paso.esNuevoPaso());

    if (indice !== -1) {
        return; 
        // Si indice es distinto a -1 significa que encontro un paso nuevo
        // por lo que se aplica early return para no dejar crear un nuevo paso
    }

    // Agrega un nuevo paso a tareaEditarVM
    const nuevoPaso = new pasosTareaVM({ modoEdicion: true, realizado: false });
    tareaEditarVM.pasos.push(nuevoPaso);

    // Todo elemento con ese nombre y que este visible tendra focus
    $("[name=txtPasoDescripcion]:visible").focus();
}

function manejarCancelarPaso(paso) {

    if (paso.esNuevoPaso()) {
        tareaEditarVM.pasos.pop();
    }
    
}

async function manejarGuardarPaso(paso) {

    paso.modoEdicion(false);

    const idTarea = tareaEditarVM.id;
    const data = obtenerDataPeticionPaso(paso)

    // Si no es nuevo entonces se actualiza (por hacer)
    const esNuevoPaso = paso.esNuevoPaso();

    if (esNuevoPaso) {
        await insertarNuevoPaso(data, idTarea, paso);
    } else {
        // actualizar paso
    }
}

function obtenerDataPeticionPaso(paso) {

    const data = JSON.stringify({
        descripcion: paso.descripcion(),
        realizado: paso.realizado()
    })

    return data;
}

async function insertarNuevoPaso(data, idTarea, paso) {

    const response = await fetch(`${urlPasos}/${idTarea}`, {
        method: 'POST',
        body: data,
        headers: { 'Content-Type': 'application/json' }
    });


    if (!response.ok) {
        mostrarMensajeErrorAPI(response);
        return;
    }

    console.log(response)
    const pasoJson = await response.json();

    paso.id(pasoJson.id);
}