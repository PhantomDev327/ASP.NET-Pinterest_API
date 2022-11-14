using System;
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

            model.AlbumUrl = GenerateUrl();
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

            model.AlbumUrl = GenerateUrl();
            var result = await _userAlbumService.CreateAlbumShare(model, UserId);

            return !string.IsNullOrEmpty(result)
                ? Ok(new { Success = true, Data = result })
                : Ok(new { Success = false, Data = result });
        }

        [AllowAnonymous]
        [HttpGet("shared/{url?}")]
        public IActionResult SharedAlbum(string url)
        {
            return View("/Views/UserAlbum/SharedAlbum.cshtml");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> GetAlbumSharedLink(int pageIndex,string siteUrl )
        {
            //if(string.IsNullOrWhiteSpace(siteUrl)) return Ok(new { StatusCode = 404, Data = "" ,Message = "Urlが間違っているか、Urlが存在しません。" });

            var albumId = await _userAlbumService.GetAlbumIdByUrl(siteUrl);

            if (albumId == 0) return Ok(new { StatusCode = 404, Data = "", Message= "アルバムが存在しませんか、期限が切された。" });

            var data = await _userAlbumMediaService.GetListAlbumById(albumId, pageIndex);

            return Ok(new { StatusCode = 200, Data = data, Message = "" });
        }

    }
}
