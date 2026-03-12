using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Net.Mail;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class SendMailRepository: BaseRepository<TM_MASTER_MAIL, int>, ISendMailRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public SendMailRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
            : base(context, options, configuration)
        {
            _context = context;
        }
        // lấy mail theo ID
        public async Task<TM_MASTER_MAIL?> GetMailByIdAsync(int id)
        {
            return await _context.TM_MASTER_MAILs.FindAsync(id);
        }
        // Lay danh sach NCC can gui mail
        public async Task<List<string>> GetSuppliersToNotifyAsync()
        {

          var res =  await _context.BaoGia_Request_of_Quotations
                .Where(m => (m.BIT_IsTemplate == null || m.BIT_IsTemplate == false) && m.BIT_LayBaoGia == true)
                .Select(m => m.CHR_MaNCC)
                .Distinct()
                .ToListAsync();
          if (res == null || res.Count == 0)
          {
              return new List<string>();
          }
           return res;
        }
        // Lay thong tin don bao gia cua nha cung cap
        public async Task<List<BaoGia_Request_of_Quotation>> GetBaoGiaRequestBySupplierAsync(string supplierCode)
        {
            var res = await _context.BaoGia_Request_of_Quotations
                .Where(m => m.CHR_MaNCC == supplierCode && (m.BIT_IsTemplate == null || m.BIT_IsTemplate == false) && m.BIT_LayBaoGia == true )
                .ToListAsync();
            if (res == null || res.Count == 0)
            {
                return new List<BaoGia_Request_of_Quotation>();
            }
            return res;
        }
        // Lay email nha cung cap
        public async Task<string?> GetSupplierEmailAsync(string supplierCode)
        {
            var email = await _context.BaoGia_NCC_Categories
                .Where(m => m.CHR_MaNCC == supplierCode)
                .Select(m => m.CHR_Mail)
                .FirstOrDefaultAsync();
            return email;
        }
        public async Task<bool> UpdateMailSentStatusAsync(List<int> listRq)
        {
            var requests = await _context.BaoGia_Request_of_Quotations
                .Where(m => listRq.Contains(m.ID))
                .ToListAsync();

            if (requests == null || !requests.Any())
            {
                return false;
            }

            foreach (var item in requests)
            {
                item.BIT_IsTemplate = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Lay thong tin nha cung cap theo ma don hang
        public async Task<List<BaoGia_Request_of_Quotation>> GetNotifyRequestCodeAsync(string requestCode)
        {
            var res = await _context.BaoGia_Request_of_Quotations
              .Where(m => (m.BIT_IsTemplate == null || m.BIT_IsTemplate == false) && m.BIT_LayBaoGia == true && m.CHR_MaDon == requestCode)
              .ToListAsync();
            if (res == null || res.Count == 0)
            {
                return new List<BaoGia_Request_of_Quotation>();
            }
            return res;
        }
        // lay thông tin phê duyệt theo phòng ban
        public async Task<string> GetRequesterEmailAsync(string section, int step)
        {
            var res = await _context.BaoGia_Master_Approver_Send_Mails
                .Where(m => m.CHR_CodeSection == section && m.ID_BaoGiaStep == step && m.CHR_Status == "ON")
                .GroupBy(m => m.CHR_UserAdid) 
                .Select(g => g.Key) 
                .ToListAsync();

            if (res == null || res.Count == 0)
                return "";

            // Thêm đuôi @brothergroup.net và nối chuỗi
            var emails = res
                .Where(adid => !string.IsNullOrEmpty(adid))
                .Select(adid => adid.Trim() + "@brothergroup.net");

            return string.Join(";", emails);
        }
        // Inset thông tin vào bảng Báo giá detail 
        public async Task<bool> InsertBaoGiaDetailAsync(List<BaoGia_Detail_of_Quotation> dtos)
        {
            if (dtos == null || dtos.Count() == 0) return false;
            var listDetailOK = new List<BaoGia_Detail_of_Quotation>();
            foreach (var detail in dtos)
            {
                var rq = await _context.BaoGia_Request_of_Quotations.FindAsync(detail.ID_RequestQuote);
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
    }
}
