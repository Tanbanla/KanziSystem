using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaNccCategoryService: IBaseService<BaoGia_NCC_Category, int, BaoGia_NCC_CategoryDTO>
    {
        // lấy danh sách theo mã NCC
        public Task<GenericResponse<List<BaoGia_NCC_CategoryDTO>>> GetBaoGiaNccCategoryByMaNCC(string maNCC);
        // lấy danh sách theo chung loai
        public Task<GenericResponse<List<BaoGia_NCC_CategoryDTO>>> GetBaoGiaNccCategoryByChungLoai(string chungLoai);
        // them thong tin
        public Task<GenericResponse<bool>> AddBaoGiaNccCategory(BaoGia_NCC_CategoryDTO baoGiaNccCategory);
        // xoa thong tin
        public Task<GenericResponse<bool>> DeleteBaoGiaNccCategory(int id);
        // them danh sach
        public Task<GenericResponse<bool>> AddListBaoGiaNccCategory(List<BaoGia_NCC_CategoryDTO> listBaoGiaNccCategory);
        // update thong tin
        public Task<GenericResponse<bool>> UpdateBaoGiaNccCategory(BaoGia_NCC_CategoryDTO baoGiaNccCategory);
        // Check Supperlier and Catergory
        public Task<GenericResponse<bool>> CheckSupperlier(string codeSupperlier, string catergory);
        // check category exist - returns list of missing supplier/category pairs
        Task<GenericResponse<List<CheckSupplierByCategoryModel>>> CheckSupperlierByCategory(List<CheckSupplierByCategoryModel> request);
        // Thêm chủng loại nhà cung cấp
        Task<GenericResponse<bool>> InsertCategoryNccAsync(BaoGia_NCC_CategoryDTO dto);
        // Xóa chủng loại theo mã nhà cung cấp và chủng loại
        Task<GenericResponse<bool>> DeleteCategoryNccAsync(List<BaoGia_NCC_CategoryDTO> listDelete);
        // Xóa Nhà cung cấp
        Task<GenericResponse<bool>> DeleteSupplierAsync(List<BaoGia_NCC_CategoryDTO> listDelete);
    }
}
