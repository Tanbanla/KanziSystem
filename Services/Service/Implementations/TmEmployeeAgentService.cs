using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Agent;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class TmEmployeeAgentService: BaseService<TM_EMPLOYEE, decimal, TM_EMPLOYEEDTO>, ITmEmployeeAgentService
    {
        private readonly ITmEmployeeAgentRepository _repo;
        private IMapper _mapper;
        public TmEmployeeAgentService(ITmEmployeeAgentRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repo = repository;
            _mapper = mapper;
        }
        public async Task<GenericResponse<TM_EMPLOYEEDTO>> GetInforEmployeeByMail(string mail)
        {
            var result = new GenericResponse<TM_EMPLOYEEDTO>();
            try
            {
                var dataRepo = await _repo.GetInforEmployeeByMail(mail);
                result.Data = _mapper.Map<TM_EMPLOYEEDTO>(dataRepo);
                result.Success = true;
            }catch(Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }
    }
}
