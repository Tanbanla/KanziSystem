using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.Models;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Net.Mail;

namespace PRJ_WAREHOUSE_BIVN.Controllers 
{
    public class RequestController : Controller
    {
        [HttpPost]
        public JsonResult _Insert_request(string Cost_Center, string Declaration, string Dealine, string Total_exchange, string Exchange_rate, string Currency, string Total, string Kind, string Type, string Status, string Place, string Loaihinhtokhai, string Group_Code, string Chophepin, string Urgent, string User_Create, List<Models.REQUEST_DETAIL> rq, string adid_dt, string adid_tt,string adid_pd, string mail_dt, string mail_tt, string mail_pd, string ten_dt, string ten_tt, string ten_pd, string ten_dy, string adid_dy, string mail_dy, string ten_xk, string adid_xk, string mail_xk, string adidnguoitao, string mailnguoitao)
        {
            var rq_detail = REQUEST_PROCESS.Insert_request( Cost_Center, Declaration, Dealine, Total_exchange, Exchange_rate, Currency, Total, Kind, Type, Status, Place, Loaihinhtokhai, Group_Code, Chophepin, Urgent, User_Create, rq, adid_dt, adid_tt, adid_pd, mail_dt, mail_tt, mail_pd, ten_dt, ten_tt, ten_pd,ten_dy,adid_dy,mail_dy,ten_xk,adid_xk,mail_xk, adidnguoitao, mailnguoitao);
          
            // gửi mail
            string body = $@"Xin chào <br />
               Đơn yêu cầu mã : {rq_detail} đã được tạo <br /><br />
               Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
               <a href='http://172.26.248.62:8057/Approval/ListData'> Link </a> <br />
               ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
               ※このメールは自動的に送付されたので、返事をしないでください。 ";
          
            string body1 = $@"Xin chào <br />
               Đơn yêu cầu mã : {rq_detail} đã được gửi đến bên xin phê duyệt <br /><br />
               Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
               <a href='http://172.26.248.62:8057/Approval/ListData'> Link </a> <br />
               ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
               ※このメールは自動的に送付されたので、返事をしないでください。 ";

            string subject = "Xác nhận phê duyệt đơn yêu cầu hàng hóa";
            if (Urgent == "True")
            {
                subject = "[Gấp] Xác nhận phê duyệt đơn yêu cầu hàng hóa";
            }
            var sendmail = REQUEST_PROCESS._sendmail(body, mailnguoitao.Replace("\n", "").Replace(" ", ""), subject);
            var sendmail1 = REQUEST_PROCESS._sendmail(body1, mail_dt.Replace("\n", "").Replace(" ", ""), subject);

            return Json("OK");
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
      
        [HttpPost]
        public JsonResult _get_confirm( string us, string Urgent, double Total, string Code_Request, string INT_STEP)
        {
            var cf = REQUEST_PROCESS.get_requestconfirm(us, Urgent, Total, Code_Request, INT_STEP);
            return Json(cf);
        }
        public JsonResult _get_request(string cost_request)
        {
            var rq = REQUEST_PROCESS._get_info_dtrq(cost_request);
            return Json(rq);
        }
      
        public JsonResult _update_request(string id_request, string regency, string step, string urgent)
        {
          
            string body = $@"Xin chào <br />
               Đơn yêu cầu mã : {id_request} đã được gửi đến bên xin phê duyệt <br /><br />
               Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
               <a href='http://172.26.248.62:8057/Approval/ListData'> Link </a> <br />
               ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
               ※このメールは自動的に送付されたので、返事をしないでください。 ";
              string subject = "Xác nhận phê duyệt đơn yêu cầu hàng hóa";
            if (urgent == "True")
            {
                subject = "[Gấp] Xác nhận phê duyệt đơn yêu cầu hàng hóa";
            }
            var buoc = int.Parse(step) + 1;
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var mail_send = _db.GET_DATA_FROM_SQL("select * from PE_REQUEST_CONFIRM where ID_REQUEST = '" + id_request + "'");
            // Định nghĩa cột email tương ứng với từng bước
            string? columnName = buoc switch
            {
                1 => "CHR_MAIL_NGUOITHAMTRA",
                2 => "CHR_MAIL_NGUOIPHEDUYET",
                3 => "CHR_MAIL_XACNHAN",
                4 => "CHR_MAIL_XUATKHO",
                5 => "CHR_MAIL_NGUOITAO",
                _ => null
            };

            if (columnName == null) return Json("Bước không hợp lệ!");

            // Lấy địa chỉ email từ DataTable
            string mailTo = mail_send.Rows[0][columnName].ToString()!;

            // Cập nhật lại nội dung Body (Sử dụng chuỗi nguyên bản @ để dễ đọc)
            if (buoc == 5)
            {
                body = $@"Xin chào <br />
                     Đơn yêu cầu mã : {id_request} của bạn ở trạng thái ĐỒNG Ý phê duyệt <br /><br />
                     Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
                     <a href='http://172.26.248.62:8057/Approval/ListData'> Link </a> <br />
                     ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                     ※このメールは自動的に送付されたので, 返事をしないでください。";
            }

            // Gửi mail 
            REQUEST_PROCESS._sendmail(body, mailTo, subject);
            var up = REQUEST_PROCESS._update_request(id_request, regency, buoc.ToString());
            return Json(up);
        }
        public JsonResult _update_dongytatca(string us, string madon)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            var kq = REQUEST_PROCESS._update_all(us);
            
            string body = $@"Xin chào <br />
                    Đơn yêu cầu mã : {madon.Split("_")[0]} của bạn ở trạng thái ĐỒNG Ý phê duyệt <br /><br />
                    Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
                    <a href='http://172.26.248.62:8057/Approval/ListData'> Link </a> <br />
                    ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                    ※このメールは自動的に送付されたので, 返事をしないでください。";

            var get_if = db.GET_DATA_FROM_SQL("select INT_STEP, CHR_MAIL_NGUOITAO, Urgent from [PE_REQUEST_CONFIRM] as a left join REQUEST as b on a.ID_REQUEST = b.Id_Request where b.Code_Request = '" + madon.Split("_")[0] + "' ");
            var mailTo = get_if.Rows[0][1].ToString();
            string subject = "Xác nhận phê duyệt đơn yêu cầu hàng hóa";

            if (get_if.Rows[0][2].ToString() == "True")
            {
                subject = "[Gấp] Xác nhận phê duyệt đơn yêu cầu hàng hóa";
            }
            // Gửi mail 
            REQUEST_PROCESS._sendmail(body, mailTo!, subject);
            
             
            return Json(kq);
        }
        public JsonResult _reject(string id_request, string reason, string regency, string step, string urgent)
        {
            string subject = "Từ chối phê duyệt đơn yêu cầu hàng hóa ";
            if(urgent == "True")
            {
                subject = "[GẤP] Từ chối phê duyệt đơn yêu cầu hàng hóa";
            }
            string body = @"Xin chào <br />
              Đơn yêu cầu mã : " + id_request + @" của bạn ở trạng thái TỪ CHỐI phê duyệt <br /><br />
              Lý do : " + reason + @"<br /><br />
              <a href='http://172.26.248.62:8057/Approval/Condition'> Link </a> <br />
              ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
              ※このメールは自動的に送付されたので、返事をしないでください。";
            var stepMap = new Dictionary<string, string>
            {
                { "0", "6" }, { "1", "7" }, { "2", "8" }, { "3", "9" },{ "4", "10" }
            };
            if (stepMap.TryGetValue(step, out string? status))
            {
                var up = REQUEST_PROCESS._update_request(id_request, regency, status);
            }
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var mail_send = _db.GET_DATA_FROM_SQL("select * from PE_REQUEST_CONFIRM where ID_REQUEST = '" + id_request + "'");
            string mail_nguoidat = mail_send.Rows[0]["CHR_MAIL_NGUOITAO"].ToString()!;
            REQUEST_PROCESS._sendmail(body, mail_nguoidat, subject);
            return Json("Hoàn thành !");
        }
        public JsonResult get_requestcondition(string Group_Code, string Code_Request, string INT_STEP, string Cost_Center, string Request_Date, string Total, string Urgent)
        {
            //Urgent = (Urgent == "Gấp") ? "1" : (Urgent == "Thông thường" ? "0" : Urgent);
            //var a =  (Total == "Dưới 3000") ? "2999"                  
            //       : (Total == "Dưới 10,000") ? "3000"
            //       : (Total == "Trên 10,000") ? "10000"
            //       : "0";
            double aa = double.Parse(Total);
            var cf = REQUEST_PROCESS.get_requestcondition(Group_Code, Code_Request, INT_STEP, Cost_Center, Request_Date, aa, Urgent);
            return Json(cf);
        }
    }
}
