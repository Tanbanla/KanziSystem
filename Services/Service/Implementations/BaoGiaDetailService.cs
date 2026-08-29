using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Master;
using System;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaDetailService : BaseService<BaoGia_Detail_of_Quotation, int, BaoGia_Detail_of_QuotationDTO>, IBaoGiaDetailService
    {
        private readonly IBaoGiaDetailRepository _repo;
        private readonly IMapper _mapper;
        private readonly IFileImportService _fileImportService;
        public BaoGiaDetailService(IBaoGiaDetailRepository repo, IMapper mapper, IConfiguration configuration, IFileImportService fileImportService) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
            _fileImportService = fileImportService;
        }
        // Tìm kiếm thông tin liên quan đến báo giá
        public async Task<GenericResponse<ListRequest<dynamic>>> SearchBaoGiaAsync(int? idRequest, string? maDon, string? maVatTu, string? maNcc, string? section, string? user, DateTime? dayMM, string? status, int? PageSize, int? PageIndex)
        {
            var result = new GenericResponse<ListRequest<dynamic>>();
            try
            {
                var data = await _repo.SearchBaoGiaAsync(idRequest, maDon, maVatTu, maNcc, section, user, dayMM, status, PageSize, PageIndex);
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
                            var saveRes = await _fileImportService.SaveFileFromPathAsync(src);
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
        // Lấy file thông tin đã nhập lên hệ thống
        public async Task<GenericResponse<IFormFile>> GetFilesToImportAsync(string keywork)
        {
            var result = new GenericResponse<IFormFile>();
            try
            {
                var data = await _repo.GetFilesToImportAsync(keywork);
                if (data == null || data.Count == 0)
               {
                    throw new Exception("Không tìm thấy dữ liệu");
               }

                var templatePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "template",
                    "TmSendMail_ConfirmName.xlsx");
                var memoryStream = new MemoryStream();

                using (var workbook = new XLWorkbook(templatePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    worksheet.Column(30).Hide();

                    int rowIndex = 13;
                    foreach (var item in data)
                    {
                        var otherRequestList = new List<string>();
                        if (!string.IsNullOrEmpty((string?)item.NVCHR_Rohs))
                            otherRequestList.Add($"ROHS: {item.NVCHR_Rohs}");
                        if (!string.IsNullOrEmpty((string?)item.NVCHR_COCQ))
                            otherRequestList.Add($"COCQ: {item.NVCHR_COCQ}");
                        if (!string.IsNullOrEmpty((string?)item.NVCHR_MSDS))
                            otherRequestList.Add($"MSDS: {item.NVCHR_MSDS}");
                        if (!string.IsNullOrEmpty((string?)item.NVCHR_AnToan))
                            otherRequestList.Add($"An toàn: {item.NVCHR_AnToan}");

                        string otherRequest = string.Join(" & ", otherRequestList);

                        worksheet.Cell(1, 3).Value = (string?)item.NVCHR_NameNCC ?? string.Empty;
                        worksheet.Cell(2, 3).Value = (string?)item.Diachi ?? string.Empty;

                        worksheet.Cell(rowIndex, 1).Value = (string?)item.CHR_MaDon ?? string.Empty;
                        worksheet.Cell(rowIndex, 2).Value = (string?)item.CHR_MaThietBi ?? string.Empty;
                        worksheet.Cell(rowIndex, 3).Value = (string?)item.CHR_MaHangNoiBo ?? string.Empty;
                        worksheet.Cell(rowIndex, 4).Value = (string?)item.BivnMaHang ?? string.Empty;
                        worksheet.Cell(rowIndex, 5).Value = (string?)item.VCHR_TenHaiQuan ?? string.Empty;
                        worksheet.Cell(rowIndex, 6).Value = item.SoluongQ ?? string.Empty;
                        worksheet.Cell(rowIndex, 7).Value = (string?)item.DonViQ ?? string.Empty;
                        worksheet.Cell(rowIndex, 8).Value = otherRequest;
                        worksheet.Cell(rowIndex, 9).Value = string.Empty;
                        worksheet.Cell(rowIndex, 10).Value = (string?)item.CHR_CodeNCC ?? string.Empty;
                        worksheet.Cell(rowIndex, 11).Value = (string?)item.VendorMaHang ?? string.Empty;
                        worksheet.Cell(rowIndex, 12).Value = (string?)item.NVCHR_TenHangHQ ?? string.Empty;
                        worksheet.Cell(rowIndex, 13).Value = (string?)item.CHR_NameEN ?? string.Empty;
                        worksheet.Cell(rowIndex, 14).Value = item.SoluongNcc ?? string.Empty;
                        worksheet.Cell(rowIndex, 15).Value = (string?)item.DonViNcc ?? string.Empty;
                        worksheet.Cell(rowIndex, 16).Value = item.FL_USD ?? item.CHR_Status ?? string.Empty;
                        worksheet.Cell(rowIndex, 17).Value = item.FL_VND ?? item.CHR_Status ?? string.Empty;
                        worksheet.Cell(rowIndex, 18).Value = (string?)item.NVCHR_MOQ ?? string.Empty;
                        worksheet.Cell(rowIndex, 19).Value = (string?)item.NVCHR_Packing ?? string.Empty;
                        worksheet.Cell(rowIndex, 20).Value = (string?)item.DTM_LeadTime ?? string.Empty;
                        worksheet.Cell(rowIndex, 21).Value = item.DTM_ShipTime?.ToString("yyyy-MM-dd") ?? string.Empty; ///
                        worksheet.Cell(rowIndex, 22).Value = (string?)item.VCHR_CamKet ?? string.Empty;
                        worksheet.Cell(rowIndex, 23).Value = (string?)item.NVCHR_DeliveryTerm ?? string.Empty;
                        worksheet.Cell(rowIndex, 24).Value = (string?)item.NVCHR_PaymentTerm ?? string.Empty;
                        worksheet.Cell(rowIndex, 25).Value = item.DTM_EffectiveDate?.ToString("yyyy-MM-dd") ?? string.Empty;
                        worksheet.Cell(rowIndex, 26).Value = item.DTM_EffectiveDate?.ToString("yyyy-MM-dd") ?? string.Empty;
                        worksheet.Cell(rowIndex, 27).Value = (string?)item.NVCHR_FileThietKe ?? string.Empty;
                        worksheet.Cell(rowIndex, 28).Value = item.DTM_NgayMuonNhan?.ToString("yyyy-MM-dd") ?? string.Empty;
                        worksheet.Cell(rowIndex, 29).Value = item.DTM_Deadline?.ToString("yyyy-MM-dd") ?? string.Empty;
                        worksheet.Cell(rowIndex, 30).Value = (string?)item.NVCHR_File ?? string.Empty;
                        worksheet.Cell(rowIndex, 31).Value = (string?)item.NVCHR_UserRequest ?? string.Empty;
                        worksheet.Cell(rowIndex, 32).Value = item.ID ?? string.Empty;

                        // Tô màu dòng
                        var rowRange = worksheet.Range(rowIndex, 1, rowIndex, 32);

                        if (item.BIT_Select)
                        {
                            rowRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
                        }

                        if (string.Equals((string?)item.CHR_Status, "Refuse",StringComparison.OrdinalIgnoreCase))
                        {
                            rowRange.Style.Fill.BackgroundColor = XLColor.LightPink;
                            rowRange.Style.Font.FontColor = XLColor.DarkRed;
                        }

                        rowIndex++;
                    }

                    workbook.SaveAs(memoryStream);
                }

                memoryStream.Position = 0;

                var fileName = $"ResultRQ_{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";
                var localFormFile = new FormFile(memoryStream, 0, memoryStream.Length, "file", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                };

                result.Data = localFormFile;
                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;

        }
    }
}
