using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaStepService: IBaseService<BaoGia_Step, int, BaoGia_StepDTO>
    {
        // Lấy danh sách bước báo giá theo mã quy trình
        public Task<GenericResponse<List<BaoGia_StepDTO>>> GetStepsByNodeAsync(string note);
        // Lay cach phuong thuc gui mail
        public Task<GenericResponse<List<BaoGia_StepDTO>>> GetStepsApproverAsync();
        // Get all
        public Task<GenericResponse<List<BaoGia_StepDTO>>> GetAll();
    }
}
