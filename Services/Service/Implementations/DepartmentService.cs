using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class DepartmentService: BaseService<DEPARTMENT, int, DEPARTMENTDTO>, IDepartmentService 
    {
        private readonly IDeparmentRepository _repo;
        private readonly IMapper _mapper;

        public DepartmentService(IDeparmentRepository repo, IMapper mapper)
            : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        // danh sach cost
        public async Task<GenericResponse<List<DEPARTMENTDTO>>> GetAllDepartmentAsync()
        {
            var result =  new GenericResponse<List<DEPARTMENTDTO>>();
            try
            {
                var departments = await _repo.GetAllDepartmentAsync();
                result.Data = _mapper.Map<List<DEPARTMENTDTO>>(departments);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
            }
            return result;
        }
        // Update Section code
        public async Task<GenericResponse<bool>> UpdateSectionAsync(List<DEPARTMENT> ds)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var updateResult = await _repo.UpdateSectionAsync(ds);
                result.Data = updateResult;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
            }
            return result;
        }
        // Lay thong tin department theo code section
        public async Task<GenericResponse<List<DEPARTMENTDTO>>> GetDepartmentBySectionAsync(string codeSection)
        {
            var result = new GenericResponse<List<DEPARTMENTDTO>>();

            try
            {
                var departments = await _repo.GetDepartmentBySectionAsync(codeSection);
                result.Data = _mapper.Map<List<DEPARTMENTDTO>>(departments);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
            }

            return result;
        }
        // Lấy thông tin theo quyền
        public async Task<GenericResponse<List<DEPARTMENTDTO>>> GetNhomViTriByDepartmentIdAsync(string user)
        {
            var result = new GenericResponse<List<DEPARTMENTDTO>>();
            try
            {
                var departments = await _repo.GetNhomViTriByDepartmentIdAsync(user);
                result.Data = _mapper.Map<List<DEPARTMENTDTO>>(departments);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
            }
            return result;
        }
    }
}
