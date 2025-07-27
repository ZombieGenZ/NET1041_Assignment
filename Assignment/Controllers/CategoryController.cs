using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment.Controllers
{
    public class CategoryController : Controller
    {
        [HttpGet]
        [Route("category")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
