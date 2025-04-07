using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOEPEP.Application.DTOs
{
    public class LoginWithRecoveryCodeInputDto
    {
        #region Properties

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Recovery Code")]
        public string RecoveryCode { get; set; } = "";

        #endregion Properties
    }
}