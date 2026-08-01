using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaDetailService : BaseService<BaoGia_Detail_of_Quotation, int, BaoGia_Detail_of_QuotationDTO>, IBaoGiaDetailService
    {
        private readonly IBaoGiaDetailRepository _repo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public BaoGiaDetailService(IBaoGiaDetailRepository repo, IMapper mapper, IConfiguration configuration) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
            _configuration = configuration;
        }
        // Tìm kiếm thông tin liên quan đến báo giá
        public async Task<GenericResponse<ListRequest<dynamic>>> SearchBaoGiaAsync(int? idRequest, string? maDon, string? maVatTu, string? maNcc, string? section, string? user, DateTime? dayMM, int? PageSize, int? PageIndex)
        {
            var result = new GenericResponse<ListRequest<dynamic>>();
            try
            {
                var data = await _repo.SearchBaoGiaAsync(idRequest, maDon, maVatTu, maNcc, section, user, dayMM, PageSize, PageIndex);
                result.Data = data;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Insert danh sách báo giá
        public async Task<GenericResponse<bool>> InsertListBaoGiaDetailAsync(List<BaoGia_Detail_of_QuotationDTO> listDto)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var listModel = _mapper.Map<List<BaoGia_Detail_of_Quotation>>(listDto);
                var isSuccess = await _repo.InsertListBaoGiaDetailAsync(listModel);
                result.Data = isSuccess;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Update lua chon NCC
        public async Task<GenericResponse<bool>> UpdateLuaChonNCCBaoGiaDetailAsync(List<dynamic> listUp, string user, string name)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var isSuccess = await _repo.UpdateLuaChonNCCBaoGiaDetailAsync(listUp, user, name);
                result.Data = isSuccess;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;

            }
            return result;
        }
        // Lấy thông tin theo ID_RequestQuote
        public async Task<GenericResponse<BaoGia_Detail_of_QuotationDTO>> GetByIdRequestQuoteAsync(int idRequest)
        {
            var result = new GenericResponse<BaoGia_Detail_of_QuotationDTO>();
            try
            {
                var data = await _repo.GetByIdRequestQuoteAsync(idRequest);
                result.Data = _mapper.Map<BaoGia_Detail_of_QuotationDTO>(data);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Update list thông tin ghi nhập báo giá
        public async Task<GenericResponse<bool>> UpdateListThongTinNhapBaoGiaAsync(List<BaoGia_Detail_of_QuotationDTO> listDto)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var listModel = _mapper.Map<List<BaoGia_Detail_of_Quotation>>(listDto);
                result.Data = await _repo.UpdateListThongTinNhapBaoGiaAsync(listModel);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // lấy id của đơn báo giá
        public async Task<GenericResponse<int?>> GetIdOfQuotationAsync(string maDon, string maVatTu, string maNB, string maNcc, string NameHQ)
        {
            var result = new GenericResponse<int?>();
            try
            {
                result.Data = await _repo.GetIdOfQuotationAsync(maDon, maVatTu, maNB, maNcc, NameHQ);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // update thông tin lựa chọn nhà  cung cấp
        public async Task<GenericResponse<BaoGia_Request_of_Quotation>> UpdatePickSupplierDetailAsync(List<BaoGia_Detail_of_QuotationDTO> dtos, string userApproverNext, string userUpdate)
        {
            var result = new GenericResponse<BaoGia_Request_of_Quotation>();
            try
            {
                var data = _mapper.Map<List<BaoGia_Detail_of_Quotation>>(dtos);
                result.Data = await _repo.UpdatePickSupplierDetailAsync(data, userApproverNext, userUpdate);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Lấy id detail theo ID RequestQuote
        public async Task<GenericResponse<int>> GetIdDetailAsync(int? idRequest)
        {
            var result = new GenericResponse<int>();
            try
            {
                result.Data = await _repo.GetIdDetailAsync(idRequest);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Cập nhật thông tin status của đơn báo giá
        public async Task<GenericResponse<bool>> UpdateStatusAsync(List<int> ids)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.UpdateStatusAsync(ids);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }


        // Cập nhật thông tin link báo giá trên hệ thống
        public async Task<GenericResponse<bool>> UpdateLinkBaoGiaAsync()
        {
            var result = new GenericResponse<bool>();
            try
            {
                var dataNeedUpdate = await _repo.GetFilesToTransferAsync();
                if(dataNeedUpdate != null && dataNeedUpdate.Any())
                {
                    // lấy thông tin để lưu file nhập báo giá
                    var listInforFile = dataNeedUpdate.Select(c => c.Link).Distinct().ToList();

                    var savedMap = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                    foreach (var src in listInforFile)
                    {
                        if (savedMap.ContainsKey(src)) continue;
                        try
                        {
                            var saveRes = await SaveFileFromPathAsync(src);
                            if (saveRes != null && saveRes.Success && !string.IsNullOrWhiteSpace(saveRes.Data))
                            {
                                savedMap[src] = saveRes.Data;
                            }
                            else
                            {
                                savedMap[src] = null;
                            }
                        }
                        catch (Exception ex)
                        {

                            savedMap[src] = null;
                            continue;
                        }
                    }
                    foreach (var dto in dataNeedUpdate)
                    {
                        if (string.IsNullOrWhiteSpace(dto.Link)) continue;
                        if (savedMap.TryGetValue(dto.Link, out var saved) && !string.IsNullOrWhiteSpace(saved))
                        {
                            dto.Link = saved;
                        }
                        else
                        {
                            var checkKey = dto.Link?.Trim().Trim('"', '\'') ?? dto.Link;
                            if (savedMap.TryGetValue(checkKey, out saved) && !string.IsNullOrWhiteSpace(saved))
                            {
                                dto.Link = saved;
                            }
                        }

                    }

                    var isSuccess = await _repo.UpdateLinkBaoGiaAsync(dataNeedUpdate);

                    result.Data = isSuccess;
                    result.Success = isSuccess;
                    if (!isSuccess)
                    {
                        result.Message = "Update database failed";
                    }

                }
                else
                {
                    result.Message = "No files to update.";
                    result.Success = false;
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Function file
        private async Task<GenericResponse<string?>> SaveFileFromPathAsync(string sourcePath)
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
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".pdf", ".xlsx", ".xls", ".docx", ".doc",".msg" };
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
    }
}
