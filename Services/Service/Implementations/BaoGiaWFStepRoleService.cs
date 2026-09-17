using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaWFStepRoleService: BaseService<BaoGia_WorkflowStepRole, int, BaoGia_WorkflowStepRoleDTO>, IBaoGiaWFStepRoleService
    {
        private readonly IBaoGiaWFStepRoleRepository _repo;
        private readonly IMapper _mapper;
        public BaoGiaWFStepRoleService(IBaoGiaWFStepRoleRepository repo, IMapper mapper, IConfiguration configuration) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
    }
}
