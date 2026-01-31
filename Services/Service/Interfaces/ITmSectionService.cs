using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Working;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface ITmSectionService: IBaseService<TM_SECTION, string, TM_SECTIONDTO>
    {
        // Lấy danh sách phòng ban
        public Task<GenericResponse<List<TM_SECTIONDTO>>> GetAllSectionsAsync();
    }
}
