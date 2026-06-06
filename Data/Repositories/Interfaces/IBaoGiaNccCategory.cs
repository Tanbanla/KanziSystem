using Microsoft.Identity.Client;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IBaoGiaNccCategory: IBaseRepository<BaoGia_NCC_Category, int>
    {
        // lấy danh sách theo mã NCC
        public Task<List<BaoGia_NCC_Category>> GetBaoGiaNccCategoryByMaNCC(string maNCC);
        // lấy danh sách theo chung loai
        public Task<List<BaoGia_NCC_Category>> GetBaoGiaNccCategoryByChungLoai(string chungLoai);
        // them thong tin
        public Task<bool> AddBaoGiaNccCategory(BaoGia_NCC_Category baoGiaNccCategory);
        // xoa thong tin
        public Task<bool> DeleteBaoGiaNccCategory(int id);
        // them danh sach
        public Task<bool> AddListBaoGiaNccCategory(List<BaoGia_NCC_Category> listBaoGiaNccCategory);
        // update thong tin
        public Task<bool> UpdateBaoGiaNccCategory(BaoGia_NCC_Category baoGiaNccCategory);
        // Check Supperlier and Catergory
        public Task<bool> CheckSupperlier(string codeSupperlier, string catergory);
        // check category exist - returns list of supplier/category pairs that are NOT present in the database
        Task<List<CheckSupplierByCategoryModel>> CheckSupperlierByCategory(List<CheckSupplierByCategoryModel> request);
    }
}
