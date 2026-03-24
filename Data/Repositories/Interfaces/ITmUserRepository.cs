using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface ITmUserRepository: IBaseRepository<TM_USER, int>
    {
        // Login
        public Task<TM_USER> Login(string username, string password);
        // Lấy thông tin user theo ADID
        public Task<TM_USER> GetUserByAdId(string adId);
        // lấy quyền user
        public Task<string> GetRoleAsync(string adId);
        // Insert thông tin và đăng ký user đăng nhập
        public Task<bool> InsertListUserAsync(List<TM_USER> users);

    }
}
