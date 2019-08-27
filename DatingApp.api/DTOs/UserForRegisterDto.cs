using System.ComponentModel.DataAnnotations;

namespace DatingApp.api.DTOs
{
    public class UserForRegisterDto
    {
        [Required]
        public string Username {get; set;}
        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage="passwod should be in between 4 and 8")]
        public string Password {get; set;}
    }
}