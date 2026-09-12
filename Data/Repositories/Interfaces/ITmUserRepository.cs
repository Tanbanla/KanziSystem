using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.View_Models.Login;

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
        // Search thông tin user
        Task<List<TM_USER>> SearchUserAsync(UserSearchModel searchModel);

        // Đăng ký user mới
        Task<bool> RegisterUserAsync(UserInsertModel userInsert);
        // Update thông tin user
        Task<bool> UpdateUserAsync(UserInsertModel userUpdate);
        Task<bool> DeleteUserAsync(string userId);

    }
}
