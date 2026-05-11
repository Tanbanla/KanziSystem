using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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

        public async Task<string?> SaveFileFromPathAsync(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath)) return null;

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
                    // ensure UNC style \\\server\share -> \\\\server\share
                    p = "\\" + p;
                }

                if (File.Exists(p))
                {
                    found = p;
                    break;
                }

                // try to expand mapped drive letter with environment variable? skip for now
            }

            if (found == null) return null;

            try
            {
                var uploadFolder = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads", "quotes");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                var fileName = Path.GetFileName(found) ?? (Guid.NewGuid().ToString() + ".dat");
                var uniqueName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}_{fileName}";
                var dest = Path.Combine(uploadFolder, uniqueName);

                // Use synchronous copy (fast) but provide Task wrapper
                File.Copy(found, dest, overwrite: false);

                var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? string.Empty).TrimEnd('/');
                var fileUrl = string.IsNullOrWhiteSpace(baseUrl) ? $"/uploads/quotes/{uniqueName}" : $"{baseUrl}/uploads/quotes/{uniqueName}";
                return await Task.FromResult(fileUrl);
            }
            catch
            {
                return null;
            }
        }
    }
}
