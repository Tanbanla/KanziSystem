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
        private readonly IMapper _mapper;
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
        // lay thong tin san pham lien quan den nha cung cap
        public async Task<GenericResponse<List<BaoGia_NCCDTO>>> GetBaoGiaNCCByNCC(string maNCC)
        {
            var result = new GenericResponse<List<BaoGia_NCCDTO>>();
            try
            {
                var data = await _repo.GetBaoGiaNCCByNCC(maNCC);
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
        // them thong tin
        public async Task<GenericResponse<bool>> AddBaoGiaNCC(BaoGia_NCCDTO baoGiaNCC)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var existingData = _mapper.Map<BaoGia_NCC>(baoGiaNCC);
                var addResult = await _repo.AddBaoGiaNCC(existingData);
                result.Data = addResult;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result; 
        }
        // xoa thong tin
        public async Task<GenericResponse<bool>> DeleteBaoGiaNCC(int id)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var deleteResult = await _repo.DeleteBaoGiaNCC(id);
                result.Data = deleteResult;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        // update thong tin
        public async Task<GenericResponse<bool>> UpdateBaoGiaNCC(BaoGia_NCCDTO baoGiaNCC)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var existingData = _mapper.Map<BaoGia_NCC>(baoGiaNCC);
                var updateResult = await _repo.UpdateBaoGiaNCC(existingData);
                result.Data = updateResult;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        // update list thong tin
        public async Task<GenericResponse<bool>> UpdateListBaoGiaNCC(List<BaoGia_NCCDTO> listBaoGia)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var existingData = _mapper.Map<List<BaoGia_NCC>>(listBaoGia);
                var updateResult = await _repo.UpdateListBaoGiaNCC(existingData);
                result.Data = updateResult;
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
