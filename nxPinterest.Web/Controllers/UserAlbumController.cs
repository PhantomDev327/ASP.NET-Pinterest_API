using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nxPinterest.Services.Models.Request;
using System.Threading.Tasks;
using nxPinterest.Services.Interfaces;

namespace nxPinterest.Web.Controllers
{
    [Authorize]
    public class UserAlbumController : BaseController
    {
        private readonly IUserAlbumService _userAlbumService;

        public UserAlbumController(IUserAlbumService userAlbumService)
        {
            this._userAlbumService = userAlbumService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserAlbumRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userAlbumService.Create(model, UserId);

            return Ok(result);
        }

        public async Task<IActionResult> GetAlbums()
        {
            var model = await _userAlbumService.GetAlbumByUser(UserId);

            return PartialView("/Views/Shared/_ShowAlbum.cshtml", model);
        }
        
    }
}
