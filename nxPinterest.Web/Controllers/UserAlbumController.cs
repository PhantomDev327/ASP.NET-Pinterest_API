﻿using System;
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
            var model = await _userAlbumService.GetAlbumByUser(UserId);

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
        [HttpGet("/shared/{url?}")]
        public async Task<IActionResult> DetailAlbum(string url)
        {
            //if (string.IsNullOrWhiteSpace(url))
            //{
            //    TempData["Message"] = "Url path not exist!";
            //    return View("~/Views/Error/204.cshtml");
            //}

            //int albumId = await _userAlbumService.GetAlbumIdByUrl(url);

            //if (albumId == 0)
            //{
            //    TempData["Message"] = "Album not exist!";
            //    return View("~/Views/Error/204.cshtml");
            //}
            return View();
        }

    }
}
