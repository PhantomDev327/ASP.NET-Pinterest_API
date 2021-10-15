using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace nxPinterest.Data.Models
{
    public partial class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            UserMedia = new HashSet<UserMedia>();
        }

        public virtual ICollection<UserMedia> UserMedia { get; set; }
    }
}
