using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaNCCService: IBaseService<BaoGia_NCC, int, BaoGia_NCCDTO>
    {
        public Task<GenericResponse<List<BaoGia_NCCDTO>>> GetBaoGiaNCCByMaHang(string maHang);
        // lay thong tin san pham lien quan den nha cung cap
        public Task<GenericResponse<List<BaoGia_NCCDTO>>> GetBaoGiaNCCByNCC(string maNCC);
        // them thong tin
        public Task<GenericResponse<bool>> AddBaoGiaNCC(BaoGia_NCCDTO baoGiaNCC);
        // xoa thong tin
        public Task<GenericResponse<bool>> DeleteBaoGiaNCC(int id);
        // update thong tin
        public Task<GenericResponse<bool>> UpdateBaoGiaNCC(BaoGia_NCCDTO baoGiaNCC);
        // update list thong tin
        public Task<GenericResponse<bool>> UpdateListBaoGiaNCC(List<BaoGia_NCCDTO> listBaoGia);
    }
}
