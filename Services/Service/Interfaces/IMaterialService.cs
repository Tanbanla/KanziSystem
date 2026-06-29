using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using static PRJ_WAREHOUSE_BIVN.View_Models.Material.MaterialVM;

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
        // lay ma hang hien tai
        public Task<GenericResponse<string>> MaterialCodeLater(string type);
        // check ma hang
        public Task<GenericResponse<string>> CheckMaterialCode(string keyword, string category);
        // Search date by Material View
        Task<GenericResponse<ListRequest<MATERIAL>>> SearchDateByMaterialViewAsync(SearchMaterialVM search);
        // Export danh sách linh kiện
        Task<GenericResponse<IFormFile>> ExportMaterialViewToExcelAsync(SearchMaterialVM search);
    }
}
