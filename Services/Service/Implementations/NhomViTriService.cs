using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class NhomViTriService: BaseService<ACC_NHOMVITRI, int, ACC_NHOMVITRIDTO>, INhomViTriService
    {
        private readonly INhomViTriRepository _repo;
        private readonly IMapper _mapper;
        public NhomViTriService(INhomViTriRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        // Lấy danh sách nhóm vị trí
        public async Task<GenericResponse<List<ACC_NHOMVITRIDTO>>> GetAllNhomViTriAsync()
        {
            var response = new GenericResponse<List<ACC_NHOMVITRIDTO>>();
            try
            {
                var nhomViTriList = await _repo.GetAllNhomViTriAsync();
                var nhomViTriDTOList = _mapper.Map<List<ACC_NHOMVITRIDTO>>(nhomViTriList);
                response.Data = nhomViTriDTOList;
                response.Success = true;
                response.Message = "Lấy danh sách nhóm vị trí thành công.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Lỗi khi lấy danh sách nhóm vị trí: " + ex.Message;
            }
            return response;
        }
        // Insert list Section
        public async Task<GenericResponse<bool>> InsertNhomViTriListAsync(List<ACC_NHOMVITRIDTO> nhomViTriDTOs)
        {
            var response = new GenericResponse<bool>();
            try
            {
                var nhomViTriList = _mapper.Map<List<ACC_NHOMVITRI>>(nhomViTriDTOs);
                var result = await _repo.InsertListSectionAsync(nhomViTriList);
                response.Data = result;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Lỗi khi thêm danh sách nhóm vị trí: " + ex.Message;
            }
            return response;
        }
    }
}
