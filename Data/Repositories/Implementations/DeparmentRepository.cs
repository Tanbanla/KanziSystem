using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class DeparmentRepository: BaseRepository<DEPARTMENT, int>, IDeparmentRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public DeparmentRepository(COST_MANAGEMENTContext context,IOptions<ConnectionStringOptions> options, IConfiguration configuration)
            : base(context, options, configuration)
        {
            _context = context;
        }
        // danh sach cost
        public async Task<List<DEPARTMENT>> GetAllDepartmentAsync()
        {
            return await _context.DEPARTMENTs.Where(c=> c.Active == true).Distinct().ToListAsync();
        }
        // Update Section code
        public async Task<bool> UpdateSectionAsync(List<DEPARTMENT> ds)
        {
            if (ds == null || ds.Count == 0)
            {
                return false;
            }
            var listInsert = new List<DEPARTMENT>();
            foreach (var department in ds)
            {
                var existingDepartment = await _context.DEPARTMENTs.Where(c => c.Cost_Center == department.Cost_Center).FirstOrDefaultAsync();
                if (existingDepartment != null)
                {
                    if(existingDepartment.CHR_Section_Code == null || existingDepartment.CHR_Section_Code == "")
                    {
                        existingDepartment.CHR_Section_Code = department.CHR_Section_Code;
                    }
                    var newDepartment = new DEPARTMENT
                    {
                        Cost_Center = existingDepartment.Cost_Center,
                        Name = existingDepartment.Name,
                        Name_Jp = existingDepartment.Name_Jp,
                        Cost_Center_Group = existingDepartment.Cost_Center_Group,
                        Active = existingDepartment.Active,
                        AcceptRequest = existingDepartment.AcceptRequest,
                        CHR_WAREHOUSE = existingDepartment.CHR_WAREHOUSE,
                        CHR_Section_Code = department.CHR_Section_Code
                    };
                    listInsert.Add(newDepartment);
                }

            }
            if(listInsert.Count != 0)
            {
                _context.DEPARTMENTs.AddRange(listInsert);
            }
            await _context.SaveChangesAsync();
            return true;
        }
        // Lay thong tin department theo code section
        public async Task<List<DEPARTMENT>> GetDepartmentBySectionAsync(string codeSection)
        {
            if (string.IsNullOrEmpty(codeSection))
            {
                return null;
            }
            var department = await _context.DEPARTMENTs.Where(c => c.CHR_Section_Code == codeSection).ToListAsync();
            return department;
        }
        // Lấy thông tin theo quyền
        public async Task<List<DEPARTMENT>> GetNhomViTriByDepartmentIdAsync(string user)
        {
            var sql = @"  select distinct d.* FROM [DEPARTMENT] as d
              left join BaoGia_Master_Approver_Send_Mail as m  on d.Cost_Center = m.CHR_CodeSection
              where m.CHR_UserAdid = @User";
            var parameters = new
            {
                User = user
            };
            return (await _conn.QueryAsync<DEPARTMENT>(sql, parameters)).ToList();
        }
    }
}
