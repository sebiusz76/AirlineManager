using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirlineManager.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "User, Moderator, Admin, SuperAdmin")]
    public class UserTestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}