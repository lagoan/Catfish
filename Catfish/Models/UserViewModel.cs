
using System.ComponentModel.DataAnnotations;


namespace Catfish.Models
{
    public class UserViewModel
    {
        [Required]
        public string Login { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required(ErrorMessage ="Password id required")]
        public string Password { get; set; }
        [Required(ErrorMessage ="ConfirmPassword id required")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

    
}