using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment.Controllers
{
    public class VoucherController : Controller
    {
        [HttpGet]
        [Route("vouchers")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Route("vouchers/my-voucher")]
        [Authorize]
        public IActionResult MyVoucher()
        {
            return View();
        }
    }
}
