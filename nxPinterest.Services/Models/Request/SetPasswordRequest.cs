using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace nxPinterest.Services.Models.Request
{
    public class SetPasswordRequest
    {

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match")]
        public string ConfirmPassword { get; set; }

        public string Email { get; set; }
    }
}
