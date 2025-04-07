using System.ComponentModel.DataAnnotations;

namespace SOEPEP.Application.DTOs
{
    public class LoginInputDto
    {
        #region Properties

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        #endregion Properties
    }
}