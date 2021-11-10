using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using nxPinterest.Data;
using nxPinterest.Data.Models;
using nxPinterest.Services;
using nxPinterest.Services.Models.Request;
using nxPinterest.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text;
using nxPinterest.Web.Models;
using nxPinterest.Services.Interfaces;

namespace nxPinterest.Web.Controllers
{
    [Authorize]
    public class UserMediaManageController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private IUserMediaManagementService _mediaManagementService;
        private readonly ApplicationDbContext _context;
        private readonly Models.DestinationPathModel _destinationPathModel;
        private Base64stringUtility encode = new Base64stringUtility("UTF-8");
        public UserMediaManageController(ILogger<HomeController> logger,
                                         IOptions<Models.DestinationPathModel> destinationPathModel,
                                         ApplicationDbContext context,
                                         IUserMediaManagementService mediaManagementService)
        {
            this._logger = logger;
            this._context = context;
            this._destinationPathModel = destinationPathModel.Value;
            _mediaManagementService = mediaManagementService;
        }

        /// <summary>
        /// Create Media User
        /// </summary>
        /// <param name="request">Data from</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UploadMediaFile(ImageRegistrationRequests request)
        {
            // Validate param
            if (!ModelState.IsValid)
            {
                // To Do
                ViewBag.Message = "Validate fails!";
                return View("~/Views/Error/204.cshtml");
            }

            // Store
            try
            {
                _mediaManagementService.UploadMediaFile(request, UserId);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }

            return RedirectToAction("Index","Home");
        }
    }
}
