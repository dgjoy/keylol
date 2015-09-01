using System.ComponentModel.DataAnnotations;

namespace Keylol.Models.ViewModels
{
    public class RegisterVM
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string GeetestChallenge { get; set; }

        [Required]
        public string GeetestSeccode { get; set; }

        [Required]
        public string GeetestValidate { get; set; }
    }

    public class LoginVM
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string GeetestChallenge { get; set; }

        [Required]
        public string GeetestSeccode { get; set; }

        [Required]
        public string GeetestValidate { get; set; }
    }
}
