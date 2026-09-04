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

            var uncMatches = System.Text.RegularExpressions.Regex.Matches(sourcePath, @"\\\\[^""]+");
            foreach (System.Text.RegularExpressions.Match match in uncMatches)
            {
                var value = match.Value.Trim().Trim('"', '\'');
                if (!string.IsNullOrWhiteSpace(value))
                {
                    candidates.Add((value, true));
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

            var savedFiles = new List<(string InputPath, string FileUrl)>();
            var processedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".pdf", ".xlsx", ".xls", ".docx", ".doc", ".msg", ".eml" };

            foreach (var candidate in candidates)
            {
                var p = candidate.Value.Replace("/", "\\").Trim();
                if (p.StartsWith("\\") && !p.StartsWith("\\\\"))
                {
                    p = "\\" + p;
                }

                if (!File.Exists(p) || !processedPaths.Add(p))
                {
                    continue;
                }

                var extension = Path.GetExtension(p).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension)) continue;

                try
                {
                    var uploadFolder = (_configuration["ApiSettings:BaseUpload"] ?? string.Empty).TrimEnd('/');
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                    var fileName = Path.GetFileName(p) ?? (Guid.NewGuid().ToString() + ".dat");
                    var uniqueName = $"{DateTime.Now:yyyyMMdd}_{Guid.NewGuid():N}_{fileName}";
                    var dest = Path.Combine(uploadFolder, uniqueName);

                    using (var sourceStream = File.OpenRead(p))
                    using (var destStream = new FileStream(dest, FileMode.Create))
                    {
                        await sourceStream.CopyToAsync(destStream);
                    }

                    var baseUrl = (_configuration["ApiSettings:BaseUpload"] ?? string.Empty).TrimEnd('/');
                    var fileUrl = string.IsNullOrWhiteSpace(baseUrl) ? $"/uploads/quotes/{uniqueName}" : $"{baseUrl}/{uniqueName}";
                    savedFiles.Add((candidate.Value, fileUrl));
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                    return result;
                }
            }

            if (savedFiles.Count == 0) return null;

            var onlyQuotedPath = quoteMatches.Count == 1 &&
                System.Text.RegularExpressions.Regex.IsMatch(sourcePath, "^\\s*([\"']).*\\1\\s*$");
            if (onlyQuotedPath && savedFiles.Count == 1)
            {
                result.Data = savedFiles[0].FileUrl;
            }
            else
            {
                result.Data = sourcePath;
                foreach (var savedFile in savedFiles)
                {
                    result.Data = ReplaceAllIgnoreCase(result.Data, savedFile.InputPath, savedFile.FileUrl);
                }
            }

            result.Success = true;
            return result;
        }
       
        private static string ReplaceFirstIgnoreCase(string input, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(oldValue)) return input;

            var index = input.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
            if (index < 0) return input;

            return string.Concat(input.AsSpan(0, index), newValue, input.AsSpan(index + oldValue.Length));
        }

        private static string ReplaceAllIgnoreCase(string input, string oldValue, string newValue)
        {
            var result = input;
            var startIndex = 0;

            while (startIndex < result.Length)
            {
                var index = result.IndexOf(oldValue, startIndex, StringComparison.OrdinalIgnoreCase);
                if (index < 0) break;

                result = string.Concat(result.AsSpan(0, index), newValue, result.AsSpan(index + oldValue.Length));
                startIndex = index + newValue.Length;
            }

            return result;
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
                string uploadFolder = (_configuration["ApiSettings:BaseUpload"] ?? string.Empty).TrimEnd('/', '\\');
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".pdf", ".xlsx", ".xls", ".docx", ".doc", ".msg", ".eml" };
                var inputPaths = new List<string>();

                var quoteMatches = System.Text.RegularExpressions.Regex.Matches(filePath, "\"([^\"]+)\"|'([^']+)'");
                foreach (System.Text.RegularExpressions.Match match in quoteMatches)
                {
                    var value = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                    if (!string.IsNullOrWhiteSpace(value)) inputPaths.Add(value.Trim());
                }

                var uncMatches = System.Text.RegularExpressions.Regex.Matches(filePath, @"\\\\[^""]+");
                foreach (System.Text.RegularExpressions.Match match in uncMatches)
                {
                    var value = match.Value.Trim().Trim('"', '\'');
                    if (!string.IsNullOrWhiteSpace(value)) inputPaths.Add(value);
                }

                if (inputPaths.Count == 0)
                {
                    inputPaths.Add(filePath.Trim().Trim('"', '\''));
                }

                var files = new List<(string PhysicalPath, string FileName)>();
                var processedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var inputPath in inputPaths)
                {
                    var raw = inputPath.Trim().Trim('"', '\'');
                    var fileNameOnly = Path.GetFileName(raw);
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

                    if (!File.Exists(physicalPath) || !processedPaths.Add(physicalPath)) continue;

                    var extension = Path.GetExtension(physicalPath).ToLowerInvariant();
                    if (allowedExtensions.Contains(extension))
                    {
                        files.Add((physicalPath, Path.GetFileName(physicalPath)));
                    }
                }

                if (files.Count == 0)
                {
                    result.Success = false;
                    result.Message = "File not found or file type is not supported.";
                    return result;
                }

                var ms = new MemoryStream();
                string localFileName;
                string contentType;

                if (files.Count == 1)
                {
                    using (var fs = File.OpenRead(files[0].PhysicalPath))
                    {
                        await fs.CopyToAsync(ms);
                    }

                    localFileName = files[0].FileName;
                    contentType = "application/octet-stream";
                }
                else
                {
                    using (var archive = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Create, true))
                    {
                        foreach (var file in files)
                        {
                            var entry = archive.CreateEntry(file.FileName, System.IO.Compression.CompressionLevel.Fastest);
                            await using var entryStream = entry.Open();
                            await using var fileStream = File.OpenRead(file.PhysicalPath);
                            await fileStream.CopyToAsync(entryStream);
                        }
                    }

                    localFileName = "quotation_files.zip";
                    contentType = "application/zip";
                }

                ms.Position = 0;
                var localFormFile = new FormFile(ms, 0, ms.Length, "file", localFileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = contentType
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
