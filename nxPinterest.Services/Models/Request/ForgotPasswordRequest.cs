using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace nxPinterest.Services.Models.Request
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Emailアドレスを入力してください")]
        public string Email { get; set; }
    }
}
