using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface INhomViTriRepository: IBaseRepository<ACC_NHOMVITRI, int>
    {
        // Lấy danh sách nhóm vị trí 
        Task<List<ACC_NHOMVITRI>> GetAllNhomViTriAsync();
        // Insert list Section
        Task<bool> InsertListSectionAsync(List<ACC_NHOMVITRI> nhomViTriList);
    }
}
