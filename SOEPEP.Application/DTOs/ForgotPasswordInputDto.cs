using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOEPEP.Application.DTOs
{
    public class ForgotPasswordInputDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
    }
}
