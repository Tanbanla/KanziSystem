using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IDepartmentService: IBaseService<DEPARTMENT, int, DEPARTMENTDTO>
    {

        // danh sach cost
        public Task<GenericResponse<List<DEPARTMENTDTO>>> GetAllDepartmentAsync();
        // Update Section code
        public Task<GenericResponse<bool>> UpdateSectionAsync(List<DEPARTMENT> ds);

        // Lay thong tin department theo code section
        public Task<GenericResponse<List<DEPARTMENTDTO>>> GetDepartmentBySectionAsync(string codeSection);
        // Lấy thông tin theo quyền
        public Task<GenericResponse<List<DEPARTMENTDTO>>> GetNhomViTriByDepartmentIdAsync(string user);
    }
}
