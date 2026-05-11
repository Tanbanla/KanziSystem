using System.Threading.Tasks;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IFileImportService
    {
        Task<string?> SaveFileFromPathAsync(string sourcePath);
    }
}
