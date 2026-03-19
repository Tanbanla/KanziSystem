using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface INhomViTriRepository : IBaseRepository<ACC_NHOMVITRI, int>
    {
        // Lấy danh sách nhóm vị trí 
        Task<List<ACC_NHOMVITRI>> GetAllNhomViTriAsync();
        // Lấy thông tin theo quyền
        Task<List<ACC_NHOMVITRIDTO>> GetNhomViTriByDepartmentIdAsync(string user);
        // Insert list Section
        Task<bool> InsertListSectionAsync(List<ACC_NHOMVITRI> nhomViTriList);
    }
}
