using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IBaoGiaNCCRepository: IBaseRepository<BaoGia_NCC, int>
    {
        // thông tin nhà cung cấp theo mã hàng
        public Task<List<BaoGia_NCC>> GetBaoGiaNCCByMaHang(string maHang);
        // lay thong tin san pham lien quan den nha cung cap
        public Task<List<BaoGia_NCC>> GetBaoGiaNCCByNCC(string maNCC);
        // them thong tin
        public Task<bool> AddBaoGiaNCC(BaoGia_NCC baoGiaNCC);
        // xoa thong tin    
        public Task<bool> DeleteBaoGiaNCC(int id);
        // update thong tin
        public Task<bool> UpdateBaoGiaNCC(BaoGia_NCC baoGiaNCC);
        // update list thong tin
        public Task<bool> UpdateListBaoGiaNCC(List<BaoGia_NCC> listBaoGia);
    }
}
