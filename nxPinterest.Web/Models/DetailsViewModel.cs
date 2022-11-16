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
        public string SearchKey { get; set; }
        public IList<string> TagList { get; set; }
        public IList<string> AlbumList { get; set; }

        public Data.Models.UserMedia UserMediaDetail { get; set; }
        public IList<Data.Models.UserMedia> SameTitleUserMediaList { get; set; }
        public IList<Data.Models.UserMedia> RelatedUserMediaList { get; set; }
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
    }
}
