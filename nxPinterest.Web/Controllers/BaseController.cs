using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

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

        protected string GeneratePathUrl()
        {
            // random path url
            return $"{Guid.NewGuid().ToString().Replace("-", "")}{DateTime.Now.ToString("yyyyMMddhhmmsss")}";
        }
    }
}
