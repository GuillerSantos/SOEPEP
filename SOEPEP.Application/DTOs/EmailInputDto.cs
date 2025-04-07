using System.ComponentModel.DataAnnotations;

namespace SOEPEP.Application.DTOs
{
    public class EmailInputDto
    {
        #region Properties

        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        public string? NewEmail { get; set; }

        #endregion Properties
    }
}