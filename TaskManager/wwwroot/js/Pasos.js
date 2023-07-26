function manejarClickAgregarPaso() {

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
    } else {
        paso.modoEdicion(false);
        paso.descripcion(paso.descripcionAnterior);
    }
    
}

async function manejarGuardarPaso(paso) {

    paso.modoEdicion(false);

    const idTarea = tareaEditarVM.id;
    const data = obtenerDataPeticionPaso(paso)
    
    // Si no es nuevo entonces se actualiza
    const esNuevoPaso = paso.esNuevoPaso();

    // Si no tiene descripcion y es nuevo entonces se elimina, si no es nuevo
    // entonces descripcion seria igual a descripcionAnterior (que por defecto es '')
    const descripcion = paso.descripcion()

    if (!descripcion) {
        paso.descripcion(paso.descripcionAnterior)

        if (esNuevoPaso) {
            tareaEditarVM.pasos.pop();
        }
    }

    if (esNuevoPaso) {
        await insertarNuevoPaso(data, idTarea, paso);
    } else {
        await actualizarPaso(data, paso.id());
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

    const pasoJson = await response.json();

    paso.id(pasoJson.id);
}

function manejarClickDescripcionPaso(paso) {

    paso.modoEdicion(true);
    $("[name=txtPasoDescripcion]:visible").focus();

    paso.descripcionAnterior =  paso.descripcion();
}

async function actualizarPaso(data, id) {
    const respuesta = await fetch(`${urlPasos}/${id}`, {
        method: 'PUT',
        body: data,
        headers: {'Content-Type': 'application/json'}
    })

    if (!respuesta.ok) {
        mostrarMensajeErrorAPI(respuesta);
        return;
    }
}