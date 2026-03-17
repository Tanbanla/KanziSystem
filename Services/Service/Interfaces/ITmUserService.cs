using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface ITmUserService: IBaseService<TM_USER, int, TM_USERDTO>
    {
        // Login
        public Task<GenericResponse<TM_USERDTO>> Login(string username, string password);
        // lấy quyền user
        public Task<GenericResponse<string>> GetRoleAsync(string adId);
    }
}
