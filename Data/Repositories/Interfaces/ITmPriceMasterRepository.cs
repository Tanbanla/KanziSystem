using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface ITmPriceMasterRepository: IBaseRepository<TM_PRICE_MASTER, long>
    {
        Task<List<TM_PRICE_MASTER>> GetByInternalPartCode(string internalPartCode);

        Task<List<TM_PRICE_MASTER>> InsertByDetailQuotation(List<BaoGia_Detail_of_Quotation> details);

        Task InsertImportedAsync(IReadOnlyCollection<TM_PRICE_MASTER> priceMasters);

        Task<TM_PRICE_MASTER> UpdatePriceMaster(TM_PRICE_MASTER tm);
    }
}
