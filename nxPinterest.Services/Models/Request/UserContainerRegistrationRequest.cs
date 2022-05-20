using System.ComponentModel.DataAnnotations;

namespace nxPinterest.Services.Models.Request
{
    public class UserContainerRegistrationRequest
    {
        [Required]
        public int container_id { get; set; }
        [Required]
        public string container_name { get; set; }

        public bool container_visibility { get; set; }

        public string CurrentPageName { get; set; }
    }
}
