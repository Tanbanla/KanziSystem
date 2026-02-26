using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface ITmCategoryRepository: IBaseRepository<TM_Category, int>
    {
        // lấy danh sách chủng loại
        public Task<List<string>> GetListCategory();
        // Tìm kiếm chủng loại theo tên
        public Task<List<TM_Category>> SearchCategoryByName(string name);
    }
}
