using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaWorkflowRoleService: BaseService<BaoGia_WorkflowRole, int, BaoGia_WorkflowRoleDTO>, IBaoGiaWorkflowRoleService
    {
        private readonly IBaoGiaWorkflowRoleRepository _repo;
        private readonly IMapper _mapper;
        public BaoGiaWorkflowRoleService(IBaoGiaWorkflowRoleRepository repo, IMapper mapper, IConfiguration configuration) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
    }
}
