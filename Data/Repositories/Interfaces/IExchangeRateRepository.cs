using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IExchangeRateRepository: IBaseRepository<EXCHANGE_RATE, int>
    {
        // Lay tien chuyen doi
        public Task<float> GetExchangeRate();
    }
}
