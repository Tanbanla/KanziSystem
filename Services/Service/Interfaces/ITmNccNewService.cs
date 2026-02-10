using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface ITmNccNewService : IBaseService<IM_NCC_NEWDTO, int, IM_NCC_NEW>
    {
        // lấy danh sách nhà cung cấp  ALL
        Task<GenericResponse<List<IM_NCC_NEWDTO>>> GetAllNccNew();
        // Lấy nhà cung cấp theo mã nhà cung cấp
        Task<GenericResponse<IM_NCC_NEWDTO>> GetNccNewByCode(string MaNCC);
        // Lây thông tin nhà cung cấp phân trang 
        Task<GenericResponse<List<IM_NCC_NEWDTO>>> GetNccNewPaging(string? CodeNcc, string? NameNcc, int pageIndex, int pageSize);
        // Xoa thong tin nha cung cap
        Task<GenericResponse<bool>> DeleteNccNewByCode(int id, string userAction);
        // Thêm thông tin nhà cung cấp 
        Task<GenericResponse<bool>> AddNccNew(IM_NCC_NEWDTO nccNew);
        // Update thong tin nha cung cap
        Task<GenericResponse<bool>> UpdateNccNew(IM_NCC_NEWDTO nccNew);
        // thêm danh sách nhà cung cấp
        Task<GenericResponse<bool>> AddListNccNew(List<IM_NCC_NEWDTO> listNccNew);
    }
}
