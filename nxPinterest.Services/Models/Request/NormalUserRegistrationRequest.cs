using System.ComponentModel.DataAnnotations;

namespace nxPinterest.Services.Models.Request
{
    public class NormalUserRegistrationRequest
    {

        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Discriminator { get; set; }
        [Required]
        public int container_id { get; set; }

        public bool user_visibility { get; set; }

        public string UserDispName { get; set; }
    }
}
