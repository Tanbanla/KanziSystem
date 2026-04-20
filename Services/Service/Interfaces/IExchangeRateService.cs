using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IExchangeRateService: IBaseService<EXCHANGE_RATE, int , EXCHANGE_RATEDTO>
    {
        // Lay tien chuyen doi
        public Task<GenericResponse<float>> GetExchangeRate();
    }
}
