using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaWFStepUserService: BaseService<BaoGia_WorkflowStepUser, int, BaoGia_WorkflowStepUserDTO>, IBaoGiaWFStepUserService
    {
        private readonly IBaoGiaWFStepUserRepositrory _repo;
        private readonly IMapper _mapper;
        public BaoGiaWFStepUserService(IBaoGiaWFStepUserRepositrory repo, IMapper mapper, IConfiguration configuration) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
    }
}
