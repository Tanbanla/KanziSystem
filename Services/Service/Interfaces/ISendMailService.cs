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
    }
}
