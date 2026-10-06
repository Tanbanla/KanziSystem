using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface ITmPriceMasterService: IBaseService<TM_PRICE_MASTER, long, TM_PRICE_MASTERDTO>
    {
        Task<GenericResponse<List<TM_PRICE_MASTERDTO>>> GetByInternalPartCode(string internalPartCode);

        Task<GenericResponse<List<TM_PRICE_MASTERDTO>>> InsertByDetailQuotation(List<BaoGia_Detail_of_Quotation> details);

        Task<GenericResponse<int>> InsertImportedAsync(IReadOnlyCollection<TM_PRICE_MASTER> priceMasters);

        Task<GenericResponse<TM_PRICE_MASTERDTO>> UpdatePriceMaster(TM_PRICE_MASTERDTO tm);
    }
}
