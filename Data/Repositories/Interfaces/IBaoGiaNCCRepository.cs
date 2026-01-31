using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IBaoGiaNCCRepository: IBaseRepository<BaoGia_NCC, int>
    {
        // thông tin nhà cung cấp theo mã hàng
        public Task<List<BaoGia_NCC>> GetBaoGiaNCCByMaHang(string maHang);
    }
}
