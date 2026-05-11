using PRJ_WAREHOUSE_BIVN.Common;
using System.Threading.Tasks;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IFileImportService
    {
        Task<GenericResponse<string?>> SaveFileFromPathAsync(string sourcePath);
    }
}
