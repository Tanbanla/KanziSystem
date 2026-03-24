using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class MasterApproverSendMailService: BaseService<BaoGia_Master_Approver_Send_Mail, int , BaoGia_Master_Approver_Send_MailDTO>, IMasterApproverSendMailService
    {
        private readonly IMasterApproverSendMailRepository _repo;
        public MasterApproverSendMailService(IMasterApproverSendMailRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
        }
        // Lấy dữ liệu theo điều kiện và phân trang
        public async Task<GenericResponse<List<BaoGia_Master_Approver_Send_MailDTO>>> GetByConditionAsync(string? sectionCode, string? adid, int? IdStep, int pageIndex, int pageSize)
        {
            var response = new GenericResponse<List<BaoGia_Master_Approver_Send_MailDTO>>();
            try
            {
                var data = await _repo.GetByConditionAsync(sectionCode, adid, IdStep, pageIndex, pageSize);
                response.Data = _mapper.Map<List<BaoGia_Master_Approver_Send_MailDTO>>(data);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error in GetByConditionAsync: {ex.Message}";
            }
            return response;
        }
        // Lưu thông tin
        public async Task<GenericResponse<bool>> SaveMasterApproverSendMailAsync(BaoGia_Master_Approver_Send_MailDTO obj)
        {
            var response = new GenericResponse<bool>();
            try
            {
                var result = await _repo.SaveMasterApproverSendMailAsync(_mapper.Map<BaoGia_Master_Approver_Send_Mail>(obj));
                response.Data = result;
                response.Success = result;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error in SaveMasterApproverSendMailAsync: {ex.Message}";
            }
            return response;
        }
        // Sửa thông tin
        public async Task<GenericResponse<bool>> UpdateMasterApproverSendMailAsync(BaoGia_Master_Approver_Send_MailDTO obj)
        {
            var response = new GenericResponse<bool>();
            try
            {
                var result = await _repo.UpdateMasterApproverSendMailAsync(_mapper.Map<BaoGia_Master_Approver_Send_Mail>(obj));
                response.Data = result;
                response.Success = result;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error in UpdateMasterApproverSendMailAsync: {ex.Message}";
            }
            return response;
        }
        // Xóa thông tin
        public async Task<GenericResponse<bool>> DeleteMasterApproverSendMailAsync(int id, string userAction)
        {
            var response = new GenericResponse<bool>();
            try
            {
                var result = await _repo.DeleteMasterApproverSendMailAsync(id,userAction);
                response.Data = result;
                response.Success = result;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error in DeleteMasterApproverSendMailAsync: {ex.Message}";
            }
            return response;
        }
        // Lấy thông tin phê duyệt step của phòng ban
        public async Task<GenericResponse<List<BaoGia_Master_Approver_Send_MailDTO>>> GetApproverByStepAndSectionAsync(int idStep, string sectionCode)
        {
            var response = new GenericResponse<List<BaoGia_Master_Approver_Send_MailDTO>>();
            try
            {
                var data = await _repo.GetApproverByStepAndSectionAsync(idStep, sectionCode);
                response.Data = _mapper.Map<List<BaoGia_Master_Approver_Send_MailDTO>>(data);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error in GetApproverByStepAndSectionAsync: {ex.Message}";
            }
            return response;
        }
        // Inser thông tin và đăng ký user đăng nhập
        public async Task<GenericResponse<bool>> InsertMasterApproverSendMailAsync(List<BaoGia_Master_Approver_Send_Mail> dtos)
        {
            var response = new GenericResponse<bool>();
            try
            {
                var result = await _repo.InsertMasterApproverSendMailAsync(dtos);
                response.Data = result;
                response.Success = true;
            }
            catch(Exception ex)
            {
                response.Success = false;
                response.Message = $"Error in InsertMasterApproverSendMailAsync: {ex.Message}";
            }
            return response;
        }
    }
}
