using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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
    }
}
