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