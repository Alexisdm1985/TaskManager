namespace TaskManager.Entidades
{
    public class PasoTarea
    {
        public Guid Id { get; set; }
        public int TareaId { get; set; }
        public Tarea Tarea { get; set; }
        public string Descripcion { get; set; }
        public int Orden { get; set; }
        public bool Realizado { get; set; }


    }
}
