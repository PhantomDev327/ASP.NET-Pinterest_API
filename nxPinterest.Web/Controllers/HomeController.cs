using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private CosmosDbService _cosmosDbService;

        public HomeController(ILogger<HomeController> logger,
                              ApplicationDbContext context,
                              Services.Interfaces.IUserMediaManagementService userMediaManagementService
                              )
        {
            _logger = logger;
            _context = context;
            this.userMediaManagementService = userMediaManagementService;
            _cosmosDbService = new CosmosDbService();
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

            nxPinterest.Services.CognitiveSearchService cognitiveSearchService = new Services.CognitiveSearchService();
            List<ApplicationUser> user = this._context.Users.Where(c => c.Id.Equals(this.UserId)).ToList();
            // OLD : SQL DB
            //vm.UserMediaList = await this.userMediaManagementService.SearchUserMediaAsync(searchKey, user[0].container_id);

            // NEW : Cosmos DB
            nxPinterest.Services.CosmosDbService cosmosDbService = new Services.CosmosDbService();
            vm.UserMediaList = await cosmosDbService.SelectByUserIDAsync(dev_Settings.cosmos_databaseName, dev_Settings.cosmos_containerName, searchKey, user[0].container_id.ToString());

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

        /// <summary>
        /// Get View Image Detail
        /// </summary>
        /// <param name="searchKey"></param>
        /// <param name="media_id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetUserMediaDetails(string searchKey,int media_id)
        {
            try
            {
                // OLD : SQL DB
                //UserMediaDetailViewModel result = await this.userMediaManagementService.GetUserMediaDetailsByIDAsync(media_id);
                // NEW : Cosmos DB
                nxPinterest.Services.CosmosDbService cosmosDbService = new Services.CosmosDbService();
                UserMediaDetailViewModel result = await cosmosDbService.GetUserMediaDetailsCosmosByIDAsync(dev_Settings.cosmos_databaseName, dev_Settings.cosmos_containerName, media_id);

                string[] tags = result.UserMediaDetailCosmos.Tags.Split('|');
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
                string[] projectTags = result.UserMediaDetailCosmos.ProjectTags?.Split('|');

                // 似ている画像取得
                // OLD : SQL DB
                //IList<Data.Models.UserMedia> UserMediaList = await this.userMediaManagementService.SearchUserMediaAsync("", result.UserMediaDetail.container_id);
                //IList<Data.Models.UserMedia> tempUserMediaList = new List<Data.Models.UserMedia>();
                //tempUserMediaList = UserMediaList;

                // NEW : Cosmos DB
                IList<Data.Models.UserMediaCosmosJSON> UserMediaList = await cosmosDbService.SelectByUserIDAsync(dev_Settings.cosmos_databaseName, dev_Settings.cosmos_containerName, searchKey, result.UserMediaDetailCosmos.container_id);
                IList<Data.Models.UserMediaCosmosJSON> tempUserMediaList = new List<Data.Models.UserMediaCosmosJSON>();
                tempUserMediaList = UserMediaList;

                Dictionary<int, List<string>> mediaTagsList = new Dictionary<int, List<string>>();
                List<int> mediaIdList = new List<int>();

                foreach (var value in tempUserMediaList)
                {
                    string[] phototags = value?.Tags != null? value?.Tags.ToString().Split('|'): null ;
                    List<string> phototags_list = new List<string>();

                    if (phototags != null && phototags.Length > 0)
                    {
                        foreach (var tag in phototags)
                        {
                            string[] current_tags = tag.Split(':');
                            if(current_tags != null && current_tags.Length == 3)
                            {
                                string current_tag_name = current_tags[0].Trim();
                                if (!string.IsNullOrEmpty(current_tag_name))
                                {
                                    double current_score = double.Parse(current_tags[1]);
                                    if (current_score >= dev_Settings.tag_confidence_threshold && current_score != 1)
                                    {
                                        phototags_list.Add(current_tag_name);
                                    }
                                }
                            } 
                        }
                        mediaTagsList.Add(value.MediaId, phototags_list);
                    }                   
                }

                if (mediaTagsList.Count > 0)
                {
                    foreach (KeyValuePair<int, List<string>> keyVal in mediaTagsList)
                    {
                        if (mediaTagsList[media_id].Intersect(keyVal.Value).Count() >= 5)
                        {
                            mediaIdList.Add(keyVal.Key);
                            continue;
                        }
                    }
                }

                result.RelatedUserMediaCosmosList = UserMediaList.Where(v => mediaIdList.Contains(v.MediaId)).ToList();

                //似ている画像で選んだ画像が含んで除く件
                for (int i = 0; i < result.RelatedUserMediaCosmosList.Count; i++)
                {
                    if (result.RelatedUserMediaCosmosList[i].MediaId == media_id)
                    {
                        result.RelatedUserMediaCosmosList.Remove(result.RelatedUserMediaCosmosList[i]);
                    }
                }
                result.project_tags_list = project_tags_list;

                ViewBag.MediaID = media_id;
                ViewBag.PorjectTags = (projectTags != null) ? string.Join(',', projectTags?.ToArray()) : null;
                ViewBag.PhotoTags = string.Join(',', photo_tags_list.ToArray());

                return PartialView("/Views/Shared/_ImageDetail.cshtml", result);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }
        }

        /// <summary>
        /// Image View For Edit Or Delete
        /// </summary>
        /// <param name="media_id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetUserMediaViewer(int media_id)
        {
            try
            {
                // OLD : SQL DB
                //UserMediaDetailViewModel result = await this.userMediaManagementService.GetUserMediaDetailsByIDAsync(media_id);
                // NEW : Cosmos DB
                nxPinterest.Services.CosmosDbService cosmosDbService = new Services.CosmosDbService();
                UserMediaDetailViewModel result = await cosmosDbService.GetUserMediaDetailsCosmosByIDAsync(dev_Settings.cosmos_databaseName, dev_Settings.cosmos_containerName, media_id);

                string[] tags = result.UserMediaDetailCosmos.Tags.Split('|');
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
                string[] projectTags = result.UserMediaDetailCosmos.ProjectTags?.Split('|');
                if (projectTags != null)
                {
                    foreach (var tag in projectTags)
                    {
                        project_tags_list.Add(tag);
                    }
                }

                // 似ている画像取得
                // OLD : SQL DB
                //IList<Data.Models.UserMedia> UserMediaList = await this.userMediaManagementService.SearchUserMediaAsync("", result.UserMediaDetail.container_id);
                // NEW : Cosmos DB
                IList<Data.Models.UserMediaCosmosJSON> UserMediaList = await cosmosDbService.SelectByUserIDAsync(dev_Settings.cosmos_databaseName, dev_Settings.cosmos_containerName, "", result.UserMediaDetailCosmos.container_id);
                IList<Data.Models.UserMediaCosmosJSON> tempUserMediaList = new List<Data.Models.UserMediaCosmosJSON>();
                tempUserMediaList = UserMediaList;

                Dictionary<int, List<string>> mediaTagsList = new Dictionary<int, List<string>>();
                List<int> mediaIdList = new List<int>();

                foreach (var value in tempUserMediaList)
                {
                    string[] phototags = value?.Tags != null ? value?.Tags.ToString().Split('|') : null;
                    List<string> phototags_list = new List<string>();

                    if (phototags != null && phototags.Length > 0)
                    {
                        foreach (var tag in phototags)
                        {
                            string[] current_tags = tag.Split(':');
                            if (current_tags != null && current_tags.Length == 3)
                            {
                                string current_tag_name = current_tags[0].Trim();
                                if (!string.IsNullOrEmpty(current_tag_name))
                                {
                                    double current_score = double.Parse(current_tags[1]);
                                    if (current_score >= dev_Settings.tag_confidence_threshold && current_score != 1)
                                    {
                                        phototags_list.Add(current_tag_name);
                                    }
                                }
                            }
                        }
                        mediaTagsList.Add(value.MediaId, phototags_list);
                    }
                }

                if (mediaTagsList.Count > 0)
                {
                    foreach (KeyValuePair<int, List<string>> keyVal in mediaTagsList)
                    {
                        if (mediaTagsList[media_id].Intersect(keyVal.Value).Count() >= 5)
                        {
                            mediaIdList.Add(keyVal.Key);
                            continue;
                        }
                    }
                }
                result.RelatedUserMediaCosmosList = UserMediaList.Where(v => mediaIdList.Contains(v.MediaId)).ToList();
                result.project_tags_list = project_tags_list;

                //似ている画像で選んだ画像が含んで除く件
                for (int i = 0; i < result.RelatedUserMediaCosmosList.Count; i++)
                {
                    if (result.RelatedUserMediaCosmosList[i].MediaId == media_id)
                    {
                        result.RelatedUserMediaCosmosList.Remove(result.RelatedUserMediaCosmosList[i]);
                    }
                }

                ViewBag.MediaID = media_id;
                ViewBag.PorjectTags = (projectTags != null) ? string.Join(',', projectTags?.ToArray()) : null;
                ViewBag.PhotoTags = string.Join(',', photo_tags_list.ToArray());

                return PartialView("/Views/Shared/_ImageViewer.cshtml", result);
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
        /// <param name="searchKey"></param>
        /// <param name="media_id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task DeleteUserMedia(string searchKey,int media_id)
        {
            // OLD : SQL DB
            //UserMediaDetailViewModel result = await this.userMediaManagementService.GetUserMediaDetailsByIDAsync(media_id);
            //await this.userMediaManagementService.DeleteFromUserMedia(result.UserMediaDetail);
            // NEW : Cosmos DB
            CosmosClient cosmosClient = new CosmosClient(dev_Settings.cosmos_endpointUri, dev_Settings.cosmos_accountKey);
            Database database = cosmosClient.GetDatabase(dev_Settings.cosmos_databaseName);
            Container container = database.GetContainer(dev_Settings.cosmos_containerName);

            UserMediaCosmosJSON _userMedia = container.GetItemLinqQueryable<UserMediaCosmosJSON>(true)
                                                                .Where(x => x.MediaId == media_id)
                                                                .AsEnumerable()
                                                                .FirstOrDefault();
            await _cosmosDbService.DeleteItemAsync(dev_Settings.cosmos_databaseName, dev_Settings.cosmos_containerName, _userMedia.Id);
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
