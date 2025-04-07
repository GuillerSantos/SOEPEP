using System.ComponentModel.DataAnnotations;

namespace SOEPEP.Application.DTOs
{
    public class IndexInputDto
    {
        #region Properties

        [Phone]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        #endregion Properties
    }
}