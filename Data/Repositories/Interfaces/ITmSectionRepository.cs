using PRJ_WAREHOUSE_BIVN.Models_Working;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface ITmSectionRepository: IBaseRepository<TM_SECTION, string>
    {
        // Lấy danh sách phòng ban
        public Task<List<TM_SECTION>> GetAllSectionsAsync();
    }
}
