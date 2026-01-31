using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaNCCService: IBaseService<BaoGia_NCC, int, BaoGia_NCCDTO>
    {
        public Task<GenericResponse<List<BaoGia_NCCDTO>>> GetBaoGiaNCCByMaHang(string maHang);
    }
}
