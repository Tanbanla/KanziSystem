using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Agent;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class MasterApproverSendMailRepository : BaseRepository<BaoGia_Master_Approver_Send_Mail, int>, IMasterApproverSendMailRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        private readonly AgentContext _agentContext;
        public MasterApproverSendMailRepository(COST_MANAGEMENTContext context, AgentContext agentContext, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
            : base(context, options, configuration)
        {
            _context = context;
            _agentContext = agentContext;
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

        public async Task<List<dynamic>> GetApproverByAgrentAsync(int idStep, string sectionCode)
        {
            // lấy thông tin phòng từ sectionCode
            var section = await _context.DEPARTMENTs.Where(d => d.Cost_Center == sectionCode)
                .Select(d =>  d.CHR_Section_Code)
                .FirstOrDefaultAsync();

            var sql = new StringBuilder();
            sql.Append(@"SELECT
                  [CHR_EMPLOYEE_ID]
                  ,[CHR_EMPLOYEE_NAME]  as NVCHR_UserName
                  ,[CHR_EMPLOYEE_ADID] as CHR_UserAdid
                  ,[CHR_EMPLOYEE_MAIL] 
                  ,[CHR_POSITION] 
                  ,[CHR_POSITION_GROUP] as NVCHR_Position
              FROM [AGENTDB].[dbo].[TM_EMPLOYEE]
              where CHR_NOTE is null and (DTM_LEAVE_DATE is null or DTM_LEAVE_DATE < Getdate())
            ");

            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(section))
            {
                sql.Append(" AND CHR_SECTION like  @section");
                parameters.Add("@section", $"%{section.Trim()} :%");
            }

            switch(idStep)
            {
                case 2:
                    sql.Append(" AND CHR_POSITION_GROUP = 'Chief'");
                    break;
                case 3:
                    sql.Append(" AND CHR_POSITION_GROUP = 'Section Manager'");
                    break;
                default:
                    break;
            }

            var result = await _conn.QueryAsync<dynamic>(
                sql.ToString(),
                parameters);
            return result.ToList();
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
                .Select(x => new { x.ID_StepBaoGia, CHR_SectionCode = x.CHR_SectionCode.Trim() })
                .Distinct()
                .ToListAsync();

            var userPermissions = await _context.BaoGia_Master_Approver_Send_Mails
                 .Where(x => x.CHR_UserAdid == adid)
                 .Select(x => new { x.ID_BaoGiaStep, CHR_CodeSection = x.CHR_CodeSection.Trim() })
                 .Distinct()
                 .ToListAsync();

            return requiredSteps.All(step =>
                userPermissions.Any(p =>
                    p.ID_BaoGiaStep == step.ID_StepBaoGia &&
                    p.CHR_CodeSection == step.CHR_SectionCode));
        }
    }
}
