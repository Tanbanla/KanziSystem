using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class FileImportService : IFileImportService
    {
        private readonly IBaoGiaDetailRepository _repoDetail;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public FileImportService(IWebHostEnvironment env, IConfiguration configuration, IBaoGiaDetailRepository repoDetail)
        {
            _env = env;
            _configuration = configuration;
            _repoDetail = repoDetail;
        }

        public async Task<GenericResponse<string?>> SaveFileFromPathAsync(string sourcePath)
        {
            var result = new GenericResponse<string?>();


            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                result.Success = false;
                result.Message = "Source path is empty.";
                return result;
            }
            ;

            // Normalize and split by comma if multiple paths provided
            var raw = sourcePath.Trim();
            raw = raw.Trim('"', '\'');
            var parts = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(p => p.Trim().Trim('"', '\''))
                            .ToList();

            string? found = null;
            foreach (var p0 in parts)
            {
                var p = p0.Replace("/", "\\").Trim();
                if (p.StartsWith("\\") && !p.StartsWith("\\\\"))
                {
                    p = "\\" + p;
                }

                if (File.Exists(p))
                {
                    found = p;
                    break;
                }
            }

            if (found == null) return null;

            var extension = Path.GetExtension(found).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".pdf", ".xlsx", ".xls", ".docx", ".doc" };
            if (!allowedExtensions.Contains(extension)) return null;

            try
            {
                var uploadFolder = (_configuration["ApiSettings:BaseUpload"] ?? string.Empty).TrimEnd('/'); ;
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                var fileName = Path.GetFileName(found) ?? (Guid.NewGuid().ToString() + ".dat");
                var uniqueName = $"{DateTime.Now:yyyyMMdd}_{Guid.NewGuid():N}_{fileName}";
                var dest = Path.Combine(uploadFolder, uniqueName);

                // Use asynchronous copy if possible, but for local files, synchronous is fine wrapped in Task
                using (var sourceStream = File.OpenRead(found))
                using (var destStream = new FileStream(dest, FileMode.Create))
                {
                    await sourceStream.CopyToAsync(destStream);
                }

                var baseUrl = (_configuration["ApiSettings:BaseUpload"] ?? string.Empty).TrimEnd('/');
                var fileUrl = string.IsNullOrWhiteSpace(baseUrl) ? $"/uploads/quotes/{uniqueName}" : $"{baseUrl}/{uniqueName}";

                result.Data = fileUrl;
                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
        }
        // Lấy file từ link 
        public async Task<GenericResponse<IFormFile>> GetFileToLinkAsync(string filePath)
        {
            var result = new GenericResponse<IFormFile>();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                result.Success = false;
                result.Message = "File path is empty.";
                return result;
            }

            try
            {
                var raw = filePath.Trim().Trim('"', '\'');
                string fileNameOnly = Path.GetFileName(raw);
                string uploadFolder = (_configuration["ApiSettings:BaseUpload"] ?? string.Empty).TrimEnd('/', '\\');

                string physicalPath = raw;

                if (raw.StartsWith("\\\\") || raw.StartsWith("//"))
                {
                    physicalPath = raw.Replace('/', Path.DirectorySeparatorChar);
                }
                else if (raw.StartsWith("/"))
                {
                    if (!string.IsNullOrWhiteSpace(uploadFolder))
                    {
                        physicalPath = Path.Combine(uploadFolder, fileNameOnly);
                    }
                    else
                    {
                        var webRoot = _env.WebRootPath ?? string.Empty;
                        var trimmed = raw.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                        physicalPath = Path.Combine(webRoot, trimmed);
                    }
                }

                physicalPath = physicalPath.Replace('/', Path.DirectorySeparatorChar);

                if (!File.Exists(physicalPath))
                {
                    result.Success = false;
                    result.Message = "File not found.";
                    return result;
                }

                var ms = new MemoryStream();
                using (var fs = File.OpenRead(physicalPath))
                {
                    await fs.CopyToAsync(ms);
                }
                ms.Position = 0;

                var localFileName = Path.GetFileName(physicalPath);
                var localFormFile = new FormFile(ms, 0, ms.Length, "file", localFileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/octet-stream"
                };

                result.Data = localFormFile;
                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
        }
    }
}
