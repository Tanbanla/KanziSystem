using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using static PRJ_WAREHOUSE_BIVN.View_Models.Material.MaterialVM;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaConfirmNameService : BaseService<BaoGia_Confirm_Name_Quotation, int, BaoGia_Confirm_Name_QuotationDTO>, IBaoGiaConfirmNameService
    {
        private readonly IBaoGiaConfirmNameRepository _repo;
        private readonly IBaoGiaDetailRepository _detailBaoGiaRespository;
        private readonly IMapper _mapper;
        public BaoGiaConfirmNameService(IBaoGiaConfirmNameRepository repo, IBaoGiaDetailRepository baoGiaDetailRepository, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
            _detailBaoGiaRespository = baoGiaDetailRepository;
        }
        // search thông tin xác nhận tên hàng
        public async Task<GenericResponse<ListRequest<dynamic>>> SearchAsync(ConfirmNameSearchRequest req, string user, string? role)
        {
            var result = new GenericResponse<ListRequest<dynamic>>();
            try
            {
                result.Data = await _repo.SearchAsync(req, user, role);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }

        public async Task<GenericResponse<bool>> EditConfirmNameAsync(ConfirmNameEditRequest request, string user, string? role)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.EditConfirmNameAsync(request, user, role);
                result.Success = result.Data;
                if (!result.Success) result.Message = "Không thể cập nhật dữ liệu.";
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
        public async Task<GenericResponse<List<BaoGia_Confirm_Name_QuotationDTO>>> AddListAsync(List<BaoGia_Confirm_Name_QuotationDTO> confirmNames)
        {
            var result = new GenericResponse<List<BaoGia_Confirm_Name_QuotationDTO>>();
            try
            {
                var data = _mapper.Map<List<BaoGia_Confirm_Name_Quotation>>(confirmNames);
                var rq = await _repo.AddListAsync(data);
                result.Data = _mapper.Map<List<BaoGia_Confirm_Name_QuotationDTO>>(rq);
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
        public async Task<GenericResponse<bool>> SaveFromFileAsync(List<ConfirmNameInputExcel> confirmNames, string user, string? Role)
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
        // Rejects Ship
        public async Task<GenericResponse<bool>> RejectShipConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.RejectShipConfirmNameListAsync(saveConfirms, user, Role);
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
        public async Task<GenericResponse<List<ResultCheckCofirmName>>> SearchSendMailConfirmNameAsync(List<int> listCheck)
        {
            var result = new GenericResponse<List<ResultCheckCofirmName>>();
            try
            {
                result.Data = await _repo.SearchSendMailConfirmNameAsync(listCheck);
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
        // Cập nhật thông tin yêu cầu PIC PUR cần xác nhận lại báo giá
        public async Task<GenericResponse<bool>> UpdateRequestForPICPURAsync(List<ConfirmNameInputExcel> baoGia, string user)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.UpdateRequestForPICPURAsync(baoGia, user);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        //Export file ten hanh xac nhan
        public async Task<GenericResponse<List<dynamic>>> ExportConfirmedMaterialNamesAsync(string? TenHang, string? SoDon, string? TrangThai, string? section, string? role, string user)
        {
            var result = new GenericResponse<List<dynamic>>();
            try
            {
                result.Data = await _repo.ExportConfirmedMaterialNamesAsync(TenHang, SoDon, TrangThai, section, role, user);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Update Name HQ role PIC PUR
        public async Task<GenericResponse<bool>> UpdateNameHQRolePICPURAsync(List<ConfirmNameInputExcel> baoGia, string user)
        {
            var result = new GenericResponse<bool>();
            try
            {
                // Lưu thông tin xác nhận tên
                var rp = await _repo.UpdateNameHQRolePICPURAsync(baoGia, user);

                // Chuyển link sang link báo giá mới
                

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
        // Done
        public async Task<GenericResponse<List<ResultCheckCofirmName>>> DoneConfirmNameAsync(List<int> listDone)
        {
            var result = new GenericResponse<List<ResultCheckCofirmName>>();
            try
            {
                result.Data = await _repo.DoneConfirmNameAsync(listDone);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Check đơn đã hoàn thành hay chưa
        public async Task<GenericResponse<List<int>>> CheckConfirmNameDoneAsync(List<int> listCheck)
        {
            var result = new GenericResponse<List<int>>();
            try
            {
                result.Data = await _repo.CheckConfirmNameDoneAsync(listCheck);
                result.Success = true;

            }
            catch(Exception ex) {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;

        }

        public async Task<GenericResponse<List<ConfirmNameHistoryDTO>>> GetConfirmNameHistoryAsync(int confirmId)
        {
            var result = new GenericResponse<List<ConfirmNameHistoryDTO>>();
            try
            {
                result.Data = await _repo.GetConfirmNameHistoryAsync(confirmId);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }

            return result;
        }
        // Count ConfirName
        public async Task<GenericResponse<CountCofirmName>> GetCountCofirmNames(ConfirmNameSearchRequest req, string user, string? role)
        {
            var result = new GenericResponse<CountCofirmName>();
            try
            {
                result.Data = await _repo.GetCountCofirmNames(req, user, role);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        public async Task<GenericResponse<bool>> ConfirmNameShipAsync(ConfirmNameEditRequest request, string user)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.ConfirmNameShipAsync(request, user);
                result.Success = result.Data;
                if (!result.Success) result.Message = "Không thể xác nhận tên hải quan.";
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
