using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using System;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using Microsoft.AspNetCore.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class SendMailService : BaseService<TM_MASTER_MAIL, int ,TM_MASTER_MAILDTO>, ISendMailService
    {
        private readonly ISendMailRepository _repo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
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
        public async Task<GenericResponse<bool>> SendMailToSupplierOrDerByCategoryAsync()
        {
            string dearMail = "";
            string titleMail = "";
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
            var mail = await _repo.GetMailByIdAsync(19);
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

                // Group requests by request code (CHR_MaDon) and part category (NVCHR_ChungLoai)
                var groupsByMaDon = listRq.GroupBy(r => new { MaDon = r.CHR_MaDon ?? string.Empty, ChungLoai = r.NVCHR_ChungLoai ?? string.Empty });
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "ExportSampleExcel.xlsx");

                foreach (var grp in groupsByMaDon)
                {
                    var rqList = grp.ToList();
                    if (!rqList.Any())
                        continue;
                    // lay email nha cung cap
                    var toEmail = await _repo.GetSupplierEmaiCategorylAsync(item, rqList.FirstOrDefault()?.NVCHR_ChungLoai);
                    if (string.IsNullOrEmpty(toEmail))
                    {
                        continue;
                    }

                    // Build table HTML for this mã đơn
                    var tableHtmlPerGroup = "<table border='1' style='border-collapse: collapse; width: 100%; font-family: Arial, sans-serif; font-size: 12px;'>";
                    tableHtmlPerGroup += "<tr style='background-color: #f2f2f2; text-align: center; vertical-align: middle; font-weight: bold;'>";
                    tableHtmlPerGroup += "<th style='padding: 8px; border: 1px solid #999; min-width: 120px;'>Số đơn yêu cầu báo giá<br/>Quotation Request Number</th>";
                    tableHtmlPerGroup += "<th style='padding: 8px; border: 1px solid #999; min-width: 120px;'>Mã thiết bị<br/>Equipment code</th>";
                    tableHtmlPerGroup += "<th style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Mã hàng nội bộ<br/>BIVN's part code</th>";
                    tableHtmlPerGroup += "<th style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Mã hàng của NCC<br/>Vendor's good code</th>";
                    tableHtmlPerGroup += "<th style='padding: 8px; border: 1px solid #999; min-width: 200px;'>Tên hàng VN dùng để mở thủ tục hải quan (dự thảo)(*)<br/>Part name (Vietnamese)</th>";
                    tableHtmlPerGroup += "<th style='padding: 8px; border: 1px solid #999; min-width: 150px;'>Tên hàng tiếng anh(*)<br/>Part name (English)</th>";
                    tableHtmlPerGroup += "<th style='padding: 8px; border: 1px solid #999; min-width: 80px;'>Số lượng<br/>Quantity(*)</th>";
                    tableHtmlPerGroup += "<th style='padding: 8px; border: 1px solid #999; min-width: 80px;'>Đơn vị <br/>Unit(*)</th>";
                    tableHtmlPerGroup += "<th style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Chủng loại hàng<br/>Part category</th>";
                    tableHtmlPerGroup += "<th style='padding: 8px; border: 1px solid #999; min-width: 100px;'>File Thiết kế<br/>Design(*)</th>";
                    tableHtmlPerGroup += "<th style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Nhà Sản xuất<br/>Maker</th>";
                    tableHtmlPerGroup += "<th style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Mã nhà cung cấp<br/>Vendor code</th>";
                    tableHtmlPerGroup += "<th style='padding: 8px; border: 1px solid #999; min-width: 150px;'>Tên nhà cung cấp<br/>Vendor name</th>";
                    tableHtmlPerGroup += "<th style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Ngày muốn nhận hàng<br/>Desired delivery date(*)</th>";
                    tableHtmlPerGroup += "<th style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Kỳ hạn báo giá<br/>Deadline for submit quotation</th>";
                    tableHtmlPerGroup += "</tr>";

                    // Create temporary excel file per group
                    string maDon = rqList.FirstOrDefault()?.CHR_MaDon ?? "UnknownMaDon";
                    string tempFileName = $"{item}_{maDon}.xlsx";
                    string tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

                    using (var workbook = new XLWorkbook(templatePath))
                    {
                        var worksheet = workbook.Worksheet(1);
                        int rowIndex = 15;
                        foreach (var rq in rqList)
                        {
                            tableHtmlPerGroup += "<tr style='vertical-align: middle;'>";
                            tableHtmlPerGroup += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaDon ?? ""}</td>";
                            tableHtmlPerGroup += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaThietBi ?? ""}</td>";
                            tableHtmlPerGroup += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaHangNoiBo ?? ""}</td>";
                            tableHtmlPerGroup += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaHangNCC ?? ""}</td>";
                            tableHtmlPerGroup += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_NameVN ?? ""}</td>";
                            tableHtmlPerGroup += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_NameEN ?? ""}</td>";
                            tableHtmlPerGroup += $"<td style='padding: 6px; border: 1px solid #999; text-align: right;'>{rq.INT_SoLuong?.ToString() ?? ""}</td>";
                            tableHtmlPerGroup += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_DonVi ?? ""}</td>";
                            tableHtmlPerGroup += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_ChungLoai ?? ""}</td>";
                            tableHtmlPerGroup += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.NVCHR_FileThietKe ?? ""}</td>";
                            tableHtmlPerGroup += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_NhaSanXuat ?? ""}</td>";
                            tableHtmlPerGroup += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaNCC ?? ""}</td>";
                            tableHtmlPerGroup += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_TenNCC ?? ""}</td>";
                            tableHtmlPerGroup += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.DTM_NgayMuonNhan?.ToString("yyyy-MM-dd") ?? ""}</td>";
                            tableHtmlPerGroup += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? ""}</td>";
                            tableHtmlPerGroup += "</tr>";

                        // phần của file dữ liệu đính kèm
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
                            rowIndex++;
                        }

                        workbook.SaveAs(tempFilePath);
                    }

                    tableHtmlPerGroup += "</table>";
                    var bodyTable = mail.CHR_BODY + tableHtmlPerGroup;
                    var body = string.Format(bodyTable, "nhà cung cấp");
                    var email = string.IsNullOrEmpty(mail.CHR_CC) ?
                    "bivn-pur-indirectpart@brother-bivn.com.vn" : mail.CHR_CC;
                    var emailForm = new EmailFormNetMailCustomSendMultiAttachFile
                    {
                        mail_from = mail.CHR_FROM,
                        mail_to = "nguyenduy.khanh@brother-bivn.com.vn", //toEmail,
                        mail_cc = "nguyenduy.khanh@brother-bivn.com.vn",//email,
                        mail_bcc = mail.CHR_BCC,
                        title = (rqList.FirstOrDefault()?.ShortName ?? rqList.FirstOrDefault()?.Ten) + " - Deadline: " + (rqList.FirstOrDefault()?.DTM_KyHan?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd")) + " - Số đơn: " + (rqList.FirstOrDefault()?.CHR_MaDon ?? maDon),
                        body = body,
                        attachmentPaths = new List<string> { tempFilePath }
                    };
                    var sendResult = await EmailSender.SendEmailNotifyCustomSendMultiAttachFileAsync(emailForm);
                    if (sendResult.Success)
                    {
                        listSended.AddRange(listRq.Select(r => (int)r.ID));
                    }
                }
            }
            // cap nhat trang thai da gui mail
            if (listSended.Any())
            {
              // await _repo.UpdateMailSentStatusAsync(listSended);
            }
            if (listBaoGiaDetail.Any())
            {
               //await _repo.InsertBaoGiaDetailAsync(listBaoGiaDetail);
            }
            return new GenericResponse<bool>
            {
                Success = true,
                Message = "Mail sent successfully"
            };
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
                    var emailCC = string.IsNullOrEmpty(mail.CHR_CC) ? "bivn-pur-indirectpart@brother-bivn.com.vn" : mail.CHR_CC;

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
                        "bivn-pur-indirectpart@brother-bivn.com.vn" : mail.CHR_CC;

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
        public async Task<GenericResponse<bool>> SendMailToSupplierAsyncOlder()
        {
            string dearMail = "";
            string titleMail = "";
            string mailTk = "";
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
            var mail = await _repo.GetMailByIdAsync(19);
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

                // Group requests by request code (CHR_MaDon) ONLY - bỏ ChungLoai
                var groupsByMaDon = listRq.GroupBy(r => r.CHR_MaDon ?? string.Empty);

                foreach (var grp in groupsByMaDon)
                {
                    var rqList = grp.ToList();
                    if (!rqList.Any())
                        continue;

                    // lay email nha cung cap (không cần category)
                    var toEmail = await _repo.GetSupplierEmailAsync(item);
                    if (string.IsNullOrEmpty(toEmail))
                    {
                        continue;
                    }

                    string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "ExportSampleExcel.xlsx");

                    // Tạo tên file với mã đơn và timestamp
                    string maDon = rqList.FirstOrDefault()?.CHR_MaDon ?? "UnknownMaDon";
                    string tempFileName = $"{item}_{maDon}.xlsx";
                    string tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

                    // Build table HTML cho mã đơn này
                    var tableHtml = new StringBuilder();
                    tableHtml.AppendLine("<table border='1' style='border-collapse: collapse; width: 100%; font-family: Arial, sans-serif; font-size: 12px;'>");
                    tableHtml.AppendLine("<tr style='background-color: #f2f2f2; text-align: center; vertical-align: middle; font-weight: bold;'>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 120px;'>Số đơn yêu cầu báo giá<br/>Quotation Request Number</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 120px;'>Mã thiết bị<br/>Equipment code</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Mã hàng nội bộ<br/>BIVN's part code</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Mã hàng của NCC<br/>Vendor's good code</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 200px;'>Tên hàng VN dùng để mở thủ tục hải quan (dự thảo)(*)<br/>Part name (Vietnamese)</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 150px;'>Tên hàng tiếng anh(*)<br/>Part name (English)</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 80px;'>Số lượng<br/>Quantity(*)</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 80px;'>Đơn vị <br/>Unit(*)</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Chủng loại hàng<br/>Part category</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 100px;'>File Thiết kế<br/>Design(*)</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Nhà Sản xuất<br/>Maker</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Mã nhà cung cấp<br/>Vendor code</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 150px;'>Tên nhà cung cấp<br/>Vendor name</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Ngày muốn nhận hàng<br/>Desired delivery date(*)</th>");
                    tableHtml.AppendLine("<th style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Kỳ hạn báo giá<br/>Deadline for submit quotation</th>");
                    tableHtml.AppendLine("</tr>");

                    using (var workbook = new XLWorkbook(templatePath))
                    {
                        var worksheet = workbook.Worksheet(1);
                        int rowIndex = 15;

                        worksheet.Column(44).Hide();
                        foreach (var rq in rqList)
                        {
                            // Thêm dòng dữ liệu vào table HTML
                            tableHtml.AppendLine("<tr style='vertical-align: middle;'>");
                            tableHtml.AppendLine($"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaDon ?? ""}</td>");
                            tableHtml.AppendLine($"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaThietBi ?? ""}</td>");
                            tableHtml.AppendLine($"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaHangNoiBo ?? ""}</td>");
                            tableHtml.AppendLine($"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaHangNCC ?? ""}</td>");
                            tableHtml.AppendLine($"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_NameVN ?? ""}</td>");
                            tableHtml.AppendLine($"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_NameEN ?? ""}</td>");
                            tableHtml.AppendLine($"<td style='padding: 6px; border: 1px solid #999; text-align: right;'>{rq.INT_SoLuong?.ToString() ?? ""}</td>");
                            tableHtml.AppendLine($"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_DonVi ?? ""}</td>");
                            tableHtml.AppendLine($"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_ChungLoai ?? ""}</td>");
                            tableHtml.AppendLine($"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.NVCHR_FileThietKe ?? ""}</td>");
                            tableHtml.AppendLine($"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_NhaSanXuat ?? ""}</td>");
                            tableHtml.AppendLine($"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaNCC ?? ""}</td>");
                            tableHtml.AppendLine($"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_TenNCC ?? ""}</td>");
                            tableHtml.AppendLine($"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.DTM_NgayMuonNhan?.ToString("yyyy-MM-dd") ?? ""}</td>");
                            tableHtml.AppendLine($"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? ""}</td>");
                            tableHtml.AppendLine("</tr>");

                            // phần của file dữ liệu đính kèm
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
                            worksheet.Cell(rowIndex, 38).Value = rq.ShortName ?? rq.NVCHR_TenNCC ?? string.Empty;
                            worksheet.Cell(rowIndex, 39).Value = rq.DTM_NgayMuonNhan?.ToString("yyyy-MM-dd") ?? "";
                            worksheet.Cell(rowIndex, 40).Value = rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? "";
                            worksheet.Cell(rowIndex, 41).Value = rq.NVCHR_FileThietKe ?? "";
                            worksheet.Cell(rowIndex, 44).Value = rq.ID ?? "";
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
                            mailTk = rq.CHR_CreateBy + "@brothergroup.net";
                            dearMail = "nhà cung cấp " + rq.Ten + " yêu cầu báo giá cho các mặt hàng như file đính kèm. Rất mong nhận được phản hồi báo giá sớm nhất từ quý nhà cung cấp. Trân trọng cảm ơn!";
                            titleMail = (rq.ShortName ?? rq.Ten) + " - Deadline: " + (rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd")) + " - Số đơn yêu cầu: " + rq.CHR_MaDon;
                            rowIndex++;
                        }

                        tableHtml.AppendLine("</table>");
                        workbook.SaveAs(tempFilePath);
                    }

                    // Đợi file được giải phóng hoàn toàn
                    await Task.Delay(100);

                    var bodyTable = mail.CHR_BODY + tableHtml.ToString();
                    var body = string.Format(bodyTable, dearMail, mailTk);
                    var email = string.IsNullOrEmpty(mail.CHR_CC) ?
                    "bivn-pur-indirectpart@brother-bivn.com.vn" : mail.CHR_CC;

                    var emailForm = new EmailFormNetMailCustomSendMultiAttachFile
                    {
                        mail_from = mail.CHR_FROM,
                        mail_to = "nguyenduy.khanh@brother-bivn.com.vn",//toEmail,
                        mail_cc = "nguyenduy.khanh@brother-bivn.com.vn",//email,
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
                        // Log lỗi nếu cần
                        Console.WriteLine($"Không thể xóa file tạm: {ex.Message}");
                    }

                    if (sendResult.Success)
                    {
                        listSended.AddRange(rqList.Select(r => (int)r.ID));
                    }
                }
            }

            // cap nhat trang thai da gui mail
            if (listSended.Any())
            {
               //await _repo.UpdateMailSentStatusAsync(listSended);
            }

            if (listBaoGiaDetail.Any())
            {
               //await _repo.InsertBaoGiaDetailAsync(listBaoGiaDetail);
            }

            return new GenericResponse<bool>
            {
                Success = true,
                Message = "Mail sent successfully"
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
                string tempFileName = $"{supplier}_{DateTime.Now:yyyyMMdd}.xlsx";
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
                        if (mailTk != rq.CHR_CreateBy + "@brothergroup.net" && checkMaDon != rq.CHR_MaDon)
                        {
                            mailTk = rq.CHR_CreateBy + "@brothergroup.net";
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
                        worksheet.Cell(rowIndex, 31).Value = rq.CHR_CreateBy + "@brothergroup.net";            // PIC explain - để trống

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
                    "bivn-pur-indirectpart@brother-bivn.com.vn" : mail.CHR_CC;

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

                // Nếu gửi mail thất bại thì xóa các ID đã thêm vào listSended
                if (!sendResult.Success)
                {
                    // Xóa các ID vừa thêm của supplier này
                    var idsToRemove = listRq.Select(r => (int)r.ID).ToList();
                    listSended = listSended.Except(idsToRemove).ToList();
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
        public async Task<GenericResponse<bool>> SendMailToSupplierByRequestCodeAndCategoryAsync(string requestCode)
        {
            string dearMail = "";
            string titleMail = "";
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
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "ExportSampleExcel.xlsx");
            // danh sach cac don da gui mail
            var listSended = new List<int>();

            foreach (var item in suppliers)
            {
                var rqByNCC = listRq.Where(r => r.CHR_MaNCC == item).ToList();
                var groupsByCatergory = rqByNCC.GroupBy(r => new { ChungLoai = r.NVCHR_ChungLoai ?? string.Empty });

                foreach (var grp in groupsByCatergory)
                {
                    var rqList = grp.ToList();
                    if (!rqList.Any())
                        continue;

                    // lay email nha cung cap
                    var toEmail = await _repo.GetSupplierEmaiCategorylAsync(item, rqList.FirstOrDefault()?.NVCHR_ChungLoai);
                    if (string.IsNullOrEmpty(toEmail))
                    {
                        continue;
                    }

                    // Tạo tên file với milliseconds để tránh trùng lặp
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                    string tempFileName = $"{item}_{requestCode}_{timestamp}.xlsx";
                    string tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

                    // Tạo file Excel trong using block riêng
                    using (var workbook = new XLWorkbook(templatePath))
                    {
                        var worksheet = workbook.Worksheet(1); // Assuming sheet1
                        int rowIndex = 15; // Start from row 15

                        // tao bang html
                        var tableHtml = "<table border='1' style='border-collapse: collapse; width: 100%; font-family: Arial, sans-serif; font-size: 12px;'>";

                        // Row 1 - Header chính
                        tableHtml += "<tr style='background-color: #f2f2f2; text-align: center; vertical-align: middle; font-weight: bold;'>";
                        tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 120px;'>Số đơn yêu cầu báo giá<br/>Quotation Request Number</th>";
                        tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 120px;'>Mã thiết bị<br/>Equipment code</th>";
                        tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Mã hàng nội bộ<br/>BIVN's part code</th>";
                        tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Mã hàng của NCC<br/>Vendor's good code</th>";
                        tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 200px;'>Tên hàng VN dùng để mở thủ tục hải quan (dự thảo)(*)<br/>Part name (Vietnamese)</th>";
                        tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 150px;'>Tên hàng tiếng anh(*)<br/>Part name (English)</th>";
                        tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 80px;'>Số lượng<br/>Quantity(*)</th>";
                        tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 80px;'>Đơn vị <br/>Unit(*)</th>";
                        tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Chủng loại hàng<br/>Part category</th>";
                        tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 100px;'>File Thiết kế<br/>Design(*)</th>";
                        tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Nhà Sản xuất<br/>Maker</th>";
                        tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 130px;'>Mã nhà cung cấp<br/>Vendor code</th>";
                        tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 150px;'>Tên nhà cung cấp<br/>Vendor name</th>";
                        tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Ngày muốn nhận hàng<br/>Desired delivery date(*)</th>";
                        tableHtml += "<th style='padding: 8px; border: 1px solid #999; min-width: 140px;'>Kỳ hạn báo giá<br/>Deadline for submit quotation</th>";
                        tableHtml += "<tr>";

                        foreach (var rq in rqList)
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
                            tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.NVCHR_FileThietKe ?? ""}</td>";
                            tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_NhaSanXuat ?? ""}</td>";
                            tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.CHR_MaNCC ?? ""}</td>";
                            tableHtml += $"<td style='padding: 6px; border: 1px solid #999;'>{rq.NVCHR_TenNCC ?? ""}</td>";
                            tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.DTM_NgayMuonNhan?.ToString("yyyy-MM-dd") ?? ""}</td>";
                            tableHtml += $"<td style='padding: 6px; border: 1px solid #999; text-align: center;'>{rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? ""}</td>";
                            tableHtml += "</tr>";

                            // phần của file dữ liệu đính kèm 
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

                            // Add thin border to the data row
                            worksheet.Range(rowIndex, 1, rowIndex, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            dearMail = "nhà cung cấp " + rq.Ten + " yêu cầu báo giá cho các mặt hàng như file đính kèm. Rất mong nhận được phản hồi báo giá sớm nhất từ quý nhà cung cấp. Trân trọng cảm ơn!";
                            titleMail = (rq.ShortName ?? rq.Ten) + " - Deadline: " + (rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd")) + " - Số đơn yêu cầu: " + rq.CHR_MaDon;
                            rowIndex++;
                        }

                        tableHtml += "</table>";
                        workbook.SaveAs(tempFilePath);
                        var bodyTable = mail.CHR_BODY + tableHtml;
                        var body = string.Format(bodyTable, dearMail);
                        var email = string.IsNullOrEmpty(mail.CHR_CC) ?
                    "bivn-pur-indirectpart@brother-bivn.com.vn" : mail.CHR_CC;

                        var emailForm = new EmailFormNetMailCustomSendMultiAttachFile
                        {
                            mail_from = mail.CHR_FROM,
                            mail_to = "nguyenduy.khanh@brother-bivn.com.vn" ,//toEmail,
                            mail_cc = "nguyenduy.khanh@brother-bivn.com.vn",//email,
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
                            listSended.AddRange(rqList.Select(r => (int)r.ID));
                        }
                    }

                }
            }
            // cap nhat trang thai da gui mail
            if (listSended.Count != 0)
            {
                //await _repo.UpdateMailSentStatusAsync(listSended);
            }
            return new GenericResponse<bool>
            {
                Success = true,
                Message = "Mail sent successfully"
            };
        }


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
                        if (mailTk != rq.CHR_CreateBy + "@brothergroup.net")
                        {
                            mailTk = rq.CHR_CreateBy + "@brothergroup.net";
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
                        worksheet.Cell(rowIndex, 31).Value = rq.CHR_CreateBy + "@brothergroup.net";            // PIC explain - để trống

                        worksheet.Cell(rowIndex, 32).Value = rq.ID ?? "";

                        dearMail = "nhà cung cấp " + rq.Ten + " yêu cầu báo giá cho các mặt hàng như file đính kèm. Rất mong nhận được phản hồi báo giá sớm nhất từ quý nhà cung cấp. Trân trọng cảm ơn!";
                        mailTk = rq.CHR_CreateBy + "@grothergroup.net";
                        titleMail = (rq.ShortName ?? rq.Ten) + " - Deadline: " + (rq.DTM_KyHan?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd")) + " - Số đơn yêu cầu: " + rq.CHR_MaDon;
                        rowIndex++;
                    }

                    workbook.SaveAs(tempFilePath);

                    var bodyTable = mail.CHR_BODY + tablePicInfo.ToString();
                    var body = string.Format(bodyTable, dearMail, mailTk);
                    var email = string.IsNullOrEmpty(mail.CHR_CC) ?
                        "bivn-pur-indirectpart@brother-bivn.com.vn" : mail.CHR_CC;

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
            bool sendResult = EmailSender.sendEmailNotify(
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
                bool sendResult = EmailSender.sendEmailNotify(
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
    }
}
