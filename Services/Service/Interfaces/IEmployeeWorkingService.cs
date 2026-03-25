using PRJ_WAREHOUSE_BIVN.Common;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IEmployeeWorkingService
    {
        // Lấy thông tin nhân viên theo ADID or MNV
        Task<GenericResponse<IEnumerable<dynamic>>> GetEmployeeWorkingByIdAsync(string adidOrMnv);
        // lay  CHR_CODE_CENTER
        Task<GenericResponse<List<string>>> GetCodeCenterBySec(string codeSecion);
        // Lay code phong ban
        Task<GenericResponse<string>> GetCodeSec(string sectionName);
    }
}
