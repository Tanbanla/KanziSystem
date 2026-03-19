using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class EmployeeWorkingService: IEmployeeWorkingService
    {
        private readonly IEmployeeWorkingRepository _repo;
        public EmployeeWorkingService(IEmployeeWorkingRepository repo)
        {
            _repo = repo;
        }
        // Lay thong tin nhan vien theo ADID or MNV
        public async Task<GenericResponse<IEnumerable<dynamic>>> GetEmployeeWorkingByIdAsync(string adidOrMnv)
        {
            var result = new GenericResponse<IEnumerable<dynamic>>();
            try
            {
                result.Data = await _repo.GetEmployeeWorkingByIdAsync(adidOrMnv);
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        public async Task<GenericResponse<List<string>>> GetCodeCenterBySec(string codeSecion)
        {
            var result = new GenericResponse<List<string>>();
            try
            {
                result.Data = await _repo.GetCodeCenterBySec(codeSecion);
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
    }
}
