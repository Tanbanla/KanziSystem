using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class ExchangeRateRepository: BaseRepository<EXCHANGE_RATE, int>, IExchangeRateRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public ExchangeRateRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
        : base(context, options, configuration)
        {
            _context = context;
        }
        // Lay tien chuyen doi
        public async Task<float> GetExchangeRate()
        {
            var exchangeRate = await _context.EXCHANGE_RATEs.Where(c => c.Currency == "VND").OrderByDescending(e => e.Id).FirstOrDefaultAsync();
            if (exchangeRate != null)
            {
                return float.Parse(exchangeRate.Rate);
            }
            else
            {
                throw new Exception("Không tìm thấy tỷ giá hối đoái.");
            }
        }
    }
}
