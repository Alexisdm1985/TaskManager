namespace TaskManager.Models
{
    public class TareaDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
    }
}

/// DTO = Data Transfer Object
/// Esto nos indica de que esta clase tendra propiedades correspondientes a datos especificos de la tabla/clase Tarea.
/// De esta manera, cuando usamos .Select( filtro ) en alguna consulta a la base de datos (o dbContext, etc. El punto es usar linq/Identity)
/// pasamos de ocupar una consulta con datos anonimos a una consulta con datos especificos (porque en el filtro ahora irian las propiedades de 
/// TareaDTO). Esto nos ayuda a ser mas especificos en los controladores con lo que se devuelve, en vez de usar <IActionRetul> que es general
/// usamos <TareaDTO> como tipo de dato.