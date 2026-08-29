using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Wordprocessing;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using System;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class SendMailService : BaseService<TM_MASTER_MAIL, int ,TM_MASTER_MAILDTO>, ISendMailService
    {
        private readonly ISendMailRepository _repo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private const string mailPICTo = "bivn-pur-indirectpart@brother-bivn.com.vn";//"bivn-pur-indirectpart@brother-bivn.com.vn";//"bivn-gagpur@brother-bivn.com.vn";//
        public SendMailService(ISendMailRepository repository, IMapper mapper, IConfiguration configuration) : base(repository, mapper)
        {
            _repo = repository;
            _mapper = mapper;
            _configuration = configuration;
        }
        // Gửi mail
        public async Task<GenericResponse<bool>> SendMailAsync(string toEmail, string ccEmail, int idMail, string? url, bool? isGap, string? section, string? idRequest, string? user)
        {
           var urlMail = _configuration["ApiSettings:BaseSendMailUrl"] ?? "";
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
           string body = string.Format(mail.CHR_BODY, urlMail+url, gapText, section, idRequest,user);

           bool sendResult = await EmailSender.sendEmailNotifyAsync(
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
        public async Task<GenericResponse<bool>> SendMailToSupplierAsync()
        {
            string dearMail = "";

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
            // lay thong tin nha cc k can xin bao gia tu db
            var suppliersNoRequest = await _repo.SupplierNeedToSendMailAsync();


            // lấy mail template
            var mail = await _repo.GetMailByIdAsync(19);
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
            var listBaoGiaDetail = new List<BaoGia_Detail_of_Quotation>();

            foreach (var supplier in suppliers)
            {
                string mailTk = "";
                string checkMaDon = "";
                // lay danh sach don link kien xin bao gia cua nha cung cap nay
                var listRq = await _repo.GetBaoGiaRequestBySupplierAsync(supplier);
                if (listRq == null || !listRq.Any())
                {
                    continue;
                }
                int rowIndex = 13;
                // lay email nha cung cap
                var toEmail = await _repo.GetSupplierEmailAsync(supplier);
                if (string.IsNullOrEmpty(toEmail))
                {
                    continue;
                }

                // Tạo file Excel tổng hợp cho nhà cung cấp này
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "TmSendMailNew.xlsx");
                string tempFileName = $"{supplier}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);
                // bảng thông tin PIC
                var tablePicInfo = new StringBuilder();
                tablePicInfo.AppendLine("<table border='1' style='border-collapse: collapse; width: 100%; font-family: Arial, sans-serif; font-size: 12px; margin-bottom: 20px;'>");
                tablePicInfo.AppendLine("<tr style='background-color: #d9e1f2; text-align: center; vertical-align: middle; font-weight: bold;'>");
                tablePicInfo.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 200px;'>Quotation Request Number<br/>Số yêu cầu báo giá</th>");
                tablePicInfo.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 300px;'>PIC explain about Product description<br/>Đảm nhiệm giải thích nếu có thắc mắc về thiết kế<br/>(Khi gửi báo giá vui lòng ko CC cho email này)</th>");
                tablePicInfo.AppendLine("</tr>");

                using (var workbook = new XLWorkbook(templatePath))
                {
                    var worksheet = workbook.Worksheet(1);

                    worksheet.Column(30).Hide();

                    foreach (var rq in listRq)
                    {
                        var maiUserCreate = await _repo.GetRequesterEmailByAdidAsync(rq.CHR_CreateBy);
                        if (mailTk != maiUserCreate && checkMaDon != rq.CHR_MaDon)
                        {
                            mailTk = maiUserCreate;
                            tablePicInfo.AppendLine("<tr style='vertical-align: middle;'>");
                            tablePicInfo.AppendLine($"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.CHR_MaDon ?? ""}</td>");
                            tablePicInfo.AppendLine($"<td style='padding: 6px; border: 1px solid #999;'>{mailTk}</td>");
                            tablePicInfo.AppendLine("</tr>");
                            checkMaDon = rq.CHR_MaDon;
                        }

                        // Tổng hợp Other request từ các trường ROHS, COCQ, MSDS, AnToan
                        var otherRequestList = new List<string>();
                        if (!string.IsNullOrEmpty(rq.NVCHR_Rohs))
                            otherRequestList.Add($"ROHS: {rq.NVCHR_Rohs}");
                        if (!string.IsNullOrEmpty(rq.NVCHR_COCQ))
                            otherRequestList.Add($"COCQ: {rq.NVCHR_COCQ}");
                        if (!string.IsNullOrEmpty(rq.NVCHR_MSDS))
                            otherRequestList.Add($"MSDS: {rq.NVCHR_MSDS}");
                        if (!string.IsNullOrEmpty(rq.NVCHR_AnToan))
                            otherRequestList.Add($"An toàn: {rq.NVCHR_AnToan}");

                        string otherRequest = string.Join(" & ", otherRequestList);
                        // Ghi dữ liệu vào file Excel theo cột mới
                        worksheet.Cell(1, 3).Value = rq.NVCHR_TenNCC ?? string.Empty;
                        worksheet.Cell(2, 3).Value = rq.Diachi ?? string.Empty;

                        // Các cột từ 23 trở đi theo thứ tự mới
                        worksheet.Cell(rowIndex, 1).Value = rq.CHR_MaDon ?? string.Empty;                    // Quotation Request Number
                        worksheet.Cell(rowIndex, 2).Value = rq.CHR_MaThietBi ?? string.Empty;                 // Equipment code
                        worksheet.Cell(rowIndex, 3).Value = rq.CHR_MaHangNoiBo ?? string.Empty;              // BIVN's part code
                        worksheet.Cell(rowIndex, 4).Value = rq.CHR_MaHangNCC ?? string.Empty;                // Vendor's good code
                        worksheet.Cell(rowIndex, 5).Value = rq.NVCHR_NameVN ?? string.Empty;                 // Product description
                        worksheet.Cell(rowIndex, 6).Value = rq.INT_SoLuong ?? string.Empty;                  // Quantity
                        worksheet.Cell(rowIndex, 7).Value = rq.NVCHR_DonVi ?? string.Empty;                  // Unit
                        worksheet.Cell(rowIndex, 8).Value = otherRequest;                                     // Other request (tổng hợp)
                        worksheet.Cell(rowIndex, 9).Value = rq.NVCHR_NhaSanXuat ?? string.Empty;             // Maker
                        worksheet.Cell(rowIndex, 10).Value = rq.CHR_MaNCC ?? string.Empty;                    // Vendor code
                        worksheet.Cell(rowIndex, 27).Value = rq.NVCHR_FileThietKe ?? "";                       // Design
                        worksheet.Cell(rowIndex, 28).Value = rq.DTM_NgayMuonNhan?.ToString("yyyy-MM-dd") ?? ""; // Delivery date
                        worksheet.Cell(rowIndex, 29).Value = rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? "";       // Deadline for submit quotation

                        worksheet.Cell(rowIndex, 31).Value = maiUserCreate;            // PIC explain - để trống

                        worksheet.Cell(rowIndex, 32).Value = rq.ID ?? "";

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
                            BIT_Select = null,
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

                        // Ghi nhận ID để cập nhật trạng thái sau khi gửi mail thành công
                        listSended.Add((int)rq.ID);

                        rowIndex++;
                    }
                    tablePicInfo.AppendLine("</table>");
                    workbook.SaveAs(tempFilePath);
                }



                // Đợi file được giải phóng hoàn toàn
                await Task.Delay(100);

                // Chuẩn bị nội dung email
                dearMail = "nhà cung cấp " + (listRq.FirstOrDefault()?.Ten ?? "") + " yêu cầu báo giá cho các mặt hàng như file đính kèm. Rất mong nhận được phản hồi báo giá sớm nhất từ quý nhà cung cấp. Trân trọng cảm ơn!";

                // Lấy ngày yêu cầu báo giá (ưu tiên DTM_KyHan, nếu null thì dùng ngày hiện tại)
                string requestDate = listRq.FirstOrDefault()?.DTM_KyHan?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");

                // Tiêu đề mail mới: Tên ngắn NCC + Ngày yêu cầu báo giá
                string shortName = listRq.FirstOrDefault()?.ShortName ?? listRq.FirstOrDefault()?.Ten ?? "NCC";
                string titleMail = $"{shortName} - {requestDate}";

                var bodyTable = mail.CHR_BODY + tablePicInfo.ToString();
                var body = string.Format(bodyTable, dearMail, mailTk);
                var emailCC = string.IsNullOrEmpty(mail.CHR_CC) ?
                    mailPICTo : mail.CHR_CC;

                var emailForm = new EmailFormNetMailCustomSendMultiAttachFile
                {
                    mail_from = mail.CHR_FROM,
                    mail_to = toEmail,
                    mail_cc = emailCC,
                    mail_bcc = mail.CHR_BCC,
                    title = titleMail,
                    body = body,
                    attachmentPaths = new List<string> { tempFilePath }
                };

                if (!suppliersNoRequest.Contains(supplier))
                {
                    var sendResult = await EmailSender.SendEmailNotifyCustomSendMultiAttachFileAsync(emailForm);
                    // Nếu gửi mail thất bại thì xóa các ID đã thêm vào listSended
                    if (!sendResult.Success)
                    {
                        // Xóa các ID vừa thêm của supplier này
                        var idsToRemove = listRq.Select(r => (int)r.ID).ToList();
                        listSended = listSended.Except(idsToRemove).ToList();
                    }
                }

                // Xóa file tạm sau khi gửi email
                try
                {
                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Không thể xóa file tạm: {ex.Message}");
                }
            }

            // cap nhat trang thai da gui mail
            if (listSended.Any())
            {
                try
                {
                    await _repo.UpdateMailSentStatusAsync(listSended);
                }
                catch (Exception ex)
                {
                    return new GenericResponse<bool>
                    {
                        Success = false,
                        Message = "Bug Update: " + ex.Message
                    };
                }
            }

            if (listBaoGiaDetail.Any())
            {
                try
                {
                    await _repo.InsertBaoGiaDetailAsync(listBaoGiaDetail);
                }
                catch (Exception ex)
                {
                    return new GenericResponse<bool>
                    {
                        Success = false,
                        Message = "Bug Insert: " + ex.Message
                    };
                }
            }

            return new GenericResponse<bool>
            {
                Success = true,
                Message = "Mail sent successfully"
            };
        }

        public async Task<GenericResponse<bool>> SendMailToSupplierOptimizedAsync()
        {
            string dearMail = "";

            var suppliers = await _repo.GetSuppliersToNotifyAsync();
            if (suppliers == null || !suppliers.Any())
            {
                return new GenericResponse<bool>
                {
                    Success = false,
                    Message = "No suppliers to notify"
                };
            }

            var suppliersNoRequest = await _repo.SupplierNeedToSendMailAsync();
            var suppliersNoRequestSet = new HashSet<string>(suppliersNoRequest ?? Enumerable.Empty<string>());

            var mail = await _repo.GetMailByIdAsync(19);
            if (mail == null)
            {
                return new GenericResponse<bool>
                {
                    Success = false,
                    Message = "Mail template not found"
                };
            }

            var listSended = new List<int>();
            var listBaoGiaDetail = new List<BaoGia_Detail_of_Quotation>();
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "TmSendMailNew.xlsx");

            foreach (var supplier in suppliers)
            {
                string mailTk = "";
                string checkMaDon = "";

                var listRq = await _repo.GetBaoGiaRequestBySupplierAsync(supplier);
                if (listRq == null || !listRq.Any())
                {
                    continue;
                }

                int rowIndex = 13;
                var toEmail = await _repo.GetSupplierEmailAsync(supplier);
                if (string.IsNullOrEmpty(toEmail))
                {
                    continue;
                }

                var createByIds = listRq
                    .Select(r => (string?)r.CHR_CreateBy)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!)
                    .Distinct()
                    .ToList();

                var requesterMailTasks = createByIds
                    .ToDictionary(adid => adid, adid => _repo.GetRequesterEmailByAdidAsync(adid));

                await Task.WhenAll(requesterMailTasks.Values);

                var requesterMails = requesterMailTasks
                    .ToDictionary(k => k.Key, v => v.Value.Result ?? string.Empty);

                string tempFileName = $"{supplier}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

                var tablePicInfo = new StringBuilder();
                tablePicInfo.AppendLine("<table border='1' style='border-collapse: collapse; width: 100%; font-family: Arial, sans-serif; font-size: 12px; margin-bottom: 20px;'>");
                tablePicInfo.AppendLine("<tr style='background-color: #d9e1f2; text-align: center; vertical-align: middle; font-weight: bold;'>");
                tablePicInfo.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 200px;'>Quotation Request Number<br/>Số yêu cầu báo giá</th>");
                tablePicInfo.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 300px;'>PIC explain about Product description<br/>Đảm nhiệm giải thích nếu có thắc mắc về thiết kế<br/>(Khi gửi báo giá vui lòng ko CC cho email này)</th>");
                tablePicInfo.AppendLine("</tr>");

                using (var workbook = new XLWorkbook(templatePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    worksheet.Column(30).Hide();

                    foreach (var rq in listRq)
                    {
                        requesterMails.TryGetValue((string?)rq.CHR_CreateBy ?? string.Empty, out string? maiUserCreate);
                        maiUserCreate ??= string.Empty;

                        if (mailTk != maiUserCreate && checkMaDon != rq.CHR_MaDon)
                        {
                            mailTk = maiUserCreate;
                            tablePicInfo.AppendLine("<tr style='vertical-align: middle;'>");
                            tablePicInfo.AppendLine($"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.CHR_MaDon ?? ""}</td>");
                            tablePicInfo.AppendLine($"<td style='padding: 6px; border: 1px solid #999;'>{mailTk}</td>");
                            tablePicInfo.AppendLine("</tr>");
                            checkMaDon = rq.CHR_MaDon;
                        }

                        var otherRequestList = new List<string>();
                        if (!string.IsNullOrEmpty(rq.NVCHR_Rohs))
                            otherRequestList.Add($"ROHS: {rq.NVCHR_Rohs}");
                        if (!string.IsNullOrEmpty(rq.NVCHR_COCQ))
                            otherRequestList.Add($"COCQ: {rq.NVCHR_COCQ}");
                        if (!string.IsNullOrEmpty(rq.NVCHR_MSDS))
                            otherRequestList.Add($"MSDS: {rq.NVCHR_MSDS}");
                        if (!string.IsNullOrEmpty(rq.NVCHR_AnToan))
                            otherRequestList.Add($"An toàn: {rq.NVCHR_AnToan}");

                        string otherRequest = string.Join(" & ", otherRequestList);

                        worksheet.Cell(1, 3).Value = rq.NVCHR_TenNCC ?? string.Empty;
                        worksheet.Cell(2, 3).Value = rq.Diachi ?? string.Empty;

                        worksheet.Cell(rowIndex, 1).Value = rq.CHR_MaDon ?? string.Empty;
                        worksheet.Cell(rowIndex, 2).Value = rq.CHR_MaThietBi ?? string.Empty;
                        worksheet.Cell(rowIndex, 3).Value = rq.CHR_MaHangNoiBo ?? string.Empty;
                        worksheet.Cell(rowIndex, 4).Value = rq.CHR_MaHangNCC ?? string.Empty;
                        worksheet.Cell(rowIndex, 5).Value = rq.NVCHR_NameVN ?? string.Empty;
                        worksheet.Cell(rowIndex, 6).Value = rq.INT_SoLuong ?? string.Empty;
                        worksheet.Cell(rowIndex, 7).Value = rq.NVCHR_DonVi ?? string.Empty;
                        worksheet.Cell(rowIndex, 8).Value = otherRequest;
                        worksheet.Cell(rowIndex, 9).Value = rq.NVCHR_NhaSanXuat ?? string.Empty;
                        worksheet.Cell(rowIndex, 10).Value = rq.CHR_MaNCC ?? string.Empty;
                        worksheet.Cell(rowIndex, 27).Value = rq.NVCHR_FileThietKe ?? "";
                        worksheet.Cell(rowIndex, 28).Value = rq.DTM_NgayMuonNhan?.ToString("yyyy-MM-dd") ?? "";
                        worksheet.Cell(rowIndex, 29).Value = rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? "";
                        worksheet.Cell(rowIndex, 31).Value = maiUserCreate;
                        worksheet.Cell(rowIndex, 32).Value = rq.ID ?? "";

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

                        listSended.Add((int)rq.ID);
                        rowIndex++;
                    }

                    tablePicInfo.AppendLine("</table>");
                    workbook.SaveAs(tempFilePath);
                }

                var firstRq = listRq.FirstOrDefault();
                dearMail = "nhà cung cấp " + (firstRq?.Ten ?? "") + " yêu cầu báo giá cho các mặt hàng như file đính kèm. Rất mong nhận được phản hồi báo giá sớm nhất từ quý nhà cung cấp. Trân trọng cảm ơn!";
                string requestDate = firstRq?.DTM_KyHan?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");
                string shortName = firstRq?.ShortName ?? firstRq?.Ten ?? "NCC";
                string titleMail = $"{shortName} - {requestDate}";

                var bodyTable = mail.CHR_BODY + tablePicInfo.ToString();
                var body = string.Format(bodyTable, dearMail, mailTk);
                var emailCC = string.IsNullOrEmpty(mail.CHR_CC)
                    ? mailPICTo
                    : mail.CHR_CC;

                var emailForm = new EmailFormNetMailCustomSendMultiAttachFile
                {
                    mail_from = mail.CHR_FROM,
                    mail_to = toEmail,
                    mail_cc = emailCC,
                    mail_bcc = mail.CHR_BCC,
                    title = titleMail,
                    body = body,
                    attachmentPaths = new List<string> { tempFilePath }
                };

                if (!suppliersNoRequestSet.Contains(supplier))
                {
                    var sendResult = await EmailSender.SendEmailNotifyCustomSendMultiAttachFileAsync(emailForm);
                    if (!sendResult.Success)
                    {
                        var idsToRemove = listRq.Select(r => (int)r.ID).ToList();
                        listSended = listSended.Except(idsToRemove).ToList();
                    }
                }

                try
                {
                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Không thể xóa file tạm: {ex.Message}");
                }
            }

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
            string dearMail = "";
            string titleMail = "";
            string mailTk = "";
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
            // lấy mail id= 19 
            var mail = await _repo.GetMailByIdAsync(19);
            if (mail == null)
            {
                return new GenericResponse<bool>
                {
                    Success = false,
                    Message = "Mail template not found"
                };
            }
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "TmSendMailNew.xlsx");
            // danh sach cac don da gui mail
            var listSended = new List<int>();
            var listBaoGiaDetail = new List<BaoGia_Detail_of_Quotation>();

            foreach (var item in suppliers)
            {
                var rqByNCC = listRq.Where(r => r.CHR_MaNCC == item).ToList();
                if (!rqByNCC.Any())
                    continue;

                // lay email nha cung cap
                var toEmail = await _repo.GetSupplierEmailAsync(item); // Không cần chủng loại
                if (string.IsNullOrEmpty(toEmail))
                {
                    continue;
                }

                // Tạo tên file với milliseconds để tránh trùng lặp
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                string tempFileName = $"{item}_{requestCode}.xlsx";
                string tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

                // Tạo file Excel trong using block riêng
                using (var workbook = new XLWorkbook(templatePath))
                {
                    var worksheet = workbook.Worksheet(1); // Assuming sheet1
                    int rowIndex = 13;

                    worksheet.Column(30).Hide();
                    // bảng thông tin PIC
                    var tablePicInfo = new StringBuilder();
                    tablePicInfo.AppendLine("<table border='1' style='border-collapse: collapse; width: 100%; font-family: Arial, sans-serif; font-size: 12px; margin-bottom: 20px;'>");
                    tablePicInfo.AppendLine("<tr style='background-color: #d9e1f2; text-align: center; vertical-align: middle; font-weight: bold;'>");
                    tablePicInfo.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 200px;'>Quotation Request Number<br/>Số yêu cầu báo giá</th>");
                    tablePicInfo.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 300px;'>PIC explain about Product description<br/>Đảm nhiệm giải thích nếu có thắc mắc về thiết kế<br/>(Khi gửi báo giá vui lòng ko CC cho email này)</th>");
                    tablePicInfo.AppendLine("</tr>");

                    foreach (var rq in rqByNCC)
                    {
                        var maiUserCreate = await _repo.GetRequesterEmailByAdidAsync(rq.CHR_CreateBy);
                        if (mailTk != maiUserCreate)
                        {
                            mailTk = maiUserCreate;
                            tablePicInfo.AppendLine("<tr style='vertical-align: middle;'>");
                            tablePicInfo.AppendLine($"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.CHR_MaDon ?? ""}</td>");
                            tablePicInfo.AppendLine($"<td style='padding: 6px; border: 1px solid #999;'>{mailTk}</td>");
                            tablePicInfo.AppendLine("</tr>");
                        }
                        // Tổng hợp Other request từ các trường ROHS, COCQ, MSDS, AnToan
                        var otherRequestList = new List<string>();
                        if (!string.IsNullOrEmpty(rq.NVCHR_Rohs))
                            otherRequestList.Add($"ROHS: {rq.NVCHR_Rohs}");
                        if (!string.IsNullOrEmpty(rq.NVCHR_COCQ))
                            otherRequestList.Add($"COCQ: {rq.NVCHR_COCQ}");
                        if (!string.IsNullOrEmpty(rq.NVCHR_MSDS))
                            otherRequestList.Add($"MSDS: {rq.NVCHR_MSDS}");
                        if (!string.IsNullOrEmpty(rq.NVCHR_AnToan))
                            otherRequestList.Add($"An toàn: {rq.NVCHR_AnToan}");
                        string otherRequest = string.Join(" & ", otherRequestList);

                        // phần của file dữ liệu đính kèm 
                        worksheet.Cell(1, 3).Value = rq.NVCHR_TenNCC ?? string.Empty;
                        worksheet.Cell(2, 3).Value = rq.Diachi ?? string.Empty;

                        // Các cột từ 23 trở đi theo thứ tự mới
                        worksheet.Cell(rowIndex, 1).Value = rq.CHR_MaDon ?? string.Empty;                    // Quotation Request Number
                        worksheet.Cell(rowIndex, 2).Value = rq.CHR_MaThietBi ?? string.Empty;                 // Equipment code
                        worksheet.Cell(rowIndex, 3).Value = rq.CHR_MaHangNoiBo ?? string.Empty;              // BIVN's part code
                        worksheet.Cell(rowIndex, 4).Value = rq.CHR_MaHangNCC ?? string.Empty;                // Vendor's good code
                        worksheet.Cell(rowIndex, 5).Value = rq.NVCHR_NameVN ?? string.Empty;                 // Product description
                        worksheet.Cell(rowIndex, 6).Value = rq.INT_SoLuong ?? string.Empty;                  // Quantity
                        worksheet.Cell(rowIndex, 7).Value = rq.NVCHR_DonVi ?? string.Empty;                  // Unit
                        worksheet.Cell(rowIndex, 8).Value = otherRequest;                                     // Other request (tổng hợp)
                        worksheet.Cell(rowIndex, 9).Value = rq.NVCHR_NhaSanXuat ?? string.Empty;             // Maker
                        worksheet.Cell(rowIndex, 10).Value = rq.CHR_MaNCC ?? string.Empty;                    // Vendor code
                        worksheet.Cell(rowIndex, 27).Value = rq.NVCHR_FileThietKe ?? "";                       // Design
                        worksheet.Cell(rowIndex, 28).Value = rq.DTM_NgayMuonNhan?.ToString("yyyy-MM-dd") ?? ""; // Delivery date
                        worksheet.Cell(rowIndex, 29).Value = rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? "";       // Deadline for submit quotation

                        worksheet.Cell(rowIndex, 31).Value = maiUserCreate;            // PIC explain - để trống

                        worksheet.Cell(rowIndex, 32).Value = rq.ID ?? "";

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


                        dearMail = "nhà cung cấp " + rq.Ten + " yêu cầu báo giá cho các mặt hàng như file đính kèm. Rất mong nhận được phản hồi báo giá sớm nhất từ quý nhà cung cấp. Trân trọng cảm ơn!";
                        mailTk = maiUserCreate;
                        titleMail = (rq.ShortName ?? rq.Ten) + " - Deadline: " + (rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd")) + " - Số đơn yêu cầu: " + rq.CHR_MaDon;
                        rowIndex++;
                    }

                    workbook.SaveAs(tempFilePath);

                    var bodyTable = mail.CHR_BODY + tablePicInfo.ToString();
                    var body = string.Format(bodyTable, dearMail, mailTk);
                    var email = string.IsNullOrEmpty(mail.CHR_CC) ?
                        mailPICTo : mail.CHR_CC;

                    var emailForm = new EmailFormNetMailCustomSendMultiAttachFile
                    {
                        mail_from = mail.CHR_FROM,
                        mail_to = toEmail,
                        mail_cc = email,
                        mail_bcc = mail.CHR_BCC,
                        title = titleMail,
                        body = body,
                        attachmentPaths = new List<string> { tempFilePath }
                    };

                    var sendResult = await EmailSender.SendEmailNotifyCustomSendMultiAttachFileAsync(emailForm);

                    // Xóa file tạm sau khi gửi email
                    try
                    {
                        if (File.Exists(tempFilePath))
                        {
                            File.Delete(tempFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log lỗi nếu cần, nhưng không ảnh hưởng đến luồng chính
                        Console.WriteLine($"Không thể xóa file tạm: {ex.Message}");
                    }

                    if (sendResult.Success)
                    {
                        listSended.AddRange(rqByNCC.Select(r => (int)r.ID));
                    }
                } 
            }
            // cap nhat trang thai da gui mail
            if (listSended.Count != 0)
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

        // Gửi mail thông báo đến người yêu cầu khi có cập nhật về đơn yêu cầu
        public async Task<GenericResponse<bool>> SendMailToRequesterAsync(string requestCode, string sectionCode, string sectionName, bool? isGap, int step)
        {
            // Lấy thông tin người yêu cầu
            var requesterEmail = await _repo.GetRequesterEmailAsync(sectionCode, step);
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
            var urlMail = _configuration["ApiSettings:BaseSendMailUrl"] ?? "";
            // Chuẩn bị nội dung mail với các tham số
            string gapText = isGap.HasValue && isGap.Value ? "Có" : "Không";
            string body = string.Format(mailTemplate.CHR_BODY ?? "", urlMail+"ApprovalQuote/Index", gapText, sectionName, requestCode);
            // Gửi mail
            bool sendResult = await EmailSender.sendEmailNotifyAsync(
                mailTemplate.CHR_SUBJECT ?? "",
                mailTemplate.CHR_FROM ?? "",
                requesterEmail,
                "", 
                mailTemplate.CHR_BCC ?? "",
                body,
                0 // Default priority
            );
            return new GenericResponse<bool>
            {
                Success = true,
                Message = "Mail sent successfully"
            };
        }
        // Mail gữi xác nhận tên và mã hàng 
        public async Task<GenericResponse<bool>> SendMailToConfirmItemAsync(int step, int codeMail, string? link, bool? isGap, string? sectionCode, string? sectionName, string user)
        {
            var result = new GenericResponse<bool>();
            try
            {
                // Lấy thông tin người yêu cầu
                var requesterEmail = await _repo.GetRequesterEmailAsync("", step);
                if (string.IsNullOrEmpty(requesterEmail))
                {
                    return new GenericResponse<bool>
                    {
                        Success = false,
                        Message = "Requester email not found"
                    };
                }
                // Lấy template mail
                var mailTemplate = await _repo.GetMailByIdAsync(codeMail);
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
                // Chuẩn bị nội dung mail với các tham số
                string body = string.Format(mailTemplate.CHR_BODY ?? "", link, gapText, sectionName, sectionCode, user);
                // Gửi mail
                bool sendResult = await EmailSender.sendEmailNotifyAsync(
                    mailTemplate.CHR_SUBJECT ?? "",
                    mailTemplate.CHR_FROM ?? "",
                    requesterEmail,
                    mailTemplate.CHR_CC ?? "", 
                    mailTemplate.CHR_BCC ?? "",
                    body,
                    0 // Default priority
                );
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error sending confirmation mail: {ex.Message}";
            }

            return result;
        }
        // Lấy thông tin mail người nhận theo bước
        public async Task<GenericResponse<string>> SendMailToRequesterAsync(string sectionCode, int step)
        {
            var result = new GenericResponse<string>();
            try
            {
                result.Data = await _repo.GetRequesterEmailAsync(sectionCode, step);
                if (string.IsNullOrEmpty(result.Data))
                {
                    result.Success = false;
                    result.Message = "Requester email not found";
                }
                else
                {
                    result.Success = true;
                }
            }
            catch(Exception ex)
            {
                result.Success = false;
                result.Message = $"Error sending mail to requester: {ex.Message}";
            }
            return result;
        }

        // Gửi mail cho nhà cung cấp tổng hợp theo từng đơn và kèm file đính kèm từ CHR_LinkFile
        public async Task<GenericResponse<bool>> SendMailToSupplierPerOrderWithAttachmentsAsync()
        {
            var result = new GenericResponse<bool>();

            // Lấy danh sách nhà cung cấp cần gửi
            var suppliers = await _repo.GetSuppliersToNotifyAsync();
            if (suppliers == null || !suppliers.Any())
            {
                return new GenericResponse<bool> { Success = false, Message = "No suppliers to notify" };
            }

            // Lấy template mail
            var mail = await _repo.GetMailByIdAsync(19);
            if (mail == null)
            {
                return new GenericResponse<bool> { Success = false, Message = "Mail template not found" };
            }

            var listBaoGiaDetail = new List<BaoGia_Detail_of_Quotation>();
            var listSended = new List<int>();

            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "ExportSampleExcel.xlsx");

            foreach (var supplier in suppliers)
            {
                // Lấy các request của nhà cung cấp
                var listRq = await _repo.GetBaoGiaRequestBySupplierAsync(supplier);
                if (listRq == null || !listRq.Any())
                    continue;

                // Group theo mã đơn (mỗi email sẽ gửi theo 1 mã đơn)
                var groupsByMaDon = listRq.GroupBy(r => r.CHR_MaDon ?? string.Empty);

                foreach (var grp in groupsByMaDon)
                {
                    var rqList = grp.ToList();
                    if (!rqList.Any())
                        continue;

                    // Lấy email nhà cung cấp
                    var toEmail = await _repo.GetSupplierEmailAsync(supplier);
                    if (string.IsNullOrEmpty(toEmail))
                        continue;

                    // Tạo file Excel tổng hợp cho mã đơn này
                    string maDon = rqList.FirstOrDefault()?.CHR_MaDon ?? "UnknownMaDon";
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                    string tempFileName = $"{supplier}_{maDon}_{timestamp}.xlsx";
                    string tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

                    var tableHtml = new StringBuilder();
                    tableHtml.AppendLine("<table border='1' style='border-collapse: collapse; width: 100%; font-family: Arial, sans-serif; font-size: 12px;'>");
                    tableHtml.AppendLine("<tr style='background-color: #f2f2f2; text-align: center; vertical-align: middle; font-weight: bold;'>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 120px;'>Số đơn yêu cầu báo giá<br/>Quotation Request Number</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 120px;'>Mã thiết bị<br/>Equipment code</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Mã hàng nội bộ<br/>BIVN's part code</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Mã hàng của NCC<br/>Vendor's good code</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 200px;'>Tên hàng VN<br/>Part name (Vietnamese)</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 150px;'>Số lượng<br/>Quantity</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 80px;'>Đơn vị <br/>Unit</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 100px;'>File Thiết kế<br/>Design</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Nhà Sản xuất<br/>Maker</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Mã nhà cung cấp<br/>Vendor code</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 150px;'>Tên nhà cung cấp<br/>Vendor name</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Kỳ hạn báo giá<br/>Deadline for submit quotation</th>");
                    tableHtml.AppendLine("</tr>");

                    // Tạo file excel tạm
                    using (var workbook = new XLWorkbook(templatePath))
                    {
                        var worksheet = workbook.Worksheet(1);
                        int rowIndex = 15;

                        foreach (var rq in rqList)
                        {
                            tableHtml.AppendLine("<tr style='vertical-align: middle;'>");
                            tableHtml.AppendLine("<td style='padding: 6px; border: 1px solid #999;'>" + (rq.CHR_MaDon ?? "") + "</td>");
                            tableHtml.AppendLine("<td style='padding: 6px; border: 1px solid #999;'>" + (rq.CHR_MaThietBi ?? "") + "</td>");
                            tableHtml.AppendLine("<td style='padding: 6px; border: 1px solid #999;'>" + (rq.CHR_MaHangNoiBo ?? "") + "</td>");
                            tableHtml.AppendLine("<td style='padding: 6px; border: 1px solid #999;'>" + (rq.CHR_MaHangNCC ?? "") + "</td>");
                            tableHtml.AppendLine("<td style='padding: 6px; border: 1px solid #999;'>" + (rq.NVCHR_NameVN ?? "") + "</td>");
                            tableHtml.AppendLine("<td style='padding: 6px; border: 1px solid #999; text-align: right;'>" + (rq.INT_SoLuong?.ToString() ?? "") + "</td>");
                            tableHtml.AppendLine("<td style='padding: 6px; border: 1px solid #999;'>" + (rq.NVCHR_DonVi ?? "") + "</td>");
                            tableHtml.AppendLine("<td style='padding: 6px; border: 1px solid #999; text-align: center;'>" + (rq.NVCHR_FileThietKe ?? "") + "</td>");
                            tableHtml.AppendLine("<td style='padding: 6px; border: 1px solid #999;'>" + (rq.NVCHR_NhaSanXuat ?? "") + "</td>");
                            tableHtml.AppendLine("<td style='padding: 6px; border: 1px solid #999;'>" + (rq.CHR_MaNCC ?? "") + "</td>");
                            tableHtml.AppendLine("<td style='padding: 6px; border: 1px solid #999;'>" + (rq.NVCHR_TenNCC ?? "") + "</td>");
                            tableHtml.AppendLine("<td style='padding: 6px; border: 1px solid #999; text-align: center;'>" + (rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? "") + "</td>");
                            tableHtml.AppendLine("</tr>");

                            // Ghi vào excel
                            worksheet.Cell(1, 3).Value = rq.NVCHR_TenNCC ?? string.Empty;
                            worksheet.Cell(2, 3).Value = rq.Diachi ?? string.Empty;
                            worksheet.Cell(rowIndex, 23).Value = rq.CHR_MaDon ?? string.Empty;
                            worksheet.Cell(rowIndex, 24).Value = rq.CHR_MaThietBi ?? string.Empty;
                            worksheet.Cell(rowIndex, 25).Value = rq.CHR_MaHangNoiBo ?? string.Empty;
                            worksheet.Cell(rowIndex, 26).Value = rq.CHR_MaHangNCC ?? string.Empty;
                            worksheet.Cell(rowIndex, 27).Value = rq.NVCHR_NameVN ?? string.Empty;
                            worksheet.Cell(rowIndex, 28).Value = rq.CHR_NameEN ?? string.Empty;
                            worksheet.Cell(rowIndex, 29).Value = rq.INT_SoLuong ?? string.Empty;
                            worksheet.Cell(rowIndex, 30).Value = rq.NVCHR_DonVi ?? string.Empty;
                            worksheet.Cell(rowIndex, 31).Value = rq.NVCHR_Rohs ?? string.Empty;
                            worksheet.Cell(rowIndex, 32).Value = rq.NVCHR_COCQ ?? string.Empty;
                            worksheet.Cell(rowIndex, 33).Value = rq.NVCHR_MSDS ?? string.Empty;
                            worksheet.Cell(rowIndex, 34).Value = rq.NVCHR_AnToan ?? string.Empty;
                            worksheet.Cell(rowIndex, 35).Value = rq.NVCHR_FileThietKe ?? string.Empty;
                            worksheet.Cell(rowIndex, 36).Value = rq.NVCHR_NhaSanXuat ?? string.Empty;
                            worksheet.Cell(rowIndex, 37).Value = rq.CHR_MaNCC ?? string.Empty;
                            worksheet.Cell(rowIndex, 38).Value = rq.NVCHR_TenNCC ?? string.Empty;
                            worksheet.Cell(rowIndex, 39).Value = rq.DTM_NgayMuonNhan?.ToString("yyyy-MM-dd") ?? "";
                            worksheet.Cell(rowIndex, 40).Value = rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? "";
                            worksheet.Range(rowIndex, 1, rowIndex, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

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
                            listSended.Add((int)rq.ID);
                            rowIndex++;
                        }

                        workbook.SaveAs(tempFilePath);
                    }

                    // Chuẩn bị danh sách file đính kèm (bao gồm file excel + file từ CHR_LinkFile), loại bỏ trùng
                    var attachmentPaths = new List<string> { tempFilePath };
                    var seenLinks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var rq in rqList)
                    {
                        try
                        {
                            var rawLink = rq.CHR_LinkFile ?? "";
                            var link = rawLink?.ToString()?.Trim();
                            if (string.IsNullOrEmpty(link))
                                continue;

                            if (seenLinks.Contains(link))
                                continue;

                            seenLinks.Add(link);

                            var fileRes = await _repo.GetFileToLinkAsync(link);
                            if (fileRes == null || !fileRes.Success || fileRes.Data == null)
                                continue;

                            var formFile = fileRes.Data;
                            var attachFileName = Path.GetFileName(formFile.FileName ?? link);
                            var attachTempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_" + attachFileName);

                            using (var stream = new FileStream(attachTempPath, FileMode.Create))
                            {
                                await formFile.CopyToAsync(stream);
                            }

                            attachmentPaths.Add(attachTempPath);
                        }
                        catch
                        {
                            // Nếu lấy file thất bại thì bỏ qua file đó và tiếp tục
                            continue;
                        }
                    }

                    // Đợi hệ thống giải phóng file
                    await Task.Delay(100);

                    // Chuẩn bị nội dung mail
                    var dearMail = "nhà cung cấp " + (rqList.FirstOrDefault()?.Ten ?? "") + " yêu cầu báo giá cho các mặt hàng như file đính kèm. Trân trọng cảm ơn!";
                    var bodyTable = mail.CHR_BODY + tableHtml.ToString();
                    var body = string.Format(bodyTable, dearMail);
                    var emailCC = string.IsNullOrEmpty(mail.CHR_CC) ? mailPICTo : mail.CHR_CC;

                    var emailForm = new EmailFormNetMailCustomSendMultiAttachFile
                    {
                        mail_from = mail.CHR_FROM,
                        mail_to = toEmail,
                        mail_cc = emailCC,
                        mail_bcc = mail.CHR_BCC,
                        title = (rqList.FirstOrDefault()?.ShortName ?? rqList.FirstOrDefault()?.Ten) + " - Số đơn: " + maDon,
                        body = body,
                        attachmentPaths = attachmentPaths
                    };

                    var sendResult = await EmailSender.SendEmailNotifyCustomSendMultiAttachFileAsync(emailForm);

                    // Xóa file tạm
                    foreach (var path in attachmentPaths)
                    {
                        try
                        {
                            if (File.Exists(path)) File.Delete(path);
                        }
                        catch { }
                    }

                    if (!sendResult.Success)
                    {
                        // Nếu gửi thất bại, loại bỏ các ID vừa thêm
                        var idsToRemove = rqList.Select(r => (int)r.ID).ToList();
                        listSended = listSended.Except(idsToRemove).ToList();
                    }
                }
            }

            if (listSended.Any())
            {
                await _repo.UpdateMailSentStatusAsync(listSended);
            }
            if (listBaoGiaDetail.Any())
            {
                await _repo.InsertBaoGiaDetailAsync(listBaoGiaDetail);
            }

            result.Success = true;
            result.Message = "Mail sent successfully";
            return result;
        }
        public async Task<GenericResponse<bool>> SendMailToSupplierAggregatedAsync()
        {
            string dearMail = "";

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

            // lấy mail template
            var mail = await _repo.GetMailByIdAsync(19);
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
            var listBaoGiaDetail = new List<BaoGia_Detail_of_Quotation>();

            foreach (var supplier in suppliers)
            {
                string mailTk = "";
                // lay danh sach don link kien xin bao gia cua nha cung cap nay
                var listRq = await _repo.GetBaoGiaRequestBySupplierAsync(supplier);
                if (listRq == null || !listRq.Any())
                {
                    continue;
                }

                // lay email nha cung cap
                var toEmail = await _repo.GetSupplierEmailAsync(supplier);
                if (string.IsNullOrEmpty(toEmail))
                {
                    continue;
                }

                // Group by order (CHR_MaDon)
                var groupedByOrder = listRq.GroupBy(r => r.CHR_MaDon).Where(g => g.Key != null);

                // Build tablePicInfo for the supplier
                var tablePicInfo = new StringBuilder();
                tablePicInfo.AppendLine("<table border='1' style='border-collapse: collapse; width: 100%; font-family: Arial, sans-serif; font-size: 12px; margin-bottom: 20px;'>");
                tablePicInfo.AppendLine("<tr style='background-color: #d9e1f2; text-align: center; vertical-align: middle; font-weight: bold;'>");
                tablePicInfo.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 200px;'>Quotation Request Number<br/>Số yêu cầu báo giá</th>");
                tablePicInfo.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 300px;'>PIC explain about Product description<br/>Đảm nhiệm giải thích nếu có thắc mắc về thiết kế<br/>(Khi gửi báo giá vui lòng ko CC cho email này)</th>");
                tablePicInfo.AppendLine("</tr>");

                var uniquePics = new HashSet<string>();
                foreach (var rq in listRq)
                {
                    string picEmail = rq.CHR_CreateBy + "@brothergroup.net";
                    if (uniquePics.Add(picEmail))
                    {
                        tablePicInfo.AppendLine("<tr style='vertical-align: middle;'>");
                        tablePicInfo.AppendLine($"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.CHR_MaDon ?? ""}</td>");
                        tablePicInfo.AppendLine($"<td style='padding: 6px; border: 1px solid #999;'>{picEmail}</td>");
                        tablePicInfo.AppendLine("</tr>");
                    }
                }
                tablePicInfo.AppendLine("</table>");

                foreach (var orderGroup in groupedByOrder)
                {
                    var orderRq = orderGroup.ToList();

                    // Tạo file Excel tổng hợp cho nhà cung cấp này và đơn này
                    string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "TmSendMailNew.xlsx");
                    string tempFileName = $"{supplier}_{orderGroup.Key}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    string tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

                    var uniqueAttachments = new HashSet<string>();
                    var attachmentPaths = new List<string>();

                    using (var workbook = new XLWorkbook(templatePath))
                    {
                        var worksheet = workbook.Worksheet(1);
                        worksheet.Column(30).Hide();

                        int rowIndex = 13;

                        foreach (var rq in orderRq)
                        {
                            // Tổng hợp Other request từ các trường ROHS, COCQ, MSDS, AnToan
                            var otherRequestList = new List<string>();
                            if (!string.IsNullOrEmpty(rq.NVCHR_Rohs))
                                otherRequestList.Add($"ROHS: {rq.NVCHR_Rohs}");
                            if (!string.IsNullOrEmpty(rq.NVCHR_COCQ))
                                otherRequestList.Add($"COCQ: {rq.NVCHR_COCQ}");
                            if (!string.IsNullOrEmpty(rq.NVCHR_MSDS))
                                otherRequestList.Add($"MSDS: {rq.NVCHR_MSDS}");
                            if (!string.IsNullOrEmpty(rq.NVCHR_AnToan))
                                otherRequestList.Add($"An toàn: {rq.NVCHR_AnToan}");

                            string otherRequest = string.Join(" & ", otherRequestList);

                            // Ghi dữ liệu vào file Excel theo cột mới
                            worksheet.Cell(1, 3).Value = rq.NVCHR_TenNCC ?? string.Empty;
                            worksheet.Cell(2, 3).Value = rq.Diachi ?? string.Empty;

                            worksheet.Cell(rowIndex, 1).Value = rq.CHR_MaDon ?? string.Empty;
                            worksheet.Cell(rowIndex, 2).Value = rq.CHR_MaThietBi ?? string.Empty;
                            worksheet.Cell(rowIndex, 3).Value = rq.CHR_MaHangNoiBo ?? string.Empty;
                            worksheet.Cell(rowIndex, 4).Value = rq.CHR_MaHangNCC ?? string.Empty;
                            worksheet.Cell(rowIndex, 5).Value = rq.NVCHR_NameVN ?? string.Empty;
                            worksheet.Cell(rowIndex, 6).Value = rq.INT_SoLuong ?? string.Empty;
                            worksheet.Cell(rowIndex, 7).Value = rq.NVCHR_DonVi ?? string.Empty;
                            worksheet.Cell(rowIndex, 8).Value = otherRequest;
                            worksheet.Cell(rowIndex, 9).Value = rq.NVCHR_NhaSanXuat ?? string.Empty;
                            worksheet.Cell(rowIndex, 10).Value = rq.CHR_MaNCC ?? string.Empty;
                            worksheet.Cell(rowIndex, 27).Value = rq.NVCHR_FileThietKe ?? "";
                            worksheet.Cell(rowIndex, 28).Value = rq.DTM_NgayMuonNhan?.ToString("yyyy-MM-dd") ?? "";
                            worksheet.Cell(rowIndex, 29).Value = rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? "";
                            worksheet.Cell(rowIndex, 31).Value = rq.CHR_CreateBy + "@brothergroup.net";
                            worksheet.Cell(rowIndex, 32).Value = rq.ID ?? "";

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

                            // Ghi nhận ID để cập nhật trạng thái sau khi gửi mail thành công
                            listSended.Add((int)rq.ID);

                            // Collect unique attachments
                            if (!string.IsNullOrEmpty(rq.CHR_LinkFile) && uniqueAttachments.Add(rq.CHR_LinkFile))
                            {
                                var fileResponse = await _repo.GetFileToLinkAsync(rq.CHR_LinkFile);
                                if (fileResponse.Success && fileResponse.Data != null)
                                {
                                    string tempAttachName = Path.GetFileName(rq.CHR_LinkFile);
                                    string tempAttachPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{tempAttachName}");
                                    using (var stream = fileResponse.Data.OpenReadStream())
                                    using (var fileStream = new FileStream(tempAttachPath, FileMode.Create))
                                    {
                                        await stream.CopyToAsync(fileStream);
                                    }
                                    attachmentPaths.Add(tempAttachPath);
                                }
                            }

                            rowIndex++;
                        }
                        workbook.SaveAs(tempFilePath);
                    }

                    // Đợi file được giải phóng hoàn toàn
                    await Task.Delay(100);

                    // Chuẩn bị nội dung email
                    dearMail = "nhà cung cấp " + (orderRq.FirstOrDefault()?.Ten ?? "") + " yêu cầu báo giá cho các mặt hàng như file đính kèm. Rất mong nhận được phản hồi báo giá sớm nhất từ quý nhà cung cấp. Trân trọng cảm ơn!";

                    // Lấy ngày yêu cầu báo giá (ưu tiên DTM_KyHan, nếu null thì dùng ngày hiện tại)
                    string requestDate = orderRq.FirstOrDefault()?.DTM_KyHan?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");

                    // Tiêu đề mail mới: Tên ngắn NCC + Mã đơn + Ngày yêu cầu báo giá
                    string shortName = orderRq.FirstOrDefault()?.ShortName ?? orderRq.FirstOrDefault()?.Ten ?? "NCC";
                    string titleMail = $"{shortName} - {orderGroup.Key} - {requestDate}";

                    var bodyTable = mail.CHR_BODY + tablePicInfo.ToString();
                    var body = string.Format(bodyTable, dearMail, mailTk);
                    var emailCC = string.IsNullOrEmpty(mail.CHR_CC) ?
                        mailPICTo : mail.CHR_CC;

                    attachmentPaths.Insert(0, tempFilePath);

                    var emailForm = new EmailFormNetMailCustomSendMultiAttachFile
                    {
                        mail_from = mail.CHR_FROM,
                        mail_to = toEmail,
                        mail_cc = emailCC,
                        mail_bcc = mail.CHR_BCC,
                        title = titleMail,
                        body = body,
                        attachmentPaths = attachmentPaths
                    };

                    var sendResult = await EmailSender.SendEmailNotifyCustomSendMultiAttachFileAsync(emailForm);

                    // Xóa file tạm sau khi gửi email
                    try
                    {
                        if (File.Exists(tempFilePath))
                        {
                            File.Delete(tempFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Không thể xóa file tạm: {ex.Message}");
                    }

                    foreach (var attachPath in attachmentPaths.Skip(1)) // Skip the Excel, already deleted
                    {
                        try
                        {
                            if (File.Exists(attachPath))
                            {
                                File.Delete(attachPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Không thể xóa file đính kèm tạm: {ex.Message}");
                        }
                    }

                    // Nếu gửi mail thất bại thì xóa các ID đã thêm vào listSended
                    if (!sendResult.Success)
                    {
                        var idsToRemove = orderRq.Select(r => (int)r.ID).ToList();
                        listSended = listSended.Except(idsToRemove).ToList();
                        listBaoGiaDetail = listBaoGiaDetail.Where(d => !idsToRemove.Contains(d.ID_RequestQuote)).ToList();
                    }
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
        // gửi mail xin xác nhận lại tên hàng
        public async Task<GenericResponse<bool>> SendMailCofirmNaneOfVendor()
        {
            var result = new GenericResponse<bool>();
            try
            {
                var data = await _repo.GetRequestNeedToConfirmNameAsync();
                if (data == null || !data.Any())
                {
                    result.Success = false;
                    result.Message = "No requests need to confirm name";
                    return result;
                }
                // Lấy thông tin mẫu mail từ cơ sở dữ liệu
                var mailTemplate = await _repo.GetMailByIdAsync(22);
                if(mailTemplate == null)
                {
                    result.Success = false;
                    result.Message = "Mail template not found";
                    return result;
                }

                var bodyTemplate = string.IsNullOrWhiteSpace(mailTemplate.CHR_BODY)
                    ? result.Message = "Mail template body is empty"
                    : mailTemplate.CHR_BODY;

                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "TmSendMail_ConfirmName.xlsx");
                var groupsByVendor = data
                    .Where(x => !string.IsNullOrWhiteSpace((string?)x.CHR_CodeNCC))
                    .GroupBy(x => ((string?)x.CHR_CodeNCC ?? string.Empty).Trim())
                    .ToList();

                if (!groupsByVendor.Any())
                {
                    result.Success = false;
                    result.Message = "No suppliers to notify";
                    return result;
                }

                int successCount = 0;
                int failCount = 0;

                foreach (var vendorGroup in groupsByVendor)
                {
                    var vendorCode = vendorGroup.Key;
                    var dataVendor = vendorGroup.ToList();
                    if (!dataVendor.Any())
                    {
                        continue;
                    }


                    var toEmail = await _repo.GetSupplierInfoAsync(vendorCode);
                    if (toEmail == null || string.IsNullOrWhiteSpace((string?)toEmail.CHR_Mail))
                    {
                        failCount++;
                        continue;
                    }

                    string tempFileName = $"{vendorCode}_{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";
                    string tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

                    try
                    {
                        using (var workbook = new XLWorkbook(templatePath))
                        {
                            var worksheet = workbook.Worksheet(1);
                            worksheet.Column(30).Hide();

                            int rowIndex = 13;
                            foreach (var item in dataVendor)
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
                                worksheet.Cell(rowIndex, 16).Value = item.FL_USD ?? string.Empty;
                                worksheet.Cell(rowIndex, 17).Value = item.FL_VND ?? string.Empty;
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

                                rowIndex++;
                            }

                            workbook.SaveAs(tempFilePath);
                        }

                        await Task.Delay(100);

                        var firstItem = dataVendor.FirstOrDefault();
                        string vendorName = (string?)firstItem?.NVCHR_NameNCC ?? "Supplier";
                        string shortName = (string?)firstItem?.ShortName ?? vendorName;
                        string expectedDeadline = DateTime.Now.Date.AddDays(1).AddHours(10).ToString("yyyy-MM-dd HH:mm");

                        string body = string.Format(bodyTemplate, shortName, toEmail.PICName ?? vendorName, expectedDeadline);
                        string titleMail = $"{shortName} - Sửa tên hàng hóa trên báo giá / Please revise the part name on the quotation.";
                        var emailCC = string.IsNullOrEmpty(mailTemplate.CHR_CC) ? mailPICTo : mailTemplate.CHR_CC;

                        var emailForm = new EmailFormNetMailCustomSendMultiAttachFile
                        {
                            mail_from = mailTemplate.CHR_FROM,
                            mail_to = toEmail.CHR_Mail,
                            mail_cc = emailCC,
                            mail_bcc = mailTemplate.CHR_BCC,
                            title = titleMail,
                            body = body,
                            attachmentPaths = new List<string> { tempFilePath }
                        };

                        //var emailForm = new EmailFormNetMailCustomSendMultiAttachFile
                        //{
                        //    mail_from = "nguyenduy.khanh@brother-bivn.com.vn",
                        //    mail_to = "nguyenduy.khanh@brother-bivn.com.vn",
                        //    mail_cc = "nguyenduy.khanh@brother-bivn.com.vn",
                        //    mail_bcc = "nguyenduy.khanh@brother-bivn.com.vn",
                        //    title = titleMail,
                        //    body = body,
                        //    attachmentPaths = new List<string> { tempFilePath }
                        //};

                        var sendResult = await EmailSender.SendEmailNotifyCustomSendMultiAttachFileAsync(emailForm);
                        if (sendResult.Success)
                        {
                            successCount++;
                        }
                        else
                        {
                            failCount++;
                        }
                    }
                    catch
                    {
                        failCount++;
                    }
                    finally
                    {
                        try
                        {
                            if (File.Exists(tempFilePath))
                            {
                                File.Delete(tempFilePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Không thể xóa file tạm: {ex.Message}");
                        }
                    }
                }

                result.Success = successCount > 0 && failCount == 0;
                result.Message = $"Sent: {successCount}, Failed: {failCount}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error sending confirmation mail: {ex.Message}";
            }
            return result;
        }
        // Tự động cập nhật trang thái đơn
        public async Task<GenericResponse<bool>> AutoUpdateRequestStatusAsync()
        {
            var result = new GenericResponse<bool>();
            try
            {
                var updateResult = await _repo.AutoUpdateRequestStatusAsync();
                if (updateResult)
                {
                    result.Success = true;
                    result.Message = "Request statuses updated successfully.";
                }
                else
                {
                    result.Success = false;
                    result.Message = "No request statuses were updated.";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error updating request statuses: {ex.Message}";
            }
            return result;
        }
    }
}
