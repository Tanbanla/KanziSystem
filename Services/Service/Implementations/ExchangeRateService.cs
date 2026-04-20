using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class ExchangeRateService: BaseService<EXCHANGE_RATE, int, EXCHANGE_RATEDTO>, IExchangeRateService
    {
        private readonly IMapper _mapper;
        private readonly IExchangeRateRepository _repo;
        public ExchangeRateService(IExchangeRateRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _mapper = mapper;
            _repo = repository;
        }
        // Lay tien chuyen doi
        public async Task<GenericResponse<float>> GetExchangeRate()
        {
            var response = new GenericResponse<float>();
            try
            {
                var exchangeRate = await _repo.GetExchangeRate();
                response.Data = exchangeRate;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
