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

namespace nxPinterest.Web.Controllers
{
    [Authorize]
    public class ImageManagementController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private StorageBlobOldService blobService;
        private readonly ApplicationDbContext _context;
        private readonly Models.DestinationPathModel _destinationPathModel;
        private Base64stringUtility encode = new Base64stringUtility("UTF-8");
        public ImageManagementController(ILogger<HomeController> logger,
                                         IOptions<Models.DestinationPathModel> destinationPathModel,
                                         ApplicationDbContext context)
        {
            this._logger = logger;
            this._context = context;
            this._destinationPathModel = destinationPathModel.Value;

            blobService = new StorageBlobOldService();
        }


        [HttpPost]
        public async Task<IActionResult> UploadMediaFile(ImageRegistrationRequests request)
        {
            bool isUpdate = request.MediaId != 0;

            if (isUpdate)
                ModelState.Remove("Images");

            if (ModelState.IsValid)
            {
                try
                {
                    string media_title = request.Title;
                    string media_desc = request.Description;
                    //int userMediaId = 0;
                    IList<IFormFile> uploaded_images = request.Images;
                    string projectTags = request.ProjectTags;

                    if (isUpdate)
                    {
                        UserMedia _userMedia = await this._context.UserMedia.FirstOrDefaultAsync(c => c.MediaId.Equals(request.MediaId));
                        _userMedia.MediaTitle = request.Title;
                        _userMedia.MediaDescription = request.Description;
                        UpdateUserMediaTags(_userMedia, projectTags);
                        await this._context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return RedirectToAction("Index", "Home");
        }

        private MemoryStream LoadFileAsMemoryStream(string filePath)
        {
            MemoryStream ms = new MemoryStream();
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int)fs.Length);
                ms.Write(bytes, 0, (int)fs.Length);
            }

            return ms;
        }

        private bool UpdateUserMediaTags(UserMedia userMedia, string projectTags)
        {
            try
            {
                if (userMedia != null)
                {
                    if (projectTags != null) {
                        projectTags = projectTags.Trim().Replace(',', '|');
                    }

                    string[] tag_list = userMedia.Tags.Split('|');
                    IList<string> phototag_list_container = new List<string>();
                    for (int i = 0; i < tag_list.Count(); i++) {
                        string[] tag_input = tag_list[i].Split(':');
                        string tag_name = tag_input[0];
                        if (!string.IsNullOrEmpty(tag_name)) {
                            decimal score = decimal.Parse(tag_input[1]);
                            int type = int.Parse(tag_input[2]);

                            if (score < 1)
                                phototag_list_container.Add(string.Format("{0}:{1}:{2}|", tag_name, score, type));
                        }
                    }

                    string tagString = String.Join("", phototag_list_container);
                    userMedia.Tags = tagString;
                    userMedia.ProjectTags = projectTags;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetFileName(string containerName, string fileName)
        {
            blobService.CreateContainerIfNotExistsAsync(containerName);
            var existFiles = blobService.GetBlobFileList(containerName);
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

            return fileName;
        }

        private byte[] ConvertImageToBytes(Image img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                return ms.ToArray();
            }
        }

        private async Task<UserMedia> InsertIntoUserMedia(string fileName, string containerName, string filePath, string media_title, string media_desc, string projectTags)
        {

            var result = blobService.UploadImageBlobAsync(Path.GetFileName(fileName), containerName, filePath);
            string tagsString, tagsJson;
            string[] _projectTags = new string[] { };


            if (result != null)
            {
                ComputerVisionService cv = new ComputerVisionService();
                UserMedia userMedia = new UserMedia();


                // 2 patterns in the prototype
                tagsString = cv.GetImageTag_str(result.Result.Uri.ToString());          // Get as parsable string -> 1) SQL and 3) Table
                tagsJson = cv.GetImageTag_json(result.Result.Uri.ToString());           // Get as json            -> 2) Cosmos and 4) Blob 

                StringBuilder _tagString = new StringBuilder(tagsString);
                TagList _tagList = System.Text.Json.JsonSerializer.Deserialize<TagList>(tagsJson, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });


                if (!string.IsNullOrEmpty(projectTags))
                {
                    _projectTags = projectTags.Split(',');
                    decimal tag_score = 1;

                    for (int i = 0; i < _projectTags.Length; i++)
                    {
                        string _projectTag = _projectTags[i].Trim();

                        _tagString.Append(string.Format("{0}:{1}:0|", _projectTag, tag_score));
                        _tagList.Tags.Add(new Tag
                        {
                            Name = _projectTag,
                            Confidence = tag_score,
                            Type = 1
                        });
                    }
                }

                // Create Model data
                userMedia.UserId = this.UserId;                                         // User ID
                userMedia.MediaTitle = media_title;
                userMedia.MediaDescription = media_desc;
                userMedia.MediaUrl = result.Result.Uri.ToString();                      // URL of the blob
                userMedia.MediaFileName = result.Result.Name;                           // File name
                userMedia.MediaFileType = result.Result.Name.Split('.').Last();         // File type
                userMedia.Tags = _tagString.ToString();                                            // Tags (Parsable string)

                tagsJson = System.Text.Json.JsonSerializer.Serialize(_tagList, new System.Text.Json.JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                // Add image info to json (same as model)
                tagsJson = ImageAnalysisUtility.AddImageInfoToTag_json(tagsJson, userMedia);
                userMedia.tagsJson = tagsJson;

                // ---------- 1) [SQL database] (as SQL schema Record) ---------- 
                _context.UserMedia.Add(userMedia);

                await _context.SaveChangesAsync();

                return userMedia;
            }
            return null;
        }

        private async Task<UserMedia> UpdateUserMediaThumbnailUrl(int media_id, string fileName, string containerName, string filePath)
        {
            UserMedia userMedia = await this._context.UserMedia.FirstOrDefaultAsync(c => c.MediaId.Equals(media_id));
            if (userMedia != null)
            {
                var result = blobService.UploadImageBlobAsync(Path.GetFileName(fileName), containerName, filePath);

                if (result != null)
                {
                    userMedia.MediaThumbnailUrl = result.Result.Uri.ToString();

                    await this._context.SaveChangesAsync();
                }
            }

            return userMedia;
        }

        private string InsertOnCosmosDB(UserMedia userMedia)
        {

            CosmosDbService cosmosDbService = new CosmosDbService();
            if (!cosmosDbService.InsertOneItemAsync(dev_Settings.cosmos_databaseName, dev_Settings.cosmos_containerName, userMedia.tagsJson).Result)
                throw new Exception("Cosmos DB への登録に失敗しました");

            string id = cosmosDbService.inserted_id;

            return id;
        }

        private string InsertOnUserMediaStorageTable(UserMedia userMedia)
        {
            UserMediaStorageTableEntity userMediaStorageTableEntity = new UserMediaStorageTableEntity();
            userMediaStorageTableEntity.UserId = userMedia.UserId;
            userMediaStorageTableEntity.MediaUrl = userMedia.MediaUrl;
            userMediaStorageTableEntity.MediaFileName = userMedia.MediaFileName;
            userMediaStorageTableEntity.MediaFileType = userMedia.MediaFileType;
            userMediaStorageTableEntity.Tags = userMedia.Tags;
            userMediaStorageTableEntity.DateTimeUploaded = userMedia.DateTimeUploaded;

            StorageTableService tableService = new StorageTableService();
            if (!tableService.InsertOrMerageEntityAsync(userMediaStorageTableEntity).Result)
                throw new Exception("Storage Table への登録に失敗しました");

            string id = tableService.inserted_id;
            return id;
        }

        private string InsertOnUserMediaBlob(UserMedia userMedia)
        {
            string datetimeStr = DateTime.Now.ToString("yyyyMMddHHmmss_");

            UserMediaBlobJSON userMediaBlobJSON = JsonConvert.DeserializeObject<UserMediaBlobJSON>(userMedia.tagsJson);
            userMediaBlobJSON.Key = (encode.Encode(userMedia.UserId + datetimeStr + userMedia.MediaFileName)).Replace("+", "=");
            string tagsJson = JsonConvert.SerializeObject(userMediaBlobJSON, Formatting.None);
            string fileName = "UserMedia/" + userMedia.UserId + "/" + datetimeStr + userMedia.MediaFileName + ".json";

            if (!blobService.StoreJsonBlobAsync(fileName, dev_Settings.blob_containerName_json, tagsJson).Result)
                throw new Exception("Storage Blob への登録に失敗しました");

            string id = fileName;
            return id;
        }

        private void InsertOnMediaId(UserMedia userMedia, string[] ids)
        {
            MediaId mediaId = new MediaId();
            mediaId.Sql_id = userMedia.MediaId;
            mediaId.Cosmos_db = ids[0];
            mediaId.Storage_table = ids[1];
            mediaId.Storage_blob = ids[2];

            _context.MediaId.Add(mediaId);
            _context.SaveChanges();
        }

        private static System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
        {
            //Get the image current width  
            int sourceWidth = imgToResize.Width;
            //Get the image current height  
            int sourceHeight = imgToResize.Height;
            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            //Calulate  width with new desired size  
            nPercentW = ((float)size.Width / (float)sourceWidth);
            //Calculate height with new desired size  
            nPercentH = ((float)size.Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;
            //New Width  
            int destWidth = (int)(sourceWidth * nPercent);
            //New Height  
            int destHeight = (int)(sourceHeight * nPercent);
            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            // Draw image with new width and height  
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();
            return (System.Drawing.Image)b;
        }
    }
}
