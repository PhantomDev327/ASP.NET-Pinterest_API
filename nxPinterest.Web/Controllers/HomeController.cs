using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using nxPinterest.Data;
using nxPinterest.Data.Models;
//using nxPinterest.Services.Models.Response;
using nxPinterest.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using nxPinterest.Services;
using Microsoft.Azure.Cosmos;

namespace nxPinterest.Web.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        public const int pageSize = nxPinterest.Services.dev_Settings.displayMaxItems_search;
        private readonly Services.Interfaces.IUserMediaManagementService userMediaManagementService;
        private readonly ApplicationDbContext _context;
        //private CosmosDbService _cosmosDbService;

        public HomeController(ILogger<HomeController> logger,
                              ApplicationDbContext context,
                              Services.Interfaces.IUserMediaManagementService userMediaManagementService
                              )
        {
            _logger = logger;
            _context = context;
            this.userMediaManagementService = userMediaManagementService;
            //_cosmosDbService = new CosmosDbService();
        }

        /// <summary>
        /// Get view index home
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="searchKey"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(int pageIndex = 1, string searchKey = "")
        {
            HomeViewModel vm = new HomeViewModel();

            //nxPinterest.Services.CognitiveSearchService cognitiveSearchService = new Services.CognitiveSearchService();

            List<ApplicationUser> user = this._context.Users.Where(c => c.Id.Equals(this.UserId)).ToList();
            if (user == null || user.Count == 0) return RedirectToAction("LogOut", "Account");

            int skip = (pageIndex - 1) * pageSize;

            vm.UserMediaList = await this.userMediaManagementService.SearchUserMediaAsync(searchKey, user[0].container_id);

            int totalPages = (int)System.Math.Ceiling((decimal)(vm.UserMediaList.Count / (decimal)pageSize));
            int totalRecordCount = vm.UserMediaList.Count;

            ViewBag.ItemCount = vm.UserMediaList.Count;
            ViewBag.UserDispName = user[0].UserDispName;

            vm.UserMediaList = vm.UserMediaList.Skip(skip).Take(pageSize).ToList();

            vm.PageIndex = pageIndex;
            vm.TotalPages = totalPages;
            vm.SearchKey = searchKey;
            vm.TotalRecords = totalRecordCount;
            vm.Discriminator = user[0].Discriminator;

            //ホーム検索画面よく使用されているタグ候補
            vm.TagsList = await this.userMediaManagementService.GetOftenUseTagsAsyc(user[0].container_id, searchKey, 30);

            //登録画面で使用されているタグ候補
            vm.ImageRegistrationVM.SuggestTagsList = vm.TagsList;

            return View(vm);
        }

        [HttpPost]
        public async Task<object> getMedia(int pageIndex = 1, string searchKey = "")
        {
            HomeViewModel vm = new HomeViewModel();

            List<ApplicationUser> user = this._context.Users.Where(c => c.Id.Equals(this.UserId)).ToList();
            if (user == null || user.Count == 0) return RedirectToAction("LogOut", "Account");

            int skip = (pageIndex - 1) * pageSize;

            vm.UserMediaList = await this.userMediaManagementService.SearchUserMediaAsync(searchKey, user[0].container_id, skip);

            int totalPages = (int)System.Math.Ceiling((decimal)(vm.UserMediaList.Count / (decimal)pageSize));
            int totalRecordCount = vm.UserMediaList.Count;

            ViewBag.ItemCount = vm.UserMediaList.Count;
            ViewBag.UserDispName = user[0].UserDispName;

            //vm.UserMediaList = vm.UserMediaList.Skip(skip).Take(pageSize).ToList();

            vm.PageIndex = pageIndex;
            vm.TotalPages = totalPages;
            vm.SearchKey = searchKey;
            vm.TotalRecords = totalRecordCount;
            vm.Discriminator = user[0].Discriminator;

            return Json(vm);
        }

        [HttpPost]
        public async Task<object> DetailsAjax(int media_id)
        {
            try
            {
                DetailsViewModel vm = new DetailsViewModel();

                List<ApplicationUser> user = this._context.Users.Where(c => c.Id.Equals(this.UserId)).ToList();
                var media = await this.userMediaManagementService.GetUserMediaAsync(media_id);
                if(media != null)
                {
                    vm.UserMediaDetail = media;
                    vm.SameTitleUserMediaList = await this.userMediaManagementService.GetUserMediaSameTitleMediasAsync(media);
                    vm.RelatedUserMediaList = await this.userMediaManagementService.GetUserMediaRelatedMediasAsync(media);

                    string[] projectTags = vm.UserMediaDetail.ProjectTags?.Split(',');
                }

                return Json(vm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get View Image Detail
        /// </summary>
        /// <param name="searchKey"></param>
        /// <param name="media_id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(string searchKey, int media_id, int pageIndex = 1)
        {
            try
            {
                DetailsViewModel vm = new DetailsViewModel();

                List<ApplicationUser> user = this._context.Users.Where(c => c.Id.Equals(this.UserId)).ToList();
                var media = await this.userMediaManagementService.GetUserMediaAsync(media_id);
                if (media != null)
                {
                    vm.UserMediaDetail = media;
                    vm.SameTitleUserMediaList = await this.userMediaManagementService.GetUserMediaSameTitleMediasAsync(media);
                    vm.RelatedUserMediaList = await this.userMediaManagementService.GetUserMediaRelatedMediasAsync(media);

                    string[] projectTags = vm.UserMediaDetail.ProjectTags?.Split(',');

                    //上限
                    int totalPages = (int)System.Math.Ceiling((decimal)(vm.RelatedUserMediaList.Count / (decimal)pageSize));
                    int skip = (pageIndex - 1) * pageSize;
                    int totalRecordCount = vm.RelatedUserMediaList.Count;

                    ViewBag.ItemCount = vm.RelatedUserMediaList.Count;

                    //vm.RelatedUserMediaList = vm.RelatedUserMediaList.Skip(skip).Take(pageSize).ToList();

                    vm.PageIndex = pageIndex;
                    vm.TotalPages = totalPages;
                    vm.SearchKey = searchKey;
                    vm.TotalRecords = totalRecordCount;
                    vm.Discriminator = user[0].Discriminator;

                    //ViewBag.MediaID = media_id;
                    //ViewBag.PorjectTags = projectTags ?? null;
                    ////ViewBag.PhotoTags = string.Join(',', photo_tags_list.ToArray());
                    //ViewBag.RelatedUserMediaList = JsonConvert.SerializeObject(vm.RelatedUserMediaList);

                    return PartialView("/Views/Home/Details.cshtml", vm);
                }
                else
                {
                    TempData["Message"] = "該当するイメージが見つかりませんでした。最初からやり直してください。";
                    return View("~/Views/Error/204.cshtml");
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }
        }

        [HttpPost]
        public async Task<object> getSimilarMedia(string searchKey, int media_id, int pageIndex = 1)
        {
            DetailsViewModel vm = new DetailsViewModel();

            var media = await this.userMediaManagementService.GetUserMediaAsync(media_id);
            if (media != null)
            {
                vm.UserMediaDetail = media;

                int skip = (pageIndex - 1) * pageSize;
                var result = await this.userMediaManagementService.GetUserMediaRelatedMediasAsync(media, skip);
                vm.RelatedUserMediaList = result;
                //int totalPages = (int)System.Math.Ceiling((decimal)(vm.RelatedUserMediaList.Count / (decimal)pageSize));
                //int totalRecordCount = vm.RelatedUserMediaList.Count;

                //ViewBag.ItemCount = vm.RelatedUserMediaList.Count;
                //vm.RelatedUserMediaList = vm.RelatedUserMediaList.Skip(skip).Take(pageSize).ToList();

                vm.SearchKey = searchKey;
                vm.PageIndex = pageIndex;
                //vm.TotalPages = totalPages;
                //vm.TotalRecords = totalRecordCount;
                ////vm.Discriminator = user[0].Discriminator;

                return Json(vm);
            }
            else
            {
                return null;

            }
        }

        /// <summary>
        /// Image View For Edit Or Delete
        /// </summary>
        /// <param name="media_id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditUserMedia(int media_id)
        {
            try
            {
                DetailsViewModel vm = new DetailsViewModel();

                //UserMediaの取得
                var media = await this.userMediaManagementService.GetUserMediaAsync(media_id);

                string[] tags = media.Tags.Split('|');
                IList<string> photo_tags_list = new List<string>();
                IList<string> project_tags_list = new List<string>();

                foreach (var tag in tags)
                {
                    string[] current_tags = tag.Split(':');
                    if (current_tags != null && current_tags.Length == 3)
                    {
                        string current_tag_name = current_tags[0].Trim();
                        if (!string.IsNullOrEmpty(current_tag_name))
                        {
                            decimal current_score = decimal.Parse(current_tags[1]);
                            if (current_score < 1)
                                photo_tags_list.Add(current_tag_name);
                            else
                                project_tags_list.Add(current_tag_name);
                        }
                    }
                }
                string[] projectTags = media.ProjectTags?.Split('|');
                if (projectTags != null)
                {
                    foreach (var tag in projectTags)
                    {
                        project_tags_list.Add(tag);
                    }
                }

                vm.UserMediaDetail = media;
                ViewBag.MediaID = media_id;
                ViewBag.PorjectTags = (projectTags != null) ? string.Join(',', projectTags?.ToArray()) : null;
                ViewBag.PhotoTags = string.Join(',', photo_tags_list.ToArray());

                return PartialView("/Views/Shared/_ImageViewer.cshtml", vm);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }
        }

        /// <summary>
        /// Delete Image
        /// </summary>
        /// <param name="media_id"></param>
        /// <returns></returns>
        [HttpPost]
        //public async Task DeleteUserMedia(string searchKey, int media_id)
        public async Task<IActionResult> DeleteUserMedia(int media_id)
        {
            try
            {
                //UserMediaの取得 1件削除
                var media = await this.userMediaManagementService.GetUserMediaAsync(media_id);
                List<UserMedia> mediaList = new List<UserMedia> { media };
                await this.userMediaManagementService.DeleteFromUserMediaList(mediaList);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errMsg = ex.Message });
            }
        }
        /// <summary>
        /// Delete Images
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        //public async Task DeleteUserMedia(string searchKey, int media_id)
//        public async Task<IActionResult> DeleteUserMedias(int[] ids)
        public async Task<IActionResult> DeleteUserMedias(string ids)
        {
            try
            {
                List<UserMedia> mediaList = new List<UserMedia>();
                foreach (var mediaId in ids?.Split(","))
                {
                    //UserMediaの取得 1件ずつ
                    var media = await this.userMediaManagementService.GetUserMediaAsync(int.Parse(mediaId));
                    mediaList.Add(media);
                }
                await this.userMediaManagementService.DeleteFromUserMediaList(mediaList);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errMsg = ex.Message });
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Data.Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
