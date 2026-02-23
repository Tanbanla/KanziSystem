using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaConfirmNameService: IBaseService<BaoGia_Confirm_Name_Quotation, int , BaoGia_Confirm_Name_QuotationDTO>
    {
        // search thông tin xác nhận tên hàng
        public Task<GenericResponse<List<BaoGia_Confirm_Name_QuotationDTO>>> SearchAsync(string? TenHang, string? SoDon, string? TrangThai, string? section,int pageIndex, int pageSize);
        // Luu thong tin
        public Task<GenericResponse<bool>> SaveConfirmNameAsync(int? Id, string? TenHaiQuan, string? MaHangNoiBo, string? Role, string User);
        // Them thong tin 
        public Task<GenericResponse<bool>> AddConfirmNameAsync(BaoGia_Confirm_Name_QuotationDTO confirmName);
        // Approve ConfirmName
        public Task<GenericResponse<bool>> ApproveConfirmNameAsync(int id, string approvedBy);
        // Reject ConfirmName
        public Task<GenericResponse<bool>> RejectConfirmNameAsync(int id, string reason, string rejectedBy);
        // Insert thong tin danh sach
        public Task<GenericResponse<bool>> AddListAsync(List<BaoGia_Confirm_Name_QuotationDTO> confirmNames);
    }
}
        // public static bool sendEmailNotify(string title, string mail_from, string mail_to, string mail_cc, string mail_bcc, string body, int priority)
        // {
        //     bool blresult = true;

        //     try
        //     {
        //         MailMessage msg = new MailMessage();
        //         msg.From = new MailAddress(mail_from);
        //         string[] arrEmail = mail_to.Split(Convert.ToChar(";"));
        //         if (arrEmail?.Length > 0)
        //         {
        //             foreach (string item_to in arrEmail)
        //             {
        //                 if (!string.IsNullOrEmpty(item_to?.Trim()))
        //                     msg.To.Add(new MailAddress(item_to));
        //             }

        //         }

        //         if (mail_cc?.Length > 0)
        //         {
        //             arrEmail = mail_cc.Split(Convert.ToChar(";"));
        //             if (arrEmail.Length > 0)
        //             {
        //                 foreach (string item_cc in arrEmail)
        //                 {
        //                     if (!string.IsNullOrEmpty(item_cc?.Trim()))
        //                         msg.CC.Add(new MailAddress(item_cc));
        //                 }
        //             }

        //         }

        //         if (mail_bcc?.Length > 0)
        //         {
        //             arrEmail = mail_bcc.Split(Convert.ToChar(";"));
        //         }
        //         if (arrEmail?.Length > 0)
        //         {
        //             foreach (string item_bcc in arrEmail)
        //             {
        //                 if (!string.IsNullOrEmpty(item_bcc?.Trim()))
        //                     msg.Bcc.Add(new MailAddress(item_bcc));
        //             }
        //         }

        //         if (priority == 1)
        //         { msg.Priority = MailPriority.High; }

        //         msg.Subject = title;
        //         msg.Body = body;
        //         //String.Format(body, 1, 2);
        //         msg.IsBodyHtml = true;

        //         SmtpClient emailClient = new SmtpClient("smtp.brother.co.jp", 25);
        //         emailClient.UseDefaultCredentials = true;
        //         emailClient.Send(msg);
        //     }
        //     catch (Exception ex)
        //     {
        //         blresult = false;
        //     }

        //     return blresult;
        // }