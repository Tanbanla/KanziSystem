using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface ITmCategoryService: IBaseService<TM_Category, int, TM_CategoryDTO>
    {
        // lấy danh sách chủng loại
        public Task<GenericResponse<List<string>>> GetListCategory();
        // Tìm kiếm chủng loại theo tên
        public Task<GenericResponse<List<TM_CategoryDTO>>> SearchCategoryByName(string name);
        // Thêm mới chủng loại
        public Task<GenericResponse<bool>> AddCategory(TM_CategoryDTO categoryDTO);
        // Thêm nhiều chủng loại 
        public Task<GenericResponse<List<TM_CategoryDTO>>> AddListCategory(List<TM_CategoryDTO> categoryDTOs);
        // Xóa thông tin chung loại
        public Task<GenericResponse<bool>> DeleteCategory(int id);
    }
}
