using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaNCCService: BaseService<BaoGia_NCC, int, BaoGia_NCCDTO>, IBaoGiaNCCService
    {
        private readonly IBaoGiaNCCRepository _repo;
        private IMapper _mapper;
        public BaoGiaNCCService(IBaoGiaNCCRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repo = repository;
            _mapper = mapper;
        }
        // lay thong tin
        public async Task<GenericResponse<List<BaoGia_NCCDTO>>> GetBaoGiaNCCByMaHang(string maHang)
        {
            var result = new GenericResponse<List<BaoGia_NCCDTO>>();
            try
            {
                var data = await _repo.GetBaoGiaNCCByMaHang(maHang);
                result.Data = _mapper.Map<List<BaoGia_NCCDTO>>(data);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
    }
}
