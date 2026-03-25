namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IEmployeeWorkingRepository
    {
        // Lấy thông tin nhân viên theo ADID or MNV
        Task<IEnumerable<dynamic>> GetEmployeeWorkingByIdAsync(string adidOrMnv);
        // lay  CHR_CODE_CENTER
        Task<List<string>> GetCodeCenterBySec(string codeSecion);
        // Lay code phong ban
        Task<string> GetCodeSec(string sectionName);
    }
}
