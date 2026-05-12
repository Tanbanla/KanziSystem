using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class FileImportService : IFileImportService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public FileImportService(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
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
                var uploadFolder = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads", "quotes");
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

                var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? string.Empty).TrimEnd('/');
                var fileUrl = string.IsNullOrWhiteSpace(baseUrl) ? $"/uploads/quotes/{uniqueName}" : $"{baseUrl}/uploads/quotes/{uniqueName}";

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
    }
}
