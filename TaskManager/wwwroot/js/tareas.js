function agregarTareas() {
    listadoTareasViewModel.tareas.push(new fnTareaViewModel({ id: 0, titulo: '' }));

    const buttonAudio = new Audio("../Content/sounds/kirby-hi.mp3");

    buttonAudio.play();
    // Focus en el input de titulo de tarea
    $("[name=titulo-tarea]").last().focus();

}

function manejarFocusOutTituloTarea(tarea) {

    const titulo = tarea.titulo();

    if (!titulo) {
        listadoTareasViewModel.tareas.pop();
        return;
    }
    
    const creacionTareaAudio = new Audio("../Content/sounds/kirby-bonus.mp3")
    creacionTareaAudio.play();
    // Seteando el id de tarea a distinto a 0 quiere decir que ya no es una nueva
    //tarea por lo que se mostrara entonces en la vista.
    tarea.id(1);

}