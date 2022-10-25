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
using Microsoft.WindowsAzure.Storage.Blob;
using nxPinterest.Services.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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

        //public async Task<IList<Data.Models.UserMedia>> ListUserMediaAsyc(string userId = "")
        //{
        //    var query = (this._context.UserMedia.AsNoTracking()
        //                             .Where(c => c.UserId.Equals(userId)));

        //    IList<Data.Models.UserMedia> userMediaList = await query.OrderByDescending(c => c.MediaId).ToListAsync();

        //    return userMediaList;
        //}

        /// <summary>
        ///     Search Image by conditions
        /// </summary>
        /// <param name="searchKey"></param>
        /// <param name="container_id"></param>
        /// <returns></returns>
        public async Task<IList<Data.Models.UserMedia>> SearchUserMediaAsync(string searchKey, int container_id, int skip, int take)
        {
            try
            {
                var query = this._context.UserMedia.AsNoTracking()
                                     .Where(c => c.ContainerId.Equals(container_id) && c.Status == 0 && c.Deleted == null);

                //todo tagテーブルで検索に変更したい
                if (!String.IsNullOrEmpty(searchKey))
                {
                    string[] listSearchKey = Regex.Split(searchKey.Trim(), "[ 　]+", RegexOptions.IgnoreCase);

                    foreach(var word in listSearchKey)
                    {
                        query = query.Where(c => c.SearchText.Contains(word));
                    }
                }

                IList<Data.Models.UserMedia> userMediaList = await query.OrderByDescending(c => c.MediaId)
                                                                        .Skip(skip).Take(take).ToListAsync();
                return userMediaList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        ///     GetUserMedia By ID
        /// </summary>
        /// <param name="media_id"></param>
        /// <returns></returns>
        public async Task<UserMedia> GetUserMediaAsync(int mediaId)
        {

            return await (this._context.UserMedia.AsNoTracking()
                                                .FirstOrDefaultAsync(c => c.MediaId.Equals(mediaId)));
        }
        /// <summary>
        ///     GetUserMediaDetails By ID
        /// </summary>
        /// <param name="media_id"></param>
        /// <returns></returns>
        public async Task<UserMediaDetailModel> GetUserMediaDetailsByIDAsync(int mediaId)
        {

            Data.Models.UserMedia userMedia = await (this._context.UserMedia.AsNoTracking()
                                                .FirstOrDefaultAsync(c => c.MediaId.Equals(mediaId)));

            UserMediaDetailModel result = new UserMediaDetailModel();

            IList<UserMedia> mediaList = new List<UserMedia>();

            if (userMedia != null)
            {
                mediaList = this._context.UserMedia.AsNoTracking()
                                     .Where(c => c.ContainerId.Equals(userMedia.ContainerId) && c.MediaTitle.Equals(userMedia.MediaTitle.TrimExtraSpaces())).ToList();

                //Todo trimは登録時に行おうよ
                //mediaList = query.Select(c => new UserMedia()
                //{
                //    MediaId = c.MediaId,
                //    UserId = c.UserId,
                //    MediaTitle = c.MediaTitle?.TrimExtraSpaces(),
                //    MediaDescription = c.MediaDescription?.TrimExtraSpaces(),
                //    MediaFileName = c.MediaFileName,
                //    MediaFileType = c.MediaFileType,
                //    MediaUrl = c.MediaUrl,
                //    Tags = c.Tags,
                //    MediaSmallUrl = c.MediaSmallUrl,
                //    MediaThumbnailUrl = c.MediaThumbnailUrl
                //}).ToList();
            }

            result.UserMediaDetail = userMedia;
            result.SameTitleUserMediaList = mediaList;

            // 似ている画像取得
            result.RelatedUserMediaList = await SearchSimilarImagesAsync(result.UserMediaDetail, result.UserMediaDetail.ContainerId);

            return result;
        }

        /// <summary>
        ///     Search Similar images　似ている画像検索
        /// </summary>
        /// <param name="userMedia"></param>
        /// <param name="container_id"></param>
        /// <returns></returns>
        public async Task<IList<Data.Models.UserMedia>> SearchSimilarImagesAsync(UserMedia userMedia, int container_id)
        {
            try
            {
                var searchTags = userMedia.Tags.Split("|").Where(w => w != "").Select(str => str.Split(":")[0]).ToList();
                //var searchTags = userMedia.Tags.Split(",").ToList(); 
                var searchMedias = this._context.UserMediaTags.AsNoTracking()
                                                    .Where(u => u.ContainerId.Equals(userMedia.ContainerId) && u.Confidence > 0.7 && searchTags.Contains(u.Tag))
                                                    .GroupBy(g => g.UserMediaName)
                                                    .Select(s => new { UserMediaName = s.Key, Confidence = s.Sum(z => z.Confidence) })
                                                    .OrderByDescending(z => z.Confidence)

                                                    .Select(s => s.UserMediaName)
                                                    .ToList();

                var userMediaList = await this._context.UserMedia.AsNoTracking()
                                                    //.Include(i => i.MediaFileName)
                                                    .Where(u => u.ContainerId.Equals(container_id) && searchMedias.Contains(u.MediaFileName))
                                                    .Where(v => !v.MediaId.Equals(userMedia.MediaId))
                                                    .ToListAsync();

                return userMediaList;
            }
            catch(Exception)
            {
                throw;
            }
        }
        
        public async Task DeleteFromUserMediaList(List<UserMedia> userMediaList)
        {
            if (userMediaList != null)
            {
                ////var userMediaList = await this._context.UserMedia.AsNoTracking()
                ////                         .Where(c => c.MediaFileName.Equals(userMedia.MediaFileName))
                ////                         .ToListAsync();
                //this._context.UserMedia.RemoveRange(userMediaList);
                //await this._context.SaveChangesAsync();

                //deleteから論理削除へ
                foreach (var userMedia in userMediaList)
                {
                    userMedia.Status = 9;
                    userMedia.Deleted = DateTime.Now;
                }
                _context.UserMedia.UpdateRange(userMediaList);
                await this._context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// よく使用するタグ
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IList<string>> GetOftenUseTagsAsyc(int containerId, string searchKey, int take)
        {
            if (!String.IsNullOrEmpty(searchKey))
            {
                string[] listSearchKey = Regex.Split(searchKey.Trim(), "[ 　]+", RegexOptions.IgnoreCase);

                var subquery = await this._context.UserMedia.AsNoTracking()
                                    .Where(c => c.ContainerId.Equals(containerId))
                                    .Select(s => new { s.MediaFileName, s.Tags }).ToListAsync();

                foreach(var word in listSearchKey)
                {
                    subquery = subquery.Where(s => s.Tags.Contains(word)).ToList();
                }
                var targetmedias = subquery.Select(s => s.MediaFileName);

                var query = await this._context.UserMediaTags.AsNoTracking()
                                    .Where(s => targetmedias.Contains(s.UserMediaName) && s.TagsType==1)
                                    .GroupBy(g => g.Tag)
                                    .OrderByDescending(w => w.Count())
                                    .Select(s => s.Key).Take(take).ToListAsync();

                //キーワード除外
                foreach (var word in listSearchKey)
                {
                    query.RemoveAll(r => r.Equals(word));
                }

                return query;
            }
            else
            {
                var query = await this._context.UserMediaTags.AsNoTracking()
                                    .Where(c => c.ContainerId.Equals(containerId) && c.TagsType == 1)
                                    .GroupBy(g => g.Tag)
                                    .OrderByDescending(w => w.Count())
                                    .Select(s => s.Key).Take(take).ToListAsync();
                return query;
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
                throw new Exception("ファイルのアップロードに失敗しました。");

            // Upload image file to Azure Blob
            string ContainerName = dev_Settings.blob_containerName_image;
            ////string ContainerNameThumb = dev_Settings.blob_containerName_thumb;

            // Login User
            var user = this._context.Users.Where(c => c.Id.Equals(UserId)).FirstOrDefault();
            string userContainerId = user.container_id.ToString();

            // File Loop
            foreach (IFormFile file in files)
            {
                //File Name
                //string orgFileName = Path.GetFileName(file.FileName);
                string orgFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff_") + Guid.NewGuid().ToString("N");
                string blobFileName = orgFileName + Path.GetExtension(file.FileName);
                string blobFilePath = "{0}/{1}/{2}";
                //string fileName = Path.GetFileName(file.FileName);

                string original = String.Format(blobFilePath, userContainerId, "original", blobFileName);
                string small = String.Format(blobFilePath, userContainerId, "small", blobFileName);
                string thumb = String.Format(blobFilePath, userContainerId, "thumb", blobFileName);


                // Upload file (no Validation)
                //Stream imageStream = file.OpenReadStream();
                using var imageStream = new MemoryStream();
                file.CopyTo(imageStream);
                imageStream.Seek(0, SeekOrigin.Begin);
                using var smallimageStream = new MemoryStream();
                var thumbnailStream = new MemoryStream();
                string tagsString = "";
                string loggedInUserId = UserId;
                UserMedia userMedia = new UserMedia();


                // ImageSharp でサムネイルを作成
                try
                {
                    IImageFormat format;
                    using (var imgSharp = Image.Load<Rgba32>(imageStream, out format))
                    {
                        // 画像の操作をMutateメソッドで行う
                        imgSharp.Mutate(x =>
                        {
                            var option = new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(860, 573) };
                            x.Resize(option);
                        });
                        imgSharp.Save(smallimageStream, format);
                        smallimageStream.Seek(0, SeekOrigin.Begin);
                    }
                }
                catch (Exception)
                {
                    // ImageSharp Error
                    throw new Exception("ImageSharp でサムネイルを作成ができませんでした。ファイルは登録されません");
                }


                // Upload file (small size)
                var small_result = _blobService.UploadStreamBlobAsync(small, ContainerName, smallimageStream);
                if (small_result == null)
                    throw new Exception("Update small image fail!");


                // Upload file (Original)
                var result = _blobService.UploadImageBlobAsync(original, ContainerName, (IFormFile)file);
                if (result == null)
                    throw new Exception("Update image fail!");


                // Get tags by ProjectTags
                string Tags = null;
                string projectTags = null;
                if (request.ProjectTags != null)
                {
                    projectTags = request.ProjectTags.Trim().Replace("|",",");
                    Tags = request.ProjectTags.Trim().Replace(",", ":1.0:1|");
                    Tags += ":1.0:1|";
                }

                // Get tags by Computer Vision API
                try
                {
                    ComputerVisionService cv = new ComputerVisionService();
                    // 1 patterns in the prototype
                    //tagsString = cv.GetImageTag_str(result.Result.Uri.ToString());

                    //アップロードが完了するまで待機　todo
                    while (true)
                    {
                        if (small_result.IsCompleted) break;
                    }
                    tagsString = cv.GetImageTag_str(small_result.Result.Uri.ToString()); 
                    if (String.IsNullOrEmpty(tagsString))
                    {
                        // Computer Vision Error
                        throw new Exception("Computer Vision による解析ができませんでした。ファイルは登録されません");
                    }

                    // Get thumbnail data
                    using var stream = cv.GetThumbnail(small_result.Result.Uri.ToString());
                    stream.CopyTo(thumbnailStream);
                    thumbnailStream.Seek(0, SeekOrigin.Begin);
                }
                catch (Exception)
                {
                    // Computer Vision Error
                    //TODO エラーでも登録
                    //throw new Exception("Computer Vision による解析ができませんでした。ファイルは登録されません");
                }

                // Upload file (thumbnai)
                var thumb_result = _blobService.UploadStreamBlobAsync(thumb, ContainerName, thumbnailStream);
                if (thumb_result == null)
                    throw new Exception("Update thumbnail image fail!");

                // Create tags data
                try
                {
                    // Create Model data
                    userMedia.UserId = loggedInUserId;                                      
                    userMedia.MediaUrl = result.Result.Uri.ToString();                      
                    userMedia.MediaFileName = orgFileName;                          
                    userMedia.MediaFileType = blobFileName.Split('.').Last();        
                    userMedia.Tags = tagsString;
                    userMedia.MediaSmallUrl = small_result.Result.Uri.ToString();
                    userMedia.MediaThumbnailUrl = thumb_result.Result.Uri.ToString();
                    if (projectTags != null)
                    {
                        userMedia.ProjectTags = projectTags;
                        userMedia.Tags += Tags;
                    }

                    userMedia.MediaTitle = request.Title;
                    userMedia.MediaDescription = request.Description ?? "";
                    userMedia.ContainerId = int.Parse(userContainerId);

                    userMedia.Created = DateTime.Now;
                    userMedia.Status = 0;


                    userMedia.DateTimeUploaded = request.DateTimeUploaded;

                    _context.UserMedia.Add(userMedia);

                    // tagテーブル titleもtag
                    UserMediaTags userMediaTags = new UserMediaTags();
                    userMediaTags.UserMediaName = orgFileName;
                    userMediaTags.ContainerId = userMedia.ContainerId;
                    userMediaTags.TagsType = 0;
                    userMediaTags.Tag = userMedia.MediaTitle;
                    userMediaTags.Confidence = 1.0;
                    userMediaTags.Created = DateTime.Now;
                    _context.UserMediaTags.Add(userMediaTags);
                    //cv解析のtagと独自tag
                    foreach (var tag in tagsString.Split("|")
                                                .Where(x => x != "")
                                                .Select(x => x.Split(":"))
                                                .GroupBy(x => x[0])
                                                .Select(x => new { Name = x.Key, Score = x.Max(m => m[1]) }))
                    {
                        //if (tag == "") break;
                        //var tagstr = tag.Split(":");
                        userMediaTags = new UserMediaTags();
                        userMediaTags.UserMediaName = orgFileName;
                        userMediaTags.ContainerId = userMedia.ContainerId;
                        userMediaTags.TagsType = 2;
                        userMediaTags.Tag = tag.Name;
                        userMediaTags.Confidence = double.Parse(tag.Score);
                        userMediaTags.Created = DateTime.Now;
                        _context.UserMediaTags.Add(userMediaTags);
                    }
                    foreach (var tag in projectTags?.Split(","))
                    {
                        if (tag == "") break;
                        userMediaTags = new UserMediaTags();
                        userMediaTags.UserMediaName = orgFileName;
                        userMediaTags.ContainerId = userMedia.ContainerId;
                        userMediaTags.TagsType = 1;
                        userMediaTags.Tag = tag;
                        userMediaTags.Confidence = 1.0;
                        userMediaTags.Created = DateTime.Now;
                        _context.UserMediaTags.Add(userMediaTags);
                    }
                    // save
                    _context.SaveChanges();
                }
                catch (Exception)
                {
                    throw new Exception("SQL database への登録に失敗しました");
                }
            }
        }

        /// <summary>
        /// Create UserMedia
        /// </summary>
        /// <param name="request">Data</param>
        /// <param name="UserId">UserId current</param>
        public void UploadIndividualMediaFile(IndividualImageRegistrationRequests request, string UserId)
        {
            var files = request.ImageInfoList;
            if (files == null)
                throw new Exception("ファイルのアップロードに失敗しました。");

            // Upload image file to Azure Blob
            string ContainerName = dev_Settings.blob_containerName_image;
            ////string ContainerNameThumb = dev_Settings.blob_containerName_thumb;

            // Login User
            var user = this._context.Users.Where(c => c.Id.Equals(UserId)).FirstOrDefault();
            string userContainerId = user.container_id.ToString();

            foreach (ImageInfo imageInfo in request.ImageInfoList)
            {
                var file = imageInfo.Images;

                //File Name
                //string orgFileName = Path.GetFileName(file.FileName);
                string orgFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff_") + Guid.NewGuid().ToString("N");
                string blobFileName = orgFileName + Path.GetExtension(file.FileName);
                string blobFilePath = "{0}/{1}/{2}";
                //string fileName = Path.GetFileName(file.FileName);

                string original = String.Format(blobFilePath, userContainerId, "original", blobFileName);
                string small = String.Format(blobFilePath, userContainerId, "small", blobFileName);
                string thumb = String.Format(blobFilePath, userContainerId, "thumb", blobFileName);

                // Upload file (no Validation)
                //Stream imageStream = file.OpenReadStream();
                using var imageStream = new MemoryStream();
                file.CopyTo(imageStream);
                imageStream.Seek(0, SeekOrigin.Begin);
                using var smallimageStream = new MemoryStream();
                var thumbnailStream = new MemoryStream();
                string tagsString = "";
                string loggedInUserId = UserId;
                UserMedia userMedia = new UserMedia();

                // ImageSharp でサムネイルを作成
                try
                {
                    IImageFormat format;
                    using (var imgSharp = Image.Load<Rgba32>(imageStream, out format))
                    {
                        // 画像の操作をMutateメソッドで行う
                        imgSharp.Mutate(x =>
                        {
                            var option = new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(860, 573) };
                            x.Resize(option);
                        });
                        imgSharp.Save(smallimageStream, format);
                        smallimageStream.Seek(0, SeekOrigin.Begin);
                    }
                }
                catch (Exception)
                {
                    // ImageSharp Error
                    throw new Exception("ImageSharp でサムネイルを作成ができませんでした。ファイルは登録されません");
                }

                // Upload file (small size)
                var small_result = _blobService.UploadStreamBlobAsync(small, ContainerName, smallimageStream);
                if (small_result == null)
                    throw new Exception("Update small image fail!");


                // Upload file (Original)
                var result = _blobService.UploadImageBlobAsync(original, ContainerName, (IFormFile)file);
                if (result == null)
                    throw new Exception("Update image fail!");


                // Get tags by ProjectTags
                string Tags = null;
                string projectTags = null;
                if (imageInfo.ProjectTags != null)
                {
                    projectTags = imageInfo.ProjectTags.Trim().Replace("|", ",");
                    Tags = imageInfo.ProjectTags.Trim().Replace(",", ":1.0:1|");
                    Tags += ":1.0:1|";
                }

                // Get tags by Computer Vision API
                try
                {
                    ComputerVisionService cv = new ComputerVisionService();
                    // 1 patterns in the prototype
                    //tagsString = cv.GetImageTag_str(result.Result.Uri.ToString());

                    //アップロードが完了するまで待機　todo
                    while (true)
                    {
                        if (small_result.IsCompleted) break;
                    }
                    tagsString = cv.GetImageTag_str(small_result.Result.Uri.ToString());
                    if (String.IsNullOrEmpty(tagsString))
                    {
                        // Computer Vision Error
                        throw new Exception("Computer Vision による解析ができませんでした。ファイルは登録されません");
                    }

                    // Get thumbnail data
                    using var stream = cv.GetThumbnail(small_result.Result.Uri.ToString());
                    stream.CopyTo(thumbnailStream);
                    thumbnailStream.Seek(0, SeekOrigin.Begin);
                }
                catch (Exception)
                {
                    // Computer Vision Error
                    //TODO エラーでも登録
                    //throw new Exception("Computer Vision による解析ができませんでした。ファイルは登録されません");
                }

                // Upload file (thumbnai)
                var thumb_result = _blobService.UploadStreamBlobAsync(thumb, ContainerName, thumbnailStream);
                if (thumb_result == null)
                    throw new Exception("Update thumbnail image fail!");

                // Create tags data
                try
                {
                    // Create Model data
                    userMedia.UserId = loggedInUserId;
                    userMedia.MediaUrl = result.Result.Uri.ToString();
                    userMedia.MediaFileName = orgFileName;
                    userMedia.MediaFileType = blobFileName.Split('.').Last();
                    userMedia.Tags = tagsString;
                    userMedia.MediaSmallUrl = small_result.Result.Uri.ToString();
                    userMedia.MediaThumbnailUrl = thumb_result.Result.Uri.ToString();
                    if (projectTags != null)
                    {
                        userMedia.ProjectTags = projectTags;
                        userMedia.Tags += Tags;
                    }

                    userMedia.MediaTitle = imageInfo.Title;
                    userMedia.MediaDescription = imageInfo.Description;
                    userMedia.ContainerId = int.Parse(userContainerId);
                    userMedia.DateTimeUploaded = imageInfo.DateTimeUploaded;

                    _context.UserMedia.Add(userMedia);

                    // tagテーブル titleもtag
                    UserMediaTags userMediaTags = new UserMediaTags();
                    userMediaTags.UserMediaName = orgFileName;
                    userMediaTags.TagsType = 0;
                    userMediaTags.Tag = userMedia.MediaTitle;
                    userMediaTags.Confidence = 1.0;
                    _context.UserMediaTags.Add(userMediaTags);
                    //cv解析のtagと独自tag
                    foreach (var tag in tagsString.Split("|"))
                    {
                        if (tag == "") break;
                        var tagstr = tag.Split(":");
                        userMediaTags = new UserMediaTags();
                        userMediaTags.UserMediaName = orgFileName;
                        userMediaTags.TagsType = 1;
                        userMediaTags.Tag = tagstr[0];
                        userMediaTags.Confidence = double.Parse(tagstr[1]);
                        _context.UserMediaTags.Add(userMediaTags);
                    }
                    foreach (var tag in projectTags.Split(","))
                    {
                        if (tag == "") break;
                        userMediaTags = new UserMediaTags();
                        userMediaTags.UserMediaName = orgFileName;
                        userMediaTags.TagsType = 2;
                        userMediaTags.Tag = tag;
                        userMediaTags.Confidence = 1.0;
                        _context.UserMediaTags.Add(userMediaTags);
                    }
                    // save
                    _context.SaveChanges();
                }
                catch (Exception)
                {
                    throw new Exception("SQL database への登録に失敗しました");
                }
            }
        }
    }
}
