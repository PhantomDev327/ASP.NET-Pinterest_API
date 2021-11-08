using nxPinterest.Data;
using nxPinterest.Services.Interfaces;
using nxPinterest.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using nxPinterest.Data.Models;
using nxPinterest.Services.Models.Response;
using nxPinterest.Services.Extensions;
using System.Text.RegularExpressions;

namespace nxPinterest.Services
{
    public class UserMediaManagementService : IUserMediaManagementService
    {
        public ApplicationDbContext _context;

        public UserMediaManagementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IList<Data.Models.UserMedia>> ListUserMediaAsyc(string userId = "")
        {
            var query = (this._context.UserMedia.AsNoTracking()
                                     .Where(c => c.UserId.Equals(userId)));

            IList<Data.Models.UserMedia> userMediaList = await query.OrderByDescending(c => c.MediaId).ToListAsync();

            return userMediaList;
        }

        /// <summary>
        ///     Search Image by conditions
        /// </summary>
        /// <param name="searchKey"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IList<Data.Models.UserMedia>> SearchUserMediaAsync(string searchKey, string userId)
        {
            try
            {
                if (searchKey.Contains(" "))
                {
                    string[] listSearchKey = Regex.Split(searchKey.Trim(), "[ 　]+", RegexOptions.IgnoreCase);
                    if (listSearchKey.Length > 0)
                    {
                        var queries = this._context.UserMedia.AsNoTracking()
                            .Where(c => c.UserId.Equals(userId))
                            .Where(c => listSearchKey.Contains(c.Tags) ||
                            listSearchKey.Contains(c.MediaTitle));
                        IList<Data.Models.UserMedia> userMediaLists = await queries.OrderByDescending(c => c.MediaId).ToListAsync();
                        return userMediaLists;
                    }
                }
                var query = (this._context.UserMedia.AsNoTracking()
                                     .Where(c => c.UserId.Equals(userId))
                                     .Where(c => c.Tags.Contains(searchKey) || c.MediaTitle.Contains(searchKey)));
                IList<Data.Models.UserMedia> userMediaList = await query.OrderByDescending(c => c.MediaId).ToListAsync();
                return userMediaList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<UserMediaDetailViewModel> GetUserMediaDetailsByIDAsync(int media_id)
        {

            Data.Models.UserMedia userMedia = await (this._context.UserMedia.AsNoTracking()
                                             .FirstOrDefaultAsync(c => c.MediaId.Equals(media_id)));

            UserMediaDetailViewModel result = new UserMediaDetailViewModel();

            IList<UserMedia> mediaList = new List<UserMedia>();

            if (userMedia != null)
            {
                var query = await this._context.UserMedia.AsNoTracking().ToListAsync();

                query = query.Select(c => new UserMedia()
                {
                    MediaId = c.MediaId,
                    UserId = c.UserId,
                    MediaTitle = c.MediaTitle.TrimExtraSpaces(),
                    MediaDescription = c.MediaDescription.TrimExtraSpaces(),
                    MediaFileName = c.MediaFileName,
                    MediaFileType = c.MediaFileType,
                    MediaUrl = c.MediaUrl,
                    Tags = c.Tags,
                    MediaThumbnailUrl = c.MediaThumbnailUrl
                })
                .Where(c => c.MediaTitle.Equals(userMedia.MediaTitle.TrimExtraSpaces()) &&
                            c.MediaDescription.Equals(userMedia.MediaDescription.TrimExtraSpaces()))
                .ToList();

                mediaList = query;
            }

            result.UserMediaDetail = userMedia;
            result.UserMediaList = mediaList;

            return result;
        }

        public async Task DeleteFromUserMedia(UserMedia userMedia)
        {
            if (userMedia != null)
            {
                var userMediaList = await this._context.UserMedia.AsNoTracking()
                                         .Where(c => c.MediaFileName.Equals(userMedia.MediaFileName))
                                         .ToListAsync();


                this._context.UserMedia.RemoveRange(userMediaList);
                await this._context.SaveChangesAsync();
            }
        }

    }
}
