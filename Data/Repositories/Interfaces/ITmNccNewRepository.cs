using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface ITmNccNewRepository : IBaseRepository<IM_NCC_NEW, int>
    {
        // lấy danh sách nhà cung cấp  ALL
        Task<List<IM_NCC_NEW>> GetAllNccNew();
        // Lấy nhà cung cấp theo mã nhà cung cấp
        Task<IM_NCC_NEW> GetNccNewByCode(string MaNCC);
        // Lây thông tin nhà cung cấp phân trang 
        Task<List<IM_NCC_NEW>> GetNccNewPaging(string keyword, int pageIndex, int pageSize);
    }
}
