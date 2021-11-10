using nxPinterest.Data;
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

namespace nxPinterest.Services
{
    public class UserMediaManagementService : IUserMediaManagementService
    {
        public ApplicationDbContext _context;
        private StorageBlobV2Service _blobService;

        public UserMediaManagementService(ApplicationDbContext context)
        {
            _context = context;
            _blobService = new StorageBlobV2Service();
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
        public async Task<IList<Data.Models.UserMedia>> SearchUserMediaAsync(string searchKey, string userId)
        {
            try
            {
                var query = this._context.UserMedia.AsNoTracking()
                                     .Where(c => c.UserId.Equals(userId));

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
                        query = query.Where(c => c.Tags.Contains(searchKey) || c.MediaTitle.Contains(searchKey));
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
                var query = await this._context.UserMedia.AsNoTracking().ToListAsync();

                query = query.Select(c => new UserMedia()
                {
                    MediaId = c.MediaId,
                    UserId = c.UserId,
                    MediaTitle = c.MediaTitle.TrimExtraSpaces(),
                    MediaDescription = c.MediaDescription.TrimExtraSpaces(),
                    MediaFileName = c.MediaFileName,
                    MediaFileType = c.MediaFileType,
                    MediaUrl = c.MediaUrl,
                    Tags = c.Tags,
                    MediaThumbnailUrl = c.MediaThumbnailUrl
                })
                .Where(c => c.MediaTitle.Equals(userMedia.MediaTitle.TrimExtraSpaces()) &&
                            c.MediaDescription.Equals(userMedia.MediaDescription.TrimExtraSpaces()))
                .ToList();

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
                var result = _blobService.UploadImageBlobAsync(fileName, ContainerName, (IFormFile)file);
                string tagsString;
                string loggedInUserId = UserId;
                UserMedia userMedia = new UserMedia();

                if (result == null)
                    throw new Exception("Update image fail!");
                //////////////////// Get tags by ProjectTags ////////////////////
                string projectTab = null;
                if (request.ProjectTags != null)
                {
                    projectTab = request.ProjectTags.Trim().Replace(',', '|');
                }

                //////////////////// Get tags by Computer Vision API ////////////////////
                try
                {
                    ComputerVisionService cv = new ComputerVisionService();
                    // 1 patterns in the prototype
                    tagsString = cv.GetImageTag_str(result.Result.Uri.ToString());          // Get as parsable string -> 1) SQL and 3) Table

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

                //////////////////// Create tags data ////////////////////
                try
                {
                    // Create Model data
                    userMedia.UserId = loggedInUserId;                                      // User ID
                    userMedia.MediaUrl = result.Result.Uri.ToString();                      // URL of the blob
                    userMedia.MediaFileName = result.Result.Name;                           // File name
                    userMedia.MediaFileType = result.Result.Name.Split('.').Last();         // File type
                    userMedia.Tags = tagsString;                                            // Tags (Parsable string)
                    userMedia.MediaThumbnailUrl = result.Result.StorageUri.SecondaryUri.ToString();
                    //To do add project tag
                    if (projectTab != null)
                    {
                        //
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unable to parse '{loggedInUserId},{userMedia.MediaFileName}':{e.Message}");
                    throw new Exception("タグ情報を整形できませんでした。ファイルは登録されません");
                }

                //////////////////// Save image info and tags data ////////////////////

                // ---------- 1) [SQL database] (as SQL schema Record) ---------- 
                try
                {
                    // Add info image
                    userMedia.MediaTitle = request.Title;
                    userMedia.MediaDescription = request.Description;
                    userMedia.DateTimeUploaded = request.DateTimeUploaded;

                    _context.UserMedia.Add(userMedia);
                    _context.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unable to save to SQL database. UserID:'{loggedInUserId}', MediaFileName:'{userMedia.MediaFileName}', ErrorMessage:'{e.Message}'");
                    throw new Exception("SQL database への登録に失敗しました");
                }
            }
        }

    }
}
