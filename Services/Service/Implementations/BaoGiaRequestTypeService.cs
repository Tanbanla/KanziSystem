using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaRequestTypeService: BaseService<BaoGia_RequestType, int, BaoGia_RequestTypeDTO>, IBaoGiaRequestTypeService
    {
        private readonly IBaoGiaRequestTypeRepository _baoGiaRequestTypeRepository;
        private readonly IMapper _mapper;
        public BaoGiaRequestTypeService(IBaoGiaRequestTypeRepository baoGiaRequestTypeRepository, IMapper mapper) : base(baoGiaRequestTypeRepository,mapper)
        {
            _baoGiaRequestTypeRepository = baoGiaRequestTypeRepository;
        }
    }
}
