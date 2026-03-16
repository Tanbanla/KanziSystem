using AutoMapper;
using AutoMapper.Configuration.Annotations;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

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
        public async Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> SearchAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, int pageIndex, int pageSize, DateTime? date, string? chungLoai)
        {
            var result = new GenericResponse<List<BaoGia_Request_of_QuotationDTO>>();
            try
            {
                var data = await _repo.SearchAsync(MaDon, MaNcc, Section, nguoiYeuCau, MaHang, status, step, pageIndex, pageSize, date, chungLoai);
                result.Data = _mapper.Map<List<BaoGia_Request_of_QuotationDTO>>(data);
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
        public async Task<GenericResponse<List<dynamic>>> GetThongTinBaoGiaGomNhomAsync(string? maDon, string? section, string? maHang, int pageIndex, int pageSize)
        {
            var result = new GenericResponse<List<dynamic>>();
            try
            {
                result.Data = await _repo.GetThongTinBaoGiaGomNhomAsync(maDon, section, maHang, pageIndex, pageSize);
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
        public async Task<GenericResponse<ListRequest<dynamic>>> SearchThongTinNhapBaoGiaAsync(string? maDon, string? section, string? maHang, int pageIndex, int pageSize)
        {
            var result = new GenericResponse<ListRequest<dynamic>>();
            try
            {
                result.Data = await _repo.SearchThongTinNhapBaoGiaAsync(maDon, section, maHang, pageIndex, pageSize);
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
        public async Task<GenericResponse<ListRequest<dynamic>>> GetThongTinBaoGiaChiTietAsync(string? maDon, string? section, string? maHang, string? maNCC, string? status, int pageIndex, int pageSize)
        {
            var result = new GenericResponse<ListRequest<dynamic>>();
            try
            {
                result.Data = await _repo.GetThongTinBaoGiaChiTietAsync(maDon, section, maHang, maNCC, status, pageIndex, pageSize);
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
