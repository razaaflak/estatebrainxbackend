using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace AngularAuthAPI.Dtos
{
    public class UserLoginDto
    {
        [Required]
        [Display(Name = "Email / Username")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [AllowNull]
        [Display(Name = "JWT Token")]
        public string? Token { get; set; }
    }
}
