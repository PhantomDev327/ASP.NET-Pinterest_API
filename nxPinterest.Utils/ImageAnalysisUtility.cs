using Newtonsoft.Json;
using nxPinterest.Data.Models;
using System;

namespace nxPinterest.Utils
{
    public static class ImageAnalysisUtility
    {
        public static string AddTypeToTag_str(string tags_str, bool type_manual = false)
        {
            try
            {
                var type = type_manual ? 1 : 0;
                var tags = tags_str.Split("|");
                var tags_str_type = "";

                foreach (var tag in tags)
                {
                    if (tag.Equals("|") || tag.Equals("")) break;
                    var tag_array = tag.Split(":");
                    tags_str_type += tag_array[0] + ":" + tag_array[1] + ":" + type.ToString() + "|";
                }
                return tags_str_type;
            }
            catch (Exception e)
            {
                Console.WriteLine($"addType_str Error {e.Message}");
                return null;
            }
        }

        public static string AddTypeToTag_json(string tags_json, bool type_manual = false)
        {
            try
            {
                var type = type_manual ? 1 : 0;

                ImageAnalysisJSON imageAnalysisJSON = JsonConvert.DeserializeObject<ImageAnalysisJSON>(tags_json);
                foreach (var tag in imageAnalysisJSON.Tags) { tag.Type = type; }

                return JsonConvert.SerializeObject(imageAnalysisJSON, Formatting.None);
            }
            catch (Exception e)
            {
                Console.WriteLine($"addType_json Error {e.Message}");
                return null;
            }
        }

        public static string AddImageInfoToTag_json(string tags_json, UserMedia userMedia)
        {

            try
            {
                ImageAnalysisJSON imageAnalysisJSON = JsonConvert.DeserializeObject<ImageAnalysisJSON>(tags_json);
                UserMediaJSON userMediaJSON = new UserMediaJSON();

                userMediaJSON.UserId = userMedia.UserId;
                userMediaJSON.MediaFileName = userMedia.MediaFileName;
                userMediaJSON.MediaFileType = userMedia.MediaFileType;
                userMediaJSON.MediaUrl = userMedia.MediaUrl;
                userMediaJSON.DateTimeUploaded = userMedia.DateTimeUploaded;
                userMediaJSON.Tags = imageAnalysisJSON.Tags;

                return JsonConvert.SerializeObject(userMediaJSON, Formatting.None);
            }
            catch (Exception e)
            {
                Console.WriteLine($"addImageInfo_json Error: {e.Message}");
                return null;
            }
        }

        public static string AddImageInfoToTag_json(string tags_json, UserMediaThumbnails thumbnails, string userId)
        {

            try
            {
                ImageAnalysisJSON imageAnalysisJSON = JsonConvert.DeserializeObject<ImageAnalysisJSON>(tags_json);
                UserMediaJSON userMediaJSON = new UserMediaJSON();

                userMediaJSON.UserId = userId;
                userMediaJSON.MediaFileName = thumbnails.MediaFileName;
                userMediaJSON.MediaFileType = thumbnails.MediaFileType;
                userMediaJSON.MediaUrl = thumbnails.MediaUrl;
                userMediaJSON.DateTimeUploaded = thumbnails.DateTimeUploaded;
                userMediaJSON.Tags = imageAnalysisJSON.Tags;

                return JsonConvert.SerializeObject(userMediaJSON, Formatting.None);
            }
            catch (Exception e)
            {
                Console.WriteLine($"addImageInfo_json Error: {e.Message}");
                return null;
            }
        }
    }
}
