using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nxPinterest.Data.Models;

namespace nxPinterest.Services.Models.Response
{
    /// <summary>
    /// 選択イメージの詳細モデル
    /// スキーマモデル＋同タイトルのリスト＋似ている画像のリスト
    /// </summary>
    public class UserMediaDetailModel
    {
        public UserMedia UserMediaDetail { get; set; }
        public IList<UserMedia> SameTitleUserMediaList { get; set; }
        public IList<UserMedia> RelatedUserMediaList { get; set; }
        //public IList<string> project_tags_list { get; set; }
    }
}
