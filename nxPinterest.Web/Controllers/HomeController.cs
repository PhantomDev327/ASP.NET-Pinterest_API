using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nxPinterest.Web.Models;
using System.Diagnostics;
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
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> GetUserMediaDetails(int media_id) {
            Data.Models.UserMedia userMedia = await this.userMediaManagementService.GetUserMediaDetailsByIDAsync(media_id);
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
