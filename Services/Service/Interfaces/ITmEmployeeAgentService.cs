using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Agent;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface ITmEmployeeAgentService: IBaseService<TM_EMPLOYEE, decimal , TM_EMPLOYEEDTO>
    {
        public Task<GenericResponse<TM_EMPLOYEEDTO>> GetInforEmployeeByMail(string mail);
        // Lấy thông tin nhân viên theo phòng ban
        public Task<GenericResponse<List<dynamic>>> GetApproverBySection(string sectionCode);

    }
}
