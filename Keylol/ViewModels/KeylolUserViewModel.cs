using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keylol.ViewModels
{
    public class RegisterViewModel
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

    public class LoginViewModel
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
