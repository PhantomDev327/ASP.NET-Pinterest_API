using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using nxPinterest.Data.Models;
using nxPinterest.Data.ViewModels;

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
        public string SearchKey { get; set; }
        public IList<string> TagList { get; set; }
        public IList<UserAlbumViewModel> AlbumList { get; set; }
        public int SizeRange { get; set; }
        public IList<UserContainer> UserContainers { get; set; }

        public int currentContainer { get; set; }

        public bool AlbumMode { get; set; } = false;

    }
}
