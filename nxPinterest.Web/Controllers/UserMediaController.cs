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
        /// Create Media File
        /// </summary>
        /// <param name="request">Form Data</param>
        /// <returns></returns>
        [HttpPost]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue, ValueLengthLimit = int.MaxValue)]
        public IActionResult UploadMediaFile(ImageRegistrationRequests request)
        {
            // Validate param
            if (!ModelState.IsValid)
            {
                // To Do
                TempData["Message"] = "Validate fails!";
                return View("~/Views/Error/204.cshtml");
            }

            // 個別編集ボタンを押下
            if (request.BtnName == "kobetsu")
            {
                if (request.Images.Count == 0)
                {
                    return IndividualImageRegistration();
                }
                IndividualImageRegistrationRequests individual = new IndividualImageRegistrationRequests();
                foreach (var file in request.Images)
                {
                    ImageInfo imageInfo = new ImageInfo();
                    imageInfo.Images = file;
                    imageInfo.Title = request.Title;
                    imageInfo.Description = request.Description;
                    imageInfo.ProjectTags = request.ProjectTags;
                    imageInfo.PhotoTags = request.PhotoTags;
                    imageInfo.DateTimeUploaded = request.DateTimeUploaded;
                    individual.ImageInfoList.Add(imageInfo);
                    CreateImageDirectory();
                }
                return UploadImageFileHidden(individual);
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

        /// <summary>
        /// 個別編集
        /// </summary>
        /// <returns></returns>
        public IActionResult IndividualImageRegistration()
        {
            CreateImageDirectory();
            Services.Models.Request.IndividualImageRegistrationRequests vm = new Services.Models.Request.IndividualImageRegistrationRequests();
            return this.View("~/Views/Shared/IndividualImageRegistration.cshtml", vm);
        }

        /// <summary>
        /// Upload Image File
        /// </summary>
        /// <param name="request">Form Data</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UploadIndividualMediaFile(IndividualImageRegistrationRequests request)
        {
            request.imageInfoListSize = request.ImageInfoList.Count;
            if (request.imageInfoListSize == 0)
            {
                TempData["Message"] = "Validate fails!";
                return View("~/Views/Error/204.cshtml");
            }
            // Validate param
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Validate fails!";
                return View("~/Views/Error/204.cshtml");
            }

            try
            {
                string path = "./wwwroot/images/temp/" + this.UserId;
                for (int i = 0; i < request.ImageInfoList.Count; i++)
                {
                    string pathImage = path +"/"+ request.ImageInfoList[i].imgName;
                    FileStream file = new FileStream(pathImage, FileMode.Open);
                    var ms = new MemoryStream();
                    file.CopyTo(ms);
                    IFormFile f = new FormFile(ms, 0, ms.Length, request.ImageInfoList[i].imgName, request.ImageInfoList[i].imgName);
                    request.ImageInfoList[i].Images = f;
                    file.Close();
                }
                _mediaManagementService.UploadIndividualMediaFile(request, UserId);
                if (Directory.Exists(path))
                {
                    foreach (string filename in Directory.GetFiles(path))
                    {
                        System.IO.File.Delete(filename);
                    }
                    Directory.Delete(path, true);
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Create hidden Image
        /// </summary>
        /// <param name="request">Form Data</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UploadImageFileHidden(IndividualImageRegistrationRequests request)
        {
            request.imageInfoListSize = request.ImageInfoList.Count;

            // UserIDでフォルダを作成
            string path = "./wwwroot/images/temp/" + this.UserId;
            string path2 = "/images/temp/" + this.UserId + "/";

            for (int i = 0; i < request.imageInfoListSize; i++){
                ImageInfo img = request.ImageInfoList[i];

                if (img.Images != null)
                {
                    
                    using (FileStream stream = new FileStream(Path.Combine(path, img.Images.FileName), FileMode.Create))
                    {
                        img.Images.CopyTo(stream);
                    }
                    String fullPath = path +"/"+ img.Images.FileName;
                    String imgNewWithouExt = Path.GetFileNameWithoutExtension(fullPath);
                    String imgNewPath = fullPath.Replace(imgNewWithouExt, imgNewWithouExt + "_00"+i);
                    System.IO.File.Move(fullPath, imgNewPath);
                    System.IO.FileInfo imgInfo = new System.IO.FileInfo(imgNewPath);
                    request.ImageInfoList[i].imgName = imgInfo.Name;
                    request.ImageInfoList[i].url = path2 + imgInfo.Name;
                }
            }
            
            return this.View("~/Views/Shared/IndividualImageRegistration.cshtml", request);
        }

        /// <summary>
        /// Create Image Directory
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        private void CreateImageDirectory()
        {
            string path = "./wwwroot/images/temp/" + this.UserId;
            if (Directory.Exists(path))
            {
                foreach (string filename in Directory.GetFiles(path))
                {
                    System.IO.File.Delete(filename);
                }
                Directory.Delete(path);
            }
            Directory.CreateDirectory(path);
        }
    }
}
