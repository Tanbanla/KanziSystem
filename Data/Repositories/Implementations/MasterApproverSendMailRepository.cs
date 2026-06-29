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
        // Lưu thông tin
        public async Task<bool> SaveMasterApproverSendMailAsync(BaoGia_Master_Approver_Send_Mail obj)
        {
            if(obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            // Kiểm tra nếu đã tồn tại thông tin với cùng ID_BaoGiaStep, CHR_UserAdid và CHR_CodeSection
            var existingEntity = await _context.BaoGia_Master_Approver_Send_Mails
                .FirstOrDefaultAsync(x => x.ID_BaoGiaStep == obj.ID_BaoGiaStep && x.CHR_UserAdid == obj.CHR_UserAdid && x.CHR_CodeSection == obj.CHR_CodeSection);
            if (existingEntity != null)
            {
                throw new InvalidOperationException("Thông tin đã tồn tại với cùng ID_BaoGiaStep, CHR_UserAdid và CHR_CodeSection.");
            }

            // Nhập thông tin phòng ban vào bảng USER_DEPT nếu chưa tồn tại
            var checkUserDept = await _context.USER_DEPTs.FirstOrDefaultAsync(x => x.CHR_USERID == obj.CHR_UserAdid && x.Cost_Center == obj.CHR_CodeSection);
            if(checkUserDept == null)
            {
                var userDept = new USER_DEPT
                {
                    CHR_USERID = obj.CHR_UserAdid,
                    Cost_Center = obj.CHR_CodeSection
                };
                await _context.USER_DEPTs.AddAsync(userDept);
            }
            await _context.BaoGia_Master_Approver_Send_Mails.AddAsync(obj);
            await _context.SaveChangesAsync();
            return true;
        }
        // Sửa thông tin
        public async Task<bool> UpdateMasterApproverSendMailAsync(BaoGia_Master_Approver_Send_Mail obj)
        {
            _context.BaoGia_Master_Approver_Send_Mails.Update(obj);
            await _context.SaveChangesAsync();
            return true;
        }
        // Xóa thông tin
        public async Task<bool> DeleteMasterApproverSendMailAsync(int id, string userAction)
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
        // Lấy thông tin phê duyệt step của phòng ban
        public async Task<List<BaoGia_Master_Approver_Send_Mail>> GetApproverByStepAndSectionAsync(int idStep, string sectionCode)
        {
            var query = from m in _context.BaoGia_Master_Approver_Send_Mails
                        where m.ID_BaoGiaStep == idStep
                              && (m.CHR_CodeSection == sectionCode || string.IsNullOrEmpty(sectionCode))
                        group m by m.CHR_UserAdid into g
                        select g.OrderBy(x => x.ID).FirstOrDefault();

            var approvers = await query.ToListAsync();
            return approvers;
        }
        // Inser thông tin và đăng ký user đăng nhập
        public async Task<bool> InsertMasterApproverSendMailAsync(List<BaoGia_Master_Approver_Send_Mail> dtos)
        {
            if (dtos == null || dtos.Count == 0)
            {
                return false;
            }
            foreach (var dto in dtos)
            {
                var existingEntity = await _context.BaoGia_Master_Approver_Send_Mails
                    .FirstOrDefaultAsync(x => x.ID_BaoGiaStep == dto.ID_BaoGiaStep && x.CHR_UserAdid == dto.CHR_UserAdid && x.CHR_CodeSection == dto.CHR_CodeSection);
                if (existingEntity != null)
                {
                    continue;
                }
                await _context.BaoGia_Master_Approver_Send_Mails.AddAsync(dto);
            }
            await _context.SaveChangesAsync();
            return true;
        }
        // Check quyền phê duyệt của user theo step và section
        public async Task<bool> CheckUserApprovalPermissionAsync(string adid, List<int> ids)
        {
            var requiredSteps = await _context.BaoGia_Request_of_Quotations
                .Where(x => ids.Contains(x.ID))
                .Select(x => new { x.ID_StepBaoGia, CHR_SectionCode = x.CHR_SectionCode.Trim()})
                .Distinct()
                .ToListAsync();

            var userPermissions = await _context.BaoGia_Master_Approver_Send_Mails
                 .Where(x => x.CHR_UserAdid == adid)
                 .Select(x => new { x.ID_BaoGiaStep, CHR_CodeSection = x.CHR_CodeSection.Trim()})
                 .Distinct()
                 .ToListAsync();

            return requiredSteps.All(step =>
                userPermissions.Any(p =>
                    p.ID_BaoGiaStep == step.ID_StepBaoGia &&
                    p.CHR_CodeSection == step.CHR_SectionCode));
        }
    }
}
