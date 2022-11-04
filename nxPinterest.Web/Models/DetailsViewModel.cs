using System;
using System.Collections.Generic;
using System.Linq;

namespace nxPinterest.Web.Models
{
    /// <summary>
    /// イメージ詳細ページのViewModel
    /// </summary>
    public class DetailsViewModel
    {
        //public Services.Models.Request.ImageRegistrationRequests ImageRegistrationRequests { get; set; } = new Services.Models.Request.ImageRegistrationRequests();
        //public nxPinterest.Services.Models.Response.UserMediaDetailModel UserMediaDetail { get; set; }
        public Data.Models.UserMedia UserMediaDetail { get; set; }
        public IList<Data.Models.UserMedia> SameTitleUserMediaList { get; set; }
        public IList<Data.Models.UserMedia> RelatedUserMediaList { get; set; }
        public string SearchKey { get; set; }
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public virtual string Discriminator { get; set; }

        public IList<string> OriginalTagsList
        {
            get
            {
                return UserMediaDetail.OriginalTags.Split(",").Where(w => w != "").ToList();
            }
        }
        //public string AITags
        //{
        //    get
        //    {
        //        //TODOロジック　0.9以上のタグが一致するMEDIA
        //        var tag = UserMediaDetail.OriginalTags.Split("|").Where(w => w != "").Select(str => str.Split(":")[0]).ToList();

        //        //todo 過渡期のみ
        //        if (tag.Count == 1) tag = tag[0].Split(",").ToList();

        //        return (tag != null) ? string.Join(',', tag.ToArray()) : null; 
        //    }
        //}
        //public IList<string> project_tags_list {
        //    get
        //    {
        //        return UserMediaDetail.OriginalTags != null ? UserMediaDetail.OriginalTags.Split(",") : new List<string>();
        //    }
        //}
        //public IList<string> tags_split(string str)
        //{
        //    //todo
        //    var tag = str.Split("|").Where(w => w != "").Select(str => str.Split(":")[0]).ToList();

        //    //todo 過渡期のみ
        //    if (tag.Count == 1) tag = tag[0].Split(",").ToList(); 

        //    return tag;
        //}

    }
}
