using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using System.IO.Pipelines;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class SendMailService : BaseService<TM_MASTER_MAIL, int ,TM_MASTER_MAILDTO>, ISendMailService
    {
        private readonly ISendMailRepository _repo;
        private readonly IMapper _mapper;
        public SendMailService(ISendMailRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repo = repository;
            _mapper = mapper;
        }
        // Gửi mail
        public async Task<GenericResponse<bool>> SendMailAsync(string toEmail, string ccEmail, int idMail, string? url, bool? isGap, string? section, string? idRequest, string? user)
        {
           var mail = await _repo.GetMailByIdAsync(idMail);
           if (mail == null)
           {
               return new GenericResponse<bool>
               {
                   Success = false,
                   Message = "Mail template not found"
               };
           }

           // Prepare body with parameters
           string gapText = isGap.HasValue && isGap.Value ? "Có" : "Không";
           string body = string.Format(mail.CHR_BODY, url, gapText, section, idRequest,user);

           bool sendResult = EmailSender.sendEmailNotify(
               mail.CHR_SUBJECT,
               mail.CHR_FROM,
               toEmail,
               ccEmail,
               mail.CHR_BCC,
               body,
               0 // Default priority
           );

           return new GenericResponse<bool>
           {
               Success = sendResult,
               Message = sendResult ? "Mail sent successfully" : "Failed to send mail"
           };
        }
        // Mail gửi nhà cung cấp 
        public async Task<GenericResponse<bool>> SendMailToSupplierAsync()
        {
            // lay thong tin nha cung cap tu db
            var suppliers = await _repo.GetSuppliersToNotifyAsync();
            if (suppliers == null || !suppliers.Any())
            {
                return new GenericResponse<bool>
                {
                    Success = false,
                    Message = "No suppliers to notify"
                };
            }
            // lấy mail 
            var mail = await _repo.GetMailByIdAsync(20);
            if (mail == null)
            {
                return new GenericResponse<bool>
                {
                    Success = false,
                    Message = "Mail template not found"
                };
            }
            // tao danh sach bao gia chi tiet
            var listBaoGiaDetail = new List<BaoGia_Detail_of_Quotation>();
            // danh sach cac don da gui mail
            var listSended = new List<int>();
            foreach (var item in suppliers)
            {
                // lay danh sach don link kien xin bao gia 
                var listRq = await _repo.GetBaoGiaRequestBySupplierAsync(item);
                if (listRq == null || !listRq.Any())
                {
                    continue;
                }
                // lay email nha cung cap
                var toEmail = await _repo.GetSupplierEmailAsync(item);
                if (string.IsNullOrEmpty(toEmail))
                {
                    continue;
                }
                // tao bang html
                var tableHtml = "<table border='1' style='border-collapse: collapse; width: 100%; font-family: Arial, sans-serif; font-size: 12px;'>";

                // Row 1 - Header chính
                tableHtml += "<tr style='background-color: #f2f2f2; text-align: center; vertical-align: middle; font-weight: bold;'>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 120px;'>Số đơn yêu cầu báo giá<br/>Quotation Request Number</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 120px;'>Mã thiết bị<br/>Equipment code</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Mã hàng nội bộ<br/>BIVN's part code</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Mã hàng của NCC<br/>Vendor's good code</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 200px;'>Tên hàng VN dùng để mở thủ tục hải quan (dự thảo)(*)<br/>Part name (Vietnamese)</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 150px;'>Tên hàng tiếng anh(*)<br/>Part name (English)</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 80px;'>Số lượng<br/>Quantity(*)</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 80px;'>Đơn vị <br/>Unit(*)</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Chủng loại hàng<br/>Part category</th>";

                // Mô tả hàng hóa - ghép 6 cột
                tableHtml += "<th colspan='6' style='padding: 8px; border: 1px solid #999; background-color: #e6e6e6;'>Mô tả hàng hóa / Description of goods</th>";

                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 120px;'>Yêu cầu ROHS<br/>ROHS requirements</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 120px;'>Yêu cầu CO/CQ<br/>CO/CQ requirements</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 200px;'>Yêu cầu MSDS kèm số CAS (đối với hóa chất)<br/>Request MSDS with CAS number (for chemicals)</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 180px;'>Yêu cầu tiêu chuẩn an toàn<br/>Request for safety standards</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 100px;'>File Thiết kế<br/>Design(*)</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Nhà Sản xuất<br/>Maker</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Mã nhà cung cấp<br/>Vendor code</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 150px;'>Tên nhà cung cấp<br/>Vendor name</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Ngày muốn nhận hàng<br/>Desired delivery date(*)</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Kỳ hạn báo giá<br/>Deadline for submit quotation</th>";
                tableHtml += "</tr>";

                // Row 2 - Header phụ cho phần mô tả hàng hóa
                tableHtml += "<tr style='background-color: #f2f2f2; text-align: center; vertical-align: middle; font-weight: bold;'>";
                tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 100px;'>Hình dáng<br/>Shape</th>";
                tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 100px;'>Chất liệu<br/>Material</th>";
                tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 180px;'>Thành phần, hàm lượng (đối với hóa chất)<br/>Composition, Content (for Chemicals)</th>";
                tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 150px;'>Kích thước(mm) (dài/rộng/cao)<br/>Dimensions (mm) (Length/Width/Height)</th>";
                tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 180px;'>Dùng cho máy/thiết bị/vị trí nào<br/>Which machine/equipment/location is it used for</th>";
                tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 150px;'>Dùng để làm gì (tính năng)<br/>Purpose of use (or function)</th>";
                tableHtml += "</tr>";

                // Gửi file báo giá đính kèm
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "ExportSampleExcel.xlsx");
                string tempFileName = $"DanhSachBaoGia_{item}.xlsx";
                string tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

                using (var workbook = new XLWorkbook(templatePath))
                {
                    var worksheet = workbook.Worksheet(1); // Assuming sheet1
                    int rowIndex = 15; // Start from row 15
                    foreach (var rq in listRq)
                    {

                        tableHtml += "<tr style='vertical-align: middle;'>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaDon ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaThietBi ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaHangNoiBo ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaHangNCC ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_NameVN ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_NameEN ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: right;'>{rq.INT_SoLuong?.ToString() ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_DonVi ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_ChungLoai ?? ""}</td>";

                        // 6 cột mô tả hàng hóa
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_HinhDang ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_ChatLieu ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_ThanhPhan ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_KichThuoc ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_DongMay ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_TinhNang ?? ""}</td>";

                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.NVCHR_Rohs ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.NVCHR_COCQ ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_MSDS ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.NVCHR_AnToan ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.NVCHR_FileThietKe ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_NhaSanXuat ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaNCC ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_TenNCC ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.DTM_NgayMuonNhan?.ToString("yyyy-MM-dd") ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? ""}</td>";
                        tableHtml += "</tr>";


                        // phần của file dữ liệu đính kèm 
                        worksheet.Cell(rowIndex, 20).Value = rq.CHR_MaDon;
                        worksheet.Cell(rowIndex, 21).Value = rq.CHR_MaThietBi;
                        worksheet.Cell(rowIndex, 22).Value = rq.CHR_MaHangNoiBo;
                        worksheet.Cell(rowIndex, 23).Value = rq.CHR_MaHangNCC;
                        worksheet.Cell(rowIndex, 24).Value = rq.NVCHR_NameVN;
                        worksheet.Cell(rowIndex, 25).Value = rq.CHR_NameEN;
                        worksheet.Cell(rowIndex, 26).Value = rq.INT_SoLuong;
                        worksheet.Cell(rowIndex, 27).Value = rq.NVCHR_DonVi;
                        worksheet.Cell(rowIndex, 28).Value = rq.NVCHR_Rohs; 
                        worksheet.Cell(rowIndex, 29).Value = rq.NVCHR_COCQ;
                        worksheet.Cell(rowIndex, 30).Value = rq.NVCHR_MSDS; 
                        worksheet.Cell(rowIndex, 31).Value = rq.NVCHR_AnToan;
                        worksheet.Cell(rowIndex, 32).Value = rq.NVCHR_FileThietKe;
                        worksheet.Cell(rowIndex, 33).Value = rq.NVCHR_NhaSanXuat;
                        worksheet.Cell(rowIndex, 34).Value = rq.CHR_MaNCC;
                        worksheet.Cell(rowIndex, 35).Value = rq.NVCHR_TenNCC;
                        worksheet.Cell(rowIndex, 36).Value = rq.DTM_NgayMuonNhan?.ToString("yyyy-MM-dd") ?? "";
                        worksheet.Cell(rowIndex, 37).Value = rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? "";
                        // Add thin border to the data row
                        worksheet.Range(rowIndex, 1, rowIndex, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        // Phần insert Rq detail
                        var itemDetail = new BaoGia_Detail_of_Quotation
                        {
                            ID_RequestQuote = rq.ID,
                            CHR_CodeNCC = rq.CHR_MaNCC ?? "",
                            NVCHR_NameNCC = rq.NVCHR_TenNCC ?? "",
                            DTM_CreateDate = DateTime.Now,
                            CHR_CreateBy = "System Send Mail",
                            CHR_MaHangNCC = rq.CHR_MaHangNCC,
                            NVCHR_TenHangHQ = rq.NVCHR_NameVN,
                            NVCHR_DonVi = "",
                            INT_SoLuong = 0,
                            FL_USD = 0,
                            FL_VND = 0,
                            NVCHR_MOQ = "",
                            DTM_LeadTime = "",
                            DTM_ShipTime = null,
                            VCHR_Rohs = "",
                            VCHR_COCQ = "",
                            VCHR_MSDS = "",
                            VCHR_AnToan = "",
                            VCHR_CamKet = "",
                            NVCHR_DeliveryTerm = "",
                            NVCHR_PaymentTerm = "",
                            NVCHR_File = ""
                        };
                        listBaoGiaDetail.Add(itemDetail);
                        rowIndex++;
                    }
                    workbook.SaveAs(tempFilePath);
                }

                tableHtml += "</table>";

                var body = mail.CHR_BODY + tableHtml;

                var emailForm = new EmailFormNetMailCustomSendMultiAttachFile
                {
                    mail_from = mail.CHR_FROM,
                    mail_to = "nguyenduy.khanh@brother-bivn.com.vn;PhuongThuy.VuThi@brother-bivn.com.vn;nguyenthi.tam@brother-bivn.com.vn;chuthuan.anh@brother-bivn.com.vn;VuThi.Toan@brother-bivn.com.vn",
                    mail_cc = "nguyenduy.khanh@brother-bivn.com.vn;PhuongThuy.VuThi@brother-bivn.com.vn;nguyenthi.tam@brother-bivn.com.vn;chuthuan.anh@brother-bivn.com.vn;VuThi.Toan@brother-bivn.com.vn",
                    mail_bcc = mail.CHR_BCC,
                    title = mail.CHR_SUBJECT,
                    body = body,
                    attachmentPaths = new List<string> { tempFilePath }
                };
                var sendResult = await EmailSender.SendEmailNotifyCustomSendMultiAttachFileAsync(emailForm);
                if (sendResult.Success)
                {
                    listSended.AddRange(listRq.Select(r => r.ID));
                }
            }
            // cap nhat trang thai da gui mail
            if (listSended.Any())
            {
                await _repo.UpdateMailSentStatusAsync(listSended);
            }
            if (listBaoGiaDetail.Any())
            {
                await _repo.InsertBaoGiaDetailAsync(listBaoGiaDetail);
            }
            return new GenericResponse<bool>
            {
                Success = true,
                Message = "Mail sent successfully"
            };
        }

        // Gửi mail nhà cung cấp theo mã đơn 
        public async Task<GenericResponse<bool>> SendMailToSupplierByRequestCodeAsync(string requestCode)
        {
            // lấy dữ liệu các đơn của mã đơn yêu cầu báo giá
            var listRq = await _repo.GetNotifyRequestCodeAsync(requestCode);
            if (listRq == null || !listRq.Any())
            {
                return new GenericResponse<bool>
                {
                    Success = false,
                    Message = "No Request code to notify"
                };
            }
            // lay thong tin nha cung cap tu db
            var suppliers = listRq.Where(r => !string.IsNullOrEmpty(r.CHR_MaNCC)).Select(r => r.CHR_MaNCC).Distinct().ToList();
            if (suppliers == null || !suppliers.Any())
            {
                return new GenericResponse<bool>
                {
                    Success = false,
                    Message = "No suppliers to notify"
                };
            }
            // lấy mail id= 20 
            var mail = await _repo.GetMailByIdAsync(20);
            if (mail == null)
            {
                return new GenericResponse<bool>
                {
                    Success = false,
                    Message = "Mail template not found"
                };
            }
            // danh sach cac don da gui mail
            var listSended = new List<int>();
            foreach (var item in suppliers)
            {
                // lay email nha cung cap
                var toEmail = await _repo.GetSupplierEmailAsync(item);
                if (string.IsNullOrEmpty(toEmail))
                {
                    continue;
                }
                // tao bang html
                var tableHtml = "<table border='1' style='border-collapse: collapse; width: 100%; font-family: Arial, sans-serif; font-size: 12px;'>";

                // Row 1 - Header chính
                tableHtml += "<tr style='background-color: #f2f2f2; text-align: center; vertical-align: middle; font-weight: bold;'>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 120px;'>Số đơn yêu cầu báo giá<br/>Quotation Request Number</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 120px;'>Mã thiết bị<br/>Equipment code</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Mã hàng nội bộ<br/>BIVN's part code</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Mã hàng của NCC<br/>Vendor's good code</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 200px;'>Tên hàng VN dùng để mở thủ tục hải quan (dự thảo)(*)<br/>Part name (Vietnamese)</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 150px;'>Tên hàng tiếng anh(*)<br/>Part name (English)</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 80px;'>Số lượng<br/>Quantity(*)</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 80px;'>Đơn vị <br/>Unit(*)</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Chủng loại hàng<br/>Part category</th>";

                // Mô tả hàng hóa - ghép 6 cột
                tableHtml += "<th colspan='6' style='padding: 8px; border: 1px solid #999; background-color: #e6e6e6;'>Mô tả hàng hóa / Description of goods</th>";

                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 120px;'>Yêu cầu ROHS<br/>ROHS requirements</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 120px;'>Yêu cầu CO/CQ<br/>CO/CQ requirements</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 200px;'>Yêu cầu MSDS kèm số CAS (đối với hóa chất)<br/>Request MSDS with CAS number (for chemicals)</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 180px;'>Yêu cầu tiêu chuẩn an toàn<br/>Request for safety standards</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 100px;'>File Thiết kế<br/>Design(*)</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Nhà Sản xuất<br/>Maker</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Mã nhà cung cấp<br/>Vendor code</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 150px;'>Tên nhà cung cấp<br/>Vendor name</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Ngày muốn nhận hàng<br/>Desired delivery date(*)</th>";
                tableHtml += "<th rowspan='2' style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Kỳ hạn báo giá<br/>Deadline for submit quotation</th>";
                tableHtml += "</tr>";

                // Row 2 - Header phụ cho phần mô tả hàng hóa
                tableHtml += "<tr style='background-color: #f2f2f2; text-align: center; vertical-align: middle; font-weight: bold;'>";
                tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 100px;'>Hình dáng<br/>Shape</th>";
                tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 100px;'>Chất liệu<br/>Material</th>";
                tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 180px;'>Thành phần, hàm lượng (đối với hóa chất)<br/>Composition, Content (for Chemicals)</th>";
                tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 150px;'>Kích thước(mm) (dài/rộng/cao)<br/>Dimensions (mm) (Length/Width/Height)</th>";
                tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 180px;'>Dùng cho máy/thiết bị/vị trí nào<br/>Which machine/equipment/location is it used for</th>";
                tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 150px;'>Dùng để làm gì (tính năng)<br/>Purpose of use (or function)</th>";
                tableHtml += "</tr>";

                // Gửi file báo giá đính kèm
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "ExportSampleExcel.xlsx");
                string tempFileName = $"DanhSachBaoGia_{item}.xlsx";
                string tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

                using (var workbook = new XLWorkbook(templatePath))
                {
                    var worksheet = workbook.Worksheet(1); // Assuming sheet1
                    int rowIndex = 15; // Start from row 15
                    var rqByNCC = listRq.Where(r => r.CHR_MaNCC == item).ToList();
                    foreach (var rq in rqByNCC)
                    {

                        tableHtml += "<tr style='vertical-align: middle;'>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaDon ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaThietBi ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaHangNoiBo ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaHangNCC ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_NameVN ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_NameEN ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: right;'>{rq.INT_SoLuong?.ToString() ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_DonVi ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_ChungLoai ?? ""}</td>";

                        // 6 cột mô tả hàng hóa
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_HinhDang ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_ChatLieu ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_ThanhPhan ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_KichThuoc ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_DongMay ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_TinhNang ?? ""}</td>";

                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.NVCHR_Rohs ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.NVCHR_COCQ ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_MSDS ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.NVCHR_AnToan ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.NVCHR_FileThietKe ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_NhaSanXuat ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaNCC ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_TenNCC ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.DTM_NgayMuonNhan?.ToString("yyyy-MM-dd") ?? ""}</td>";
                        tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? ""}</td>";
                        tableHtml += "</tr>";


                        // phần của file dữ liệu đính kèm 


                        worksheet.Cell(rowIndex, 20).Value = rq.CHR_MaDon;
                        worksheet.Cell(rowIndex, 21).Value = rq.CHR_MaThietBi;
                        worksheet.Cell(rowIndex, 22).Value = rq.CHR_MaHangNoiBo;
                        worksheet.Cell(rowIndex, 23).Value = rq.CHR_MaHangNCC;
                        worksheet.Cell(rowIndex, 24).Value = rq.NVCHR_NameVN;
                        worksheet.Cell(rowIndex, 25).Value = rq.CHR_NameEN;
                        worksheet.Cell(rowIndex, 26).Value = rq.INT_SoLuong;
                        worksheet.Cell(rowIndex, 27).Value = rq.NVCHR_DonVi;
                        worksheet.Cell(rowIndex, 28).Value = rq.NVCHR_Rohs;
                        worksheet.Cell(rowIndex, 29).Value = rq.NVCHR_COCQ;
                        worksheet.Cell(rowIndex, 30).Value = rq.NVCHR_MSDS;
                        worksheet.Cell(rowIndex, 31).Value = rq.NVCHR_AnToan;
                        worksheet.Cell(rowIndex, 32).Value = rq.NVCHR_FileThietKe;
                        worksheet.Cell(rowIndex, 33).Value = rq.NVCHR_NhaSanXuat;
                        worksheet.Cell(rowIndex, 34).Value = rq.CHR_MaNCC;
                        worksheet.Cell(rowIndex, 35).Value = rq.NVCHR_TenNCC;
                        worksheet.Cell(rowIndex, 36).Value = rq.DTM_NgayMuonNhan?.ToString("yyyy-MM-dd") ?? "";
                        worksheet.Cell(rowIndex, 37).Value = rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? "";
                        // Add thin border to the data row
                        worksheet.Range(rowIndex, 1, rowIndex, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        rowIndex++;
                    }
                    workbook.SaveAs(tempFilePath);
                }

                tableHtml += "</table>";

                var body = mail.CHR_BODY + tableHtml;
                //nguyenduy.khanh@brother-bivn.com.vn;PhuongThuy.VuThi@brother-bivn.com.vn;nguyenthi.tam@brother-bivn.com.vn;chuthuan.anh@brother-bivn.com.vn;VuThi.Toan@brother-bivn.com.vn
                var emailForm = new EmailFormNetMailCustomSendMultiAttachFile
                {
                    mail_from = mail.CHR_FROM,
                    mail_to = "nguyenduy.khanh@brother-bivn.com.vn;PhuongThuy.VuThi@brother-bivn.com.vn;nguyenthi.tam@brother-bivn.com.vn;chuthuan.anh@brother-bivn.com.vn;VuThi.Toan@brother-bivn.com.vn",
                    mail_cc = "nguyenduy.khanh@brother-bivn.com.vn;PhuongThuy.VuThi@brother-bivn.com.vn;nguyenthi.tam@brother-bivn.com.vn;chuthuan.anh@brother-bivn.com.vn;VuThi.Toan@brother-bivn.com.vn",
                    mail_bcc = mail.CHR_BCC,
                    title = mail.CHR_SUBJECT,
                    body = body,
                    attachmentPaths = new List<string> { tempFilePath }
                };
                var sendResult = await EmailSender.SendEmailNotifyCustomSendMultiAttachFileAsync(emailForm);
                if (sendResult.Success)
                {
                    listSended.AddRange(listRq.Select(r => r.ID));
                }
            }
            // cap nhat trang thai da gui mail
            if (listSended.Any())
            {
                await _repo.UpdateMailSentStatusAsync(listSended);
            }
            return new GenericResponse<bool>
            {
                Success = true,
                Message = "Mail sent successfully"
            };
        }
        // Gửi mail thông báo đến người yêu cầu khi có cập nhật về đơn yêu cầu
        public async Task<GenericResponse<bool>> SendMailToRequesterAsync(string requestCode, string section, bool? isGap, int step)
        {
            // Lấy thông tin người yêu cầu
            var requesterEmail = await _repo.GetRequesterEmailAsync(section, step);
            if (string.IsNullOrEmpty(requesterEmail))
            {
                return new GenericResponse<bool>
                {
                    Success = false,
                    Message = "Requester email not found"
                };
            }
            // Lấy template mail
            var mailTemplate = await _repo.GetMailByIdAsync(11); 
            if (mailTemplate == null)
            {
                return new GenericResponse<bool>
                {
                    Success = false,
                    Message = "Mail template not found"
                };
            }
            // Chuẩn bị nội dung mail với các tham số
            string gapText = isGap.HasValue && isGap.Value ? "Có" : "Không";
            string body = string.Format(mailTemplate.CHR_BODY, "http://172.26.248.62:8057/ApprovalQuote/Index", gapText, section, requestCode);
            // Gửi mail
            bool sendResult = EmailSender.sendEmailNotify(
                mailTemplate.CHR_SUBJECT,
                mailTemplate.CHR_FROM,
                requesterEmail,//"nguyenduy.khanh@brother-bivn.com.vn",//
                "", // CC nếu cần
                mailTemplate.CHR_BCC,
                body,
                0 // Default priority
            );
            return new GenericResponse<bool>
            {
                Success = true,
                Message = "Mail sent successfully"
            };
        }
    }
}
