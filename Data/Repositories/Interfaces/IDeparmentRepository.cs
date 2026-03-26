using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IDeparmentRepository: IBaseRepository<DEPARTMENT, int>
    {
        // danh sach cost
        public Task<List<DEPARTMENT>> GetAllDepartmentAsync();
        // Update Section code
        public Task<bool> UpdateSectionAsync(List<DEPARTMENT> ds);
        // Lay thong tin department theo code section
        public Task<List<DEPARTMENT>> GetDepartmentBySectionAsync(string codeSection);
        // Lấy thông tin theo quyền
        public Task<List<DEPARTMENT>> GetNhomViTriByDepartmentIdAsync(string user);
    }
}
