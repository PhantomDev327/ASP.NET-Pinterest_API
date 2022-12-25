using System;
using System.Collections.Generic;
using System.Linq;

namespace nxPinterest.Web.Models
{
    /// <summary>
    /// イメージ詳細ページのViewModel
    /// </summary>
    public class DetailsViewModel : BaseViewModel
    {

        public Data.Models.UserMedia UserMediaDetail { get; set; }
        public IList<Data.Models.UserMedia> SameTitleUserMediaList { get; set; }
        public IList<Data.Models.UserMedia> RelatedUserMediaList { get; set; }
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }

        public IList<string> OriginalTagsList
        {
            get
            {
                if (!string.IsNullOrEmpty(UserMediaDetail.OriginalTags) &&  UserMediaDetail.OriginalTags.Split("|").Count() > 0)
                    return UserMediaDetail.OriginalTags.Split(",").Where(w => w != "").ToList();
                else
                    return new List<string>();                
            }
        }
        public IList<string> AITagsList
        {
            get
            {
                if (!string.IsNullOrEmpty(UserMediaDetail.AITags) && UserMediaDetail.AITags.Split("|").Count() > 0)
                    return UserMediaDetail.AITags.Split(",").Where(w => w != "").ToList();
                else
                    return new List<string>();                
            }
        }
        public IList<string> FullTagsList
        {
            get
            {
                if (!string.IsNullOrEmpty(UserMediaDetail.Tags) && UserMediaDetail.Tags.Split("|").Count() > 0 && UserMediaDetail.Tags.Contains(":"))
                    return UserMediaDetail.Tags.Split("|").Where(w => w != "").Select(str => str.Split(":")[0] + "(" + str.Split(":")[1] + ")").ToList();
                else
                    return new List<string>();
            }
        }
    }
}
