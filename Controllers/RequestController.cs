using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class RequestController : Controller
    {
        [HttpPost]
        public JsonResult _Insert_request(string Cost_Center, string Declaration, string Dealine, string Total_exchange, string Exchange_rate, string Currency, string Total, string Kind, string Type, string Status, string Place, string Loaihinhtokhai, string Group_Code, string Chophepin, string Urgent, string User_Create, List<REQUEST_DETAIL> rq, string adid_dt, string adid_tt,string adid_pd, string mail_dt, string mail_tt, string mail_pd, string ten_dt, string ten_tt, string ten_pd)
        {
            var rq_detail = REQUEST_PROCESS.Insert_request( Cost_Center, Declaration, Dealine, Total_exchange, Exchange_rate, Currency, Total, Kind, Type, Status, Place, Loaihinhtokhai, Group_Code, Chophepin, Urgent, User_Create, rq, adid_dt, adid_tt, adid_pd, mail_dt, mail_tt, mail_pd, ten_dt, ten_tt, ten_pd);             
            return Json(rq_detail);
        }
        public JsonResult _get_rate()
        {
            var rate = REQUEST_PROCESS.get_rate();
            return Json(rate);
        }
        public JsonResult _get_phongchiuphi()
        {
            List<string> pcp = REQUEST_PROCESS.phongchiuphi();
            return Json(pcp);
        }
        public JsonResult _send_mail(string adid,string Urgent)
        {
                    
            if(Urgent == "1")
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("BIVNWarehouse.sys@brother-bivn.com.vn");
                mail.Subject = "[Gấp] Xin phê duyệt đơn yêu cầu hàng hóa";
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.brother.co.jp";
                //smtp.EnableSsl = true;
                NetworkCredential networkCredential = new NetworkCredential();
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = networkCredential;
                smtp.Port = 25;
                mail.Body = "Xin chào <br />" +
                    "Đơn yêu cầu đã được gửi đến bên xin phê duyệt <br /><br />" +
                    "Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />" +
                    "<a href=''> Link </a> <br />" +
                    "※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />" +
                    "※このメールは自動的に送付されたので、返事をしないでください。 ";

                mail.To.Add(adid);
                smtp.Send(mail);
                return Json("Gửi mail thành công !");
            }
            else
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("BIVNWarehouse.sys@brother-bivn.com.vn");
                mail.Subject = "Xin phê duyệt đơn yêu cầu hàng hóa";
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.brother.co.jp";
                //smtp.EnableSsl = true;
                NetworkCredential networkCredential = new NetworkCredential();
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = networkCredential;
                smtp.Port = 25;
                mail.Body = "Xin chào <br />" +
                    "Đơn yêu cầu đã được gửi đến bên xin phê duyệt <br /><br />" +
                    "Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />" +
                    "<a href=''> Link </a> <br />" +
                    "※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />" +
                    "※このメールは自動的に送付されたので、返事をしないでください。 ";

                mail.To.Add(adid);
                smtp.Send(mail);
                return Json("Gửi mail thành công !");

            }             
        }
        [HttpPost]
        public JsonResult _get_confirm( string us)
        {
            var cf = REQUEST_PROCESS.get_requestconfirm(us);
            return Json(cf);
        }
        public JsonResult _get_request(string cost_request)
        {
            var rq = REQUEST_PROCESS._get_info_dtrq(cost_request);
            return Json(rq);
        }
        public JsonResult _update_request(string id_request, string regency, string step)
        {
            var buoc = int.Parse(step) + 1;

            var up = REQUEST_PROCESS._update_request(id_request, regency, step);
            return Json(up);
        }

    }
}
