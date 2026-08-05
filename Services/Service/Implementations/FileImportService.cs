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

            var candidates = new List<(string Value, bool IsQuoted)>();
            var quoteMatches = System.Text.RegularExpressions.Regex.Matches(sourcePath, "\"([^\"]+)\"|'([^']+)'");
            foreach (System.Text.RegularExpressions.Match match in quoteMatches)
            {
                var value = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    candidates.Add((value.Trim(), true));
                }
            }

            var raw = sourcePath.Trim().Trim('"', '\'');
            if (!string.IsNullOrWhiteSpace(raw))
            {
                candidates.Add((raw, false));
            }

            var parts = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                           .Select(p => p.Trim().Trim('"', '\''));
            foreach (var part in parts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    candidates.Add((part, false));
                }
            }

            string? found = null;
            string? matchedInputPath = null;
            var matchedFromQuoted = false;
            foreach (var candidate in candidates)
            {
                var p = candidate.Value.Replace("/", "\\").Trim();
                if (p.StartsWith("\\") && !p.StartsWith("\\\\"))
                {
                    p = "\\" + p;
                }
                if (File.Exists(p))
                {
                    found = p;
                    matchedInputPath = candidate.Value;
                    matchedFromQuoted = candidate.IsQuoted;
                    break;
                }
            }

            if (found == null) return null;

            var extension = Path.GetExtension(found).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".pdf", ".xlsx", ".xls", ".docx", ".doc", ".msg" };
            if (!allowedExtensions.Contains(extension)) return null;

            try
            {
                var uploadFolder = (_configuration["ApiSettings:BaseUpload"] ?? string.Empty).TrimEnd('/');
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

                var onlyQuotedPath = System.Text.RegularExpressions.Regex.IsMatch(sourcePath, "^\\s*([\"']).*\\1\\s*$");
                if (matchedFromQuoted && !string.IsNullOrWhiteSpace(matchedInputPath) && !onlyQuotedPath)
                {
                    result.Data = ReplaceFirstIgnoreCase(sourcePath, matchedInputPath, fileUrl);
                }
                else
                {
                    result.Data = fileUrl;
                }

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

        private static string ReplaceFirstIgnoreCase(string input, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(oldValue)) return input;

            var index = input.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
            if (index < 0) return input;

            return string.Concat(input.AsSpan(0, index), newValue, input.AsSpan(index + oldValue.Length));
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
