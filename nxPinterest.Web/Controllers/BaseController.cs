using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Extensions;

namespace nxPinterest.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        public BaseController() { 
        
        }

        protected string UserId
        {
            get {
                return User.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
        }

        protected string GenerateUrl(int albumId)
        {
            if (albumId == 0) return null;

            var request = HttpContext.Request;

            var fomatUrl = $"{request.GetDisplayUrl()}/shared/${Guid.NewGuid().ToString().Replace("-", "")}-{albumId}";

            return fomatUrl;
        }
    }
}
