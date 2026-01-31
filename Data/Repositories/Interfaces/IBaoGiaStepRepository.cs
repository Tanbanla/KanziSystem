using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IBaoGiaStepRepository: IBaseRepository<BaoGia_Step, int>
    {
        // Lấy danh sách bước báo giá theo mã quy trình
        public Task<List<BaoGia_Step>> GetStepsByNodeAsync(string note);
        // Lay cach phuong thuc gui mail
        public Task<List<BaoGia_Step>> GetStepsApproverAsync();
    }
}
