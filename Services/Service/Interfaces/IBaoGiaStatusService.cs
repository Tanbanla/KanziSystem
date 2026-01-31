using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaStatusService: IBaseService<BaoGia_Status, int, BaoGia_StatusDTO>
    {

        // Lấy danh sach trang thai
        public Task<GenericResponse<List<BaoGia_StatusDTO>>> GetListStatusAsync();
    }
}
