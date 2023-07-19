function agregarTareas() {
    listadoTareasViewModel.tareas.push(new fnTareaViewModel({ id: 0, titulo: '' }));

    // Focus en el input de titulo de tarea
    $("[name=titulo-tarea]").last().focus();

}

function manejarFocusOutTituloTarea(tarea) {

    const titulo = tarea.titulo();

    if (!titulo) {
        listadoTareasViewModel.tareas.pop();
        return;
    }

    // Seteando el id de tarea a distinto a 0 quiere decir que ya no es una nueva
    //tarea por lo que se mostrara entonces en la vista.
    tarea.id(1);
}