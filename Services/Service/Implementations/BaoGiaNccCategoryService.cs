using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaNccCategoryService: BaseService<BaoGia_NCC_Category, int ,BaoGia_NCC_CategoryDTO>, IBaoGiaNccCategoryService
    {
        private readonly IBaoGiaNccCategory _repo;
        private readonly IMapper _mapper;

        public BaoGiaNccCategoryService(IBaoGiaNccCategory repository, IMapper mapper) : base(repository, mapper)
        {
            _repo = repository;
            _mapper = mapper;
        }
        // lấy danh sách theo mã NCC
        public async Task<GenericResponse<List<BaoGia_NCC_CategoryDTO>>> GetBaoGiaNccCategoryByMaNCC(string maNCC)
        {
            var result = new GenericResponse<List<BaoGia_NCC_CategoryDTO>>();
            try
            {
                var data = await _repo.GetBaoGiaNccCategoryByMaNCC(maNCC);
                result.Data = _mapper.Map<List<BaoGia_NCC_CategoryDTO>>(data);
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // lấy danh sách theo chung loai
        public async Task<GenericResponse<List<BaoGia_NCC_CategoryDTO>>> GetBaoGiaNccCategoryByChungLoai(string chungLoai)
        {
            var result = new GenericResponse<List<BaoGia_NCC_CategoryDTO>>();
            try
            {
                var data = await _repo.GetBaoGiaNccCategoryByChungLoai(chungLoai);
                result.Data = _mapper.Map<List<BaoGia_NCC_CategoryDTO>>(data);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // them thong tin
        public async Task<GenericResponse<bool>> AddBaoGiaNccCategory(BaoGia_NCC_CategoryDTO baoGiaNccCategory)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var entity = _mapper.Map<BaoGia_NCC_Category>(baoGiaNccCategory);
                result.Data = await _repo.AddBaoGiaNccCategory(entity);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // xoa thong tin
        public async Task<GenericResponse<bool>> DeleteBaoGiaNccCategory(int id)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.DeleteBaoGiaNccCategory(id);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // them danh sach
        public async Task<GenericResponse<bool>> AddListBaoGiaNccCategory(List<BaoGia_NCC_CategoryDTO> listBaoGiaNccCategory)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var entities = _mapper.Map<List<BaoGia_NCC_Category>>(listBaoGiaNccCategory);
                result.Data = await _repo.AddListBaoGiaNccCategory(entities);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // update thong tin
        public async Task<GenericResponse<bool>> UpdateBaoGiaNccCategory(BaoGia_NCC_CategoryDTO baoGiaNccCategory)
        {
            var result = new GenericResponse<bool>();
            try
            {
               var entity = _mapper.Map<BaoGia_NCC_Category>(baoGiaNccCategory);
               result.Data = await _repo.UpdateBaoGiaNccCategory(entity);
               result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Check Supperlier and Catergory
        public async Task<GenericResponse<bool>> CheckSupperlier(string codeSupperlier, string catergory)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.CheckSupperlier(codeSupperlier, catergory);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
    }
}
