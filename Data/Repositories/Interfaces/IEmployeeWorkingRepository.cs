namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IEmployeeWorkingRepository
    {
        // Lấy thông tin nhân viên theo ADID or MNV
        Task<IEnumerable<dynamic>> GetEmployeeWorkingByIdAsync(string adidOrMnv);
    }
}
