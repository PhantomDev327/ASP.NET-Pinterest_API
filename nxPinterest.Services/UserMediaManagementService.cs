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
//using nxPinterest.Services.Models.Response;
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
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace nxPinterest.Services
{
    public class UserMediaManagementService : IUserMediaManagementService
    {
        private readonly ILogger _logger;
        private readonly IWebHostEnvironment _env;
        #region Field
        public ApplicationDbContext _context;
        private StorageBlobService _blobService;
        #endregion

        public UserMediaManagementService(ApplicationDbContext context,
                                ILogger<UserMediaManagementService> logger,
                                IWebHostEnvironment env)
        {
            _context = context;
            _blobService = new StorageBlobService();
            _logger = logger;
            _env = env;
        }

        //public async Task<IList<UserMedia>> ListUserMediaAsyc(string userId = "")
        //{
        //    var query = (this._context.UserMedia.AsNoTracking()
        //                             .Where(c => c.UserId.Equals(userId)));

        //    IList<UserMedia> userMediaList = await query.OrderByDescending(c => c.MediaId).ToListAsync();

        //    return userMediaList;
        //}

        /// <summary>
        ///     Search Image by conditions
        /// </summary>
        /// <param name="searchKey"></param>
        /// <param name="container_id"></param>
        /// <returns></returns>
        public async Task<IList<UserMedia>> SearchUserMediaAsync(string searchKey, int container_id, int skip, int take)
        {
            try
            {
                var query = this._context.UserMedia.AsNoTracking()
                                     .Where(c => c.ContainerId.Equals(container_id) && c.Status == 0 && c.Deleted == null);

                //todo tagテーブルで検索に変更したい？どっちが早いか
                if (!String.IsNullOrEmpty(searchKey))
                {
                    string[] listSearchKey = Regex.Split(searchKey.Trim(), "[, 　]+", RegexOptions.IgnoreCase);

                    foreach(var word in listSearchKey)
                    {
                        query = query.Where(c => c.SearchText.Contains(word));
                    }
                }

                IList<UserMedia> userMediaList = await query.OrderByDescending(c => c.MediaId)
                                                                        .Skip(skip).Take(take).ToListAsync();
                return userMediaList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IList<UserMedia>> SearchAlbumMediaAsync(string searchKey, int skip, int take)
        {
            try
            {                
                var userMediaList = await (
                                        from um in _context.UserMedia.AsNoTracking()
                                        join am in _context.UserAlbumMedias.AsNoTracking()
                                        on um.MediaId equals am.UserMediaId
                                        join ua in _context.UserAlbums.AsNoTracking()
                                        on am.AlbumId equals ua.AlbumId
                                        where ua.AlbumUrl.EndsWith(searchKey) && ua.AlbumVisibility == true && ua.AlbumDeletedat == null
                                        && um.Status == 0 && um.Deleted == null
                                        orderby am.AlbumMediaId descending
                                        select um
                                    )
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
        ///     GetUserMediaSameTitleMediasAsync By UserMedia
        ///     同じタイトルの画像を取得
        /// </summary>
        /// <param name="media_id"></param>
        /// <returns></returns>
        public async Task<IList<UserMedia>> GetUserMediaSameTitleMediasAsync(UserMedia media)
        {
            return await this._context.UserMedia.AsNoTracking()
                                 .Where(c => c.ContainerId.Equals(media.ContainerId) && c.MediaTitle.Equals(media.MediaTitle.TrimExtraSpaces())).ToListAsync();
        }

        /// <summary>
        ///     GetUserMediaRelatedMediasAsync By UserMedia
        ///     似ている画像を取得
        /// </summary>
        /// <param name="media_id"></param>
        /// <returns></returns>
        public async Task<IList<UserMedia>> GetUserMediaRelatedMediasAsync(UserMedia media, int skip, int take)
        {
            try
            {
                //同じオリジナルタグを持っているmediaは対象
                //AIが付けたタグのいずれかが一致するmediaも対象
                //スコアの大きい順
                //TODO オリジナルタグとタイトルはスコア換算値を上げたい
                var point = 5;

                var searchTags = media.Tags.Split("|").Where(w => w != "").Select(str => str.Split(":")[0]).ToList();

                //todo 過渡期のみ
                if (searchTags.Count == 1) searchTags = searchTags[0].Split(",").ToList();

                var sameOrgTagMedia = from q in this._context.UserMediaTags.AsNoTracking()
                                      where searchTags.Contains(q.Tag) && q.ContainerId == media.ContainerId && q.UserMediaName != media.MediaFileName
                                      group q by q.UserMediaName into G
                                      select new
                                      {
                                          UserMediaName = G.Key,
                                          Confidence = G.Sum(z => (z.TagsType != 2 ? z.Confidence * point : z.Confidence))
                                      };

                var userMediaList = await (
                                        from q in this._context.UserMedia.AsNoTracking()
                                        join s in sameOrgTagMedia
                                        on q.MediaFileName equals s.UserMediaName
                                        orderby s.Confidence descending 
                                        select q
                                    )
                                    .Skip(skip).Take(take).ToListAsync();

                return userMediaList;
            }
            catch (Exception)
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
            //30日以内にアップロードした写真が対象
            var subquery = await this._context.UserMedia.AsNoTracking()
                                .Where(c => c.ContainerId.Equals(containerId) && c.Uploaded >= DateTime.Today.AddDays(-30))
                                .Select(s => new { s.MediaFileName, s.SearchText }).ToListAsync();

            if (String.IsNullOrEmpty(searchKey)) searchKey = "";

            string[] listSearchKey = Regex.Split(searchKey.Trim(), "[ 　]+", RegexOptions.IgnoreCase);

            foreach (var word in listSearchKey)
            {
                subquery = subquery.Where(s => s.SearchText.Contains(word)).ToList();
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

            //    var query = await this._context.UserMediaTags.AsNoTracking()
            //                        .Where(c => c.ContainerId.Equals(containerId) && c.TagsType == 1 && c.Created >= DateTime.Today.AddDays(-60))
            //                        .GroupBy(g => g.Tag)
            //                        .OrderByDescending(w => w.Count())
            //                        .Select(s => s.Key).Take(take).ToListAsync();
        }

        /// <summary>
        /// Create UserMedia
        /// </summary>
        /// <param name="request">Data</param>
        /// <param name="UserId">UserId current</param>
        public IList<UserMedia> UploadMediaFile(ImageRegistrationRequests request, string UserId)
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

            IList<UserMedia> returnResult = new List<UserMedia>();

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
                string thumbUrl = "https://sozosya.blob.core.windows.net/pinterest/1/thumb/noimage.png";

                // Upload file (no Validation)
                //Stream imageStream = file.OpenReadStream();
                using var imageStream = new MemoryStream();
                file.CopyTo(imageStream);
                imageStream.Seek(0, SeekOrigin.Begin);
                using var smallimageStream = new MemoryStream();
                var thumbnailStream = new MemoryStream();
                string aitagsString = "";
                string loggedInUserId = UserId;
                string takeDateString = "";
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
                            //3:2
                            var option = new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(1140, 760) };
                            x.Resize(option);
                        });
                        imgSharp.Save(smallimageStream, format);
                        smallimageStream.Seek(0, SeekOrigin.Begin);

                        // 撮影日
                        if (imgSharp.Metadata.ExifProfile == null)
                        {
                            takeDateString = "";
                        }
                        else
                        {
                            var exif = imgSharp.Metadata.ExifProfile.GetValue(ExifTag.DateTimeOriginal)?.Value;
                            if (string.IsNullOrEmpty(exif))
                            {
                                takeDateString = "";
                            }
                            else
                            {
                                var sb = new StringBuilder(imgSharp.Metadata.ExifProfile.GetValue(ExifTag.DateTimeOriginal).Value);
                                //var sb = new StringBuilder(imgSharp.Metadata.ExifProfile.GetValue(ExifTag.DateTime).Value);
                                takeDateString = sb.Replace(":", "/", 0, 10).ToString();
                                //TODO yyyy/MM/dd HH:mm: というデータがきた
                                if (takeDateString.Length < "1976/12/19 23:59:59".Length)
                                {
                                    takeDateString = takeDateString.TrimEnd() + "00";
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ImageSharp Error
                    //throw new Exception("ImageSharp でサムネイルを作成ができませんでした。ファイルは登録されません");
                    _logger.LogWarning("{DT} ImageSharp でサムネイルを作成ができませんでした。ファイルは登録されません", DateTime.UtcNow.ToLongTimeString());
                }

                // Upload file (small size)
                var small_result = _blobService.UploadStreamBlobAsync(small, ContainerName, smallimageStream);
                if (small_result == null)
                    //throw new Exception("Update small image fail!");
                    _logger.LogWarning("{DT} Update small image fail!", DateTime.UtcNow.ToLongTimeString());

                // Upload file (Original)
                var result = _blobService.UploadImageBlobAsync(original, ContainerName, (IFormFile)file);
                if (result == null)
                    throw new Exception("Update image fail!");

                // Get tags by ProjectTags
                string originalTagsWithScore = "";
                string originalTags = "";
                if (!string.IsNullOrEmpty(request.OriginalTags))
                {
                    originalTags = request.OriginalTags.Trim().Replace("|",",");
                    originalTagsWithScore = request.OriginalTags.Trim().Replace(",", ":1.0:1|");
                    originalTagsWithScore += ":1.0:1|";
                }

                // Get tags by Computer Vision API
                try
                {
                    ComputerVisionService cv = new ComputerVisionService(_env);
                    // 1 patterns in the prototype
                    //tagsString = cv.GetImageTag_str(result.Result.Uri.ToString());

                    //アップロードが完了するまで待機　todo
                    while (true)
                    {
                        if (small_result.IsCompleted) break;
                    }
                    aitagsString = cv.GetImageTag_str(small_result.Result.Uri.ToString());
                    if (String.IsNullOrEmpty(aitagsString))
                    {
                        // Computer Vision Error ⇒ 正常で解析されない写真もあり
                        //throw new Exception("Computer Vision による解析ができませんでした。ファイルは登録されません");
                    }

                    // Get thumbnail data
                    using var stream = cv.GetThumbnail(small_result.Result.Uri.ToString());
                    stream.CopyTo(thumbnailStream);
                    thumbnailStream.Seek(0, SeekOrigin.Begin);

                    //サムネイル作成に失敗したらnoimageで登録しておく
                    if (thumbnailStream != null)
                    {
                        // Upload file (thumbnai)
                        var thumb_result = _blobService.UploadStreamBlobAsync(thumb, ContainerName, thumbnailStream);
                        if (thumb_result == null)
                            throw new Exception("Update thumbnail image fail!");

                        thumbUrl = thumb_result.Result.Uri.ToString();
                    }
                    else
                    {
                        thumbUrl = "https://sozosya.blob.core.windows.net/pinterest/1/thumb/noimage.png";
                    }
                }
                catch (Exception)
                {
                    // Computer Vision Error
                    //TODO エラーでも登録
                    //throw new Exception("Computer Vision による解析ができませんでした。ファイルは登録されません");
                }

                try
                {
                    // Create Model data
                    userMedia.UserId = loggedInUserId;
                    userMedia.MediaFileName = orgFileName;
                    userMedia.MediaFileType = blobFileName.Split('.').Last();
                    userMedia.MediaUrl = result.Result.Uri.ToString();
                    userMedia.MediaSmallUrl = small_result.Result.Uri.ToString();
                    userMedia.MediaThumbnailUrl = thumbUrl;
                    userMedia.MediaTitle = request.Title;
                    userMedia.MediaDescription = request.Description ?? "";
                    userMedia.ContainerId = int.Parse(userContainerId);
                    userMedia.Status = 0;
                    userMedia.Uploaded = DateTime.Now;
                    userMedia.Created = string.IsNullOrEmpty(takeDateString) ? DateTime.Now : DateTime.ParseExact(takeDateString, "yyyy/MM/dd HH:mm:ss", null);
                    userMedia.Tags = "";
                    userMedia.AITags = string.Join(",", aitagsString.Split("|").Where(s => s != "").Select(s => s.Substring(0, s.IndexOf(":"))));

                    if (!string.IsNullOrEmpty(originalTags))
                    {
                        userMedia.OriginalTags = originalTags;
                        userMedia.Tags += originalTagsWithScore;
                    }

                    userMedia.Tags += aitagsString;

                    _context.UserMedia.Add(userMedia);

                    returnResult.Add(userMedia);

                    // tagテーブル titleもtag(type=0)
                    UserMediaTags userMediaTags = new UserMediaTags();
                    userMediaTags.UserMediaName = orgFileName;
                    userMediaTags.ContainerId = userMedia.ContainerId;
                    userMediaTags.TagsType = 0;
                    userMediaTags.Tag = userMedia.MediaTitle;
                    userMediaTags.Confidence = 1.0;
                    userMediaTags.Created = DateTime.Now;
                    _context.UserMediaTags.Add(userMediaTags);
                    //cv解析のtagと独自tag
                    foreach (var tag in originalTags.Split(","))
                    {
                        //オリジナルはtype=1
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
                    foreach (var tag in aitagsString.Split("|")
                                                .Where(x => x != "")
                                                .Select(x => x.Split(":"))
                                                .GroupBy(x => x[0])
                                                .Select(x => new { Name = x.Key, Score = x.Max(m => m[1]) }))
                    {
                        //cv解析のtagはtype=2
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
                    //撮影日のtagはtype=3
                    if (!string.IsNullOrEmpty(takeDateString))
                    {
                        userMediaTags = new UserMediaTags();
                        userMediaTags.UserMediaName = orgFileName;
                        userMediaTags.ContainerId = userMedia.ContainerId;
                        userMediaTags.TagsType = 3;
                        userMediaTags.Tag = takeDateString;
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
            return returnResult;
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
                string aitagsString = "";
                string loggedInUserId = UserId;
                string takeDateString = "";
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
                            //3:2
                            var option = new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(1140, 760) };
                            x.Resize(option);
                        });
                        imgSharp.Save(smallimageStream, format);
                        smallimageStream.Seek(0, SeekOrigin.Begin);

                        // 撮影日
                        if (imgSharp.Metadata.ExifProfile == null)
                        {
                            takeDateString = "";
                        }
                        else
                        {
                            var exif = imgSharp.Metadata.ExifProfile.GetValue(ExifTag.DateTimeOriginal)?.Value;
                            if (string.IsNullOrEmpty(exif))
                            {
                                takeDateString = "";
                            }
                            else
                            {
                                var sb = new StringBuilder(imgSharp.Metadata.ExifProfile.GetValue(ExifTag.DateTimeOriginal).Value);
                                //var sb = new StringBuilder(imgSharp.Metadata.ExifProfile.GetValue(ExifTag.DateTime).Value);
                                takeDateString = sb.Replace(":", "/", 0, 10).ToString();
                                //TODO yyyy/MM/dd HH:mm: というデータがきた
                                if (takeDateString.Length < "1976/12/19 23:59:59".Length)
                                {
                                    takeDateString = takeDateString.TrimEnd() + "00";
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ImageSharp Error
                    //throw new Exception("ImageSharp でサムネイルを作成ができませんでした。ファイルは登録されません");
                    _logger.LogWarning("{DT} ImageSharp でサムネイルを作成ができませんでした。ファイルは登録されません", DateTime.UtcNow.ToLongTimeString());
                }

                // Upload file (small size)
                var small_result = _blobService.UploadStreamBlobAsync(small, ContainerName, smallimageStream);
                if (small_result == null)
                    //throw new Exception("Update small image fail!");
                    _logger.LogWarning("{DT} Update small image fail!", DateTime.UtcNow.ToLongTimeString());

                // Upload file (Original)
                var result = _blobService.UploadImageBlobAsync(original, ContainerName, (IFormFile)file);
                if (result == null)
                    throw new Exception("Update image fail!");


                // Get tags by ProjectTags
                string originalTagsWithScore = "";
                string originalTags = "";
                if (!string.IsNullOrEmpty(imageInfo.OriginalTags))
                {
                    originalTags = imageInfo.OriginalTags.Trim().Replace("|", ",");
                    originalTagsWithScore = imageInfo.OriginalTags.Trim().Replace(",", ":1.0:1|");
                    originalTagsWithScore += ":1.0:1|";
                }

                // Get tags by Computer Vision API
                try
                {
                    ComputerVisionService cv = new ComputerVisionService(_env);
                    // 1 patterns in the prototype
                    //tagsString = cv.GetImageTag_str(result.Result.Uri.ToString());

                    //アップロードが完了するまで待機　todo
                    while (true)
                    {
                        if (small_result.IsCompleted) break;
                    }
                    aitagsString = cv.GetImageTag_str(small_result.Result.Uri.ToString());
                    if (String.IsNullOrEmpty(aitagsString))
                    {
                        // Computer Vision Error ⇒ 正常に解析されない写真もあり
                        //throw new Exception("Computer Vision による解析ができませんでした。ファイルは登録されません");
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

                try
                {
                    // Create Model data
                    userMedia.UserId = loggedInUserId;
                    userMedia.MediaFileName = orgFileName;
                    userMedia.MediaFileType = blobFileName.Split('.').Last();
                    userMedia.MediaUrl = result.Result.Uri.ToString();
                    userMedia.MediaSmallUrl = small_result.Result.Uri.ToString();
                    userMedia.MediaThumbnailUrl = thumb_result.Result.Uri.ToString();
                    userMedia.MediaTitle = imageInfo.Title;
                    userMedia.MediaDescription = imageInfo.Description ?? "";
                    userMedia.ContainerId = int.Parse(userContainerId);
                    userMedia.Status = 0;
                    userMedia.Uploaded = DateTime.Now;
                    userMedia.Created = string.IsNullOrEmpty(takeDateString) ? DateTime.Now : DateTime.ParseExact(takeDateString, "yyyy/MM/dd HH:mm:ss", null);
                    userMedia.Tags = "";
                    userMedia.AITags = string.Join(",", aitagsString.Split("|").Where(s => s != "").Select(s => s.Substring(0, s.IndexOf(":"))));

                    if (!string.IsNullOrEmpty(originalTags))
                    {
                        userMedia.OriginalTags = originalTags;
                        userMedia.Tags += originalTagsWithScore;
                    }
                    userMedia.Tags += aitagsString;

                    _context.UserMedia.Add(userMedia);

                    // tagテーブル titleもtag(type=0)
                    UserMediaTags userMediaTags = new UserMediaTags();
                    userMediaTags.UserMediaName = orgFileName;
                    userMediaTags.ContainerId = userMedia.ContainerId;
                    userMediaTags.TagsType = 0;
                    userMediaTags.Tag = userMedia.MediaTitle;
                    userMediaTags.Confidence = 1.0;
                    userMediaTags.Created = DateTime.Now;
                    _context.UserMediaTags.Add(userMediaTags);
                    //cv解析のtagと独自tag
                    foreach (var tag in originalTags.Split(","))
                    {
                        //オリジナルはtype=1
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
                    foreach (var tag in aitagsString.Split("|")
                                                .Where(x => x != "")
                                                .Select(x => x.Split(":"))
                                                .GroupBy(x => x[0])
                                                .Select(x => new { Name = x.Key, Score = x.Max(m => m[1]) }))
                    {
                        //cv解析のtagはtype=2
                        //if (tag == "") break;
                        userMediaTags = new UserMediaTags();
                        userMediaTags.UserMediaName = orgFileName;
                        userMediaTags.ContainerId = userMedia.ContainerId;
                        userMediaTags.TagsType = 2;
                        userMediaTags.Tag = tag.Name;
                        userMediaTags.Confidence = double.Parse(tag.Score);
                        userMediaTags.Created = DateTime.Now;
                        _context.UserMediaTags.Add(userMediaTags);
                    }
                    //撮影日のtagはtype=3
                    if (!string.IsNullOrEmpty(takeDateString))
                    {
                        userMediaTags = new UserMediaTags();
                        userMediaTags.UserMediaName = orgFileName;
                        userMediaTags.ContainerId = userMedia.ContainerId;
                        userMediaTags.TagsType = 3;
                        userMediaTags.Tag = takeDateString;
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
        public bool UpdateUserMedia(UserMedia userMedia)
        {
            try
            {
                //var userMedia = _context.UserMedia.Find(userMedia.MediaId);

                // Update Model data
                userMedia.Modified = DateTime.Now;

                // OriginalとAIを分割する
                var AITagsList = userMedia.Tags.Split("|").Where(w => w != "")
                            .Select(str => str.Split(":"))
                            .Where(t => double.Parse(t[1]) < 1.0)
                            .ToList();

                userMedia.Tags = "";

                // Get tags by ProjectTags
                string originalTagsWithScore = "";
                string originalTags = "";
                if (!string.IsNullOrEmpty(userMedia.OriginalTags))
                {
                    originalTags = userMedia.OriginalTags.Trim().Replace("|", ",");
                    originalTagsWithScore = userMedia.OriginalTags.Trim().Replace(",", ":1.0:1|");
                    originalTagsWithScore += ":1.0:1|";
                }
                if (originalTags != null)
                {
                    userMedia.OriginalTags = originalTags;
                    userMedia.Tags += originalTagsWithScore;
                }
                foreach (var tag in AITagsList)
                {
                    userMedia.Tags += string.Concat(tag[0], ":", tag[1], ":", tag[2], "|");
                }

                _context.Update(userMedia);


                // tagテーブルの(type=0,1)は削除新規
                var query = from q in _context.UserMediaTags
                            where q.UserMediaName == userMedia.MediaFileName 
                            && new byte[] {0,1}.Contains(q.TagsType)
                            select q;
                _context.UserMediaTags.RemoveRange(query);

                //タイトルtag(type=0)
                UserMediaTags userMediaTags = new UserMediaTags();
                userMediaTags.UserMediaName = userMedia.MediaFileName;
                userMediaTags.ContainerId = userMedia.ContainerId;
                userMediaTags.TagsType = 0;
                userMediaTags.Tag = userMedia.MediaTitle;
                userMediaTags.Confidence = 1.0;
                userMediaTags.Created = DateTime.Now;
                _context.UserMediaTags.Add(userMediaTags);
                //独自tag(type=1)
                foreach (var tag in userMedia.OriginalTags.Split(","))
                {
                    //オリジナルはtype=1
                    if (tag == "") break;
                    userMediaTags = new UserMediaTags();
                    userMediaTags.UserMediaName = userMedia.MediaFileName;
                    userMediaTags.ContainerId = userMedia.ContainerId;
                    userMediaTags.TagsType = 1;
                    userMediaTags.Tag = tag;
                    userMediaTags.Confidence = 1.0;
                    userMediaTags.Created = DateTime.Now;
                    _context.UserMediaTags.Add(userMediaTags);
                }

                // save
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                throw new Exception("SQL database への登録に失敗しました");
            }
        }
        /// <summary>
        /// Create UserMedia
        /// </summary>
        /// <param name="UserId">UserId current</param>
        public void thumbnailRecovery(string UserId)
        {
            // Upload image file to Azure Blob
            string ContainerName = dev_Settings.blob_containerName_image;
            ////string ContainerNameThumb = dev_Settings.blob_containerName_thumb;

            // Login User
            var user = this._context.Users.Where(c => c.Id.Equals(UserId)).FirstOrDefault();
            string userContainerId = user.container_id.ToString();

            var noImageList = _context.UserMedia.Where(w => w.MediaThumbnailUrl.Contains("noimage") && w.ContainerId==user.container_id)
                            .ToList();

            // File Loop
            foreach (var userMedia in noImageList)
            {
                string thumbUrl = "";
                string thumbPath = String.Format("{0}/{1}/{2}", userContainerId, "thumb", userMedia.MediaFileName + "." + userMedia.MediaFileType);
                string aitagsString = "";

                // Get tags by Computer Vision API
                try
                {
                    ComputerVisionService cv = new ComputerVisionService(_env);

                    aitagsString = cv.GetImageTag_str(userMedia.MediaSmallUrl);
                    if (String.IsNullOrEmpty(aitagsString))
                    {
                        // Computer Vision Error ⇒ 正常に解析されない写真もあり
                        //throw new Exception("Computer Vision による解析ができませんでした。ファイルは登録されません");
                    }

                    // Get thumbnail data
                    var thumbnailStream = new MemoryStream();
                    using var stream = cv.GetThumbnail(userMedia.MediaSmallUrl);
                    stream.CopyTo(thumbnailStream);
                    thumbnailStream.Seek(0, SeekOrigin.Begin);

                    //サムネイル作成に失敗したらnoimageで登録しておく
                    if (thumbnailStream != null)
                    {
                        // Upload file (thumbnai)
                        var thumb_result = _blobService.UploadStreamBlobAsync(thumbPath, ContainerName, thumbnailStream);
                        if (thumb_result == null)
                            throw new Exception("Update thumbnail image fail!");

                        thumbUrl = thumb_result.Result.Uri.ToString();
                    }
                    else
                    {
                        thumbUrl = "https://sozosya.blob.core.windows.net/pinterest/1/thumb/noimage.png";
                    }
                }
                catch (Exception)
                {
                    // Computer Vision Error
                    //throw new Exception("Computer Vision による解析ができませんでした。ファイルは登録されません");
                }

                try
                {
                    // Get tags by ProjectTags
                    string originalTagsWithScore = "";
                    string originalTags = "";
                    if (!string.IsNullOrEmpty(userMedia.OriginalTags))
                    {
                        originalTags = userMedia.OriginalTags.Trim().Replace("|", ",");
                        originalTagsWithScore = userMedia.OriginalTags.Trim().Replace(",", ":1.0:1|");
                        originalTagsWithScore += ":1.0:1|";
                    }

                    if (!string.IsNullOrEmpty(userMedia.OriginalTags))
                    {
                        userMedia.Tags = originalTagsWithScore;
                    }

                    userMedia.AITags = string.Join(",", aitagsString.Split("|").Where(s => s != "").Select(s => s.Substring(0, s.IndexOf(":"))));
                    userMedia.Tags += aitagsString;
                    userMedia.MediaThumbnailUrl = thumbUrl;
                    _context.Update(userMedia);

                    // tagテーブル
                    UserMediaTags userMediaTags = new UserMediaTags();

                    //cv解析のtag
                    foreach (var tag in aitagsString.Split("|")
                                                .Where(x => x != "")
                                                .Select(x => x.Split(":"))
                                                .GroupBy(x => x[0])
                                                .Select(x => new { Name = x.Key, Score = x.Max(m => m[1]) }))
                    {
                        //cv解析のtagはtype=2
                        //if (tag == "") break;
                        //var tagstr = tag.Split(":");
                        userMediaTags = new UserMediaTags();
                        userMediaTags.UserMediaName = userMedia.MediaFileName;
                        userMediaTags.ContainerId = userMedia.ContainerId;
                        userMediaTags.TagsType = 2;
                        userMediaTags.Tag = tag.Name;
                        userMediaTags.Confidence = double.Parse(tag.Score);
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
    }
}
