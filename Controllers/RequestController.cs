using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.Models;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Drawing;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace PRJ_WAREHOUSE_BIVN.Controllers 
{
    public class RequestController : Controller
    {
        [HttpPost]
        public JsonResult _Insert_request(string Cost_Center, string Declaration, string Dealine, float Total_exchange, string Exchange_rate, string Currency, float Total, string Kind, string Type, string Status, string Place, string Loaihinhtokhai, string Group_Code, string Chophepin, string Urgent, string User_Create, List<Models.REQUEST_DETAIL> rq, string adid_dt, string adid_tt,string adid_pd, string mail_dt, string mail_tt, string mail_pd, string ten_dt, string ten_tt, string ten_pd, string ten_dy, string adid_dy, string mail_dy, string ten_xk, string adid_xk, string mail_xk, string adidnguoitao, string mailnguoitao)
        {

            SQL_Connect_DB20 _db = new SQL_Connect_DB20();

            string _macoderq = _db.ReturnString($@"
                                DECLARE @Prefix NVARCHAR(10) = '{Cost_Center}.';
                                DECLARE @Today NVARCHAR(8) = FORMAT(GETDATE(), 'yyyyMMdd');
                                DECLARE @SearchPattern NVARCHAR(20) = @Prefix + @Today + '-%';
                                DECLARE @MaxNum INT = 0;
                                DECLARE @NewCode NVARCHAR(30);

                                SELECT @MaxNum = MAX(CAST(REPLACE([Code_Request], @Prefix + @Today + '-', '') AS INT))
                                FROM [dbo].[REQUEST] WHERE [Code_Request] LIKE @SearchPattern;

                                SET @NewCode = @Prefix + @Today + '-' + CAST(ISNULL(@MaxNum, 0) + 1 AS NVARCHAR(10));

                                WHILE EXISTS (SELECT 1 FROM [dbo].[REQUEST] WHERE [Code_Request] = @NewCode)
                                BEGIN 
                                    SET @MaxNum = @MaxNum + 1;
                                    SET @NewCode = @Prefix + @Today + '-' + CAST(@MaxNum + 1 AS NVARCHAR(10));
                                END

                                SELECT @NewCode;");
            // Kiểm tra chi phí dự toán phòng ban Cost
            bool Checksotien = false;
            double Sotienphaichiuchiphi = 0;
            var data = Common.CostManage.Tinhdutoanconlai(Cost_Center, Dealine, Declaration, _macoderq);
            for (int i = 0; i < rq.Count; i++)
            {
                if (rq[i].Amount == 0) continue;
                string phongchiuchipheCheck = rq[i].Phongchiuchiphi!;
                phongchiuchipheCheck = phongchiuchipheCheck.Length > 0 ? phongchiuchipheCheck : Cost_Center;

                Checksotien = Checksotien | phongchiuchipheCheck.Contains(Cost_Center);

                Sotienphaichiuchiphi += phongchiuchipheCheck.Contains(Cost_Center) ? USD(Convert.ToDouble(rq[i].Total_exchange), Convert.ToDouble(rq[i].Total_exchange)) : 0;
            }
            if(Checksotien)
            {
                if (Convert.ToDouble(Common.CostManage.Ketqua_Chiphi["Tong"].ToString()) < Sotienphaichiuchiphi)
                {
                    if (_db.GET_DATA_FROM_SQL("SELECT * FROM [TM_LOAIHINHTOKHIA] WHERE [Value] = '" + Declaration + "' AND [CoverChiPhi] = 'False' ").Rows.Count > 0)
                    {
                       return Json("Số tiền của yêu cầu lớn hơn số tiền còn lại. \nTuy nhiên hệ thống vẫn cho phép bạn đặt hàng \nVui lòng xác nhận lại với quản lý chi phí nếu cần thiết");
                    }
                    else
                    {
                        return Json("Số tiền của yêu cầu: " + Sotienphaichiuchiphi + " lớn hơn số tiền còn lại: " + Convert.ToDouble(Common.CostManage.Ketqua_Chiphi["Tong"].ToString()) + ", yêu cầu xem xét lại");                       
                    }
                }
            }
            if (string.IsNullOrEmpty(mailnguoitao))
            {
                mailnguoitao = adidnguoitao + "@brothergroup.net";
            }
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
        public double USD(double NT, double dongia)
        {
            double USD = 0;
            USD = NT / dongia;
            return USD;
        }
        [HttpPost]
        public JsonResult _Insert_request_GA(string Cost_Center, string Declaration, string Dealine, float Total_exchange, string Exchange_rate, string Currency, float Total, string Kind, string Type, string Status, string Place, string Loaihinhtokhai, string Group_Code, string Chophepin, string Urgent, string User_Create, List<Models.REQUEST_DETAIL>? rq, string adid_dt, string adid_tt, string adid_pd, string mail_dt, string mail_tt, string mail_pd, string ten_dt, string ten_tt, string ten_pd, string ten_qlsc, string adid_qlsc, string mail_qlsc, string ten_xk, string adid_xk, string mail_xk, string adidnguoitao, string mailnguoitao, string ten_qltc, string adid_qltc, string mail_qltc)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();

            string _macoderq = _db.ReturnString($@"
                                DECLARE @Prefix NVARCHAR(10) = '{Cost_Center}.';
                                DECLARE @Today NVARCHAR(8) = FORMAT(GETDATE(), 'yyyyMMdd');
                                DECLARE @SearchPattern NVARCHAR(20) = @Prefix + @Today + '-%';
                                DECLARE @MaxNum INT = 0;
                                DECLARE @NewCode NVARCHAR(30);

                                SELECT @MaxNum = MAX(CAST(REPLACE([Code_Request], @Prefix + @Today + '-', '') AS INT))
                                FROM [dbo].[REQUEST] WHERE [Code_Request] LIKE @SearchPattern;

                                SET @NewCode = @Prefix + @Today + '-' + CAST(ISNULL(@MaxNum, 0) + 1 AS NVARCHAR(10));

                                WHILE EXISTS (SELECT 1 FROM [dbo].[REQUEST] WHERE [Code_Request] = @NewCode)
                                BEGIN 
                                    SET @MaxNum = @MaxNum + 1;
                                    SET @NewCode = @Prefix + @Today + '-' + CAST(@MaxNum + 1 AS NVARCHAR(10));
                                END

                                SELECT @NewCode;");
            // Kiểm tra chi phí dự toán phòng ban Cost
            bool Checksotien = false;
            double Sotienphaichiuchiphi = 0;
            var data = Common.CostManage.Tinhdutoanconlai(Cost_Center, Dealine, Declaration, _macoderq);
            for (int i = 0; i < rq.Count; i++)
            {
                if (rq[i].Amount == 0) continue;
                string phongchiuchipheCheck = rq[i].Phongchiuchiphi!;
                phongchiuchipheCheck = phongchiuchipheCheck.Length > 0 ? phongchiuchipheCheck : Cost_Center;

                Checksotien = Checksotien | phongchiuchipheCheck.Contains(Cost_Center);

                Sotienphaichiuchiphi += phongchiuchipheCheck.Contains(Cost_Center) ? USD(Convert.ToDouble(rq[i].Total_exchange), Convert.ToDouble(rq[i].Total_exchange)) : 0;
            }
            if (Checksotien)
            {
                if (Convert.ToDouble(Common.CostManage.Ketqua_Chiphi["Tong"].ToString()) < Sotienphaichiuchiphi)
                {
                    if (_db.GET_DATA_FROM_SQL("SELECT * FROM [TM_LOAIHINHTOKHIA] WHERE [Value] = '" + Declaration + "' AND [CoverChiPhi] = 'False' ").Rows.Count > 0)
                    {
                        return Json("Số tiền của yêu cầu lớn hơn số tiền còn lại. \nTuy nhiên hệ thống vẫn cho phép bạn đặt hàng \nVui lòng xác nhận lại với quản lý chi phí nếu cần thiết");
                    }
                    else
                    {
                        return Json("Số tiền của yêu cầu: " + Sotienphaichiuchiphi + " lớn hơn số tiền còn lại: " + Convert.ToDouble(Common.CostManage.Ketqua_Chiphi["Tong"].ToString()) + ", yêu cầu xem xét lại");
                    }
                }
            }
            string subject = "Xác nhận phê duyệt đơn yêu cầu hàng hóa";
            if (Urgent == "True")
            {
                subject = "[Gấp] Xác nhận phê duyệt đơn yêu cầu hàng hóa";
            }
            if (string.IsNullOrEmpty(mailnguoitao))
            {
                mailnguoitao = adidnguoitao + "@brothergroup.net";
            }
            var rq_detail = REQUEST_PROCESS_GA.Insert_request_GA(Cost_Center, Declaration, Dealine, Total_exchange, Exchange_rate, Currency, Total, Kind, Type, Status, Place, Loaihinhtokhai, Group_Code, Chophepin, Urgent, User_Create, rq, adid_dt, adid_tt, adid_pd, mail_dt, mail_tt, mail_pd, ten_dt, ten_tt, ten_pd, ten_qlsc, adid_qlsc, mail_qlsc, ten_xk, adid_xk, mail_xk, adidnguoitao, mailnguoitao, ten_qltc, adid_qltc, mail_qltc);

            // gửi mail
            string body = $@"Xin chào <br />
               Đơn yêu cầu mã : {rq_detail} đã được tạo <br /><br />
               Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
               <a href='http://172.26.248.62:8057/Approval/ListData_GA'> Link </a> <br />
               ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
               ※このメールは自動的に送付されたので、返事をしないでください。 ";

            string body1 = $@"Xin chào <br />
               Đơn yêu cầu mã : {rq_detail} đã được gửi đến bạn phê duyệt <br /><br />
               Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
               <a href='http://172.26.248.62:8057/Approval/ListData_GA'> Link </a> <br />
               ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
               ※このメールは自動的に送付されたので、返事をしないでください。 ";

         
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
        public JsonResult _get_vitri(string cost)
        {
            List<string> vitri = REQUEST_PROCESS.vitri(cost);
            return Json(vitri);
        }
        [HttpPost]
        public JsonResult _get_confirm( string us, string Urgent, double Total, string Code_Request, string INT_STEP)
        {
            var cf = REQUEST_PROCESS.get_requestconfirm(us, Urgent, Total, Code_Request, INT_STEP);
            return Json(cf);
        }
        [HttpPost]
        public JsonResult _get_confirm_GA(string us, string Urgent, double Total, string Code_Request, string INT_STEP)
        {
            var cf = REQUEST_PROCESS_GA.get_requestconfirm(us, Urgent, Total, Code_Request, INT_STEP);
            return Json(cf);
        }
        public JsonResult _get_request(string cost_request)
        {
            var rq = REQUEST_PROCESS._get_info_dtrq(cost_request);
            return Json(rq);
        }
        public JsonResult _get_request_GA(string cost_request)
        {
            var rq = REQUEST_PROCESS_GA._get_info_dtrq(cost_request);
            return Json(rq);
        }

        public JsonResult _update_request(string id_request, string regency, string step, string urgent)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var get_ma = _db.ReturnString("select Code_Request from REQUEST where Id_Request = '" + id_request + "'");

            string body = $@"Xin chào <br />
               Đơn yêu cầu mã : {get_ma} đã được gửi đến bên xin phê duyệt <br /><br />
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
                     Đơn yêu cầu mã : {get_ma} của bạn ở trạng thái ĐỒNG Ý phê duyệt <br /><br />
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
        public JsonResult _update_request_GA(string id_request, string regency, string step, string urgent)
        {
            if(regency == "XACNHAN")
            {
                regency = "XUATKHO";
            }
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var get_ma = _db.ReturnString("select Code_Request from REQUEST where Id_Request = '" + id_request + "'");
            string body = $@"Xin chào <br />
               Đơn yêu cầu mã : {get_ma} đã được gửi đến bên xin phê duyệt <br /><br />
               Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
               <a href='http://172.26.248.62:8057/Approval/ListData_GA'> Link </a> <br />
               ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
               ※このメールは自動的に送付されたので、返事をしないでください。 ";
            string subject = "Xác nhận phê duyệt đơn yêu cầu hàng hóa";
            if (urgent == "True")
            {
                subject = "[Gấp] Xác nhận phê duyệt đơn yêu cầu hàng hóa";
            }
            var buoc = int.Parse(step) + 1;
          
            var mail_send = _db.GET_DATA_FROM_SQL("select * from PE_REQUEST_CONFIRM_GA where ID_REQUEST = '" + id_request + "'");
            // Định nghĩa cột email tương ứng với từng bước
            string? columnName = buoc switch
            {
                1 => "CHR_MAIL_NGUOITHAMTRA",
                2 => "CHR_MAIL_NGUOIPHEDUYET",
                3 => "CHR_MAIL_XUATKHO",
                4 => "CHR_MAIL_QLSC",
                5 => "CHR_MAIL_QLTC",
                _ => null
            };

            if (columnName == null) return Json("Bước không hợp lệ!");

            // Lấy địa chỉ email từ DataTable
            string mailTo = mail_send.Rows[0][columnName].ToString()!;

            // Cập nhật lại nội dung Body (Sử dụng chuỗi nguyên bản @ để dễ đọc)
            if (buoc == 5)
            {
                body = $@"Xin chào <br />
                     Đơn yêu cầu mã : {get_ma} của bạn ở trạng thái ĐỒNG Ý phê duyệt <br /><br />
                     Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
                     <a href='http://172.26.248.62:8057/Approval/ListData_GA'> Link </a> <br />
                     ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                     ※このメールは自動的に送付されたので, 返事をしないでください。";
            }
            else
            {
                var up = REQUEST_PROCESS_GA._update_request(id_request, regency, buoc.ToString());
                return Json(up);
            }
            // Gửi mail 
            REQUEST_PROCESS_GA._sendmail(body, mailTo, subject);

            return Json("OK");
        }
        public JsonResult _update_dongytatca(string us, string madon)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            var kq = REQUEST_PROCESS._update_all(us, madon);
            
            string body = $@"Xin chào <br />
                    Đơn yêu cầu mã : {madon.Split("_")[0]} của bạn ở trạng thái ĐỒNG Ý phê duyệt <br /><br />
                    Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
                    <a href='http://172.26.248.62:8057/Approval/ListData'> Link </a> <br />
                    ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                    ※このメールは自動的に送付されたので, 返事をしないでください。";

            var get_if = db.GET_DATA_FROM_SQL("select INT_STEP, CHR_MAIL_NGUOITAO, Urgent from [PE_REQUEST_CONFIRM] as a left join REQUEST as b on a.ID_REQUEST = b.Id_Request where b.Code_Request = '" + madon.Split("_")[0] + "' ");
         
            string subject = "Xác nhận phê duyệt đơn yêu cầu hàng hóa";

            if (get_if.Rows[0][2].ToString() == "True")
            {
                subject = "[Gấp] Xác nhận phê duyệt đơn yêu cầu hàng hóa";
            }
            // Gửi mail

            int buoc = int.Parse(get_if.Rows[0][0].ToString()!);
            var mail_send = db.GET_DATA_FROM_SQL("select * from PE_REQUEST_CONFIRM where ID_REQUEST = '" + madon.Split("_")[0] + "'");
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
                     Đơn yêu cầu mã : {madon.Split("_")[0]} của bạn ở trạng thái ĐỒNG Ý phê duyệt <br /><br />
                     Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
                     <a href='http://172.26.248.62:8057/Approval/ListData'> Link </a> <br />
                     ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                     ※このメールは自動的に送付されたので, 返事をしないでください。";
            }

            var up = REQUEST_PROCESS._update_request(madon.Split("_")[0], columnName, buoc.ToString());
            REQUEST_PROCESS._sendmail(body, mailTo!, subject);            
             
            return Json(kq);
        }
        public JsonResult _update_dongytatca_GA(string us, string madon)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            var kq = REQUEST_PROCESS_GA._update_all(us, madon);

            string body = $@"Xin chào <br />
                    Đơn yêu cầu mã : {madon.Split("_")[0]} của bạn ở trạng thái ĐỒNG Ý phê duyệt <br /><br />
                    Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
                    <a href='http://172.26.248.62:8057/Approval/ListData_GA'> Link </a> <br />
                    ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                    ※このメールは自動的に送付されたので, 返事をしないでください。";

            var get_if = db.GET_DATA_FROM_SQL("select INT_STEP, CHR_MAIL_NGUOITAO, Urgent from [PE_REQUEST_CONFIRM_GA] as a left join REQUEST as b on a.ID_REQUEST = b.Id_Request where b.Code_Request = '" + madon.Split("_")[0] + "' ");

            string subject = "Xác nhận phê duyệt đơn yêu cầu hàng hóa";

            if (get_if.Rows[0][2].ToString() == "True")
            {
                subject = "[Gấp] Xác nhận phê duyệt đơn yêu cầu hàng hóa";
            }
            // Gửi mail

            int buoc = int.Parse(get_if.Rows[0][0].ToString()!);
            var mail_send = db.GET_DATA_FROM_SQL("select * from PE_REQUEST_CONFIRM_GA where ID_REQUEST = '" + madon.Split("_")[0] + "'");
            // Định nghĩa cột email tương ứng với từng bước
            string? columnName = buoc switch
            {
                1 => "CHR_MAIL_NGUOITHAMTRA",
                2 => "CHR_MAIL_NGUOIPHEDUYET",
                3 => "CHR_MAIL_XUATKHO",
                4 => "CHR_MAIL_QLSC",
                5 => "CHR_MAIL_QLTC",
                _ => null
            };

            if (columnName == null) return Json("Bước không hợp lệ!");

            // Lấy địa chỉ email
            string mailTo = mail_send.Rows[0][columnName].ToString()!;

            // Cập nhật lại nội dung Body
            if (buoc == 5)
            {
                body = $@"Xin chào <br />
                     Đơn yêu cầu mã : {madon.Split("_")[0]} của bạn ở trạng thái ĐỒNG Ý phê duyệt <br /><br />
                     Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
                     <a href='http://172.26.248.62:8057/Approval/ListData_GA'> Link </a> <br />
                     ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                     ※このメールは自動的に送付されたので, 返事をしないでください。";
            }
            // update đơn
            var up = REQUEST_PROCESS_GA._update_request(madon.Split("_")[0], columnName, buoc.ToString());
            // gửi mail
            REQUEST_PROCESS_GA._sendmail(body, mailTo!, subject);

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
            var mail_send = _db.ReturnString("select CHR_MAIL_NGUOITAO from PE_REQUEST_CONFIRM where ID_REQUEST = '" + id_request + "'");
            // Gửi về người tạo đơn
            string mail_nguoidat = mail_send.Trim();
            REQUEST_PROCESS._sendmail(body, mail_nguoidat, subject);
            return Json("Hoàn thành !");
        }
        public JsonResult _reject_GA(string id_request, string reason, string regency, string step, string urgent)
        {
            string subject = "Từ chối phê duyệt đơn yêu cầu hàng hóa ";
            if (urgent == "True")
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
                var up = REQUEST_PROCESS_GA._update_request(id_request, regency, status);
            }
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var mail_send = _db.ReturnString("select CHR_MAIL_NGUOITAO from PE_REQUEST_CONFIRM_GA where ID_REQUEST = '" + id_request + "'");

            string mail_nguoidat = mail_send.Trim();
            REQUEST_PROCESS_GA._sendmail(body, mail_nguoidat, subject);
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
        public JsonResult get_requestcondition_GA(string Group_Code, string Code_Request, string INT_STEP, string Cost_Center, string Request_Date, string Total, string Urgent)
        {
            //Urgent = (Urgent == "Gấp") ? "1" : (Urgent == "Thông thường" ? "0" : Urgent);
            //var a =  (Total == "Dưới 3000") ? "2999"                  
            //       : (Total == "Dưới 10,000") ? "3000"
            //       : (Total == "Trên 10,000") ? "10000"
            //       : "0";
            double aa = double.Parse(Total);
            var cf = REQUEST_PROCESS_GA.get_requestcondition(Group_Code, Code_Request, INT_STEP, Cost_Center, Request_Date, aa, Urgent);
            return Json(cf);
        }
        public JsonResult _layphongban(string ph)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            var phongban = db.ReturnString("select [CHR_Section_Code] from [DEPARTMENT] where [Cost_Center] = '" + ph.Split(':')[0] + "' ");
            //string originSection = ph.Split(':')[1];
            //if(ph.Split(':')[1].Contains(" "))
            //{
            //    originSection = ph.Split(":")[1].Split(' ').Last() ;
            //}    
            //string ghep = phongban + " : " + originSection;
            return Json(phongban);
        }

        [HttpPost]
        public JsonResult _sua_request(string Cost_Center,string Declaration,string Code_Request, string iD_REQUEST, string Dealine, float Total_exchange, string Exchange_rate, float Total, string Place, string Urgent, string User_Create, List<Models.REQUEST_DETAIL> rq)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
           
                var list = db.ReturnString("select count(*) from [PE_REQUEST_CONFIRM] where INT_STEP = '0' and  [ID_REQUEST] = '" + iD_REQUEST + "' ");
                if(list == "0")
                {
                    list = db.ReturnString("select count(*) from [PE_REQUEST_CONFIRM_PE] where INT_STEP = '0' and  [ID_REQUEST] = '" + iD_REQUEST + "' ");
                }
                if (list != "0")
                {
                    SQL_Connect_DB20 _db = new SQL_Connect_DB20();
     
                    // Kiểm tra chi phí dự toán phòng ban Cost
                    bool Checksotien = false;
                    double Sotienphaichiuchiphi = 0;
                    var data = Common.CostManage.Tinhdutoanconlai(Cost_Center, Dealine, Declaration, Code_Request);
                    for (int i = 0; i < rq.Count; i++)
                    {
                        if (rq[i].Amount == 0) continue;
                        string phongchiuchipheCheck = rq[i].Phongchiuchiphi!;
                        phongchiuchipheCheck = phongchiuchipheCheck.Length > 0 ? phongchiuchipheCheck : Cost_Center;

                    Checksotien = Checksotien | phongchiuchipheCheck.Contains(Cost_Center);

                        Sotienphaichiuchiphi += phongchiuchipheCheck.Contains(Cost_Center) ? USD(Convert.ToDouble(rq[i].Total_exchange), Convert.ToDouble(rq[i].Total_exchange)) : 0;
                    }
                    if (Checksotien)
                    {
                        if (Convert.ToDouble(Common.CostManage.Ketqua_Chiphi["Tong"].ToString()) < Sotienphaichiuchiphi)
                        {
                            if (_db.GET_DATA_FROM_SQL("SELECT * FROM [TM_LOAIHINHTOKHIA] WHERE [Value] = '" + Declaration + "' AND [CoverChiPhi] = 'False' ").Rows.Count > 0)
                            {
                                return Json("Số tiền của yêu cầu lớn hơn số tiền còn lại. \nTuy nhiên hệ thống vẫn cho phép bạn đặt hàng \nVui lòng xác nhận lại với quản lý chi phí nếu cần thiết");
                            }
                            else
                            {
                                return Json("Số tiền của yêu cầu: " + Sotienphaichiuchiphi + " lớn hơn số tiền còn lại: " + Convert.ToDouble(Common.CostManage.Ketqua_Chiphi["Tong"].ToString()) + ", yêu cầu xem xét lại");
                            }
                        }
                    }

                    for (int i = 0; i < rq.Count; i++)
                    {
                        var update = db.GET_DATA_FROM_SQL($@"IF EXISTS (SELECT 1 FROM REQUEST_DETAIL
                        WHERE Code_Request = '{Code_Request}' AND Material_Code = '{rq[i].Material_Code}')
                        BEGIN
                            -- Thực hiện Update
                            UPDATE REQUEST_DETAIL
                            SET 
                                Material_Name = N'{rq[i].Material_Name}',
                                Account_Name = N'{rq[i].Account_Name}',
                                Account_Code = '{rq[i].Account_Code}',
                                Unit = N'{rq[i].Unit}',
                                Amount = '{rq[i].Amount}',
                                Price = '{rq[i].Price}',
                                Total = '{rq[i].Total_exchange}',
                                Vitri = '{rq[i].Vitri}',
                                Poisition = N'{rq[i].Poisition}',
                                Currency = N'{rq[i].Currency!.Trim()}',
                                Aim = N'{rq[i].Aim}',
                                Total_exchange = '{rq[i].Total_exchange}'
                            
                            WHERE Code_Request = '{Code_Request}' AND Material_Code = '{rq[i].Material_Code}'
                        END
                        ELSE
                        BEGIN
                            -- Thực hiện Insert
                            INSERT INTO REQUEST_DETAIL (Code_Request,Material_Name, Material_Code, Account_Name,Account_Code,Unit, Amount, Price, Total, Vitri, Poisition,Currency, Aim, Id_Request,Total_exchange,Rate,Material_Name_EN,Material_Name_ENJP)
                            VALUES ('{Code_Request}',N'{rq[i].Material_Name}','{rq[i].Material_Code}','{rq[i].Account_Name}','{rq[i].Account_Code}',N'{rq[i].Unit}','{rq[i].Amount}','{rq[i].Price}','{rq[i].Total_exchange}','{rq[i].Vitri}',N'{rq[i].Poisition}',N'{rq[i].Currency!.Trim()}',N'{rq[i].Aim}','{iD_REQUEST}','{rq[i].Total_exchange}','1','','')
                        END");
                    }
                    db.GET_DATA_FROM_SQL($"Update [REQUEST] set [Total_exchange] = '{Total_exchange}', Exchange_rate = '{Exchange_rate}', [Dealine] = '{Dealine}', Total = '{Total}',Place = N'{Place}',Urgent = '{Urgent}', User_Create = '{User_Create}', Create_Date = GETDATE() where Id_Request = '{iD_REQUEST}'");

                    return Json("OK");
                }
                else
                {
                    return Json("Sai trạng thái");
                }
          
         
        }
        public JsonResult _Xoadong(string id)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            var xoa = db.GET_DATA_FROM_SQL("delete from [REQUEST_DETAIL] where Id_RequestDetail = '" + id + "'");
            return Json("OK");
        }
    
    }
}
