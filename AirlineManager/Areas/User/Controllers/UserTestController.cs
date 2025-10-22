using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirlineManager.Areas.User.Controllers
{
    [Area("User")]
    [AllowAnonymous]
    public class UserTestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
