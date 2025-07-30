using Assignment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment.Controllers
{
    public class RefundApiController : Controller
    {
        private readonly ApplicationDbContext _context;
        public RefundApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPut]
        [Route("api/refund/{id:long}")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Complete(long id)
        {
            try
            {
                var extingRefund = _context.Refunds.FirstOrDefault(r => r.Id == id && r.Status == RefundStatusEnum.Pending);

                if (extingRefund == null)
                {
                    return NotFound();
                }

                extingRefund.Status = RefundStatusEnum.Complete;
                extingRefund.UpdatedAt = DateTime.Now;
                _context.Refunds.Update(extingRefund);
                _context.SaveChanges();

                return Json(new
                {
                    code = "COMPLETE_REFUND_SUCCESS",
                    message = "Hoàn thành hoàn tiền thành công"
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    code = "COMPLETE_REFUND_FAILURE",
                    message = e.Message
                });
            }
        }
        [HttpDelete]
        [Route("api/refund/{id:long}")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Refuse(long id)
        {
            try
            {
                var extingRefund = _context.Refunds.FirstOrDefault(r => r.Id == id && r.Status == RefundStatusEnum.Pending);

                if (extingRefund == null)
                {
                    return NotFound();
                }

                extingRefund.Status = RefundStatusEnum.Refuse;
                extingRefund.UpdatedAt = DateTime.Now;
                _context.Refunds.Update(extingRefund);
                _context.SaveChanges();

                return Json(new
                {
                    code = "REFUSE_REFUND_SUCCESS",
                    message = "Từ chối hoàn tiền thành công"
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    code = "REFUSE_REFUND_FAILURE",
                    message = e.Message
                });
            }
        }
    }
}
