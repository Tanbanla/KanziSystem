using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IMaterialRepository: IBaseRepository<MATERIAL, int>
    {
        // Lấy theo mã hàng
        public Task<MATERIAL> GetByMaHangAsync(string maHang);
        // Tìm kiếm hàng hóa và phân trang
        public Task<List<MATERIAL>> SearchAsync(string? MaHang, string? Name, string? NhomHang, int? pageIndex, int? pageSize);
        // Lấy danh sách hàng hóa
        public Task<List<MATERIAL>> GetMaterialsByNameOrCodeAsync(string keyword);
        // danh sach chung loai
        public Task<List<dynamic>> GetListMaterial();

        // update danh sách linh kiện
        public Task<bool> UpdateMaterialAsync(List<MATERIAL> materials);
        // check mã linh kiện 
        public Task<bool> CheckMaHangExistsAsync(string codeMaterial);
        // Insert 
        public Task<bool> InsertMaterial(MATERIAL mt);
        // Insert nhiều
        public Task<bool> UpdateListThongTinNoList(List<MATERIAL> listMT);

    }
}
