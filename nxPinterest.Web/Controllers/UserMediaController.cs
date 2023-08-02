using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nxPinterest.Data;
using nxPinterest.Data.Models;
using nxPinterest.Services.Models.Request;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using nxPinterest.Web.Models;
using nxPinterest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace nxPinterest.Web.Controllers
{
    [Authorize]
    public class UserMediaController : BaseController
    {
        //private readonly ILogger<HomeController> _logger;
        public const int pageSize = nxPinterest.Services.dev_Settings.displayMaxItems_search;
        private readonly IUserMediaManagementService userMediaManagementService;
        private readonly IUserAlbumService userAlbumService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext _context;
        //private CosmosDbService _cosmosDbService;

        public UserMediaController(ApplicationDbContext context,
                                    IUserMediaManagementService mediaManagementService,
                                    UserManager<ApplicationUser> userManager,
                                    IUserAlbumService userAlbumService)
        {
            this._context = context;
            this.userMediaManagementService = mediaManagementService;
            this.userAlbumService = userAlbumService;
            this.userManager = userManager;
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
            vm.currentContainer = user[0].container_id;

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
                if (media != null)
                {
                    vm.UserMediaDetail = media;
                    vm.SameTitleUserMediaList = await this.userMediaManagementService.GetUserMediaSameTitleMediasAsync(media);
                    //画面のajaxで取得している
                    //vm.RelatedUserMediaList = await this.userMediaManagementService.GetUserMediaRelatedMediasAsync(media);

                    string[] originalTags = vm.UserMediaDetail.OriginalTags?.Split(',');
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
        public async Task<IActionResult> Details(string searchKey, int media_id, int pageIndex = 1, int sizeIndex = 3)
        {
            try
            {
                DetailsViewModel vm = new DetailsViewModel();
                var user = await this.userManager.FindByIdAsync(this.UserId);
                var media = await this.userMediaManagementService.GetUserMediaAsync(media_id);
                if (media != null)
                {
                    vm.UserMediaDetail = media;
                    vm.SameTitleUserMediaList = await this.userMediaManagementService.GetUserMediaSameTitleMediasAsync(media);
                    //画面のajaxで取得している
                    //vm.RelatedUserMediaList = await this.userMediaManagementService.GetUserMediaRelatedMediasAsync(media);

                    //string[] originalTags = vm.UserMediaDetail.OriginalTags.Split(',');

                    ////上限
                    //int totalPages = (int)System.Math.Ceiling((decimal)(vm.RelatedUserMediaList.Count / (decimal)pageSize));
                    //int skip = (pageIndex - 1) * pageSize;
                    //int totalRecordCount = vm.RelatedUserMediaList.Count;

                    //ViewBag.ItemCount = vm.RelatedUserMediaList.Count;

                    ////vm.RelatedUserMediaList = vm.RelatedUserMediaList.Skip(skip).Take(pageSize).ToList();

                    vm.PageIndex = pageIndex;
                    //vm.TotalPages = totalPages;
                    vm.SearchKey = searchKey;
                    //vm.TotalRecords = totalRecordCount;
                    vm.Discriminator = user.Discriminator;
                    vm.UserDispName = user.UserDispName;

                    vm.SizeRange = sizeIndex;

                    ////ViewBag.MediaID = media_id;
                    ////ViewBag.PorjectTags = originalTags ?? null;
                    //////ViewBag.PhotoTags = string.Join(',', photo_tags_list.ToArray());
                    ////ViewBag.RelatedUserMediaList = JsonConvert.SerializeObject(vm.RelatedUserMediaList);
                    // get user containers
                    string container_ids = user.ContainerIds ?? "";
                    string[] containerArray = container_ids.Split(',');

                    if (containerArray.Length == 0 || containerArray[0] == "")
                    {
                        vm.UserContainers = await this._context.UserContainer.Where(c => c.container_id == user.container_id).ToListAsync();
                    }
                    else
                    {
                        var containerIds = containerArray
                            .Where(x => int.TryParse(x, out _))
                            .Select(int.Parse)
                            .ToList();

                        vm.UserContainers = await this._context.UserContainer.Where(c => containerIds.Contains(c.container_id)).ToListAsync();
                    }
                    //よく使用されているタグ候補
                    vm.TagList = await this.userMediaManagementService.GetOftenUseTagsAsyc(this.container_id, searchKey, 30);

                    //よく使用されているアルバムの一覧 TODO
                    var album = await userAlbumService.GetAlbumUserByContainer(this.container_id);
                    vm.AlbumList = album.Select(n => new nxPinterest.Data.ViewModels.UserAlbumViewModel
                    {
                        AlbumName = n.AlbumName,
                        AlbumUrl = n.AlbumUrl
                    }).ToList();

                    vm.currentContainer = this.container_id;

                    //return PartialView("/Views/Home/Details.cshtml", vm);
                    return View("/Views/UserMedia/Details.cshtml", vm);
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
        public async Task<IActionResult> Edit(int media_id)
        {
            try
            {
                EditViewModel vm = new EditViewModel();

                //UserMediaの取得
                var media = await this.userMediaManagementService.GetUserMediaAsync(media_id);
                vm.MediaId = media.MediaId;
                vm.MediaFileName = media.MediaFileName;
                vm.MediaFileType = media.MediaFileType;
                vm.MediaTitle = media.MediaTitle;
                vm.MediaDescription = media.MediaDescription;
                vm.Tags = media.Tags;
                vm.AITags = media.AITags;
                vm.OriginalTags = media.OriginalTags;
                vm.Created = media.Created;
                vm.Uploaded = media.Uploaded ?? DateTime.Now;
                vm.Modified = media.Modified;
                vm.Deleted = media.Deleted;
                vm.MediaUrl = media.MediaUrl;
                vm.MediaSmallUrl = media.MediaSmallUrl;
                vm.MediaThumbnailUrl = media.MediaThumbnailUrl;
                vm.Status = media.Status;
                vm.UserId = media.UserId;
                vm.ContainerId = media.ContainerId;

                var user = await this.userManager.FindByIdAsync(this.UserId);

                vm.UserDispName = user.UserDispName;
                vm.Discriminator = user.Discriminator;
                vm.TagList = await userMediaManagementService.GetOftenUseTagsAsyc(user.container_id, "", 30);

                string container_ids = user.ContainerIds ?? "";
                string[] containerArray = container_ids.Split(',');

                if (containerArray.Length == 0 || containerArray[0] == "")
                {
                    vm.UserContainers = await this._context.UserContainer.Where(c => c.container_id == user.container_id).ToListAsync();
                }
                else
                {
                    var containerIds = containerArray
                        .Where(x => int.TryParse(x, out _))
                        .Select(int.Parse)
                        .ToList();

                    vm.UserContainers = await this._context.UserContainer.Where(c => containerIds.Contains(c.container_id)).ToListAsync();
                }

                var album = await userAlbumService.GetAlbumUserByContainer(user.container_id);
                vm.AlbumList = album.Select(n=> new nxPinterest.Data.ViewModels.UserAlbumViewModel
                {
                    AlbumName = n.AlbumName,
                    AlbumUrl = n.AlbumUrl
                }).ToList();

                vm.currentContainer = user.container_id;

                return View(vm);
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
        [HttpPost]
        public async Task<IActionResult> Edit(EditViewModel vm)
        {
            try
            {
                //UserMedia userMedia = new UserMedia();

                //UserMediaの設定 todo 画面にない項目を取得しなおし
                var userMedia = await this.userMediaManagementService.GetUserMediaAsync(vm.MediaId);
                //userMedia.MediaId = vm.MediaId;
                //userMedia.MediaFileName = vm.MediaFileName;
                //userMedia.MediaFileType = vm.MediaFileType;
                userMedia.MediaTitle = vm.MediaTitle;
                userMedia.MediaDescription = vm.MediaDescription;
                //userMedia.Tags = vm.Tags;
                //userMedia.AITags = vm.AITags;
                userMedia.OriginalTags = vm.OriginalTags;
                //userMedia.Created = vm.Created;
                //userMedia.Uploaded = vm.Uploaded;
                //userMedia.Modified = vm.Modified;
                //userMedia.Deleted = vm.Deleted;
                //userMedia.MediaUrl = vm.MediaUrl;
                //userMedia.MediaSmallUrl = vm.MediaSmallUrl;
                //userMedia.MediaThumbnailUrl = vm.MediaThumbnailUrl;
                //userMedia.Status = vm.Status;
                //userMedia.UserId = vm.UserId;
                //userMedia.ContainerId = vm.ContainerId;

                var ret = this.userMediaManagementService.UpdateUserMedia(userMedia);

                //return Json(new { success = ret });
                return RedirectToAction("Details", "UserMedia", new { searchKey = vm.SearchKey, media_id = vm.MediaId, sizeIndex = vm.SizeRange });
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                //return Json(new { success = false, errMsg = ex.Message });
                return View();
            }
        }
        /// <summary>
        /// Delete Image
        /// </summary>
        /// <param name="media_id"></param>
        /// <returns></returns>
        [HttpPost]
        //public async Task DeleteUserMedia(string searchKey, int media_id)
        public async Task<IActionResult> DeleteById(int media_id)
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
        public async Task<IActionResult> DeleteByIds(string ids)
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

        [HttpPost]
        public async Task<IActionResult> RemoveMediaFromAlbumByIds(string albumName, string ids)
        {
            try
            {
                List<int> mediaIdList = new List<int>();
                var albumId = await this.userAlbumService.GetAlbumIdByNameAsync(albumName);

                foreach (var mediaId in ids?.Split(","))
                {
                    //UserMediaの取得 1件ずつ
                    var media = await this.userMediaManagementService.GetUserMediaAsync(int.Parse(mediaId));
                    mediaIdList.Add(media.MediaId);
                }

                await this.userAlbumService.RemoveMediaFromAlbum((int)albumId, mediaIdList);
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
                //return View(request);
                // To Do
                //TempData["Message"] = "Validate fails!";
                TempData["Message"] = "No title entered.";
                return View("~/Views/Error/204.cshtml");
            }

            // 個別編集ボタンを押下
            if (request.BtnName == "kobetsu")
            {
                if (request.Images.Count == 0)
                {
                    return IndividualImageRegistration();
                }
                IndividualImageRegisterViewModel individual = new IndividualImageRegisterViewModel();
                foreach (var file in request.Images)
                {
                    RegisterImageInfo imageInfo = new RegisterImageInfo();
                    imageInfo.Images = file;
                    imageInfo.Title = request.Title;
                    imageInfo.Description = request.Description;
                    imageInfo.OriginalTags = request.OriginalTags;
                    imageInfo.AITags = request.AITags;
                    imageInfo.DateTimeUploaded = request.DateTimeUploaded;
                    individual.ImageInfoList.Add(imageInfo);
                    CreateImageDirectory();
                }
                return UploadImageFileHidden(individual);
            }

            // Store
            try
            {
                userMediaManagementService.UploadMediaFile(request, UserId);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// thumbnailRecovery
        /// </summary>
        /// <param name="request">Form Data</param>
        /// <returns></returns>
        public IActionResult thumbnailRecovery()
        {
            // Store
            try
            {
                userMediaManagementService.thumbnailRecovery(UserId);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// 個別編集
        /// </summary>
        /// <returns></returns>
        public IActionResult IndividualImageRegistration()
        {
            CreateImageDirectory();
            IndividualImageRegisterViewModel vm = new IndividualImageRegisterViewModel();
            return this.View("~/Views/UserMedia/IndividualImageRegistration.cshtml", vm);
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
                    string pathImage = path + "/" + request.ImageInfoList[i].imgName;
                    FileStream file = new FileStream(pathImage, FileMode.Open);
                    var ms = new MemoryStream();
                    file.CopyTo(ms);
                    IFormFile f = new FormFile(ms, 0, ms.Length, request.ImageInfoList[i].imgName, request.ImageInfoList[i].imgName);
                    request.ImageInfoList[i].Images = f;
                    file.Close();
                }
                userMediaManagementService.UploadIndividualMediaFile(request, UserId);
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
        public IActionResult UploadImageFileHidden(IndividualImageRegisterViewModel request)
        {
            request.imageInfoListSize = request.ImageInfoList.Count;

            // UserIDでフォルダを作成
            string path = "./wwwroot/images/temp/" + this.UserId;
            string path2 = "/images/temp/" + this.UserId + "/";

            for (int i = 0; i < request.imageInfoListSize; i++)
            {
                RegisterImageInfo img = request.ImageInfoList[i];

                if (img.Images != null)
                {

                    using (FileStream stream = new FileStream(Path.Combine(path, img.Images.FileName), FileMode.Create))
                    {
                        img.Images.CopyTo(stream);
                    }
                    String fullPath = path + "/" + img.Images.FileName;
                    String imgNewWithouExt = Path.GetFileNameWithoutExtension(fullPath);
                    String imgNewPath = fullPath.Replace(imgNewWithouExt, imgNewWithouExt + "_00" + i);
                    System.IO.File.Move(fullPath, imgNewPath);
                    System.IO.FileInfo imgInfo = new System.IO.FileInfo(imgNewPath);
                    request.ImageInfoList[i].imgName = imgInfo.Name;
                    request.ImageInfoList[i].url = path2 + imgInfo.Name;
                }
            }

            return this.View("~/Views/UserMedia/IndividualImageRegistration.cshtml", request);
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

        /// <summary>
        /// Update SameTitle MediaFile
        /// </summary>
        /// <param name="request">Form Data</param>
        /// <returns></returns>
        public async Task<IActionResult> UpdateSameTitleMediaFile(int mediaId)
        {
            var media = this.userMediaManagementService.GetUserMediaAsync(mediaId).Result;
            if (media != null)
            {
                EditMultiSelectImageViewModel vm = new EditMultiSelectImageViewModel();

                List<ApplicationUser> user = this._context.Users.Where(c => c.Id.Equals(this.UserId)).ToList();
                if (user == null || user.Count == 0) return RedirectToAction("LogOut", "Account");
                vm.Discriminator = user[0].Discriminator;
                vm.UserDispName = user[0].UserDispName;

                //よく使用されているタグ候補
                vm.TagList = await this.userMediaManagementService.GetOftenUseTagsAsyc(this.container_id, "", 30);

                // get user containers
                string container_ids = user[0].ContainerIds ?? "";
                string[] containerArray = container_ids.Split(',');

                if (containerArray.Length == 0 || containerArray[0] == "")
                {
                    vm.UserContainers = await this._context.UserContainer.Where(c => c.container_id == user[0].container_id).ToListAsync();
                }
                else
                {
                    var containerIds = containerArray
                        .Where(x => int.TryParse(x, out _))
                        .Select(int.Parse)
                        .ToList();

                    vm.UserContainers = await this._context.UserContainer.Where(c => containerIds.Contains(c.container_id)).ToListAsync();
                }
                vm.DetailsMediaId = mediaId;
                vm.UserMediaList = this.userMediaManagementService.GetUserMediaSameTitleMediasAsync(media).Result;

                return View("~/Views/UserMedia/EditMultiSelectImage.cshtml", vm);
            }
            else
            {
                TempData["Message"] = "該当するイメージが見つかりませんでした。最初からやり直してください。";
                return View("~/Views/Error/204.cshtml");
            }
        }
        /// <summary>
        /// Update MultiSelect MediaFile
        /// </summary>
        /// <param name="request">Form Data</param>
        /// <returns></returns>
        public IActionResult UpdateMultiSelectMediaFile(string mediaIds)
        {
            try
            {
                List<UserMedia> mediaList = new List<UserMedia>();
                foreach (var mediaId in mediaIds?.Split(","))
                {
                    //UserMediaの取得 1件ずつ
                    var media = this.userMediaManagementService.GetUserMediaAsync(int.Parse(mediaId)).Result;
                    mediaList.Add(media);
                }
                if (mediaList.Count >= 1)
                {
                    EditMultiSelectImageViewModel vm = new EditMultiSelectImageViewModel();
                    vm.UserMediaList = mediaList;

                    return View("~/Views/UserMedia/EditMultiSelectImage.cshtml", vm);
                }
                else
                {
                    TempData["Message"] = "該当するイメージが見つかりませんでした。最初からやり直してください。";
                    return View("~/Views/Error/204.cshtml");
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = string.Format("予期せぬエラ－が発生しました。最初からやり直してください。({0})", ex.Message);
                return View("~/Views/Error/204.cshtml");
            }
        }
        /// <summary>
        /// Update MultiSelect MediaFile
        /// </summary>
        /// <param name="request">Form Data</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateMultiSelectMediaFile(EditMultiSelectImageViewModel vm)
        {
            try
            {
                foreach (var media in vm.UserMediaList)
                {
                    //UserMediaの設定 todo 画面にない項目を取得しなおし
                    var userMedia = await this.userMediaManagementService.GetUserMediaAsync(media.MediaId);
                    userMedia.MediaTitle = media.MediaTitle;
                    userMedia.MediaDescription = media.MediaDescription;
                    userMedia.OriginalTags = media.OriginalTags;
                    //userMedia.Tags = vm.Tags;
                    //userMedia.AITags = vm.AITags;
                    //userMedia.Created = vm.Created;
                    //userMedia.Uploaded = vm.Uploaded;
                    //userMedia.Modified = vm.Modified;
                    //userMedia.Deleted = vm.Deleted;
                    //userMedia.MediaUrl = vm.MediaUrl;
                    //userMedia.MediaSmallUrl = vm.MediaSmallUrl;
                    //userMedia.MediaThumbnailUrl = vm.MediaThumbnailUrl;
                    //userMedia.Status = vm.Status;
                    //userMedia.UserId = vm.UserId;
                    //userMedia.ContainerId = vm.ContainerId;

                    var ret = this.userMediaManagementService.UpdateUserMedia(userMedia);
                }

                return RedirectToAction("Details", "UserMedia", new { searchKey = vm.SearchKey, media_id = vm.DetailsMediaId, sizeIndex = vm.SizeRange });
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                //return Json(new { success = false, errMsg = ex.Message });
                return View();
            }
        }
    }
}
