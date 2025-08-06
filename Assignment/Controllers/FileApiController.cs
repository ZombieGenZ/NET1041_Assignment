using Assignment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    public class FileApiController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly ApplicationDbContext _context;
        private readonly FileExtensionContentTypeProvider _contentTypeProvider;

        public FileApiController(IWebHostEnvironment webHostEnvironment, ApplicationDbContext context)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
            _contentTypeProvider = new FileExtensionContentTypeProvider();
        }

        [HttpPost]
        [Route("api/files")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Uploads(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new
                {
                    code = "NOT_FILE_UPLOAD",
                    message = "Không được để trống tệp tin gửi lên"
                });
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                return Json(new
                {
                    code = "INVALID_FILE_TYPE",
                    message = "Chỉ cho phép tải lên các tệp tin ảnh"
                });
            }

            try
            {
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string originalFileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
                string fileExtension = Path.GetExtension(file.FileName);

                string uniqueId = Guid.NewGuid().ToString();

                string uniqueFileName = $"{originalFileNameWithoutExtension}-{uniqueId}{fileExtension}";
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string fileId = Guid.NewGuid().ToString();
                string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                string fileUrl = $"{baseUrl}/uploads/{uniqueFileName}";
                long fileSizeInBytes = file.Length;
                double fileSizeInKb = (double)fileSizeInBytes / 1024;

                await _context.Files.AddAsync(new Files()
                {
                    FileId = fileId,
                    FileName = uniqueFileName,
                    FileUrl = fileUrl,
                    FileType = fileExtension,
                    FileAngleName = file.FileName,
                    FilePath = filePath,
                    FileSize = fileSizeInKb
                });
                await _context.SaveChangesAsync();

                return Json(new
                {
                    code = "UPLOAD_SUCCESS",
                    url = fileUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "UPLOAD_FAILURE",
                    message = ex.Message
                });
            }
        }

        [NonAction]
        public static async Task<Files?> Upload(IWebHostEnvironment webHostEnvironment, HttpRequest request, ApplicationDbContext context, IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                return null;
            }

            try
            {
                string uploadFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string originalFileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
                string fileExtension = Path.GetExtension(file.FileName);

                string uniqueId = Guid.NewGuid().ToString();

                string uniqueFileName = $"{originalFileNameWithoutExtension}-{uniqueId}{fileExtension}";
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string fileId = Guid.NewGuid().ToString();
                string baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
                string fileUrl = $"{baseUrl}/uploads/{uniqueFileName}";
                long fileSizeInBytes = file.Length;
                double fileSizeInKb = (double)fileSizeInBytes / 1024;

                Files newFile = new Files()
                {
                    FileId = fileId,
                    FileName = uniqueFileName,
                    FileUrl = fileUrl,
                    FileType = fileExtension,
                    FileAngleName = file.FileName,
                    FilePath = filePath,
                    FileSize = fileSizeInKb
                };
                await context.Files.AddAsync(newFile);
                await context.SaveChangesAsync();

                return newFile;
            }
            catch
            {
                return null;
            }
        }

        //[HttpDelete]
        //[Route("api/files/{id}")]
        //public async Task<IActionResult> Delete(string id)
        //{
        //    try
        //    {
        //        var fileRecord = await _context.Files.FirstOrDefaultAsync(f => f.FileId == id);

        //        if (fileRecord == null)
        //        {
        //            return Json(new
        //            {
        //                code = "FILE_NOT_FOUND",
        //                message = $"Không tìm thấy tệp tin với ID: {id}"
        //            });
        //        }

        //        string filePath = fileRecord.FilePath;

        //        if (System.IO.File.Exists(filePath))
        //        {
        //            System.IO.File.Delete(filePath);
        //        }

        //        _context.Files.Remove(fileRecord);
        //        await _context.SaveChangesAsync();

        //        return Json(new
        //        {
        //            code = "DELETE_SUCCESS",
        //            message = $"Tệp tin '{fileRecord.FileAngleName}' đã được xóa thành công!"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error deleting file: {ex.Message}");
        //        return StatusCode(500, new
        //        {
        //            code = "DELETE_FAILURE",
        //            message = ex.Message
        //        });
        //    }
        //}
    }
}
