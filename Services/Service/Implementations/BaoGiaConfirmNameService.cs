using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaConfirmNameService : BaseService<BaoGia_Confirm_Name_Quotation, int, BaoGia_Confirm_Name_QuotationDTO>, IBaoGiaConfirmNameService
    {
        private readonly IBaoGiaConfirmNameRepository _repo;
        private readonly IMapper _mapper;
        public BaoGiaConfirmNameService(IBaoGiaConfirmNameRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        // search thông tin xác nhận tên hàng
        public async Task<GenericResponse<ListRequest<dynamic>>> SearchAsync(string? TenHang, string? SoDon, string? TrangThai, string? section, string? role,int pageIndex, int pageSize)
        {
            var result = new GenericResponse<ListRequest<dynamic>>();
            try
            {
                result.Data = await _repo.SearchAsync(TenHang, SoDon, TrangThai, section, role, pageIndex, pageSize);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        // Luu thong tin
        public async Task<GenericResponse<bool>> SaveConfirmNameAsync(int? Id, string? TenHaiQuan, string? MaHangNoiBo, string? Role, string User)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var res = await _repo.SaveConfirmNameAsync(Id, TenHaiQuan, MaHangNoiBo, Role, User);
                result.Data = res;
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
        public async Task<GenericResponse<bool>> AddConfirmNameAsync(BaoGia_Confirm_Name_QuotationDTO confirmName)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var data = _mapper.Map<BaoGia_Confirm_Name_Quotation>(confirmName);
                result.Data = await _repo.AddConfirmNameAsync(data);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Approve ConfirmName
        public async Task<GenericResponse<bool>> ApproveConfirmNameAsync(int id, string approvedBy)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.ApproveConfirmNameAsync(id, approvedBy);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Reject ConfirmName
        public async Task<GenericResponse<bool>> RejectConfirmNameAsync(int id, string reason, string rejectedBy)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.RejectConfirmNameAsync(id, reason, rejectedBy);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Insert thong tin danh sach
        public async Task<GenericResponse<bool>> AddListAsync(List<BaoGia_Confirm_Name_QuotationDTO> confirmNames)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var data = _mapper.Map<List<BaoGia_Confirm_Name_Quotation>>(confirmNames);
                result.Data = await _repo.AddListAsync(data);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // luu thong tin nhap file
        public async Task<GenericResponse<bool>> SaveFromFileAsync(List<BaoGia_Confirm_Name_Quotation> confirmNames, string user, string? Role)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.SaveFromFileAsync(confirmNames, user, Role);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // luu thong tin 
        public async Task<GenericResponse<bool>> SaveConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.SaveConfirmNameListAsync(saveConfirms, user, Role);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;

            }
            return result;
        }
        // Approvers
        public async Task<GenericResponse<bool>> ApproveConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.ApproveConfirmNameListAsync(saveConfirms, user, Role);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Rejects Acc
        public async Task<GenericResponse<bool>> RejectAccConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.RejectAccConfirmNameListAsync(saveConfirms, user, Role);
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Export Code Cofirmed
        public async Task<GenericResponse<List<dynamic>>> ExportCodeConfirmedAsync()
        {
            var result = new GenericResponse<List<dynamic>>();
            try
            {
                result.Data = await _repo.ExportCodeConfirmedAsync();
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Từ chối xác nhận tên hàng
        public async Task<GenericResponse<bool>> RejectConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.RejectConfirmNameListAsync(saveConfirms, user, Role);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Check mã đơn đã xác nhận tên hàng đã hoàn thành hay chưa
        public async Task<GenericResponse<List<ResultCheckCofirmName>>> CheckDonHangConfirmedAsync(List<int> listCheck)
        {
            var result = new GenericResponse<List<ResultCheckCofirmName>>();
            try
            {
                result.Data = await _repo.CheckDonHangConfirmedAsync(listCheck);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Cập nhật thông tin đơn báo giá sau khi trả lại
        public async Task<GenericResponse<bool>> UpdateRequestFromFileAsync(List<BaoGia_Request_of_QuotationDTO> baoGia, string user)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var rq = _mapper.Map<List<BaoGia_Request_of_Quotation>>(baoGia);
                result.Data = await _repo.UpdateRequestFromFileAsync(rq, user);
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
