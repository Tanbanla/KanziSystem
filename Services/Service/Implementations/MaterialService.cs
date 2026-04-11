using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class MaterialService: BaseService<MATERIAL, int, MATERIALDTO>, IMaterialService
    {
        private readonly IMaterialRepository _repo;
        public MaterialService(IMaterialRepository repository, IMapper mapper): base(repository, mapper)
        {
            _repo = repository;
        }
        // Lấy theo mã hàng
        public async Task<GenericResponse<MATERIALDTO>> GetByMaHangAsync(string maHang)
        {
            var result = new GenericResponse<MATERIALDTO>();
            try
            {
                var material = await _repo.GetByMaHangAsync(maHang);
                result.Data = _mapper.Map<MATERIALDTO>(material);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Tìm kiếm hàng hóa và phân trang
        public async Task<GenericResponse<List<MATERIALDTO>>> SearchAsync(string? MaHang, string? Name, string? NhomHang, int? pageIndex, int? pageSize)
        {
            var result = new GenericResponse<List<MATERIALDTO>>();
            try
            {
                var materials = await _repo.SearchAsync(MaHang, Name, NhomHang, pageIndex, pageSize);
                result.Data = _mapper.Map<List<MATERIALDTO>>(materials);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Lấy danh sách hàng hóa
        public async Task<GenericResponse<List<MATERIALDTO>>> GetMaterialsByNameOrCodeAsync(string keyword)
        {
            var result = new GenericResponse<List<MATERIALDTO>>();
            try
            {
                var materials = await _repo.GetMaterialsByNameOrCodeAsync(keyword);
                result.Data = _mapper.Map<List<MATERIALDTO>>(materials);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        public async Task<GenericResponse<List<dynamic>>> GetListMaterial()
        {
            var result = new GenericResponse<List<dynamic>>();
            try
            {
                var materials = await _repo.GetListMaterial();
                result.Data = materials;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // update danh sách linh kiện
        public async Task<GenericResponse<bool>> UpdateMaterialAsync(List<MATERIALDTO> materials)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var materialsToUpdate = _mapper.Map<List<MATERIAL>>(materials);
                await _repo.UpdateMaterialAsync(materialsToUpdate);
                result.Data = true;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // update thông tin danh sách linh kiện
        public async Task<GenericResponse<bool>> UpdateListThongTin(List<MATERIALDTO> listDTO)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var materials = _mapper.Map<List<MATERIAL>>(listDTO);
                // You may need to implement this method in IMaterialRepository if not already present
                var updateResult = await _repo.UpdateMaterialAsync(materials);
                result.Data = updateResult;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }

        // check mã linh kiện 
        public async Task<GenericResponse<bool>> CheckMaHangExistsAsync(string codeMaterial)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var exists = await _repo.CheckMaHangExistsAsync(codeMaterial);
                result.Data = exists;
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }

        // Insert 
        public async Task<GenericResponse<bool>> InsertMaterial(MATERIALDTO mt)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var dto = _mapper.Map<MATERIAL>(mt);
                result.Data = await _repo.InsertMaterial(dto);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        // Insert nhiều cho ma hang No list
        public async Task<GenericResponse<bool>> UpdateListThongTinNoList(List<MATERIALDTO> listMT)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var materials = _mapper.Map<List<MATERIAL>>(listMT);
                var updateResult = await _repo.UpdateListThongTinNoList(materials);
                result.Data = updateResult;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // lay ma hang hien tai
        public async Task<GenericResponse<string>> MaterialCodeLater(string type)
        {
            var result = new GenericResponse<string>();
            try
            {
                result.Data = await _repo.MaterialCodeLater(type);
                result.Success = true;
            }catch(Exception ex)
            {
                result.Message =ex.Message;
                result.Success = false;
            }
            return result;
        }
        // check ma hang
        public async Task<GenericResponse<string>> CheckMaterialCode(string keyword, string category)
        {
            var result = new GenericResponse<string>();
            try
            {
                result.Data = await _repo.CheckMaterialCode(keyword, category);
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
