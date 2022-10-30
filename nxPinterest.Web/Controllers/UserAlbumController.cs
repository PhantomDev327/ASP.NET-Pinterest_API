using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace nxPinterest.Web.Controllers
{
    [Authorize]
    public class UserAlbumController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
