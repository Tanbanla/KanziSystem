using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class MasterApproverSendMailRepository : BaseRepository<BaoGia_Master_Approver_Send_Mail, int>, IMasterApproverSendMailRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public MasterApproverSendMailRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
            : base(context, options, configuration)
        {
            _context = context;
        }
        // Lấy dữ liệu theo điều kiện và phân trang
        public async Task<List<BaoGia_Master_Approver_Send_Mail>> GetByConditionAsync(string? sectionCode, string? adid, int? IdStep, int pageIndex, int pageSize)
        {
            try
            {
                var query = _context.BaoGia_Master_Approver_Send_Mails.AsQueryable();
                if (!string.IsNullOrEmpty(sectionCode))
                {
                    query = query.Where(x => x.CHR_CodeSection.Contains(sectionCode));
                }
                if (!string.IsNullOrEmpty(adid))
                {
                    query = query.Where(x => x.CHR_UserAdid.Contains(adid));
                }
                if (IdStep.HasValue)
                {
                    query = query.Where(x => x.ID_BaoGiaStep == IdStep.Value);
                }
                return await query
                    .OrderBy(x => x.ID)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByConditionAsync: {ex.Message}");
                return new List<BaoGia_Master_Approver_Send_Mail>();
            }
        }
        // Lưu thông tin
        public async Task<bool> SaveMasterApproverSendMailAsync(BaoGia_Master_Approver_Send_Mail obj)
        {
            try
            {
                await _context.BaoGia_Master_Approver_Send_Mails.AddAsync(obj);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveMasterApproverSendMailAsync: {ex.Message}");
                return false;
            }
        }
        // Sửa thông tin
        public async Task<bool> UpdateMasterApproverSendMailAsync(BaoGia_Master_Approver_Send_Mail obj)
        {
            try
            {
                _context.BaoGia_Master_Approver_Send_Mails.Update(obj);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateMasterApproverSendMailAsync: {ex.Message}");
                return false;
            }
        }
        // Xóa thông tin
        public async Task<bool> DeleteMasterApproverSendMailAsync(int id, string userAction)
        {
            try
            {
                var entity = await _context.BaoGia_Master_Approver_Send_Mails.FindAsync(id);
                if (entity == null)
                {
                    return false;
                }
                _context.BaoGia_Master_Approver_Send_Mails.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteMasterApproverSendMailAsync: {ex.Message}");
                return false;
            }
        }
    }
}
