using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface ITmCategoryService: IBaseService<TM_Category, int, TM_CategoryDTO>
    {
        // lấy danh sách chủng loại
        public Task<GenericResponse<List<string>>> GetListCategory();
    }
}
