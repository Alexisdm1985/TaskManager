function agregarTareas() {
    listadoTareasViewModel.tareas.push(new fnTareaViewModel({ id: 0, titulo: '' }));

    const buttonAudio = new Audio("../Content/sounds/kirby-hi.mp3");

    buttonAudio.play();
    // Focus en el input de titulo de tarea
    $("[name=titulo-tarea]").last().focus();

}

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

// Obtener lista de tareas
async function ObtenerListaTareas() {
    listadoTareasViewModel.cargandoInfo(true);

    const response = await fetch(urlTareas, {
        method: 'GET',
        headers: {'Content-Type': 'application/json'}
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