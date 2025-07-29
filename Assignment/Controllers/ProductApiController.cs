using Assignment.Models;
using Assignment.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    public class ProductApiController : Controller
    {
        private readonly GeminiApiClient _geminiApiClient;
        public readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductApiController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, GeminiApiClient geminiApiClient)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _geminiApiClient = geminiApiClient;
        }
        [HttpGet]
        [Route("api/products")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Get([FromQuery] string? text)
        {
            try
            {
                var query = _context.Products.AsQueryable();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    var searchTerms = text.Split('+', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var term in searchTerms)
                    {
                        var lowerCaseTerm = term.ToLower();

                        query = query.Where(p => p.Name.ToLower().Contains(lowerCaseTerm) || p.Description.ToLower().Contains(lowerCaseTerm));
                    }
                }

                return Json(query.ToList());
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "GET_PRODUCT_FAILURE",
                    message = e.Message
                });
            }
        }
        [HttpPost]
        [Route("api/products")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Create([FromForm] ProductBase product)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(product.Name) || string.IsNullOrWhiteSpace(product.Description) || string.IsNullOrWhiteSpace(product.Ingredients))
                {
                    List<string> errors = new List<string>();
                    if (string.IsNullOrWhiteSpace(product.Name))
                    {
                        errors.Add("tên sản phẩm");
                    }

                    if (string.IsNullOrWhiteSpace(product.Description))
                    {
                        errors.Add("mô tả sản phẩm");
                    }

                    if (string.IsNullOrWhiteSpace(product.Ingredients))
                    {
                        errors.Add("thành phần sản phẩm");
                    }

                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = $"Vui lòng nhập {string.Join(", ", errors)}"
                    });
                }

                if (product?.ProductImage?.Length == 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Vui lòng chọn ảnh bìa sản phẩm"
                    });
                }

                if (!product.ProductImage.ContentType.StartsWith("image/"))
                {
                    return Json(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Chỉ cho phép tải lên các tệp tin ảnh"
                    });
                }

                if (product.Price < 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Vui lòng nhập giá sản phẩm lớn hơn hoặc bằng 0"
                    });
                }

                if (product.Stock <= 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Vui lòng nhập số lượng sản phẩm lớn hơn 0"
                    });
                }

                if (product.Discount < 0 || product.Discount > 100)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Vui lòng nhập mức giảm giá từ 0% đến 100%"
                    });
                }

                if (product.PreparationTime < 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Thời gian chuẩn bị sản phẩm không hợp lệ"
                    });
                }

                if (product.Calories < 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số calo của sản phẩm không hợp lệ"
                    });
                }

                var existCategory = _context.Categories.FirstOrDefault(c => c.Id == product.CategoryId);
                if (existCategory == null)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Danh mục không tồn tại"
                    });
                }

                Files? file = await FileApiController.Upload(_webHostEnvironment, Request, _context, product.ProductImage);

                if (file == null)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Không thể tải lên tệp tin"
                    });
                }

                MetaTagGenerator generator = new MetaTagGenerator
                {
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Discount = product.Discount,
                    PreparationTime = product.PreparationTime,
                    Calories = product.Calories,
                    Ingredients = product.Ingredients,
                    IsSpicy = product.IsSpicy,
                    IsVegetarian = product.IsVegetarian,
                    CategoryName = existCategory.Name,
                    CategoryDescription = existCategory.Description
                };

                string meta = await _geminiApiClient.GenerateContentAsync(generator.ToString());

                Products newProduct = new Products
                {
                    Name = product.Name,
                    Description = product.Description,
                    Path = UrlGenerator.GenerateUrl(_context, product.Name),
                    Price = product.Price,
                    Stock = product.Stock,
                    Discount = product.Discount,
                    IsPublish = product.IsPublish,
                    CategoryId = product.CategoryId,
                    ProductImageId = file.FileId,
                    ProductImageUrl = file.FileUrl,
                    PreparationTime = product.PreparationTime,
                    Calories = product.Calories,
                    Ingredients = product.Ingredients,
                    IsSpicy = product.IsSpicy,
                    IsVegetarian = product.IsVegetarian,
                    MetaTag = meta.Replace("```html", "").Replace("```", "")
                };
                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    code = "CREATE_PRODUCT_SUCCESS",
                    message = "Tạo sản phẩm thành công",
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    code = "CREATE_PRODUCT_FAILURE",
                    message = e.Message
                });
            }
        }
        [HttpPut]
        [Route("api/products/{id:long}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Update(long id, [FromForm] ProductBase product)
        {
            try
            {
                var existingProduct = _context.Products.FirstOrDefault(c => c.Id == id);
                if (existingProduct == null)
                {
                    return NotFound(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Sản phẩm không tồn tại"
                    });
                }

                if (string.IsNullOrWhiteSpace(product.Name) || string.IsNullOrWhiteSpace(product.Description) || string.IsNullOrWhiteSpace(product.Ingredients))
                {
                    List<string> errors = new List<string>();
                    if (string.IsNullOrWhiteSpace(product.Name))
                    {
                        errors.Add("tên sản phẩm");
                    }

                    if (string.IsNullOrWhiteSpace(product.Description))
                    {
                        errors.Add("mô tả sản phẩm");
                    }

                    if (string.IsNullOrWhiteSpace(product.Ingredients))
                    {
                        errors.Add("Thành phần sản phẩm");
                    }

                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = $"Vui lòng nhập {string.Join(", ", errors)}"
                    });
                }

                if (product.ProductImage != null && product.ProductImage.Length == 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Vui lòng chọn ảnh bìa sản phẩm"
                    });
                }

                if (product.ProductImage != null && !product.ProductImage.ContentType.StartsWith("image/"))
                {
                    return Json(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Chỉ cho phép tải lên các tệp tin ảnh"
                    });
                }

                if (product.Price < 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Vui lòng nhập giá sản phẩm lớn hơn hoặc bằng 0"
                    });
                }

                if (product.Stock < 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Vui lòng nhập số lượng sản phẩm lớn hơn hoặc bằng 0"
                    });
                }

                if (product.Discount < 0 || product.Discount > 100)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Vui lòng nhập mức giảm giá từ 0% đến 100%"
                    });
                }

                if (product.PreparationTime < 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Thời gian chuẩn bị sản phẩm không hợp lệ"
                    });
                }

                if (product.Calories < 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số calo của sản phẩm không hợp lệ"
                    });
                }

                var existCategory = _context.Categories.FirstOrDefault(c => c.Id == product.CategoryId);
                if (existCategory == null)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Danh mục không tồn tại"
                    });
                }

                if (product.ProductImage != null)
                {
                    Files? file = await FileApiController.Upload(_webHostEnvironment, Request, _context, product.ProductImage);

                    if (file == null)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Không thể tải lên tệp tin"
                        });
                    }

                    existingProduct.ProductImageId = file.FileId;
                    existingProduct.ProductImageUrl = file.FileUrl;
                }

                MetaTagGenerator generator = new MetaTagGenerator
                {
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Discount = product.Discount,
                    PreparationTime = product.PreparationTime,
                    Calories = product.Calories,
                    Ingredients = product.Ingredients,
                    IsSpicy = product.IsSpicy,
                    IsVegetarian = product.IsVegetarian,
                    CategoryName = existCategory.Name,
                    CategoryDescription = existCategory.Description
                };

                string meta = await _geminiApiClient.GenerateContentAsync(generator.ToString());

                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Path = UrlGenerator.GenerateUrl(_context, product.Name, existingProduct.Path);
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.Discount = product.Discount;
                existingProduct.IsPublish = product.IsPublish;
                existingProduct.PreparationTime = product.PreparationTime;
                existingProduct.Calories = product.Calories;
                existingProduct.Ingredients = product.Ingredients;
                existingProduct.IsSpicy = product.IsSpicy;
                existingProduct.IsVegetarian = product.IsVegetarian;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.MetaTag = meta.Replace("```html", "").Replace("```", "");
                existingProduct.UpdatedAt = DateTime.Now;
                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    code = "UPDATE_PRODUCT_SUCCESS",
                    message = "Cập nhật sản phẩm thành công",
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    code = "UPDATE_PRODUCT_FAILURE",
                    message = e.Message
                });
            }
        }
        [HttpDelete]
        [Route("api/products/{id:long}")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Delete(long id)
        {
            try
            {
                var existingProduct = _context.Products.Include(products => products.OrderDetails).FirstOrDefault(c => c.Id == id);
                if (existingProduct == null)
                {
                    return NotFound(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Sản phẩm không tồn tại"
                    });
                }

                if (existingProduct.OrderDetails.Count > 0)
                {
                    return UnprocessableEntity(
                        new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Không thể xóa sản phẩm khi vẫn còn đơn hàng đang sử dụng sản phẩm"
                        });
                }

                _context.Products.Remove(existingProduct);
                _context.SaveChanges();

                return Json(new
                {
                    code = "DELETE_PRODUCT_SUCCESS",
                    message = "Cập nhật sản phẩm thành công",
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    code = "DELETE_PRODUCT_FAILURE",
                    message = e.Message
                });
            }
        }

        [HttpPost]
        [Route("api/products/get-by-id")]
        public IActionResult GetByIds([FromBody] GetCartDataModel data)
        {
            try
            {
                if (data == null || data.ProductIds == null || !data.ProductIds.Any())
                {
                    return Json(new List<object>());
                }

                return Json(_context.Products.Where(p => data.ProductIds.Contains(p.Id)).ToList());
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "GET_PRODUCT_FAILURE",
                    message = e.Message
                });
            }
        }
    }
}
