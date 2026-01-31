using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface ITmNccNewService : IBaseService<IM_NCC_NEW, int, IM_NCC_NEWDTO>
    {
        // lấy danh sách nhà cung cấp  ALL
        Task<GenericResponse<List<IM_NCC_NEWDTO>>> GetAllNccNew();
        // Lấy nhà cung cấp theo mã nhà cung cấp
        Task<GenericResponse<IM_NCC_NEWDTO>> GetNccNewByCode(string MaNCC);
        // Lây thông tin nhà cung cấp phân trang 
        Task<GenericResponse<List<IM_NCC_NEWDTO>>> GetNccNewPaging(string keyword, int pageIndex, int pageSize);
    }
}
