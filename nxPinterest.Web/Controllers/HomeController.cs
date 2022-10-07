using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using nxPinterest.Data;
using nxPinterest.Data.Models;
using nxPinterest.Services.Models.Response;
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
        public const int pageSize = nxPinterest.Services.dev_Settings.pageSize_regist;
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

            vm.UserMediaList = await this.userMediaManagementService.SearchUserMediaAsync(searchKey, user[0].container_id);

            int totalPages = (int)System.Math.Ceiling((decimal)(vm.UserMediaList.Count / (decimal)pageSize));
            int skip = (pageIndex - 1) * pageSize;
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
            vm.TagsList = await this.userMediaManagementService.GetOftenUseTagsAsyc(user[0].container_id);

            //登録画面で使用されているタグ候補
            vm.ImageRegistrationVM.SuggestTagsList = vm.TagsList;

            return View(vm);
        }

        [HttpPost]
        public async Task<object> getMedia(int pageIndex = 1, string searchKey = "")
        {
            HomeViewModel vm = new HomeViewModel();

            List<ApplicationUser> user = this._context.Users.Where(c => c.Id.Equals(this.UserId)).ToList();
            vm.UserMediaList = await this.userMediaManagementService.SearchUserMediaAsync(searchKey, user[0].container_id);

            int totalPages = (int)System.Math.Ceiling((decimal)(vm.UserMediaList.Count / (decimal)pageSize));
            int skip = (pageIndex - 1) * pageSize;
            int totalRecordCount = vm.UserMediaList.Count;

            ViewBag.ItemCount = vm.UserMediaList.Count;
            ViewBag.UserDispName = user[0].UserDispName;

            vm.UserMediaList = vm.UserMediaList.Skip(skip).Take(pageSize).ToList();

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
                UserMediaDetailModel result = await this.userMediaManagementService.GetUserMediaDetailsByIDAsync(media_id);
                if(result != null)
                {
                    vm.UserMediaDetail = result.UserMediaDetail;
                    vm.SameTitleUserMediaList = result.SameTitleUserMediaList;
                    vm.RelatedUserMediaList = result.RelatedUserMediaList;
                    string[] tags = vm.UserMediaDetail.Tags.Split('|');

                    IList<string> photo_tags_list = new List<string>();
                    IList<string> project_tags_list = new List<string>();

                    foreach (var tag in tags)
                    {
                        string[] current_tags = tag.Split(':');
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
                    string[] projectTags = result.UserMediaDetail.ProjectTags?.Split('|');
                    if (projectTags != null)
                    {
                        foreach (var tag in projectTags)
                        {
                            project_tags_list.Add(tag);
                        }
                    }
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
                UserMediaDetailModel result = await this.userMediaManagementService.GetUserMediaDetailsByIDAsync(media_id);
                if (result != null)
                {
                    vm.UserMediaDetail = result.UserMediaDetail;
                    vm.SameTitleUserMediaList = result.SameTitleUserMediaList;
                    vm.RelatedUserMediaList = result.RelatedUserMediaList;
                    string[] tags = vm.UserMediaDetail.Tags.Split('|');

                    IList<string> photo_tags_list = new List<string>();
                    IList<string> project_tags_list = new List<string>();

                    //TODO　tagsにphotoタグもprojectタグも一か所に格納されている前提の作り
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
                    string[] projectTags = vm.UserMediaDetail.ProjectTags?.Split('|');

                    //上限
                    int totalPages = (int)System.Math.Ceiling((decimal)(vm.RelatedUserMediaList.Count / (decimal)pageSize));
                    int skip = (pageIndex - 1) * pageSize;
                    int totalRecordCount = vm.RelatedUserMediaList.Count;

                    ViewBag.ItemCount = vm.RelatedUserMediaList.Count;

                    vm.RelatedUserMediaList = vm.RelatedUserMediaList.Skip(skip).Take(pageSize).ToList();

                    vm.PageIndex = pageIndex;
                    vm.TotalPages = totalPages;
                    vm.SearchKey = searchKey;
                    vm.TotalRecords = totalRecordCount;
                    vm.Discriminator = user[0].Discriminator;

                    ViewBag.MediaID = media_id;
                    ViewBag.PorjectTags = (projectTags != null) ? string.Join(',', projectTags?.ToArray()) : null;
                    ViewBag.PhotoTags = string.Join(',', photo_tags_list.ToArray());
                    ViewBag.RelatedUserMediaList = JsonConvert.SerializeObject(vm.RelatedUserMediaList);

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
            var result = await this.userMediaManagementService.SearchSimilarImagesAsync(media, media.ContainerId);
            if (result != null)
            {
                vm.UserMediaDetail = media;
                //vm.SameTitleUserMediaList = result.SameTitleUserMediaList;
                vm.RelatedUserMediaList = result;

                int totalPages = (int)System.Math.Ceiling((decimal)(vm.RelatedUserMediaList.Count / (decimal)pageSize));
                int skip = (pageIndex - 1) * pageSize;
                int totalRecordCount = vm.RelatedUserMediaList.Count;

                ViewBag.ItemCount = vm.RelatedUserMediaList.Count;
                vm.RelatedUserMediaList = vm.RelatedUserMediaList.Skip(skip).Take(pageSize).ToList();

                vm.PageIndex = pageIndex;
                vm.TotalPages = totalPages;
                vm.SearchKey = searchKey;
                vm.TotalRecords = totalRecordCount;
                //vm.Discriminator = user[0].Discriminator;

                return Json(vm);
            }
            else
            {
                return null;

            }
        }

        ///// <summary>
        ///// Image View For Edit Or Delete
        ///// </summary>
        ///// <param name="media_id"></param>
        ///// <returns></returns>
        //public async Task<IActionResult> GetUserMediaViewer(int media_id)
        //{
        //    try
        //    {
        //        UserMediaDetailModel result = await this.userMediaManagementService.GetUserMediaDetailsByIDAsync(media_id);

        //        string[] tags = result.UserMediaDetail.Tags.Split('|');
        //        IList<string> photo_tags_list = new List<string>();
        //        IList<string> project_tags_list = new List<string>();

        //        foreach (var tag in tags)
        //        {
        //            string[] current_tags = tag.Split(':');
        //            if (current_tags != null && current_tags.Length == 3)
        //            {
        //                string current_tag_name = current_tags[0].Trim();
        //                if (!string.IsNullOrEmpty(current_tag_name))
        //                {
        //                    decimal current_score = decimal.Parse(current_tags[1]);
        //                    if (current_score < 1)
        //                        photo_tags_list.Add(current_tag_name);
        //                    else
        //                        project_tags_list.Add(current_tag_name);
        //                }
        //            }
        //        }
        //        string[] projectTags = result.UserMediaDetail.ProjectTags?.Split('|');
        //        //string[] projectTags = result.UserMediaDetailCosmos.ProjectTags?.Split('|');
        //        if (projectTags != null)
        //        {
        //            foreach (var tag in projectTags)
        //            {
        //                project_tags_list.Add(tag);
        //            }
        //        }

        //        // 似ている画像取得
        //        IList<Data.Models.UserMedia> UserMediaList = await this.userMediaManagementService.SearchUserMediaAsync("", result.UserMediaDetail.ContainerId);
        //        IList<Data.Models.UserMedia> tempUserMediaList = new List<Data.Models.UserMedia>();
        //        tempUserMediaList = UserMediaList;

        //        Dictionary<int, List<string>> mediaTagsList = new Dictionary<int, List<string>>();
        //        List<int> mediaIdList = new List<int>();

        //        foreach (var value in tempUserMediaList)
        //        {
        //            string[] phototags = value?.Tags != null ? value?.Tags.ToString().Split('|') : null;
        //            List<string> phototags_list = new List<string>();

        //            if (phototags != null && phototags.Length > 0)
        //            {
        //                foreach (var tag in phototags)
        //                {
        //                    string[] current_tags = tag.Split(':');
        //                    if (current_tags != null && current_tags.Length == 3)
        //                    {
        //                        string current_tag_name = current_tags[0].Trim();
        //                        if (!string.IsNullOrEmpty(current_tag_name))
        //                        {
        //                            double current_score = double.Parse(current_tags[1]);
        //                            if (current_score >= dev_Settings.tag_confidence_threshold && current_score != 1)
        //                            {
        //                                phototags_list.Add(current_tag_name);
        //                            }
        //                        }
        //                    }
        //                }
        //                mediaTagsList.Add(value.MediaId, phototags_list);
        //            }
        //        }

        //        if (mediaTagsList.Count > 0)
        //        {
        //            foreach (KeyValuePair<int, List<string>> keyVal in mediaTagsList)
        //            {
        //                if (mediaTagsList[media_id].Intersect(keyVal.Value).Count() >= 5)
        //                {
        //                    mediaIdList.Add(keyVal.Key);
        //                    continue;
        //                }
        //            }
        //        }
        //        result.RelatedUserMediaList = UserMediaList.Where(v => mediaIdList.Contains(v.MediaId)).ToList();

        //        //ViewBag.RelatedUserMediaList = JsonConvert.SerializeObject(result.RelatedUserMediaList);

        //        //似ている画像で選んだ画像が含んで除く件
        //        //for (int i = 0; i < result.RelatedUserMediaCosmosList.Count; i++)
        //        //{
        //        //    if (result.RelatedUserMediaCosmosList[i].MediaId == media_id)
        //        //    {
        //        //        result.RelatedUserMediaCosmosList.Remove(result.RelatedUserMediaCosmosList[i]);
        //        //    }
        //        //}
        //        for (int i = 0; i < result.RelatedUserMediaList.Count; i++)
        //        {
        //            if (result.RelatedUserMediaList[i].MediaId == media_id)
        //            {
        //                result.RelatedUserMediaList.Remove(result.RelatedUserMediaList[i]);
        //            }
        //        }

        //        ViewBag.MediaID = media_id;
        //        ViewBag.PorjectTags = (projectTags != null) ? string.Join(',', projectTags?.ToArray()) : null;
        //        ViewBag.PhotoTags = string.Join(',', photo_tags_list.ToArray());

        //        return PartialView("/Views/Shared/_ImageViewer.cshtml", result);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Message"] = ex.Message;
        //        return View("~/Views/Error/204.cshtml");
        //    }
        //}

        /// <summary>
        /// Delete Image
        /// </summary>
        /// <param name="searchKey"></param>
        /// <param name="media_id"></param>
        /// <returns></returns>
        [HttpPost]
        //public async Task DeleteUserMedia(string searchKey, int media_id)
        public async Task<IActionResult> DeleteUserMedia(int media_id)
        {
            try
            {
                UserMediaDetailModel result = await this.userMediaManagementService.GetUserMediaDetailsByIDAsync(media_id);
                await this.userMediaManagementService.DeleteFromUserMedia(result.UserMediaDetail);
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
