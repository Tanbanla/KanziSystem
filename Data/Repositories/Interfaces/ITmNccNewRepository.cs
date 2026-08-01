using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface ITmNccNewRepository : IBaseRepository<IM_NCC_NEW, int>
    {
        // lấy danh sách nhà cung cấp  ALL
        Task<List<IM_NCC_NEW>> GetAllNccNew();
        // Lấy nhà cung cấp theo mã nhà cung cấp
        Task<IM_NCC_NEW> GetNccNewByCode(string MaNCC);
        // Lây thông tin nhà cung cấp phân trang 
        Task<List<IM_NCC_NEW>> GetNccNewPaging(string? CodeNcc, string? NameNcc, int pageIndex, int pageSize);
        // Xoa thong tin nha cung cap
        Task<bool> DeleteNccNewByCode(int id, string userAction);
        // Thêm thông tin nhà cung cấp 
        Task<bool> AddNccNew(IM_NCC_NEW nccNew);
        // Update thong tin nha cung cap
        Task<bool> UpdateNccNew(IM_NCC_NEW nccNew);
        // thêm danh sách nhà cung cấp
        Task<bool> AddListNccNew(List<IM_NCC_NEW> listNccNew);
        // Update Short Name Suppelier
        Task<bool> UpdateShortNames(List<IM_NCC_NEW> listUpate);
        //Exprort master Vender
        Task<List<dynamic>> ExportMasterVender();
        // Lấy danh sách nhà cung cấp không cần xác nhận thủ tục hải quan
        Task<List<string>> ListNotConfirmName();
    }
}
