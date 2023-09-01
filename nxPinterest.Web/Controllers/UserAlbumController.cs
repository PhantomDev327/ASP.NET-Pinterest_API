using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nxPinterest.Services.Models.Request;
using System.Threading.Tasks;
using nxPinterest.Services.Interfaces;
using nxPinterest.Data.Models;
using nxPinterest.Data.ViewModels;
using nxPinterest.Web.Models;
using System.Collections.Generic;
using nxPinterest.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace nxPinterest.Web.Controllers
{
    [Authorize]
    public class UserAlbumController : BaseController
    {
        private readonly IUserAlbumService _userAlbumService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IUserMediaManagementService userMediaManagementService;
        public const int pageSize = nxPinterest.Services.dev_Settings.pageSize_regist;

        public UserAlbumController(IUserAlbumService userAlbumService, UserManager<ApplicationUser> userManager,IUserMediaManagementService userMediaManagementService, ApplicationDbContext context)
        {
            this._userAlbumService = userAlbumService;
            this.userMediaManagementService = userMediaManagementService;
            this._userManager = userManager;
            this._context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserAlbumRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await this._userManager.FindByIdAsync(this.UserId);
            model.AlbumUrl = GeneratePathUrl();
            var result = await _userAlbumService.Create(model, UserId, user.container_id);

            return Ok(result);
        }

        public async Task<IActionResult> GetAlbums()
        {
            var user = await this._userManager.FindByIdAsync(this.UserId);
            var model = await _userAlbumService.GetAlbumUserByContainer(user.container_id);

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
            if (string.IsNullOrWhiteSpace(pathUrl)) return Ok(new { StatusCode = 404, Data = "", Message = "リンクが間違っているか存在しません。" });

            var albumId = await _userAlbumService.GetAlbumIdByPathUrlAsync(pathUrl);

            if (albumId == 0) return Ok(new { StatusCode = 404, Data = "", Message= "アルバムが存在しません。" });

            var data = await _userAlbumService.GetListAlbumById(albumId, pageIndex);

            return Ok(new { StatusCode = 200, Data = data, Message = "" });
        }


        [HttpPost]
        public async Task<IActionResult> GetSelectedAlbums(int pageIndex, int albumID)
        {
            
            if (albumID == 0) return Ok(new { StatusCode = 404, Data = "", Message = "Not found data。" });

            var createAlbumDate = await _userAlbumService.GetCreateDateAlbumNameAsync(albumID);

            var data = await _userAlbumService.GetListAlbumById(albumID, pageIndex);

            var albumVm = new HomeAlbumViewModel
            {
                Albums = data,
                AlbumCreateDate = createAlbumDate
            };

            return Ok(new { StatusCode = 200, Data = albumVm, Message = "" });
        }


        [HttpPost]
        public async Task<IActionResult> Update(string oldAlbumName, string newAlbumName)
        {
            if (string.IsNullOrEmpty(newAlbumName))
            {
                return Ok(new { Success = false, Message= "アルバム名を入力してください" });
            }

            if (oldAlbumName.Equals(newAlbumName))
            {
                return Ok(new { Success = true, Data = oldAlbumName });
            }
            if (await _userAlbumService.IsAlbumNameExistAsync(newAlbumName))
            {
                return Ok(new { Success = false, Message = "アルバム名が存在されました。他のアルバム名を入力してください。" });
            }

            var albumId = await _userAlbumService.GetAlbumIdByNameAsync(oldAlbumName);

            if (albumId == 0) return Ok(new { Success = false, Message = "" });

            var model = new UserAlbum
            {
                AlbumId = albumId,
                AlbumName = newAlbumName,
                AlbumUpdatedat = DateTime.Now
            };
            var result = _userAlbumService.UpdateAlbumAsync(albumId, model);

            return Ok(new { Success = true, Data = result.AlbumName });
        }


        [HttpPost]
        public async Task<IActionResult> Remove(string removeAlbumName)
        {
            var result = await _userAlbumService.RemoveAlbum(removeAlbumName);

            return Ok(new { Success = true, Data = result.AlbumName });
        }

        public async Task<IActionResult> ShareManage(int pageIndex = 1)
        {
            ShareViewModel vm = new ShareViewModel();
            List<ApplicationUser> user = this._context.Users.Where(c => c.Id.Equals(this.UserId)).ToList();
            if (user == null || user.Count == 0) return RedirectToAction("LogOut", "Account");

            var album = await _userAlbumService.GetSharedAlbumByUser(this.UserId, user[0].Discriminator, user[0].container_id);
            vm.AlbumCommentList = album.Select(n=> new nxPinterest.Data.ViewModels.UserAlbumViewModel 
            { 
                AlbumName = n.AlbumName,
                AlbumId = n.AlbumId,
                AlbumUrl = $"{Request.Scheme}://{Request.Host}/{"shared"}/{n.AlbumUrl}",
                AlbumCreatedat = n.AlbumCreatedat,
                AlbumExpireDate = n.AlbumExpireDate,
                Comment = n.Comment,
                UserId = this._context.Users.Where(c => c.Id.Equals(n.UserId)).First().UserDispName
            }).ToList();
            ViewBag.ItemCount = vm.AlbumCommentList.Count;

            vm.Discriminator = user[0].Discriminator;
            vm.UserDispName = user[0].UserDispName;

            int totalPages = (int)System.Math.Ceiling((decimal)(vm.AlbumCommentList.Count / (decimal)pageSize));
            int skip = (pageIndex - 1) * pageSize;
            int totalRecordCount = vm.AlbumCommentList.Count;

            vm.AlbumCommentList = vm.AlbumCommentList.Skip(skip).Take(pageSize).ToList();
            vm.PageIndex = pageIndex;
            vm.TotalPages = totalPages;
            vm.TotalRecords = totalRecordCount;

            //よく使用されているアルバムの一覧 TODO
            var sideAlbum = await _userAlbumService.GetAlbumUserByContainer(user[0].container_id);
            vm.AlbumList = sideAlbum.Select(n => new nxPinterest.Data.ViewModels.UserAlbumViewModel
            {
                AlbumName = n.AlbumName,
                AlbumUrl = n.AlbumUrl,
                AlbumId = n.AlbumId
            }).ToList();

            //登録画面で使用されているタグ候補
            vm.ImageRegistrationVM.SuggestTagsList = vm.TagList;

            // get user containers
            string container_ids = user[0].ContainerIds ?? "";
            string[] containerArray = container_ids.Split(',');

            if (containerArray.Length == 0 || containerArray[0] == "")
            {
                vm.UserContainers = await this._context.UserContainer.Where(c => c.container_id == user[0].container_id).ToListAsync();
            }
            else
            {
                var containerIds = containerArray
                    .Where(x => int.TryParse(x, out _))
                    .Select(int.Parse)
                    .ToList();

                vm.UserContainers = await this._context.UserContainer.Where(c => containerIds.Contains(c.container_id)).ToListAsync();

            }
            vm.currentContainer = user[0].container_id;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAlbum(int albumID)
        {
            var result = await _userAlbumService.DeleteAlbum(albumID);

            return Ok(new { success = result });
        }

        [HttpPost]
        public IActionResult SetThumbnail(int albumId, string thumbnail)
        {
            UserAlbum album = this._context.UserAlbums.Where(c => c.AlbumId.Equals(albumId)).FirstOrDefault();
            album.AlbumThumbnailUrl = thumbnail;
            album.AlbumUpdatedat = DateTime.Now;

            var result = _userAlbumService.UpdateAlbumThumbnail(albumId, album);
            return Json(new { success = result });
        }

        /// <summary>
        /// Get View Album Detail
        /// </summary>
        /// <param name="albumID"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int albumID)
        {
            HomeViewModel vm = new HomeViewModel();

            List<ApplicationUser> user = this._context.Users.Where(c => c.Id.Equals(this.UserId)).ToList();
            if (user == null || user.Count == 0) return RedirectToAction("LogOut", "Account");

            int container = user[0].container_id;
            vm.Discriminator = user[0].Discriminator;
            vm.UserDispName = user[0].UserDispName;

            vm.SizeRange = 3;

            //よく使用されているタグ候補
            vm.TagList = await this.userMediaManagementService.GetOftenUseTagsAsyc(user[0].container_id, "", 30);

            // get user containers
            string container_ids = user[0].ContainerIds ?? "";
            string[] containerArray = container_ids.Split(',');

            if (containerArray.Length == 0 || containerArray[0] == "")
            {
                vm.UserContainers = await this._context.UserContainer.Where(c => c.container_id == user[0].container_id).ToListAsync();
            }
            else
            {
                var containerIds = containerArray
                    .Where(x => int.TryParse(x, out _))
                    .Select(int.Parse)
                    .ToList();

                vm.UserContainers = await this._context.UserContainer.Where(c => containerIds.Contains(c.container_id)).ToListAsync();
            }

            var album = await _userAlbumService.GetAlbumUserByContainer(container);
            vm.AlbumList = album.Select(n=> new nxPinterest.Data.ViewModels.UserAlbumViewModel
            {
                AlbumName = n.AlbumName,
                AlbumUrl = n.AlbumUrl,
                AlbumId = n.AlbumId
            }).ToList();

            vm.currentContainer = container;
            vm.ImageRegistrationVM.SuggestTagsList = vm.TagList;
            vm.CurrentAlbum = albumID;

            return View(vm);
        }

    }
}
