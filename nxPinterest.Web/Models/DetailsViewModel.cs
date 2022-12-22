﻿using System;
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
                return UserMediaDetail.OriginalTags.Split(",").Where(w => w != "").ToList();
            }
        }
        public IList<string> AITagsList
        {
            get
            {
                return UserMediaDetail.AITags.Split(",").Where(w => w != "").ToList();
            }
        }
        public IList<string> FullTagsList
        {
            get
            {
                return UserMediaDetail.Tags.Split("|").Where(w => w != "").Select(str => str.Split(":")[0] + "(" + str.Split(":")[1] + ")").ToList();
            }
        }
    }
}
