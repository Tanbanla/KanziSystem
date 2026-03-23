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
        // update danh sách linh kiện
        public Task<GenericResponse<bool>> UpdateMaterialAsync(List<MATERIALDTO> materials);
        // check mã linh kiện 
        public Task<GenericResponse<bool>> CheckMaHangExistsAsync(string codeMaterial);
        // Insert 
        public Task<GenericResponse<bool>> InsertMaterial(MATERIALDTO mt);  
        //insert nhiều
        public Task<GenericResponse<bool>> UpdateListThongTin(List<MATERIALDTO> listDTO);
        // Insert nhiều cho ma hang No list
        public Task<GenericResponse<bool>> UpdateListThongTinNoList(List<MATERIALDTO> listMT);
    }
}
