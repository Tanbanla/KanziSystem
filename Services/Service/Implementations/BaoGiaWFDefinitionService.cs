using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaWFDefinitionService: BaseService<BaoGia_WorkflowDefinition, int, BaoGia_WorkflowDefinitionDTO>, IBaoGiaWFDefinitionService
    {
        private readonly IBaoGiaWFDefinitionRepository _repo;
        private readonly IMapper _mapper;
        public BaoGiaWFDefinitionService(IBaoGiaWFDefinitionRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repo = repository;
            _mapper = mapper;
        }
    }
}
