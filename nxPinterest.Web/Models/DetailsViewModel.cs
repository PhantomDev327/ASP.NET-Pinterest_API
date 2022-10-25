using System;
using System.Collections.Generic;

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

        public IList<string> project_tags_list {
            get
            {
                return UserMediaDetail.ProjectTags != null ? UserMediaDetail.ProjectTags.Split(",") : new List<string>();
            }
        }
    }
}
