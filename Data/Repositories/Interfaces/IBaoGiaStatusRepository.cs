using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IBaoGiaStatusRepository: IBaseRepository<BaoGia_Status, int>
    {
        // Lấy danh sach trang thai
        public Task<List<BaoGia_Status>> GetListStatusAsync();
    }
}
