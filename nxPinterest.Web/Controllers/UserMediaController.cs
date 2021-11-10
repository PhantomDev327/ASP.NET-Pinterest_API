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
    public class UserMediaController : BaseController
    {
        #region Field
        private IUserMediaManagementService _mediaManagementService;
        private readonly ApplicationDbContext _context;
        #endregion

        public UserMediaController(ApplicationDbContext context,
                                         IUserMediaManagementService mediaManagementService)
        {
            this._context = context;
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
                TempData["Message"] = "Validate fails!";
                return View("~/Views/Error/204.cshtml");
            }

            // Store
            try
            {
                _mediaManagementService.UploadMediaFile(request, UserId);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }
            return RedirectToAction("Index","Home");
        }
    }
}
