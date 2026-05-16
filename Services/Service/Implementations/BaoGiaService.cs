using AutoMapper;
using AutoMapper.Configuration.Annotations;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaService : BaseService<BaoGia_Request_of_Quotation, int, BaoGia_Request_of_QuotationDTO>, IBaoGiaService
    {
        private readonly IBaoGiaRepository _repo;
        private readonly IMapper _mapper;
        public BaoGiaService(IBaoGiaRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        // Lấy thông tin báo giá theo mã báo giá
        public async Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> GetByMaBaoGiaAsync(string maBaoGia)
        {
            var result = new GenericResponse<List<BaoGia_Request_of_QuotationDTO>>();
            try
            {
                result.Data = _mapper.Map<List<BaoGia_Request_of_QuotationDTO>>(await _repo.GetByMaBaoGiaAsync(maBaoGia));
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Tìm kiếm thông tin báo giá và phân trang
        public async Task<GenericResponse<ListRequest<BaoGia_Request_of_QuotationDTO>>> SearchAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, string? user, int pageIndex, int pageSize, DateTime? date, string? chungLoai)
        {
            var result = new GenericResponse<ListRequest<BaoGia_Request_of_QuotationDTO>>();
            try
            {
                var data = await _repo.SearchAsync(MaDon, MaNcc, Section, nguoiYeuCau, MaHang, status, step, user, pageIndex, pageSize, date, chungLoai);
                var mappedData = _mapper.Map<List<BaoGia_Request_of_QuotationDTO>>(data.Data);
                result.Data = new ListRequest<BaoGia_Request_of_QuotationDTO>
                {
                    Data = mappedData,
                    TotalCount = data.TotalCount
                };
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Nhap bao gia
        public async Task<GenericResponse<bool>> NhapBaoGiaAsync(BaoGia_Request_of_QuotationDTO baoGia)
        {
            var result = new GenericResponse<bool>();
            var data = _mapper.Map<BaoGia_Request_of_Quotation>(baoGia);
            try
            {
                result.Data = await _repo.NhapBaoGiaAsync(data);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
                result.Data = false;
            }
            return result;
        }
        // Nhap danh sach bao gia
        public async Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> NhapDanhSachBaoGiaAsync(List<BaoGia_Request_of_QuotationDTO> danhSachBaoGia)
        {
            var result = new GenericResponse<List<BaoGia_Request_of_QuotationDTO>>();
            var data = _mapper.Map<List<BaoGia_Request_of_Quotation>>(danhSachBaoGia);
            try
            {
                var inserted = await _repo.NhapDanhSachBaoGiaAsync(data);
                result.Data = _mapper.Map<List<BaoGia_Request_of_QuotationDTO>>(inserted);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Cap nhat thông tin báo giá
        public async Task<GenericResponse<bool>> CapNhatThongTinBaoGiaAsync(BaoGia_Request_of_QuotationDTO baoGia)
        {
            var result = new GenericResponse<bool>();
            var data = _mapper.Map<BaoGia_Request_of_Quotation>(baoGia);
            try
            {
                result.Data = await _repo.CapNhatThongTinBaoGiaAsync(data);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
                result.Data = false;
            }
            return result;
        }
        // Lấy danh sách mã đơn báo giá
        public async Task<GenericResponse<List<string>>> GetListMaDonBGAsync()
        {
            var result = new GenericResponse<List<string>>();
            try
            {
                var maDons = await _repo.GetListMaDonBGAsync();
                result.Data = maDons;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Cập nhâp danh sách mã đơn báo giá
        public async Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> CapNhatDanhSachBGAsync(List<BaoGia_Request_of_QuotationDTO> danhSachMaDonBG)
        {
            var result = new GenericResponse<List<BaoGia_Request_of_QuotationDTO>>();
            try
            {
                var data = _mapper.Map<List<BaoGia_Request_of_Quotation>>(danhSachMaDonBG);
                var updated = await _repo.CapNhatDanhSachBGAsync(data);
                result.Data = _mapper.Map<List<BaoGia_Request_of_QuotationDTO>>(updated);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }

            return result;
        }
        // Cập nhật đơn báo giá
        public async Task<GenericResponse<BaoGia_Request_of_QuotationDTO>> CapNhatDonBaoGiaAsync(BaoGia_Request_of_QuotationDTO baogia)
        {
            var result = new GenericResponse<BaoGia_Request_of_QuotationDTO>();
            try
            {
                var data = _mapper.Map<BaoGia_Request_of_Quotation>(baogia);
                var updated = await _repo.CapNhatDonBaoGiaAsync(data);
                result.Data = _mapper.Map<BaoGia_Request_of_QuotationDTO>(updated);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Lấy thông tin báo giá gom nhóm
        public async Task<GenericResponse<ListRequest<dynamic>>> GetThongTinBaoGiaGomNhomAsync(string? maDon, string? section, string? maHang, string? status, string user, int pageIndex, int pageSize)
        {
            var result = new GenericResponse<ListRequest<dynamic>>();
            try
            {
                result.Data = await _repo.GetThongTinBaoGiaGomNhomAsync(maDon, section, maHang, status, user, pageIndex, pageSize);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }

            return result;
        }
        // Xuất báo giá
        public async Task<GenericResponse<List<int>>> ExportBaoGiaAsync(string? maDon)
        {
            var result = new GenericResponse<List<int>>();
            try
            {
                result.Data = await _repo.ExportBaoGiaAsync(maDon);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }

            return result;
        }
        // Tìm kiến thông tin nhập báo nhập báo giá theo mã đơn yêu cầu
        public async Task<GenericResponse<ListRequest<dynamic>>> SearchThongTinNhapBaoGiaAsync(string? maDon, string? section, string? maHang, string? user, int pageIndex, int pageSize)
        {
            var result = new GenericResponse<ListRequest<dynamic>>();
            try
            {
                result.Data = await _repo.SearchThongTinNhapBaoGiaAsync(maDon, section, maHang, user, pageIndex, pageSize);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Lấy thông tin kèm chi tiết báo giá
        public async Task<GenericResponse<ListRequest<dynamic>>> GetThongTinBaoGiaChiTietAsync(string? maDon, string? section, string? maHang, string? maNCC, string? status, string user, int pageIndex, int pageSize)
        {
            var result = new GenericResponse<ListRequest<dynamic>>();
            try
            {
                result.Data = await _repo.GetThongTinBaoGiaChiTietAsync(maDon, section, maHang, maNCC, status, user, pageIndex, pageSize);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // lấy mã đơn theo Adid
        public async Task<GenericResponse<List<string>>> GetMaDonByAdidAsync(string adid, int step)
        {
            var result = new GenericResponse<List<string>>();
            try
            {
                result.Data = await _repo.GetMaDonByAdidAsync(adid, step);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // update thông tin màn hình lịch sử báo giá
        public async Task<GenericResponse<UpdateHistoryResult>> UpdateThongTinLichSuBaoGiaAsync(List<BaoGia_Request_of_QuotationDTO> baoGias)
        {
            var result = new GenericResponse<UpdateHistoryResult>();
            try
            {
                result.Data = await _repo.UpdateThongTinLichSuBaoGiaAsync(_mapper.Map<List<BaoGia_Request_of_Quotation>>(baoGias));
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Get thông tin đơn phê duyệt lựa chọn ncc
        public async Task<GenericResponse<List<dynamic>>> GetSupplierApprovalInfoAsync(string maDon)
        {
            var result = new GenericResponse<List<dynamic>>();
            try
            {
                result.Data = await _repo.GetSupplierApprovalInfoAsync(maDon);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Xuất file phê duyệt báo giá 
        public async Task<GenericResponse<List<dynamic>>> GetExportApprovalInfoAsync(List<string> listMaDon)
        {
            var result = new GenericResponse<List<dynamic>>();
            try
            {
                result.Data = await _repo.GetExportApprovalInfoAsync(listMaDon);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message= ex.Message;
                result.Success = false;
            }

            return result;
        }
        // Phê duyệt thông tin lựa chọn nhà cung cấp
        public async Task<GenericResponse<List<BaoGia_Request_of_Quotation>>> UpdateApprovarOK(string maDon, string userNext, string userUpdate)
        {
            var result = new GenericResponse<List<BaoGia_Request_of_Quotation>>();
            try
            {
                result.Data = await _repo.UpdateApprovarOK(maDon, userNext, userUpdate);
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Phê duyệt thông tin lựa chọn nhà cung cấp
        public async Task<GenericResponse<List<BaoGia_Request_of_Quotation>>> UpdateApprovarNG(string maDon, string Reason,string userUpdate)
        {
            var result = new GenericResponse<List<BaoGia_Request_of_Quotation>>();
            try
            {
                result.Data = await _repo.UpdateApprovarNG(maDon, Reason, userUpdate);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        public async Task<GenericResponse<ListRequest<dynamic>>> SearchRequestDone(string? maDon, string? section, string? maHang, string? maNCC, string user, int pageIndex, int pageSize)
        {
            var result =  new GenericResponse<ListRequest<dynamic>>();
            try
            {
                result.Data = await _repo.SearchRequestDone(maDon,section, maHang,maNCC,user,pageIndex,pageSize);
                result.Success = true;
            }catch(Exception ex)
            {
                result.Message=ex.Message;
                result.Success = false;
            }

            return result;
        }
        // update người phê duyệt cho đơn
        public async Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> UpdateUserApprovalHistory(UpdateHistoryResult update)
        {
            var result = new GenericResponse<List<BaoGia_Request_of_QuotationDTO>> ();
            try
            {
                var Data = await _repo.UpdateUserApprovalHistory(update);
                result.Data = _mapper.Map<List<BaoGia_Request_of_QuotationDTO>>(Data);
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // update ma hang noi bo
        public async Task<GenericResponse<bool>> UpdateCodeMaterialBIVN(List<ConfirmNameDTO> list)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.UpdateCodeMaterialBIVN(list);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Phê duyệt list lựa chọn nhà cung cấp
        public async Task<GenericResponse<List<BaoGia_Request_of_Quotation>>> UpdateApprover(List<ApproverDTO> dataApprovers, string userNext, string userUpdate)
        {
            var result = new GenericResponse<List<BaoGia_Request_of_Quotation>>();
            try
            {
                result.Data = await _repo.UpdateApprover(dataApprovers, userNext, userUpdate);
                result.Success = true;
            }catch(Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Xóa đơn xin báo giá
        public async Task<GenericResponse<bool>> DeleteDonXinBaoGiaAsync(string maDon, string reason, string userUpdate)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.DeleteDonXinBaoGiaAsync(maDon,reason, userUpdate);
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Xóa từng đơn
        public async Task<GenericResponse<bool>> DeleteDonBaoGiaAsync(int id, string reason, string userUpdate)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.DeleteDonBaoGiaAsync(id,reason, userUpdate);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }

            return result;
        }
        // Trả lại đơn báo giá
        public async Task<GenericResponse<List<BaoGia_Request_of_Quotation>>> TraLaiDonBaoGiaAsync(string maDon, string userUpdate, string reason)
        {
            var result = new GenericResponse<List<BaoGia_Request_of_Quotation>>();
            try
            {
                result.Data = await _repo.TraLaiDonBaoGiaAsync(maDon, userUpdate, reason);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }

            return result;
        }
        // lấy danh sách đơn yêu cầu hàng hóa
        public async Task<GenericResponse<List<string>>> GetMaDonYeuCauHangHoaAsync()
        {
            var result = new GenericResponse<List<string>>();
            try
            {
                result.Data = await _repo.GetMaDonYeuCauHangHoaAsync();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // update phê duyệt đơn báo giá
        public Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> UpdatePheDuyetDonBaoGiaAsync(List<BaoGia_Request_of_QuotationDTO> baoGias)
        {
            var result = new GenericResponse<List<BaoGia_Request_of_QuotationDTO>>();
            try
            {
                var data = _mapper.Map<List<BaoGia_Request_of_Quotation>>(baoGias);
                var updated =  _repo.UpdatePheDuyetDonBaoGiaAsync(data);
                result.Data = _mapper.Map<List<BaoGia_Request_of_QuotationDTO>>(updated.Result);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return Task.FromResult(result);
        }
        // Check đơn return
        public async Task<GenericResponse<bool>> CheckDonReturnAsync(List<string> maDons)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.CheckDonReturnAsync(maDons);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Export history báo giá
        public async Task<GenericResponse<List<dynamic>>> ExportHistoryBaoGiaAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, string? user, string? chungLoai)
        {
            var result = new GenericResponse<List<dynamic>>();
            try
            {
                result.Data = await _repo.ExportHistoryBaoGiaAsync(MaDon, MaNcc, Section, nguoiYeuCau, MaHang, status, step, user, chungLoai);
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
