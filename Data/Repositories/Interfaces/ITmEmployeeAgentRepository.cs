using PRJ_WAREHOUSE_BIVN.Models_Agent;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface ITmEmployeeAgentRepository: IBaseRepository<TM_EMPLOYEE, decimal>
    {
        public Task<TM_EMPLOYEE> GetInforEmployeeByMail(string mail);
    }
}
