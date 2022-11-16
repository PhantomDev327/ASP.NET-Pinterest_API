using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nxPinterest.Services.Models.Request;
using System.Threading.Tasks;
using nxPinterest.Services.Interfaces;
using ImageMagick;
using nxPinterest.Data.ViewModels;

namespace nxPinterest.Web.Controllers
{
    [Authorize]
    public class UserAlbumController : BaseController
    {
        private readonly IUserAlbumService _userAlbumService;

        private readonly IUserAlbumMediaService _userAlbumMediaService;

        public UserAlbumController(IUserAlbumService userAlbumService, IUserAlbumMediaService userAlbumMediaService)
        {
            this._userAlbumService = userAlbumService;
            _userAlbumMediaService = userAlbumMediaService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserAlbumRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            model.AlbumUrl = GeneratePathUrl();
            var result = await _userAlbumService.Create(model, UserId);

            return Ok(result);
        }

        public async Task<IActionResult> GetAlbums()
        {
            var model = await _userAlbumService.GetAlbumUserByContainer(UserId);

            return PartialView("/Views/Shared/_ShowAlbum.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateShareUserMedia(CreateUserAlbumSharedRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var compareDate = (TimeSpan)(Convert.ToDateTime(model.AlbumExpireDate.Value.ToString("yyyy-MM-dd")) - Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")));

            if (compareDate.Days < 0) return BadRequest(ModelState);

            model.AlbumUrl = GeneratePathUrl();

            var keyPathUrl = await _userAlbumService.CreateAlbumShare(model, UserId);

            if (!string.IsNullOrEmpty(keyPathUrl))
            {
                string url = $"{Request.Scheme}://{Request.Host}/{"shared"}/{keyPathUrl}";

                return Ok(new { Success = true, Data = url });
            }

            return Ok(new { Success = false, Data = keyPathUrl });
        }

        [AllowAnonymous]
        [HttpGet("shared/{url?}")]
        public IActionResult SharedAlbum(string url)
        {
            return View("/Views/UserAlbum/SharedAlbum.cshtml");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> GetAlbumSharedLink(int pageIndex,string pathUrl)
        {
            if (string.IsNullOrWhiteSpace(pathUrl)) return Ok(new { StatusCode = 404, Data = "", Message = "Urlが間違っているか、Urlが存在しません。" });

            var albumId = await _userAlbumService.GetAlbumIdByPathUrlAsync(pathUrl);

            if (albumId == 0) return Ok(new { StatusCode = 404, Data = "", Message= "アルバムが存在されていません、または共有期限が切れました。" });

            var data = await _userAlbumMediaService.GetListAlbumById(albumId, pageIndex);

            return Ok(new { StatusCode = 200, Data = data, Message = "" });
        }


        [HttpPost]
        public async Task<IActionResult> GetSelectedAlbums(int pageIndex, string albumName)
        {
            if (string.IsNullOrEmpty(albumName)) return Ok(new { StatusCode = 404, Data = "", Message = "Not found data。" });

            var albumId = await _userAlbumService.GetAlbumIdByNameAsync(albumName);

            if (albumId == 0) return Ok(new { StatusCode = 404, Data = "", Message = "Not found data。" });

            var createAlbumDate = await _userAlbumService.GetCreateDateAlbumNameAsync(albumId);

            var data = await _userAlbumMediaService.GetListAlbumById(albumId, pageIndex);

            var albumVm = new HomeAlbumViewModel
            {
                Albums = data,
                AlbumCreateDate = createAlbumDate
            };

            return Ok(new { StatusCode = 200, Data = albumVm, Message = "" });
        }

    }
}
