using System.ComponentModel.DataAnnotations;

namespace nxPinterest.Web.Models
{
    public class UserEditViewModel
    {
        [Required]
        public string Email { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        public int container_id { get; set; }

        public bool user_visibility { get; set; }

        public string PhoneNumber { get; set; }

        public string UserDispName { get; set; }

    }
}
