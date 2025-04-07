using System.ComponentModel.DataAnnotations;

namespace SOEPEP.Application.DTOs
{
    public class DeletePersonalDataInputDto
    {
        #region Properties

        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        #endregion Properties
    }
}