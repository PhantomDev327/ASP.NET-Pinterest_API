using System.ComponentModel.DataAnnotations;

namespace nxPinterest.Services.Models.Request
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "ログインID(Email)を入力してください")]
        public string Email { get; set; }

        [Required(ErrorMessage = "パスワードを入力してください")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
