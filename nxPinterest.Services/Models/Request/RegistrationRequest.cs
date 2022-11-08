using System.ComponentModel.DataAnnotations;

namespace nxPinterest.Services.Models.Request
{
    public class RegistrationRequest
    {
        [Required(ErrorMessage = "Emailアドレスを入力してください")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "名前を入力してください")]
        public string UserDispName { get; set; }

        public int ContainerId { get; set; } = 2;

        //[Required]
        //[DataType(DataType.Password)]
        //public string Password { get; set; }

        //[DataType(DataType.Password)]
        //[Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
        //public string ConfirmPassword { get; set; }
    }
}
