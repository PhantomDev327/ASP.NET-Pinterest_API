using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        public HomeController(ILogger<HomeController> logger,
                              Services.Interfaces.IUserMediaManagementService userMediaManagementService)
        {
            _logger = logger;
            this.userMediaManagementService = userMediaManagementService;
        }

        public async Task<IActionResult> Index(int pageIndex = 1, string searchKey = "")
        {
            HomeViewModel vm = new HomeViewModel();

            if (string.IsNullOrEmpty(searchKey))
                vm.UserMediaList = await this.userMediaManagementService.ListUserMediaAsyc(this.UserId);
            else
                vm.UserMediaList = await this.userMediaManagementService.SearchUserMediaAsync(searchKey, this.UserId);

            int totalPages = (int)System.Math.Ceiling((decimal)(vm.UserMediaList.Count / (decimal)pageSize));
            int skip = (pageIndex - 1) * pageSize;

            ViewBag.ItemCount = vm.UserMediaList.Count;

            vm.UserMediaList = vm.UserMediaList.Skip(skip).Take(pageSize).ToList();

            vm.PageIndex = pageIndex;
            vm.TotalPages = totalPages;
            vm.SearchKey = searchKey;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> GetUserMediaDetails(int media_id)
        {
            try
            {
                UserMediaDetailViewModel result = await this.userMediaManagementService.GetUserMediaDetailsByIDAsync(media_id);

                string[] tags = result.UserMediaDetail.Tags.Split('|');
                IList<string> photo_tags = new List<string>();
                IList<string> project_tags = new List<string>();

                for (int i = 0; i < tags.Count(); i++)
                {
                    string[] current_tags = tags[i].Split(':');
                    string current_tag_name = current_tags[0].Trim();
                    if (!string.IsNullOrEmpty(current_tag_name)) {
                        decimal current_score = decimal.Parse(current_tags[1]);
                        if (current_score < 1)
                            photo_tags.Add(current_tag_name);
                        else
                            project_tags.Add(current_tag_name);
                    }
                }

                ViewBag.PorjectTags = string.Join(',', project_tags.ToArray());
                ViewBag.PhotoTags = string.Join(',', photo_tags.ToArray());

                return PartialView("/Views/Shared/_ImageViewer.cshtml", result);
            }
            catch (Exception ex) {
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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
