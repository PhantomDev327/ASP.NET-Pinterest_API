﻿using nxPinterest.Data;
using nxPinterest.Services.Interfaces;
using nxPinterest.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using nxPinterest.Data.Models;
using nxPinterest.Services.Models.Response;
using nxPinterest.Services.Extensions;
using System.Text.RegularExpressions;
using nxPinterest.Services.Models.Request;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Blob;

namespace nxPinterest.Services
{
    public class UserMediaManagementService : IUserMediaManagementService
    {
        #region Field
        public ApplicationDbContext _context;
        private StorageBlobService _blobService;
        private CosmosDbService _cosmosDbService;
        #endregion

        public UserMediaManagementService(ApplicationDbContext context)
        {
            _context = context;
            _blobService = new StorageBlobService();
            _cosmosDbService = new CosmosDbService();
        }

        public async Task<IList<Data.Models.UserMedia>> ListUserMediaAsyc(string userId = "")
        {
            var query = (this._context.UserMedia.AsNoTracking()
                                     .Where(c => c.UserId.Equals(userId)));

            IList<Data.Models.UserMedia> userMediaList = await query.OrderByDescending(c => c.MediaId).ToListAsync();

            return userMediaList;
        }

        /// <summary>
        ///     Search Image by conditions
        /// </summary>
        /// <param name="searchKey"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IList<Data.Models.UserMedia>> SearchUserMediaAsync(string searchKey, int container_id)
        {

            //var cosmosdata = this._cosmosDbService.SelectByUserIDAsync(dev_Settings.cosmos_databaseName, dev_Settings.cosmos_containerName, UserId);

            try
            {
                var query = this._context.UserMedia.AsNoTracking()
                                     .Where(c => c.container_id.Equals(container_id));

                if (!String.IsNullOrEmpty(searchKey))
                {
                    string[] listSearchKey = Regex.Split(searchKey.Trim(), "[ 　]+", RegexOptions.IgnoreCase);

                    if (listSearchKey.Count() > 1)
                    {
                        query = query.Where(c => listSearchKey.Contains(c.Tags)
                                || listSearchKey.Contains(c.MediaTitle));
                    }
                    else
                    {
                        query = query.Where(c => c.ProjectTags.Contains(searchKey) || c.MediaTitle.Contains(searchKey));
                    }
                }

                IList<Data.Models.UserMedia> userMediaList = await query.OrderByDescending(c => c.MediaId).ToListAsync();
                return userMediaList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<UserMediaDetailViewModel> GetUserMediaDetailsByIDAsync(int media_id)
        {

            Data.Models.UserMedia userMedia = await (this._context.UserMedia.AsNoTracking()
                                             .FirstOrDefaultAsync(c => c.MediaId.Equals(media_id)));

            UserMediaDetailViewModel result = new UserMediaDetailViewModel();

            IList<UserMedia> mediaList = new List<UserMedia>();

            if (userMedia != null)
            {
                var query = this._context.UserMedia.AsNoTracking()
                                     .Where(c => c.container_id.Equals(userMedia.container_id)).ToList();

                query = query.Select(c => new UserMedia()
                {
                    MediaId = c.MediaId,
                    UserId = c.UserId,
                    MediaTitle = c.MediaTitle?.TrimExtraSpaces(),
                    MediaDescription = c.MediaDescription?.TrimExtraSpaces(),
                    MediaFileName = c.MediaFileName,
                    MediaFileType = c.MediaFileType,
                    MediaUrl = c.MediaUrl,
                    Tags = c.Tags,
                    MediaThumbnailUrl = c.MediaThumbnailUrl
                })
                .Where(c => !string.IsNullOrEmpty(c.MediaTitle) && !string.IsNullOrEmpty(userMedia.MediaTitle) && c.MediaTitle.Equals(userMedia.MediaTitle.TrimExtraSpaces())).ToList();

                mediaList = query;
            }

            result.UserMediaDetail = userMedia;
            result.UserMediaList = mediaList;

            return result;
        }

        public async Task DeleteFromUserMedia(UserMedia userMedia)
        {
            if (userMedia != null)
            {
                var userMediaList = await this._context.UserMedia.AsNoTracking()
                                         .Where(c => c.MediaFileName.Equals(userMedia.MediaFileName))
                                         .ToListAsync();


                this._context.UserMedia.RemoveRange(userMediaList);
                await this._context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Create UserMedia
        /// </summary>
        /// <param name="request">Data</param>
        /// <param name="UserId">UserId current</param>
        public void UploadMediaFile(ImageRegistrationRequests request, string UserId)
        {
            var files = request.Images;
            if (files == null)
                throw new Exception("タグ情報を整形できませんでした。ファイルは登録されません");
            
            foreach (IFormFile file in files)
            {
                // Upload image file to Azure Blob
                string ContainerName = dev_Settings.blob_containerName_image;
                string fileName = Path.GetFileName(file.FileName);
                fileName = DateTime.Now.ToString("yyyyMMddHHmmss_") + fileName;

                // If same name file exist, change file name.
                _blobService.CreateContainerIfNotExistsAsync(ContainerName);
                var existFiles = _blobService.GetBlobFileList(ContainerName);
                if (existFiles.Result != null)
                {
                    foreach (var existfile in existFiles.Result)
                    {
                        if (fileName.Equals(existfile.ToString()))
                        {
                            //fileName = DateTime.Now.ToString("yyyyMMddHHmmss_") + fileName;
                            fileName = fileName + "_1";
                            break;
                        }
                    }
                }

                // Upload file (no Validation)
                Stream imageStream = file.OpenReadStream();
                string tagsString;
                string loggedInUserId = UserId;
                UserMedia userMedia = new UserMedia();
                var result = _blobService.UploadImageBlobAsync(fileName, ContainerName, (IFormFile)file);

                if (result == null)
                    throw new Exception("Update image fail!");

                // Get tags by ProjectTags
                string projectTab = null;
                if (request.ProjectTags != null)
                    projectTab = request.ProjectTags.Trim().Replace(',', '|');

                // Get tags by Computer Vision API
                try
                {
                    ComputerVisionService cv = new ComputerVisionService();
                    // 1 patterns in the prototype
                    tagsString = cv.GetImageTag_str(result.Result.Uri.ToString());          
                    if (String.IsNullOrEmpty(tagsString))
                    {
                        // Computer Vision Error
                        throw new Exception("Computer Vision による解析ができませんでした。ファイルは登録されません");
                    }
                }
                catch (Exception)
                {
                    // Computer Vision Error
                    throw new Exception("Computer Vision による解析ができませんでした。ファイルは登録されません");
                }

                // Create tags data
                try
                {
                    // Create Model data
                    userMedia.UserId = loggedInUserId;                                      
                    userMedia.MediaUrl = result.Result.Uri.ToString();                      
                    userMedia.MediaFileName = result.Result.Name;                          
                    userMedia.MediaFileType = result.Result.Name.Split('.').Last();        
                    userMedia.Tags = tagsString;                                            
                    userMedia.MediaThumbnailUrl = result.Result.Uri.ToString().Replace(dev_Settings.blob_containerName_image, dev_Settings.blob_containerName_thumb);
                    if (projectTab != null)
                        userMedia.ProjectTags = projectTab;
                }
                catch (Exception)
                {
                    throw new Exception("タグ情報を整形できませんでした。ファイルは登録されません");
                }

                // Save image info and tags data 
                try
                {
                    List<ApplicationUser> user = this._context.Users.Where(c => c.Id.Equals(UserId)).ToList();

                    // Add info image
                    userMedia.MediaTitle = request.Title;
                    userMedia.MediaDescription = request.Description;
                    userMedia.container_id = user[0].container_id;
                    userMedia.DateTimeUploaded = request.DateTimeUploaded;

                    //string id = _cosmosDbService.inserted_id;
                    _context.UserMedia.Add(userMedia);
                    _context.SaveChanges();
                }
                catch (Exception)
                {
                    throw new Exception("SQL database への登録に失敗しました");
                }

                // save data for cosmos db
                try
                {
                    UserMediaCosmosJSON userMediaJSON = new UserMediaCosmosJSON();
                    userMediaJSON.UserId = UserId;
                    userMediaJSON.MediaId = userMedia.MediaId;
                    userMediaJSON.MediaTitle = request.Title;
                    userMediaJSON.MediaDescription = request.Description;
                    userMediaJSON.ProjectTags = request.ProjectTags;

                    var jsonString = JsonConvert.SerializeObject(userMediaJSON, Formatting.None);

                    if (!_cosmosDbService.InsertOneItemAsync(dev_Settings.cosmos_databaseName, dev_Settings.cosmos_containerName, jsonString).Result)
                        throw new Exception("Cosmos DB への登録に失敗しました");
                }
                catch (Exception)
                {
                    throw new Exception("Cosmos DB への登録に失敗しました");
                }
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> UploadMediaFile(ImageRegistrationRequests request)
        //{
        //    bool isUpdate = request.MediaId != 0;

        //    if (isUpdate)
        //        ModelState.Remove("Images");

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            string media_title = request.Title;
        //            string media_desc = request.Description;
        //            //int userMediaId = 0;
        //            IList<IFormFile> uploaded_images = request.Images;
        //            string projectTags = request.ProjectTags;

        //            if (isUpdate)
        //            {
        //                UserMedia _userMedia = await this._context.UserMedia.FirstOrDefaultAsync(c => c.MediaId.Equals(request.MediaId));
        //                _userMedia.MediaTitle = request.Title;
        //                _userMedia.MediaDescription = request.Description;
        //                UpdateUserMediaTags(_userMedia, projectTags);
        //                await this._context.SaveChangesAsync();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }
        //    }
        //    return RedirectToAction("Index", "Home");
        //}

        /// <summary>
        /// Create UserMedia
        /// </summary>
        /// <param name="request">Data</param>
        /// <param name="UserId">UserId current</param>
        public void UploadImageFile(IndividualImageRegistrationRequests request, string UserId)
        {
            foreach (ImageInfo imageInfo in request.ImageInfoList)
            {
                var file = imageInfo.Images;
                if (file == null)
                    throw new Exception("タグ情報を整形できませんでした。ファイルは登録されません");

                // Upload image file to Azure Blob
                string ContainerName = dev_Settings.blob_containerName_image;
                string fileName = Path.GetFileName(file.FileName);

                // If same name file exist, change file name.
                _blobService.CreateContainerIfNotExistsAsync(ContainerName);
                var existFiles = _blobService.GetBlobFileList(ContainerName);
                if (existFiles.Result != null)
                {
                    foreach (var existfile in existFiles.Result)
                    {
                        if (fileName.Equals(existfile.ToString()))
                        {
                            fileName = DateTime.Now.ToString("yyyyMMddHHmmss_") + fileName;
                            break;
                        }
                    }
                }

                // Upload file (no Validation)
                Stream imageStream = file.OpenReadStream();
                string tagsString;
                string loggedInUserId = UserId;
                UserMedia userMedia = new UserMedia();
                var result = _blobService.UploadImageBlobAsync(fileName, ContainerName, (IFormFile)file);

                if (result == null)
                    throw new Exception("Update image fail!");

                // Get tags by ProjectTags
                string projectTab = null;
                if (imageInfo.ProjectTags != null)
                    projectTab = imageInfo.ProjectTags.Trim().Replace(',', '|');

                // Get tags by Computer Vision API
                try
                {
                    ComputerVisionService cv = new ComputerVisionService();
                    // 1 patterns in the prototype
                    tagsString = cv.GetImageTag_str(result.Result.Uri.ToString());
                    if (String.IsNullOrEmpty(tagsString))
                    {
                        // Computer Vision Error
                        throw new Exception("Computer Vision による解析ができませんでした。ファイルは登録されません");
                    }
                }
                catch (Exception)
                {
                    // Computer Vision Error
                    throw new Exception("Computer Vision による解析ができませんでした。ファイルは登録されません");
                }

                // Create tags data
                try
                {
                    // Create Model data
                    userMedia.UserId = loggedInUserId;
                    userMedia.MediaUrl = result.Result.Uri.ToString();
                    userMedia.MediaFileName = result.Result.Name;
                    userMedia.MediaFileType = result.Result.Name.Split('.').Last();
                    userMedia.Tags = tagsString;
                    userMedia.MediaThumbnailUrl = result.Result.Uri.ToString().Replace(dev_Settings.blob_containerName_image, dev_Settings.blob_containerName_thumb);
                    if (projectTab != null)
                        userMedia.ProjectTags = projectTab;
                }
                catch (Exception)
                {
                    throw new Exception("タグ情報を整形できませんでした。ファイルは登録されません");
                }

                // Save image info and tags data 
                try
                {
                    List<ApplicationUser> user = this._context.Users.Where(c => c.Id.Equals(UserId)).ToList();
                    // Add info image
                    userMedia.MediaTitle = imageInfo.Title;
                    userMedia.MediaDescription = imageInfo.Description;
                    userMedia.container_id = user[0].container_id;
                    userMedia.DateTimeUploaded = imageInfo.DateTimeUploaded;

                    //string id = _cosmosDbService.inserted_id;
                    _context.UserMedia.Add(userMedia);
                    _context.SaveChanges();
                }
                catch (Exception)
                {
                    throw new Exception("SQL database への登録に失敗しました");
                }

                // save data for cosmos db
                try
                {
                    UserMediaCosmosJSON userMediaJSON = new UserMediaCosmosJSON();
                    userMediaJSON.UserId = UserId;
                    userMediaJSON.MediaId = userMedia.MediaId;
                    userMediaJSON.MediaTitle = imageInfo.Title;
                    userMediaJSON.MediaDescription = imageInfo.Description;
                    userMediaJSON.ProjectTags = imageInfo.ProjectTags;

                    var jsonString = JsonConvert.SerializeObject(userMediaJSON, Formatting.None);

                    if (!_cosmosDbService.InsertOneItemAsync(dev_Settings.cosmos_databaseName, dev_Settings.cosmos_containerName, jsonString).Result)
                        throw new Exception("Cosmos DB への登録に失敗しました");
                }
                catch (Exception)
                {
                    throw new Exception("Cosmos DB への登録に失敗しました");
                }
            }
        }

    }
}
