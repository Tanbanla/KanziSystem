using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.View_Models.Login;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface ITmUserService: IBaseService<TM_USER, int, TM_USERDTO>
    {
        // Login
        public Task<GenericResponse<TM_USERDTO>> Login(string username, string password);
        // lấy quyền user
        public Task<GenericResponse<string>> GetRoleAsync(string adId);
        // Inser thông tin và đăng ký user đăng nhập
        public Task<GenericResponse<bool>> InsertListUserAsync(List<TM_USER> users);
        // Search thông tin user
        Task<GenericResponse<ListRequest<dynamic>>> SearchUserAsync(UserSearchModel searchModel);

        // Đăng ký user mới
        Task<GenericResponse<bool>> RegisterUserAsync(UserInsertModel userInsert);
        // Update thông tin user
        Task<GenericResponse<bool>> UpdateUserAsync(UserInsertModel userUpdate);
        Task<GenericResponse<bool>> DeleteUserAsync(string userId);
    }
}
