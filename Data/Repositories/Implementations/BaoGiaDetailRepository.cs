using Dapper;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Text;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class BaoGiaDetailRepository: BaseRepository<BaoGia_Detail_of_Quotation , int>, IBaoGiaDetailRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public BaoGiaDetailRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
        : base(context, options, configuration) {
            _context = context;
        }
        public async Task<List<dynamic>> SearchBaoGiaAsync(int? idRequest, string? maDon, string? maVatTu, string? maNcc, string? section, DateTime? dayMM,int? PageSize, int? PageIndex)
        {
            var sql = new StringBuilder(@"SELECT d.*, r.CHR_MaHangNoiBo, r.INT_SoLuong,r.NVCHR_DonVi
              FROM [COST_MANAGEMENT].[dbo].[BaoGia_Detail_of_Quotation] as d
              left join [COST_MANAGEMENT].[dbo].[BaoGia_Request_of_Quotation] as r
              on d.ID_RequestQuote = r.ID where 1 = 1");
            var parameters = new DynamicParameters();
            if(idRequest != 0 && idRequest != null)
            {
                sql.Append(" AND r.ID = @IdRequest");
                parameters.Add("IdRequest", idRequest);
            }
            if (!string.IsNullOrEmpty(maDon))
            {
                sql.Append(" AND r.CHR_MaDon = @Madon");
                parameters.Add("Madon",maDon);
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
            if (!string.IsNullOrEmpty(section))
            {
                sql.Append(" AND r.CHR_SectionCode = @Section");
                parameters.Add("Section", section);
            }
            if (dayMM != null)
            {
                sql.Append(" AND CONVERT(DATE, r.DTM_NgayMuonNhan) = CONVERT(DATE, @Day)");
                parameters.Add("Day", dayMM);
            }
            sql.Append(" ORDER BY d.ID DESC");
            if (PageSize > 0 && PageIndex > 0)
            {
                sql.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
                var offset = (PageIndex - 1) * PageSize;
                parameters.Add("Offset", offset);
                parameters.Add("PageSize", PageSize);
            }
            return (await _conn.QueryAsync<dynamic>(sql.ToString(), parameters)).ToList();
        }
        public async Task<bool> InsertListBaoGiaDetailAsync(List<BaoGia_Detail_of_Quotation> listDto)
        {
            try
            {
                await _context.BaoGia_Detail_of_Quotations.AddRangeAsync(listDto);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
               return false;
            }
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
    }
}