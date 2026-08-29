using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Drawing.Printing;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface ISendMailService : IBaseService<TM_MASTER_MAIL, int, TM_MASTER_MAILDTO>
    {
        // Gửi mail
        public Task<GenericResponse<bool>> SendMailAsync(string toEmail, string ccEmail, int idMail, string? url, bool? isGap, string? section, string? idRequest, string? user);
        // Mail gửi nhà cung cấp 
        public Task<GenericResponse<bool>> SendMailToSupplierAsync();
        // Gửi mail nhà cung cấp theo mã đơn 
        public Task<GenericResponse<bool>> SendMailToSupplierByRequestCodeAsync(string requestCode);
        // Gửi mail thông báo đến người yêu cầu khi có cập nhật về đơn yêu cầu
        public Task<GenericResponse<bool>> SendMailToRequesterAsync(string requestCode, string sectionCode, string sectionName, bool? isGap, int step);
        // Mail gữi xác nhận tên và mã hàng 
        public Task<GenericResponse<bool>> SendMailToConfirmItemAsync(int step, int codeMail, string? link, bool? isGap, string? sectionCode, string? sectionName ,string user);
        // Lấy thông tin mail người nhận theo bước
        public Task<GenericResponse<string>> SendMailToRequesterAsync(string sectionCode, int step);
        // gửi mail xin xác nhận lại tên hàng
        Task<GenericResponse<bool>> SendMailCofirmNaneOfVendor();
        // Tự động cập nhật trang thái đơn
        Task<GenericResponse<bool>> AutoUpdateRequestStatusAsync();
    }
}
