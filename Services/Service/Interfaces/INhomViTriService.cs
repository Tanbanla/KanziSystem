using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface INhomViTriService : IBaseService<ACC_NHOMVITRI, int, ACC_NHOMVITRIDTO>
    {
        // Lấy danh sách nhóm vị trí 
        Task<GenericResponse<List<ACC_NHOMVITRIDTO>>> GetAllNhomViTriAsync();
        // Lấy thông tin theo quyền
        Task<GenericResponse<List<ACC_NHOMVITRIDTO>>> GetNhomViTriByDepartmentIdAsync(string user);
        // Insert list Section
        Task<GenericResponse<bool>> InsertNhomViTriListAsync(List<ACC_NHOMVITRIDTO> nhomViTriDTOs);
    }
}
