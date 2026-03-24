using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using System.DirectoryServices.AccountManagement;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class TmUserService: BaseService<TM_USER, int, TM_USERDTO>, ITmUserService   
    {
        private readonly ITmUserRepository _repo;
        public TmUserService(ITmUserRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
        }
        // Login
        public async Task<GenericResponse<TM_USERDTO>> Login(string username, string password)
        {
            var response = new GenericResponse<TM_USERDTO>();
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    response.Success = false;
                    response.Message = "Username and password cannot be empty.";
                    return response;
                }
                response.Data = _mapper.Map<TM_USERDTO>(await _repo.Login(username, password));
                if (response.Data == null)
                {
                    PrincipalContext pc = new PrincipalContext(ContextType.Domain, "AP");
                    bool isValid = pc.ValidateCredentials(username, password, ContextOptions.Signing);

                    if (!isValid)
                    {
                        response.Success = false;
                        response.Message = "Invalid username or password.";
                        return response;
                    }
                    else
                    {
                        response.Data = _mapper.Map<TM_USERDTO>(await _repo.GetUserByAdId(username));
                        if (response.Data == null)
                        {
                            response.Success = false;
                            response.Message = "Invalid username or password.";
                            return response;
                        }
                        response.Success = true;
                        response.Message = "Login successful.";
                    }
                }
                response.Success = true;
                response.Message = "Login successful.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error in Login: {ex.Message}";
            }
            return response;
        }
        // lấy quyền user
        public async Task<GenericResponse<string>> GetRoleAsync(string adId)
        {
            var response = new GenericResponse<string>();
            try
            {
                var role = await _repo.GetRoleAsync(adId);
                response.Data = role;
                response.Success = true;
                response.Message = "Role retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error in GetRoleAsync: {ex.Message}";
            }
            return response;
        }
        // Inser thông tin và đăng ký user đăng nhập
        public async Task<GenericResponse<bool>> InsertListUserAsync(List<TM_USER> users)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.InsertListUserAsync(users);
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Success = false;
                result.Message = $"Error in InsertMasterApproverSendMailAsync: {ex.Message}";
            }
            return result;
        }
    }
}
