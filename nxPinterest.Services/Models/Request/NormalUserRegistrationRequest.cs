using System.ComponentModel.DataAnnotations;

namespace nxPinterest.Services.Models.Request
{
    public class NormalUserRegistrationRequest
    {

        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Emailアドレスを入力してください")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "権限を入力してください")]
        public string Discriminator { get; set; }
        [Required(ErrorMessage = "コンテナを入力してください")]
        public int container_id { get; set; }

        public bool user_visibility { get; set; }

        public string UserDispName { get; set; }
    }
}
