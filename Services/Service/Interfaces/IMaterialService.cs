using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IMaterialService: IBaseService<MATERIAL, int, MATERIALDTO>
    {
        // Lấy theo mã hàng
        public Task<GenericResponse<MATERIALDTO>> GetByMaHangAsync(string maHang);
        // Tìm kiếm hàng hóa và phân trang
        public Task<GenericResponse<List<MATERIALDTO>>> SearchAsync(string? MaHang, string? Name, string? NhomHang, int? pageIndex, int? pageSize);
        // Lấy danh sách hàng hóa
        public Task<GenericResponse<List<MATERIALDTO>>> GetMaterialsByNameOrCodeAsync(string keyword);
        public Task<GenericResponse<List<dynamic>>> GetListMaterial();
    }
}
