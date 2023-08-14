using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace nxPinterest.Data.Models
{
    public partial class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Discriminator = null;
            container_id = 0;
            id = null;
            user_visibility = false;
            UserDispName = null;
            UserMedia = new HashSet<UserMedia>();
        }

        public virtual ICollection<UserMedia> UserMedia { get; set; }

        public List<UserAlbum> UserAlbums { get; set; } = new List<UserAlbum>();

        public virtual string Discriminator { get; set; }

        public virtual int container_id { get; set; }

        public virtual string id { get; }

        public virtual bool user_visibility { get; set; }

        public virtual string UserDispName { get; set; }
        public virtual string ContainerIds { get; set; }
        public virtual string? DisplayMode { get; set; }
    }
}
