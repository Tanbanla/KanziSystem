using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class BaoGiaStepRepository: BaseRepository<BaoGia_Step, int>, IBaoGiaStepRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public BaoGiaStepRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
            : base(context, options, configuration)
        {
            _context = context;
        }
        // Lấy danh sách bước báo giá theo mã quy trình
        public async Task<List<BaoGia_Step>> GetStepsByNodeAsync(string note)
        {
            try
            {
                var query = _context.BaoGia_Steps.AsQueryable();
                if (!string.IsNullOrEmpty(note))
                {
                    query = query.Where(x => x.CHR_Note == note);
                }
                return await query
                    .OrderBy(x => x.ID)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetStepsByNodeAsync: {ex.Message}");
                return new List<BaoGia_Step>();
            }
        }
        // Lay cach phuong thuc gui mail
        public async Task<List<BaoGia_Step>> GetStepsApproverAsync()
        {
            try
            {
                return await _context.BaoGia_Steps
                    .Where(x => x.CHR_Note == "PUBLISH" && x.CHR_Status == "APPROVAL")
                    .OrderBy(x => x.INT_StepNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetStepsApproverAsync: {ex.Message}");
                return new List<BaoGia_Step>();
            }
        }
    }
}
