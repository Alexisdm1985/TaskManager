using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "Error.Campo.Requerido")]
        [EmailAddress(ErrorMessage = "Error.Campo.Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Error.Campo.Requerido")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}
