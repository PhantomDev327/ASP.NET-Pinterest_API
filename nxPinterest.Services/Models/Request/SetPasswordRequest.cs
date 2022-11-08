using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace nxPinterest.Services.Models.Request
{
    public class SetPasswordRequest
    {

        [Required(ErrorMessage = "新しいパスワードを入力してください")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "パスワードと確認用パスワードが一致していません")]
        public string ConfirmPassword { get; set; }

        public string Email { get; set; }
    }
}
