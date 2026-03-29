using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Reflection.PortableExecutable;
using System.Text;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class BaoGiaDetailRepository : BaseRepository<BaoGia_Detail_of_Quotation, int>, IBaoGiaDetailRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public BaoGiaDetailRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
        : base(context, options, configuration)
        {
            _context = context;
        }
        public async Task<ListRequest<dynamic>> SearchBaoGiaAsync(int? idRequest, string? maDon, string? maVatTu, string? maNcc, string? section, string? user, DateTime? dayMM, int? PageSize, int? PageIndex)
        {
            var baseFrom = new StringBuilder();
            baseFrom.Append(@"FROM [BaoGia_Detail_of_Quotation] as d
			LEFT JOIN [BaoGia_Request_of_Quotation] AS r ON d.ID_RequestQuote = r.ID
			LEFT JOIN [BaoGia_Master_Approver_Send_Mail] AS s ON r.CHR_SectionCode = s.CHR_CodeSection
            WHERE 1 = 1");

            var whereBuilder = new StringBuilder();
            var parameters = new DynamicParameters();
            if (!string.IsNullOrEmpty(user))
            {
                whereBuilder.Append(" AND s.CHR_UserAdid = @Adid");
                parameters.Add("Adid", user);
            }
            if (idRequest != 0 && idRequest != null)
            {
                whereBuilder.Append(" AND r.ID = @IdRequest");
                parameters.Add("IdRequest", idRequest);
            }
            if (!string.IsNullOrEmpty(maDon))
            {
                whereBuilder.Append(" AND r.CHR_MaDon = @Madon");
                parameters.Add("Madon", maDon);
            }
            if (!string.IsNullOrEmpty(maVatTu))
            {
                whereBuilder.Append(" AND r.CHR_MaHangNoiBo = @MaVatTu");
                parameters.Add("MaVatTu", maVatTu);
            }
            if (!string.IsNullOrEmpty(maNcc))
            {
                whereBuilder.Append(" AND CHR_CodeNCC = @MaNcc");
                parameters.Add("MaNcc", maNcc);
            }
            if (!string.IsNullOrEmpty(section))
            {
                whereBuilder.Append(" AND r.CHR_SectionCode = @Section");
                parameters.Add("Section", section);
            }
            if (dayMM != null)
            {
                whereBuilder.Append(" AND CONVERT(DATE, r.DTM_NgayMuonNhan) = CONVERT(DATE, @Day)");
                parameters.Add("Day", dayMM);
            }

            // Build select SQL
            var selectSql = new StringBuilder();
            selectSql.Append(@"SELECT distinct d.*, 
                    r.CHR_MaHangNoiBo, 
                    r.CHR_MaDon,
                    CAST(CASE WHEN r.CHR_MaHangNCC = d.CHR_MaHangNCC THEN 1 ELSE 0 END AS BIT) AS IsMatch_MaHangNCC,
                    CAST(CASE WHEN r.NVCHR_NameVN = d.NVCHR_TenHangHQ THEN 1 ELSE 0 END AS BIT) AS IsMatch_NameVN,
                    CAST(CASE WHEN r.CHR_NameEN = d.CHR_NameEN THEN 1 ELSE 0 END AS BIT) AS IsMatch_NameEN,
                    CAST(CASE WHEN (r.INT_SoLuong = d.INT_SoLuong or d.INT_SoLuong = 0) THEN 1 ELSE 0 END AS BIT) AS IsMatch_SoLuong,
                    CAST(CASE WHEN (r.NVCHR_DonVi = d.NVCHR_DonVi or d.NVCHR_DonVi is null) THEN 1 ELSE 0 END AS BIT) AS IsMatch_DonVi,
				    CAST(CASE 
					    WHEN r.NVCHR_Rohs = N'Need' AND (d.VCHR_Rohs = N'NG' OR d.VCHR_Rohs = N'No need') THEN 0
					    WHEN (r.NVCHR_Rohs = d.VCHR_Rohs OR d.VCHR_Rohs = N'OK' OR d.VCHR_Rohs = N'' )  THEN 1 
					    WHEN(r.NVCHR_Rohs ='') THEN 1
					    ELSE 0 
				    END AS BIT) AS IsMatch_Rohs,
				    CAST(CASE 
					    WHEN r.NVCHR_COCQ = N'Need' AND (d.VCHR_COCQ = N'NG' OR d.VCHR_COCQ = N'No need') THEN 0
					    WHEN (r.NVCHR_COCQ = d.VCHR_COCQ OR d.VCHR_COCQ = N'OK' OR d.VCHR_COCQ = N'') THEN 1 
					    WHEN( R.NVCHR_COCQ ='') THEN 1
					    ELSE 0 
				    END AS BIT) AS IsMatch_COCQ,

				    CAST(CASE 
					    WHEN r.NVCHR_MSDS = N'Need' AND (d.VCHR_MSDS = N'NG' OR d.VCHR_MSDS = N'No need') THEN 0
					    WHEN (r.NVCHR_MSDS = d.VCHR_MSDS OR d.VCHR_MSDS = N'OK' OR d.VCHR_MSDS = N'') THEN 1 
					    WHEN(r.NVCHR_MSDS ='') THEN 1
					    ELSE 0 
				    END AS BIT) AS IsMatch_MSDS,

				    CAST(CASE 
					    WHEN r.NVCHR_AnToan = N'Need' AND (d.VCHR_AnToan = N'NG' OR d.VCHR_AnToan = N'No need') THEN 0
					    WHEN (r.NVCHR_AnToan = d.VCHR_AnToan OR d.VCHR_AnToan = N'OK' OR d.VCHR_AnToan = N'') THEN 1 
					    WHEN(r.NVCHR_AnToan ='') THEN 1
					    ELSE 0 
				    END AS BIT) AS IsMatch_AnToan,
                    CAST(CASE WHEN (CAST(r.DTM_NgayMuonNhan AS DATE) = CAST(d.DTM_ShipTime AS DATE) or d.DTM_ShipTime is null ) THEN 1 ELSE 0 END AS BIT) AS IsMatch_Ngay,
                    CAST(CASE WHEN d.VCHR_CamKet != N'Đồng ý (accept)' then 0 else 1 end as bit) As IsMatchCamKet
            ");
            selectSql.Append(baseFrom.ToString());
            selectSql.Append(whereBuilder.ToString());
            selectSql.Append(" ORDER BY d.ID DESC");

            if (PageSize > 0 && PageIndex > 0)
            {
                selectSql.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
                var offset = (PageIndex - 1) * PageSize;
                parameters.Add("Offset", offset);
                parameters.Add("PageSize", PageSize);
            }

            var result = await _conn.QueryAsync<dynamic>(selectSql.ToString(), parameters);

            // Build count SQL using same FROM/WHERE so total respects filters
            var countSql = new StringBuilder();
            countSql.Append("SELECT COUNT(distinct d.ID) ");
            countSql.Append(baseFrom.ToString());
            countSql.Append(whereBuilder.ToString());

            var totalCount = await _conn.ExecuteScalarAsync<int>(countSql.ToString(), parameters);

            return new ListRequest<dynamic>
            {
                Data = result.ToList(),
                TotalCount = totalCount,
            };
        }
        public async Task<bool> InsertListBaoGiaDetailAsync(List<BaoGia_Detail_of_Quotation> listDto)
        {
            if (listDto == null || listDto.Count() == 0) return false;
            var listDetailOK = new List<BaoGia_Detail_of_Quotation>();
            foreach (var detail in listDto)
            {
                var rq = await _context.BaoGia_Request_of_Quotations
                .Where(c => c.BIT_LayBaoGia == true && c.ID == detail.ID_RequestQuote)
                .FirstOrDefaultAsync();
                if (rq == null) continue;
                rq.ID_Status = "WAIT_NCC";
                // kiểm tra dữ liệu Insert
                var exists = await _context.BaoGia_Detail_of_Quotations
                    .AnyAsync(c => c.ID_RequestQuote == detail.ID_RequestQuote);
                if (exists) continue;
                listDetailOK.Add(detail);
            }
            await _context.BaoGia_Detail_of_Quotations.AddRangeAsync(listDetailOK);
            await _context.SaveChangesAsync();
            return true;
        }
        // Update lua chon NCC
        public async Task<bool> UpdateLuaChonNCCBaoGiaDetailAsync(List<dynamic> listUp, string user, string name)
        {
            if (listUp == null || listUp.Count == 0)
            {
                return false;
            }
            // lưu thông tin chọn
            foreach (var l in listUp)
            {
                var jsonElement = (System.Text.Json.JsonElement)l;
                var id = int.Parse(jsonElement.GetProperty("ID").GetString());
                var bit = jsonElement.GetProperty("BIT_Select").GetBoolean();
                var reason = jsonElement.GetProperty("NVCHR_ReasonPick").GetString();
                var detail = await _context.BaoGia_Detail_of_Quotations.FindAsync(id);
                if (detail != null)
                {
                    detail.BIT_Select = bit;
                    detail.NVCHR_ReasonPick = reason;
                    // lưu lịch sử lựa chọn
                    var quote = await _context.BaoGia_Request_of_Quotations.FindAsync(detail.ID_RequestQuote);
                    quote.ID_Status = "APPROVAL";
                    quote.ID_StepBaoGia = quote.ID_StepBaoGia + 1;
                    quote.INT_SoLanUpdate = quote.INT_SoLanUpdate + 1;
                    quote.DTM_UpdateLater = DateTime.Now;

                    var history = new BaoGia_History_Request_of_Quotation
                    {
                        ID = 0,
                        ID_RequestQuote = detail.ID_RequestQuote,
                        CHR_MaDon = quote.CHR_MaDon,
                        CHR_UpdateBy = user,
                        NVCHR_UpdateName = name,
                        CHR_Updatedate = DateTime.Now,
                        CHR_ChangedColumns = "BIT_Select,NVCHR_ReasonPick",
                        CHR_OldData = "",
                        CHR_NewData = System.Text.Json.JsonSerializer.Serialize(quote),
                        NVCHR_LyDo = "Chọn nhà cung cấp",
                        CHR_ActionType = "Update"
                    };

                    await _context.BaoGia_History_Request_of_Quotations.AddAsync(history);
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }
        // Lấy thông tin theo ID_RequestQuote
        public async Task<BaoGia_Detail_of_Quotation> GetByIdRequestQuoteAsync(int idRequest)
        {
            var a = await _context.BaoGia_Detail_of_Quotations
                .Where(b => b.ID_RequestQuote == idRequest)
                .FirstOrDefaultAsync();
            return a;
        }
        // Update infor input bao gia
        public async Task<bool> UpdateListThongTinNhapBaoGiaAsync(List<BaoGia_Detail_of_Quotation> listDto)
        {
            if (listDto == null || listDto.Count == 0)
            {
                return false;
            }
            // lưu lịch sử thay đổi
            var historyList = new List<BaoGia_History_Detail_Request>();
            foreach (var item in listDto)
            {
                var detail = _context.BaoGia_Detail_of_Quotations.Find(item.ID);
                if (detail != null)
                {
                    // luu lịch sử thay đổi
                    var history = new BaoGia_History_Detail_Request
                    {
                        ID = 0,
                        ID_RQ_Detail = detail.ID,
                        NVCHR_dataOld = System.Text.Json.JsonSerializer.Serialize(detail),
                        NVCHR_dataNew = System.Text.Json.JsonSerializer.Serialize(item),
                        CHR_CreateBy = item.CHR_UpdateBy,
                        DTM_CreateBy = DateTime.Now
                    };
                    historyList.Add(history);

                    detail.CHR_MaHangNCC = item.CHR_MaHangNCC;
                    detail.NVCHR_TenHangHQ = item.NVCHR_TenHangHQ;
                    detail.FL_USD = item.FL_USD;
                    detail.FL_VND = item.FL_VND;
                    detail.DTM_EndDate = item.DTM_EndDate;
                    detail.NVCHR_MOQ = item.NVCHR_MOQ;
                    detail.DTM_LeadTime = item.DTM_LeadTime;
                    detail.DTM_ShipTime = item.DTM_ShipTime;
                    detail.NVCHR_Packing = item.NVCHR_Packing;
                    detail.NVCHR_Note = item.NVCHR_Note;
                    detail.NVCHR_File = item.NVCHR_File;
                    detail.FL_ExchangeRate = item.FL_ExchangeRate;
                    detail.FL_TaxRate = item.FL_TaxRate;
                    detail.FL_TaxAmount = item.FL_TaxAmount;
                    detail.FL_TotalAfterTax = item.FL_TotalAfterTax;
                    detail.NVCHR_PaymentTerm = item.NVCHR_PaymentTerm;
                    detail.NVCHR_Warranty = item.NVCHR_Warranty;
                    detail.NVCHR_DeliveryTerm = item.NVCHR_DeliveryTerm;
                    detail.CHR_UpdateBy = item.CHR_UpdateBy;
                    detail.DTM_UpdateDate = DateTime.Now;
                    detail.INT_NumberEdit = detail.INT_NumberEdit != null ? detail.INT_NumberEdit + 1 : 1;
                    detail.INT_SoLuong = item.INT_SoLuong;
                    detail.FL_Sum = (item.FL_VND != null && item.INT_SoLuong != null) ? item.FL_VND * item.INT_SoLuong : null;
                    detail.VCHR_Rohs = item.VCHR_Rohs;
                    detail.VCHR_COCQ = item.VCHR_COCQ;
                    detail.VCHR_MSDS = item.VCHR_MSDS;
                    detail.VCHR_AnToan = item.VCHR_AnToan;
                    detail.VCHR_CamKet = item.VCHR_CamKet;
                    detail.NVCHR_DonVi = item.NVCHR_DonVi;
                    detail.DTM_EffectiveDate = item.DTM_EffectiveDate;
                    detail.DTM_ExpiryDate = item.DTM_ExpiryDate;
                    detail.BIT_Select = null;
                }
                // save step BaoGia_Request
                var rq = await _context.BaoGia_Request_of_Quotations.FindAsync(detail.ID_RequestQuote);
                if (rq != null)
                {
                    rq.ID_StepBaoGia = 7;
                    rq.ID_Status = "WAIT_PICK_NCC";
                }
            }
            await _context.BaoGia_History_Detail_Requests.AddRangeAsync(historyList);
            await _context.SaveChangesAsync();
            return true;
        }
        // lấy id của đơn báo giá
        public async Task<int?> GetIdOfQuotationAsync(string maDon, string maVatTu, string maNcc, string NameHQ)
        {
            var sql = new StringBuilder(@"SELECT d.*
              FROM [BaoGia_Detail_of_Quotation] as d
              left join [BaoGia_Request_of_Quotation] as r
              on d.ID_RequestQuote = r.ID where 1 = 1");
            var parameters = new DynamicParameters();
            if (!string.IsNullOrEmpty(maDon))
            {
                sql.Append(" AND r.CHR_MaDon = @Madon");
                parameters.Add("Madon", maDon);
            }
            if (!string.IsNullOrEmpty(maVatTu))
            {
                sql.Append(" AND r.CHR_MaHangNoiBo = @MaVatTu");
                parameters.Add("MaVatTu", maVatTu);
            }
            if (!string.IsNullOrEmpty(maNcc))
            {
                sql.Append(" AND CHR_CodeNCC = @MaNcc");
                parameters.Add("MaNcc", maNcc);
            }
            if (!string.IsNullOrEmpty(NameHQ))
            {
                sql.Append(" AND d.NVCHR_TenHangHQ = @NameHQ");
                parameters.Add("NameHQ", NameHQ);
            }
            var data = (await _conn.QueryAsync<BaoGia_Detail_of_Quotation>(sql.ToString(), parameters)).ToList();

            var result = data
                .Select(b => b.ID).FirstOrDefault();
            return result;
        }
        // update thông tin lựa chọn nhà  cung cấp
        public async Task<bool> UpdatePickSupplierDetailAsync(List<BaoGia_Detail_of_Quotation> dtos, string userApproverNext)
        {
            if (dtos == null || dtos.Count == 0)
            {
                return false;
            }
            // lưu lịch sử thay đổi
            var historyList = new List<BaoGia_History_Detail_Request>();
            foreach (var item in dtos)
            {
                var detail = await _context.BaoGia_Detail_of_Quotations.Where(c=> c.ID_RequestQuote == item.ID).FirstOrDefaultAsync();
                if (detail != null)
                {
                    detail.BIT_Select = item.BIT_Select;
                    detail.NVCHR_ReasonPick = item.NVCHR_ReasonPick;
                    // save rq
                    var rq = await _context.BaoGia_Request_of_Quotations.FindAsync(detail.ID_RequestQuote);
                    if (rq != null)
                    {
                        rq.ID_StepBaoGia = 9;
                        rq.ID_Status = "WAIT_APPROVE";
                        rq.NVCHR_UserRequest = userApproverNext;
                    }
                    // luu lịch sử thay đổi
                    var history = new BaoGia_History_Detail_Request
                    {
                        ID = 0,
                        ID_RQ_Detail = detail.ID,
                        NVCHR_dataOld = System.Text.Json.JsonSerializer.Serialize(detail),
                        NVCHR_dataNew = System.Text.Json.JsonSerializer.Serialize(item),
                        CHR_CreateBy = item.CHR_UpdateBy,
                        DTM_CreateBy = DateTime.Now
                    };
                    historyList.Add(history);
                }
            }
            await _context.BaoGia_History_Detail_Requests.AddRangeAsync(historyList);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
