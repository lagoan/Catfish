
using System.ComponentModel.DataAnnotations;


namespace Catfish.Models
{
    public class LoginViewModel
    {
        [Required]
        // [EmailAddress]
        [Display(Name = "UserName")]
        public string Login { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

}