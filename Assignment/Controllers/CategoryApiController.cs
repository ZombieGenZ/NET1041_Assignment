using Assignment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using String = System.String;

namespace Assignment.Controllers
{
    public class CategoryApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("api/categories")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Get([FromQuery] string? text)
        {
            try
            {
                var query = _context.Categories.AsQueryable();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    var searchTerms = text.Split('+', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var term in searchTerms)
                    {
                        var lowerCaseTerm = term.ToLower();

                        query = query.Where(c => c.Name.ToLower().Contains(lowerCaseTerm) || c.Description.ToLower().Contains(lowerCaseTerm));
                    }
                }

                return Json(query.ToList());
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "GET_CATEGORY_FAILURE",
                    message = e.Message
                });
            }
        }
        [HttpPost]
        [Route("api/categories")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Create([FromBody] CategoryBase category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    List<string> errors = new List<string>();
                    if (string.IsNullOrWhiteSpace(category.Name))
                    {
                        errors.Add("tên danh mục");
                    }

                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = $"Vui lòng nhập {string.Join(", ", errors)}"
                    });
                }

                if (category.Index < 1)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Độ ưu tiên không được bé hơn 0"
                    });
                }

                _context.Categories.Add(new Categories()
                {
                    Name = category.Name,
                    Description = category.Description ?? string.Empty,
                    Index = category.Index
                });
                _context.SaveChanges();

                return Json(new
                {
                    code = "CREATE_CATEGORY_SUCCESS",
                    message = "Tạo danh mục thành công",
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "CREATE_CATEGORY_FAILURE",
                    message = e.Message
                });
            }
        }
        [HttpPut]
        [Route("api/categories/{id:long}")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Update(long id, [FromBody] CategoryBase category)
        {
            try
            {
                var existingCategory = _context.Categories.FirstOrDefault(c => c.Id == id);
                if (existingCategory == null)
                {
                    return NotFound(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Danh mục không tồn tại"
                    });
                }

                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    List<string> errors = new List<string>();
                    if (string.IsNullOrWhiteSpace(category.Name))
                    {
                        errors.Add("tên danh mục");
                    }

                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = $"Vui lòng nhập {string.Join(", ", errors)}"
                    });
                }

                if (category.Index < 1)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Độ ưu tiên không được bé hơn 0"
                    });
                }

                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description ?? String.Empty;
                existingCategory.Index = category.Index;
                existingCategory.UpdatedAt = DateTime.Now;

                _context.SaveChanges();

                return Json(new
                {
                    code = "UPDATE_CATEGORY_SUCCESS",
                    message = "Cập nhật danh mục thành công",
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "UPDATE_CATEGORY_FAILURE",
                    message = e.Message
                });
            }
        }
        [HttpDelete]
        [Route("api/categories/{id:long}")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Delete(long id)
        {
            try
            {
                var existingCategory = _context.Categories.Include(categories => categories.Products).FirstOrDefault(c => c.Id == id);
                if (existingCategory == null)
                {
                    return NotFound(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Danh mục không tồn tại"
                    });
                }

                if (existingCategory.Products.Count > 0)
                {
                    return UnprocessableEntity(
                        new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Không thể xóa danh mục khi vẫn còn sản phẩm đang sử dụng danh mục"
                        });
                }

                _context.Categories.Remove(existingCategory);
                _context.SaveChanges();
                return Json(new
                {
                    code = "DELETE_CATEGORY_SUCCESS",
                    message = "Xoá danh mục thành công",
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "DELETE_CATEGORY_FAILURE",
                    message = e.Message
                });
            }
        }
    }
}
