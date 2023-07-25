// Agrega una nueva tarea en memoria para luego hacer el POST
// (es el input que aparece luego de apretar crear nueva tarea)
function agregarTareas() {
    listadoTareasViewModel.tareas.push(new fnTareaViewModel({ id: 0, titulo: '' }));

    const buttonAudio = new Audio("../Content/sounds/kirby-hi.mp3");

    buttonAudio.play();
    // Focus en el input de titulo de tarea
    $("[name=titulo-tarea]").last().focus();

}

// POST
async function manejarFocusOutTituloTarea(tarea) {

    const titulo = tarea.titulo();

    if (!titulo) {
        listadoTareasViewModel.tareas.pop();
        return;
    }

    const creacionTareaAudio = new Audio("../Content/sounds/kirby-bonus.mp3")
    creacionTareaAudio.play();

    // POST NUEVA TAREA
    const data = JSON.stringify(titulo);
    const response = await fetch(urlTareas, {
        method: 'POST',
        body: data,
        headers: { 'Content-Type': 'application/json' }
    });

    if (response.ok) {
        const json = await response.json();

        // Seteando el ID de la tarea, se marcara como tarea no es nueva (ya que el campo es observable por knockout)
        // y entonces se mostrara.
        tarea.id(json.id);
    } else {
        mostrarMensajeErrorAPI(response);
    }
}

// Obtener lista de tareas GET
async function ObtenerListaTareas() {
    listadoTareasViewModel.cargandoInfo(true);

    const response = await fetch(urlTareas, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    })

    if (!response.ok) {
        mostrarMensajeErrorAPI(response);
        return;
    }

    listadoTareasViewModel.tareas([]);

    const json = await response.json();

    json.forEach((tarea) => {
        listadoTareasViewModel.tareas.push(new fnTareaViewModel(tarea));
    })

    listadoTareasViewModel.cargandoInfo(false);
}



// SORTABLE funcionalidad JQUIERY UI

async function actualizarOrdenTareas() {

    const ids = obtenerIdsTareas();
    await ordenarTareasDB(ids);
    await ObtenerListaTareas();

    // ---------- Este bloque es igual al del video pero no me funciona --------------------------

    // Ordenar en memoria
    //const tareasOrdenadas = listadoTareasViewModel.tareas.sorted(function (a, b) {

    //    let indicePrimeraTarea = ids.indexOf(a.id().toString());
    //    let indiceSegundaTarea = ids.indexOf(b.id().toString());

    //    return indicePrimeraTarea - indiceSegundaTarea;
    //});

    //tareasOrdenadas.forEach(t => console.log(t.titulo()))
    //listadoTareasViewModel.tareas([]);
    //listadoTareasViewModel.tareas(tareasOrdenadas);
    //listadoTareasViewModel.tareas().forEach((t) => console.log(t.titulo(), " - despues"));
    // ---------------------------------------------------------------------------------------------
}

// Ordenar en base de datos
async function ordenarTareasDB(idsTareas) {

    const data = JSON.stringify(idsTareas);

    //POST
    const response = await fetch(`${urlTareas}/ordenar`, {
        method: 'POST',
        body: data,
        headers: { 'Content-Type': 'application/json' }
    });

    if (!response.ok) {
        mostrarMensajeErrorAPI(response);
    }
}

// Obtiene el id de las tareas en la vista index.cshtml
function obtenerIdsTareas() {

    const idsOrdenadosEnMemoria = $("[name=titulo-tarea]").map(function () {
        return Number($(this).attr("data-id")); // Retorna el Id de cada cada tarea
        //return $(this).attr("data-id"); // Retorna el Id de cada cada tarea
    }).get();

    return idsOrdenadosEnMemoria;
}

// Lambda que otorga al div "reordenable"
// la capacidad de ordenar las tareas.
$(function () {
    $("#reordenable").sortable({
        axis: 'y',
        stop: async function () {
            await actualizarOrdenTareas();
        }
    });
})

// Abre modal con la tarea para su edicion
async function manejarClickTarea(tarea) {

    // Validacion
    if (tarea.esNuevaTarea()) {
        return;
    }

    // FETCH GET
    const response = await fetch(`${urlTareas}/${tarea.id()}`, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    })

    if (!response.ok) {
        mostrarMensajeErrorAPI(response);
        return;
    }

    const data = await response.json()

    tareaEditarVM.id = data.id;
    tareaEditarVM.titulo(data.titulo);
    tareaEditarVM.descripcion(data.descripcion);

    modalEditarTareaBootstrap.show();

}

// Maneja los cambios en los inputs del _ModalEditarTarea.cshtml
async function manejarChangeTarea() {

    const modelo = {
        id: tareaEditarVM.id,
        titulo: tareaEditarVM.titulo(),
        descripcion: tareaEditarVM.descripcion()
    }

    if (!modelo.titulo) {
        return;
    }

    // Fetch PUT
    await editarTarea(modelo);

    // Obtener index para ordenar las tareas en memoria
    mostrarCambiosEnMemoria(modelo);
}

async function editarTarea(tarea) {

    const response = await fetch(`${urlTareas}/${tarea.id}`, {
        method: 'PUT',
        body: JSON.stringify(tarea),
        headers: { 'Content-Type': 'application/json' }
    })

    if (!response.ok) {
        mostrarMensajeErrorAPI(response)
        return;
    }
}

function mostrarCambiosEnMemoria({ titulo, id }) {

    const indiceTarea = listadoTareasViewModel.tareas().findIndex(t => t.id() === id);
    const tareaEnMemoria = listadoTareasViewModel.tareas()[indiceTarea];

    tareaEnMemoria.titulo(titulo);
}

function confirmarEliminarTarea(tarea) {

    // Esconde el modal de EditarTarea
    modalEditarTareaBootstrap.hide();

    const dataModal = {
        titulo: `${tarea.titulo()}`,
        callBackConfirmar: () => {
            eliminarTarea(tarea);
        },
        callBackCancelar: () => {
            modalEditarTareaBootstrap.show();
        }
    }

    modalBorrarTarea(dataModal);
}

async function eliminarTarea(tarea) {

    const response = await fetch(`${urlTareas}/${tarea.id}`, {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' }
    })

    if (!response.ok) {
        mostrarMensajeErrorAPI(response)
        return;
    }

    // Eliminar la tarea en memoria
    // (Podemos volver a cargar la vista...pero para que xd, al parecer es mejor trabajar en memoria)
    const indice = obtenterIndiceTarea(tarea.id);
    
    listadoTareasViewModel.tareas.splice(indice, 1);
}

function obtenterIndiceTarea(id) {
    return listadoTareasViewModel.tareas().findIndex(t => t.id() === id);
}