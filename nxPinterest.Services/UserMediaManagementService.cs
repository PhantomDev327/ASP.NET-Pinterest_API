using nxPinterest.Data;
using nxPinterest.Services.Interfaces;
using nxPinterest.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace nxPinterest.Services
{
    public class UserMediaManagementService : IUserMediaManagementService
    {
        public ApplicationDbContext _context;

        public UserMediaManagementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IList<Data.Models.UserMedia>> ListUserMediaAsyc(int pageIndex = 1, int pageSize = 5, string userId = "", string searchKey = "")
        {
            var query = (this._context.UserMedia.AsNoTracking()
                                     .Include(c => c.UserMediaThumbnails)
                                     .Where(c => c.UserId.Equals(userId)));

            if (!string.IsNullOrEmpty(searchKey))
            {
                query = query.Where(c => c.MediaTitle.Trim().ToLower().Contains(searchKey.Trim().ToLower()) ||
                                         c.MediaDescription.Trim().ToLower().Contains(searchKey.Trim().ToLower()));
            }

            IList<Data.Models.UserMedia> userMediaList = await query.OrderByDescending(c => c.MediaId)
                                                                    //.Skip((pageSize - 1) * pageSize)
                                                                    //.Take(pageSize)
                                                                    .ToListAsync();
            return userMediaList;
        }

        public async Task<Data.Models.UserMedia> GetUserMediaDetailsByIDAsync(int media_id) {

            Data.Models.UserMedia userMedia = await (this._context.UserMedia.AsNoTracking()
                                             .Include(c => c.UserMediaThumbnails)
                                             .FirstOrDefaultAsync(c => c.MediaId.Equals(media_id)));
            return userMedia;

        }

}
}
