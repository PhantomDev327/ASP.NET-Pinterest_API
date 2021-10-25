using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nxPinterest.Web.Models;
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
            vm.UserMediaList = await this.userMediaManagementService.ListUserMediaAsyc(pageIndex, pageSize, this.UserId, searchKey);

            int totalPages = (int)System.Math.Ceiling((decimal)(vm.UserMediaList.Count / (decimal)pageSize));
            int skip = (pageIndex - 1) * pageSize;

            ViewBag.ItemCount = vm.UserMediaList.Count;

            vm.UserMediaList = vm.UserMediaList.Skip(skip).Take(pageSize).ToList();

            vm.PageIndex = pageIndex;
            vm.TotalPages = totalPages;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> GetUserMediaDetails(int media_id) {
            Data.Models.UserMedia userMedia = await this.userMediaManagementService.GetUserMediaDetailsByIDAsync(media_id);
            string[] tags = userMedia.Tags.Split('|');
            IList<string> photo_tags = new List<string>();

            for (int i = 0; i < tags.Count(); i++) {
                string[] current_tags = tags[i].Split(':');

                photo_tags.Add(current_tags[0]);
            }

            ViewBag.PhotoTags = string.Join(',', photo_tags.ToArray());

            return PartialView("/Views/Shared/_ImageViewer.cshtml", userMedia);
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
