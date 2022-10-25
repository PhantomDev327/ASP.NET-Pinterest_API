using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using nxPinterest.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace nxPinterest.Services
{
    public class ComputerVisionService
    {
        private static ComputerVisionClient client;
        private static string subscriptionKey;
        private static string endpoint;

        public ComputerVisionService()
        {
            subscriptionKey = dev_Settings.computervision_subscriptionKey;
            endpoint = dev_Settings.computervision_endpoint;
            client = Authenticate(endpoint, subscriptionKey);
        }

        private static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            return new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            { Endpoint = endpoint };
        }

        public string GetImageTag_json(string ImageUrl)
        {
            return ImageAnalysisUtility.AddTypeToTag_json(AnalyzeImageUrl_Tags_json(client, ImageUrl).Result);
        }

        public string GetImageTag_str(string ImageUrl)
        {
            var str = AnalyzeImageUrl_Tags_str(client, ImageUrl).Result;
            return ImageAnalysisUtility.AddTypeToTag_str(str);
        }

        public string GetImageTag_list(string ImageUrl)
        {
            var str = AnalyzeImageUrl_Tags_str(client, ImageUrl).Result;
            return ImageAnalysisUtility.AddTypeToTag_str(str);
        }

        private static async Task<string> AnalyzeImageUrl_Tags_json(ComputerVisionClient client, string imageUrl)
        {
            Microsoft.Rest.HttpOperationResponse<TagResult> results
                = await client.TagImageWithHttpMessagesAsync(imageUrl, language: dev_Settings.computervision_language);

            return results.Response.Content.ReadAsStringAsync().Result;
        }

        private static async Task<string> AnalyzeImageUrl_Tags_str(ComputerVisionClient client, string imageUrl)
        {
            TagResult results = await client.TagImageAsync(imageUrl, language: dev_Settings.computervision_language);

            string tag_str = "";
            foreach (var tag in results.Tags) { tag_str += tag.Name + ":" + tag.Confidence + "|"; }

            return tag_str;
        }

        /* 
         * [FOR TEST] COMPUTER VISION ANALYZE IMAGE
         * Analyze URL image. Extracts captions, categories, tags, objects, faces, racy/adult/gory content,
         * brands, celebrities, landmarks, color scheme, and image types.
         */
        public static async Task AnalyzeImageUrl(ComputerVisionClient client, string imageUrl)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("COMPUTER VISION ANALYZE IMAGE - URL");
            Console.WriteLine();

            // Creating a list that defines the features to be extracted from the image. 
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Categories,
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces,
                VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags,
                VisualFeatureTypes.Adult,
                VisualFeatureTypes.Color,
                VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects
            };
            Console.WriteLine($"Analyzing the image {Path.GetFileName(imageUrl)}...");
            Console.WriteLine();

            ImageAnalysis results = await client.AnalyzeImageAsync(imageUrl, visualFeatures: features, language: dev_Settings.computervision_language);

            // Summary
            Console.WriteLine("Summary:");
            foreach (var caption in results.Description.Captions)
            {
                Console.WriteLine($"{caption.Text} with confidence {caption.Confidence}");
            }
            Console.WriteLine();

            // Categories
            Console.WriteLine("Categories:");
            foreach (var category in results.Categories)
            {
                Console.WriteLine($"{category.Name} with confidence {category.Score}");
            }
            Console.WriteLine();

            // Tags
            Console.WriteLine("Tags:");
            foreach (var tag in results.Tags)
            {
                Console.WriteLine($"{tag.Name} with confidence {tag.Confidence}");
            }
            Console.WriteLine();

            // Objects
            Console.WriteLine("Objects:");
            foreach (var obj in results.Objects)
            {
                Console.WriteLine($"{obj.ObjectProperty} with confidence {obj.Confidence} at location {obj.Rectangle.X}, " +
                  $"{obj.Rectangle.X + obj.Rectangle.W}, {obj.Rectangle.Y}, {obj.Rectangle.Y + obj.Rectangle.H}");
            }
            Console.WriteLine();

            // Faces
            Console.WriteLine("Faces:");
            foreach (var face in results.Faces)
            {
                Console.WriteLine($"A {face.Gender} of age {face.Age} at location {face.FaceRectangle.Left}, " +
                  $"{face.FaceRectangle.Left}, {face.FaceRectangle.Top + face.FaceRectangle.Width}, " +
                  $"{face.FaceRectangle.Top + face.FaceRectangle.Height}");
            }
            Console.WriteLine();

            // Adult or racy content, if any.
            Console.WriteLine("Adult:");
            Console.WriteLine($"Has adult content: {results.Adult.IsAdultContent} with confidence {results.Adult.AdultScore}");
            Console.WriteLine($"Has racy content: {results.Adult.IsRacyContent} with confidence {results.Adult.RacyScore}");
            Console.WriteLine($"Has gory content: {results.Adult.IsGoryContent} with confidence {results.Adult.GoreScore}");
            Console.WriteLine();

            // Well-known (or custom, if set) brands.
            Console.WriteLine("Brands:");
            foreach (var brand in results.Brands)
            {
                Console.WriteLine($"Logo of {brand.Name} with confidence {brand.Confidence} at location {brand.Rectangle.X}, " +
                  $"{brand.Rectangle.X + brand.Rectangle.W}, {brand.Rectangle.Y}, {brand.Rectangle.Y + brand.Rectangle.H}");
            }
            Console.WriteLine();

            // Celebrities in image, if any.
            Console.WriteLine("Celebrities:");
            foreach (var category in results.Categories)
            {
                if (category.Detail?.Celebrities != null)
                {
                    foreach (var celeb in category.Detail.Celebrities)
                    {
                        Console.WriteLine($"{celeb.Name} with confidence {celeb.Confidence} at location {celeb.FaceRectangle.Left}, " +
                          $"{celeb.FaceRectangle.Top}, {celeb.FaceRectangle.Height}, {celeb.FaceRectangle.Width}");
                    }
                }
            }
            Console.WriteLine();

            // Popular landmarks in image, if any.
            Console.WriteLine("Landmarks:");
            foreach (var category in results.Categories)
            {
                if (category.Detail?.Landmarks != null)
                {
                    foreach (var landmark in category.Detail.Landmarks)
                    {
                        Console.WriteLine($"{landmark.Name} with confidence {landmark.Confidence}");
                    }
                }
            }
            Console.WriteLine();

            // Identifies the color scheme.
            Console.WriteLine("Color Scheme:");
            Console.WriteLine("Is black and white?: " + results.Color.IsBWImg);
            Console.WriteLine("Accent color: " + results.Color.AccentColor);
            Console.WriteLine("Dominant background color: " + results.Color.DominantColorBackground);
            Console.WriteLine("Dominant foreground color: " + results.Color.DominantColorForeground);
            Console.WriteLine("Dominant colors: " + string.Join(",", results.Color.DominantColors));
            Console.WriteLine();
            // </snippet_color>

            // Detects the image types.
            Console.WriteLine("Image Type:");
            Console.WriteLine("Clip Art Type: " + results.ImageType.ClipArtType);
            Console.WriteLine("Line Drawing Type: " + results.ImageType.LineDrawingType);
            Console.WriteLine();
        }
        public Stream GetThumbnail(string ImageUrl)
        {
            // Get thumbnail data
            var thumbnailStream = client.GenerateThumbnailAsync(248, 165, ImageUrl, true).Result;
            return thumbnailStream;
        }
    }
}
