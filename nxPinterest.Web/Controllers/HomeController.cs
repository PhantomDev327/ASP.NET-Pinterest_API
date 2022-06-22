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

namespace nxPinterest.Web.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        public const int pageSize = nxPinterest.Services.dev_Settings.pageSize_regist;
        private readonly Services.Interfaces.IUserMediaManagementService userMediaManagementService;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger,
                              ApplicationDbContext context,
                              Services.Interfaces.IUserMediaManagementService userMediaManagementService
                              )
        {
            _logger = logger;
            _context = context;
            this.userMediaManagementService = userMediaManagementService;
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

            return View(vm);
        }

        [HttpPost]
        public async Task<object> GetUserMediaDetailsAjax(int media_id)
        {
            try
            {
                UserMediaDetailViewModel result = await this.userMediaManagementService.GetUserMediaDetailsByIDAsync(media_id);

                string[] tags = result.UserMediaDetail.Tags.Split('|');
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

                // 似ている画像取得
                IList<Data.Models.UserMedia> UserMediaList = await this.userMediaManagementService.SearchUserMediaAsync("", result.UserMediaDetail.container_id);

                IList<Data.Models.UserMedia> tempUserMediaList = new List<Data.Models.UserMedia>();
                tempUserMediaList = UserMediaList;

                Dictionary<int, List<string>> mediaTagsList = new Dictionary<int, List<string>>();
                List<int> mediaIdList = new List<int>();

                foreach (var value in tempUserMediaList)
                {
                    string[] phototags = value?.Tags.Split('|');
                    List<string> phototags_list = new List<string>();

                    foreach (var tag in phototags)
                    {
                        string[] current_tags = tag.Split(':');
                        string current_tag_name = current_tags[0].Trim();
                        if (!string.IsNullOrEmpty(current_tag_name))
                        {
                            double current_score = double.Parse(current_tags[1]);
                            if (current_score > 0.7)
                            {
                                phototags_list.Add(current_tag_name);
                            }
                        }
                    }
                    mediaTagsList.Add(value.MediaId, phototags_list);
                }

                foreach (KeyValuePair<int, List<string>> keyVal in mediaTagsList)
                {
                    if (mediaTagsList[media_id].Intersect(keyVal.Value).Count() >= 5)
                    {
                        mediaIdList.Add(keyVal.Key);
                        continue;
                    }
                }
                result.RelatedUserMediaList = UserMediaList.Where(v => mediaIdList.Contains(v.MediaId)).ToList();

                //似ている画像で選んだ画像が含んで除く件
                for (int i = 0; i < result.RelatedUserMediaList.Count; i++)
                {
                    if (result.RelatedUserMediaList[i].MediaId == media_id)
                    {
                        result.RelatedUserMediaList.Remove(result.RelatedUserMediaList[i]);
                    }
                }
                //追加ロジック ssa20220526
                result.project_tags_list = project_tags_list;

                return Json(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IActionResult> GetUserMediaDetails(int media_id)
        {
            try
            {
                UserMediaDetailViewModel result = await this.userMediaManagementService.GetUserMediaDetailsByIDAsync(media_id);

                string[] tags = result.UserMediaDetail.Tags.Split('|');
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

                // 似ている画像取得
                IList<Data.Models.UserMedia> UserMediaList = await this.userMediaManagementService.SearchUserMediaAsync("", result.UserMediaDetail.container_id);
                
                IList<Data.Models.UserMedia> tempUserMediaList = new List<Data.Models.UserMedia>();
                tempUserMediaList = UserMediaList;

                Dictionary<int, List<string>> mediaTagsList = new Dictionary<int, List<string>>();
                List<int> mediaIdList = new List<int>();

                foreach (var value in tempUserMediaList)
                {
                    string[] phototags = value?.Tags.Split('|');
                    List<string> phototags_list = new List<string>();

                    foreach (var tag in phototags)
                    {
                        string[] current_tags = tag.Split(':');
                        string current_tag_name = current_tags[0].Trim();
                        if (!string.IsNullOrEmpty(current_tag_name))
                        {
                            double current_score = double.Parse(current_tags[1]);
                            if (current_score > 0.7)
                            {
                                phototags_list.Add(current_tag_name);
                            }
                        }
                    }
                    mediaTagsList.Add(value.MediaId, phototags_list);
                }

                foreach (KeyValuePair<int, List<string>> keyVal in mediaTagsList)
                {
                    if (mediaTagsList[media_id].Intersect(keyVal.Value).Count() >= 5)
                    {
                        mediaIdList.Add(keyVal.Key);
                        continue;
                    }
                }
                result.RelatedUserMediaList = UserMediaList.Where(v => mediaIdList.Contains(v.MediaId)).ToList();

                //似ている画像で選んだ画像が含んで除く件
                for (int i = 0; i < result.RelatedUserMediaList.Count; i++)
                {
                    if (result.RelatedUserMediaList[i].MediaId == media_id)
                    {
                        result.RelatedUserMediaList.Remove(result.RelatedUserMediaList[i]);
                    }
                }
                //追加ロジック ssa20220526
                result.project_tags_list = project_tags_list;
                ViewBag.RelatedUserMediaList = JsonConvert.SerializeObject(result.RelatedUserMediaList);
                ViewBag.MediaID = media_id;
                ViewBag.PorjectTags = (projectTags != null) ? string.Join(',', projectTags?.ToArray()) : null;
                ViewBag.PhotoTags = string.Join(',', photo_tags_list.ToArray());

                return PartialView("/Views/Shared/_ImageDetail.cshtml", result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IActionResult> GetUserMediaViewer(int media_id)
        {
            try
            {
                UserMediaDetailViewModel result = await this.userMediaManagementService.GetUserMediaDetailsByIDAsync(media_id);

                string[] tags = result.UserMediaDetail.Tags.Split('|');
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

                // 似ている画像取得
                IList<Data.Models.UserMedia> UserMediaList = await this.userMediaManagementService.SearchUserMediaAsync("", result.UserMediaDetail.container_id);

                IList<Data.Models.UserMedia> tempUserMediaList = new List<Data.Models.UserMedia>();
                tempUserMediaList = UserMediaList;

                Dictionary<int, List<string>> mediaTagsList = new Dictionary<int, List<string>>();
                List<int> mediaIdList = new List<int>();

                foreach (var value in tempUserMediaList)
                {
                    string[] phototags = value?.Tags.Split('|');
                    List<string> phototags_list = new List<string>();

                    foreach (var tag in phototags)
                    {
                        string[] current_tags = tag.Split(':');
                        string current_tag_name = current_tags[0].Trim();
                        if (!string.IsNullOrEmpty(current_tag_name))
                        {
                            double current_score = double.Parse(current_tags[1]);
                            if (current_score > 0.7)
                            {
                                phototags_list.Add(current_tag_name);
                            }
                        }
                    }
                    mediaTagsList.Add(value.MediaId, phototags_list);
                }

                foreach (KeyValuePair<int, List<string>> keyVal in mediaTagsList)
                {
                    if (mediaTagsList[media_id].Intersect(keyVal.Value).Count() >= 5)
                    {
                        mediaIdList.Add(keyVal.Key);
                        continue;
                    }
                }
                result.RelatedUserMediaList = UserMediaList.Where(v => mediaIdList.Contains(v.MediaId)).ToList();
                result.project_tags_list = project_tags_list;
                ViewBag.RelatedUserMediaList = JsonConvert.SerializeObject(result.RelatedUserMediaList);
                ViewBag.MediaID = media_id;
                ViewBag.PorjectTags = (projectTags != null) ? string.Join(',', projectTags?.ToArray()) : null;
                ViewBag.PhotoTags = string.Join(',', photo_tags_list.ToArray());

                return PartialView("/Views/Shared/_ImageViewer.cshtml", result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserMedia(int media_id)
        {
            try
            {
                UserMediaDetailViewModel result = await this.userMediaManagementService.GetUserMediaDetailsByIDAsync(media_id);
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
