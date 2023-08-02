using System.ComponentModel.DataAnnotations;
using nxPinterest.Data.Models;
using System.Collections.Generic;
using nxPinterest.Data.ViewModels;

namespace nxPinterest.Services.Models.Request
{
    public class UserContainerRegistrationRequest
    {
        [Required]
        public int container_id { get; set; }
        [Required(ErrorMessage = "コンテナ名を入力してください")]
        public string container_name { get; set; }

        public bool container_visibility { get; set; }

        public string CurrentPageName { get; set; }

        public string Discriminator { get; set; }
        public string UserDispName { get; set; }
        public IList<UserContainer> UserContainers { get; set; }

        public int currentContainer { get; set; }

        public bool AlbumMode { get; set; } = false;
        public IList<string> TagList { get; set; }
        public IList<UserAlbumViewModel> AlbumList { get; set; }
        public int SizeRange { get; set; }
        public string SearchKey { get; set; }
    }
}
