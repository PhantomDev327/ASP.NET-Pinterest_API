using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using nxPinterest.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nxPinterest.Services
{
    public class CognitiveSearchService
    {

        public List<SearchResultUserMedia> Search_SQL(string searchKeywords, string userId)
        {
            try
            {
                Uri endpoint = new Uri(dev_Settings.cognitivesearch_endpoint);
                AzureKeyCredential credential = new AzureKeyCredential(dev_Settings.cognitivesearch_adminApiKey);
                SearchClient client = new SearchClient(endpoint, dev_Settings.cognitivesearch_index_SQL, credential);

                SearchOptions options = new SearchOptions()
                {
                    Filter = "user_id eq '" + userId + "'",     // If wanna share images with all users, remove this filter.
                    SearchFields = { "Tags" }
                };

                SearchResults<SearchIndexUserMediaSQL> response = client.Search<SearchIndexUserMediaSQL>(searchKeywords, options);
                List<SearchResultUserMedia> searchResult = new List<SearchResultUserMedia>();

                if (response != null)
                {
                    foreach (SearchResult<SearchIndexUserMediaSQL> result in response.GetResults())
                    {
                        SearchResultUserMedia document = new SearchResultUserMedia();
                        document.Id = result.Document.media_id;
                        document.UserId = result.Document.user_id;
                        document.MediaFileName = result.Document.media_file_name;
                        document.MediaFileType = result.Document.media_file_type;
                        document.MediaUrl = result.Document.media_url;
                        document.DateTimeUploaded = DateTime.Parse(result.Document.DateTimeUploaded);
                        document.Tags = "";

                        var tags = result.Document.Tags.Split("|");

                        foreach (var tag in tags.Select((value, index) => new { value, index }))
                        {
                            if (tag.value.Equals("|") || tag.value.Equals("")) break;
                            var tag_array = tag.value.Split(":");

                            if (tag.index == 0 || (Double.Parse(tag_array[1]) >= dev_Settings.tag_confidence_threshold))
                                document.Tags += tag_array[0] + "%2B";
                        }
                        int del_index = document.Tags.LastIndexOf("%2B");
                        if (del_index > 0) document.Tags = document.Tags.Remove(del_index);
                        document.SearchScore = (Double.Parse(result.Score.ToString()) * 100.0).ToString("F2");
                        searchResult.Add(document);
                    }
                }
                return searchResult;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public List<SearchResultUserMedia> Search_Cosmos(string searchKeywords, string userId)
        {
            try
            {
                Uri endpoint = new Uri(dev_Settings.cognitivesearch_endpoint);
                AzureKeyCredential credential = new AzureKeyCredential(dev_Settings.cognitivesearch_adminApiKey);
                SearchClient client = new SearchClient(endpoint, dev_Settings.cognitivesearch_index_Cosmos, credential);

                SearchOptions options = new SearchOptions()
                {
                    Filter = "userid eq '" + userId + "'"       // If wanna share images with all users, remove this filter.
                };

                SearchResults<SearchIndexUserMediaCosmos> response = client.Search<SearchIndexUserMediaCosmos>(searchKeywords, options);
                List<SearchResultUserMedia> searchResult = new List<SearchResultUserMedia>();

                if (response != null)
                {

                    foreach (SearchResult<SearchIndexUserMediaCosmos> result in response.GetResults())
                    {
                        SearchResultUserMedia document = new SearchResultUserMedia();
                        document.Id = result.Document.mediaurl;
                        document.UserId = result.Document.userid;
                        document.MediaFileName = result.Document.mediafilename;
                        document.MediaFileType = result.Document.mediafiletype;
                        document.MediaUrl = result.Document.mediaurl;
                        document.DateTimeUploaded = DateTime.Parse(result.Document.datetimeuploaded);
                        document.Tags = "";

                        foreach (var tag in result.Document.Tags.Select((value, index) => new { value, index }))
                        {
                            if (tag.index == 0 || (tag.value.confidence >= dev_Settings.tag_confidence_threshold))
                                document.Tags += tag.value.name + "%2B";
                        }
                        int del_index = document.Tags.LastIndexOf("%2B");
                        if (del_index > 0) document.Tags = document.Tags.Remove(del_index);
                        document.SearchScore = (Double.Parse(result.Score.ToString()) * 100.0).ToString("F2");
                        searchResult.Add(document);
                    }
                }
                return searchResult;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public List<SearchResultUserMedia> Search_Table(string searchKeywords, string userId)
        {
            try
            {
                Uri endpoint = new Uri(dev_Settings.cognitivesearch_endpoint);
                AzureKeyCredential credential = new AzureKeyCredential(dev_Settings.cognitivesearch_adminApiKey);
                SearchClient client = new SearchClient(endpoint, dev_Settings.cognitivesearch_index_Table, credential);

                SearchOptions options = new SearchOptions()
                {
                    Filter = "UserId eq '" + userId + "'",      // If wanna share images with all users, remove this filter.
                    SearchFields = { "Tags" }
                };

                SearchResults<SearchIndexUserMediaTable> response = client.Search<SearchIndexUserMediaTable>(searchKeywords, options);
                List<SearchResultUserMedia> searchResult = new List<SearchResultUserMedia>();

                if (response != null)
                {
                    foreach (SearchResult<SearchIndexUserMediaTable> result in response.GetResults())
                    {
                        SearchResultUserMedia document = new SearchResultUserMedia();
                        document.Id = result.Document.Key;
                        document.UserId = result.Document.UserId;
                        document.MediaFileName = result.Document.MediaFileName;
                        document.MediaFileType = result.Document.MediaFileType;
                        document.MediaUrl = result.Document.MediaUrl;
                        document.DateTimeUploaded = result.Document.DateTimeUploaded;
                        document.Tags = "";

                        var tags = result.Document.Tags.Split("|");

                        foreach (var tag in tags.Select((value, index) => new { value, index }))
                        {
                            if (tag.value.Equals("|") || tag.value.Equals("")) break;
                            var tag_array = tag.value.Split(":");

                            if (tag.index == 0 || (Double.Parse(tag_array[1]) >= dev_Settings.tag_confidence_threshold))
                                document.Tags += tag_array[0] + "%2B";
                        }
                        int del_index = document.Tags.LastIndexOf("%2B");
                        if (del_index > 0) document.Tags = document.Tags.Remove(del_index);
                        document.SearchScore = (Double.Parse(result.Score.ToString()) * 100.0).ToString("F2");
                        searchResult.Add(document);
                    }
                }
                return searchResult;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        public List<SearchResultUserMedia> Search_Blob(string searchKeywords, string userId)
        {
            try
            {
                Uri endpoint = new Uri(dev_Settings.cognitivesearch_endpoint);
                AzureKeyCredential credential = new AzureKeyCredential(dev_Settings.cognitivesearch_adminApiKey);
                SearchClient client = new SearchClient(endpoint, dev_Settings.cognitivesearch_index_Blob, credential);

                SearchOptions options = new SearchOptions()
                {
                    Filter = "userid eq '" + userId + "'"       // If wanna share images with all users, remove this filter.
                };

                SearchResults<SearchIndexUserMediaBlob> response = client.Search<SearchIndexUserMediaBlob>(searchKeywords, options);
                List<SearchResultUserMedia> searchResult = new List<SearchResultUserMedia>();

                if (response != null)
                {
                    foreach (SearchResult<SearchIndexUserMediaBlob> result in response.GetResults())
                    {
                        SearchResultUserMedia document = new SearchResultUserMedia();

                        document.Id = result.Document.key;
                        document.UserId = result.Document.userid;
                        document.MediaFileName = result.Document.mediafilename;
                        document.MediaFileType = result.Document.mediafiletype;
                        document.MediaUrl = result.Document.mediaurl;
                        document.DateTimeUploaded = DateTime.Parse(result.Document.datetimeuploaded);
                        document.Tags = "";

                        Console.WriteLine(result.Document.Tags.Count.ToString());
                        foreach (var tag in result.Document.Tags.Select((value, index) => new { value, index }))
                        {
                            if (tag.index == 0 || (tag.value.confidence >= dev_Settings.tag_confidence_threshold))
                                document.Tags += tag.value.name + "%2B";
                        }
                        int del_index = document.Tags.LastIndexOf("%2B");
                        if (del_index > 0) document.Tags = document.Tags.Remove(del_index);
                        document.SearchScore = (Double.Parse(result.Score.ToString()) * 100.0).ToString("F2");
                        searchResult.Add(document);
                    }
                }
                return searchResult;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
