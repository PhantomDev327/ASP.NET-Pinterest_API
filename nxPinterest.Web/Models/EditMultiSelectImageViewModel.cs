using System.Collections.Generic;

namespace nxPinterest.Web.Models
{
    public class EditMultiSelectImageViewModel : BaseViewModel
    {
        public int DetailsMediaId { get; set; }
        public IList<Data.Models.UserMedia> UserMediaList { get; set; } = new List<Data.Models.UserMedia>();
    }

}
