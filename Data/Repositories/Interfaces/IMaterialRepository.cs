using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using static PRJ_WAREHOUSE_BIVN.View_Models.Material.MaterialVM;

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
        // Lay ma hang lon nhat hien tai
        public Task<string> MaterialCodeLater(string type);
        // check ma hang
        public Task<string> CheckMaterialCode(string codeNcc, string category, string NameEN);
        public Task<string> CheckMaterialCodeByGoodCode(string codeNcc);
        public Task<string> CheckMaterialCodeByName(string category, string NameEN);
        // Search date by Material View
        Task<ListRequest<MATERIAL>> SearchDateByMaterialViewAsync(SearchMaterialVM search);
        // Delete Material
        Task<bool> DeleteMaterialAsync(string codeMaterial);
        // Update Material
        Task<bool> UpdateMaterialAsync(MATERIAL mt);
        // delete list material
        Task<bool> DeleteMaterials(List<string> listCodeMaterial);
    }
}
