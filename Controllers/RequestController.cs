using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.Models;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Drawing;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace PRJ_WAREHOUSE_BIVN.Controllers 
{
    public class Insertt
    {
        public string? Cost_Center { get; set; }
        public string? Declaration { get; set; }
        public string? Dealine { get; set; }
        public float Total_exchange { get; set; }
        public string? Exchange_rate { get; set; }
        public string? Currency { get; set; }
        public float Total { get; set; }
        public string? Kind { get; set; }
        public string? Typee { get; set; }
        public string? Status { get; set; }
        public string? Place { get; set; }
        public string? Loaihinhtokhai { get; set; }
        public string? Group_Code { get; set; }
        public string? Chophepin { get; set; }
        public string? Urgent { get; set; }
        public string? User_Create { get; set; }
        public string? adid_dt { get; set; }
        public string? adid_tt { get; set; }
        public string? adid_pd { get; set; }
        public string? mail_dt { get; set; }
        public string? mail_tt { get; set; }
        public string? mail_pd { get; set; }
        public string? ten_dt { get; set; }
        public string? ten_tt { get; set; }
        public string? ten_pd { get; set; }
        public string? ten_dy { get; set; }
        public string? adid_dy { get; set; }
        public string? mail_dy { get; set; }
        public string? ten_xk { get; set; }
        public string? adid_xk { get; set; }
        public string? mail_xk { get; set; }
        public string? adidnguoitao { get; set; }
        public string? mailnguoitao { get; set; }
        public List<Models.REQUEST_DETAIL>? rq { get; set; }
    }
    public class Insertt_GA
    {
        public string? Cost_Center { get; set; }
        public string? Declaration { get; set; }
        public string? Dealine { get; set; }
        public float Total_exchange { get; set; }
        public string? Exchange_rate { get; set; }
        public string? Currency { get; set; }
        public float Total { get; set; }
        public string? Kind { get; set; }
        public string? Typee { get; set; }
        public string? Status { get; set; }
        public string? Place { get; set; }
        public string? Loaihinhtokhai { get; set; }
        public string? Group_Code { get; set; }
        public string? Chophepin { get; set; }
        public string? Urgent { get; set; }
        public string? User_Create { get; set; }
        public string? adid_dt { get; set; }
        public string? adid_tt { get; set; }
        public string? adid_pd { get; set; }
        public string? mail_dt { get; set; }
        public string? mail_tt { get; set; }
        public string? mail_pd { get; set; }
        public string? ten_dt { get; set; }
        public string? ten_tt { get; set; }
        public string? ten_pd { get; set; }
        public string? ten_dy { get; set; }
        public string? adid_dy { get; set; }
        public string? mail_dy { get; set; }
        public string? ten_xk { get; set; }
        public string? adid_xk { get; set; }
        public string? mail_xk { get; set; }
        public string? adidnguoitao { get; set; }
        public string? mailnguoitao { get; set; }
        public string? ten_qlsc { get; set; }
        public string? mail_qlsc { get; set; }
        public string? adid_qlsc { get; set; }
        public string? ten_qltc { get; set; }
        public string? mail_qltc { get; set; }
        public string? adid_qltc { get; set; }
        public List<Models.REQUEST_DETAIL>? rq { get; set; }
    }
    public class RequestController : Controller
    {
        [HttpPost]
        public JsonResult _Insert_request([FromBody] Insertt model)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();

            string _macoderq = _db.ReturnString($@"
                          DECLARE @Prefix NVARCHAR(10) = '{model.Cost_Center}.';
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
            var data = Common.CostManage.Tinhdutoanconlai(model.Cost_Center, model.Dealine, model.Declaration, _macoderq);
            for (int i = 0; i < model.rq.Count; i++)
            {
                if (model.rq[i].Amount == 0) continue;
                string phongchiuchipheCheck = model.rq[i].Phongchiuchiphi!;
                phongchiuchipheCheck = phongchiuchipheCheck.Length > 0 ? phongchiuchipheCheck : model.Cost_Center;

                Checksotien = Checksotien | phongchiuchipheCheck.Contains(model.Cost_Center);

                Sotienphaichiuchiphi += phongchiuchipheCheck.Contains(model.Cost_Center) ? USD(Convert.ToDouble(model.rq[i].Total_exchange), Convert.ToDouble(model.rq[i].Total_exchange)) : 0;
            }
            if (Checksotien)
            {
                if (Convert.ToDouble(Common.CostManage.Ketqua_Chiphi["Tong"].ToString()) < Sotienphaichiuchiphi)
                {
                    if (_db.GET_DATA_FROM_SQL("SELECT * FROM [TM_LOAIHINHTOKHIA] WHERE [Value] = '" + model.Declaration + "' AND [CoverChiPhi] = 'False' ").Rows.Count > 0)
                    {
                        return Json("Số tiền của yêu cầu lớn hơn số tiền còn lại. \nTuy nhiên hệ thống vẫn cho phép bạn đặt hàng \nVui lòng xác nhận lại với quản lý chi phí nếu cần thiết");
                    }
                    else
                    {
                        return Json("Số tiền của yêu cầu: " + Sotienphaichiuchiphi + " lớn hơn số tiền còn lại: " + Convert.ToDouble(Common.CostManage.Ketqua_Chiphi["Tong"].ToString()) + ", yêu cầu xem xét lại");
                    }
                }
            }

            if (string.IsNullOrEmpty(model.mailnguoitao?.Replace("\n", "").Replace(" ", "")) || model.mailnguoitao.Replace("\n", "").Replace(" ", "") == "")
            {
                model.mailnguoitao = model.adidnguoitao + "@brothergroup.net";
            }

            var rq_detail = REQUEST_PROCESS.Insert_request(model.Cost_Center, model.Declaration, model.Dealine, model.Total_exchange, model.Exchange_rate, model.Currency, model.Total, model.Kind, model.Typee, model.Status, model.Place, model.Loaihinhtokhai, model.Group_Code, model.Chophepin, model.Urgent, model.User_Create, model.rq, model.adid_dt, model.adid_tt, model.adid_pd, model.mail_dt, model.mail_tt, model.mail_pd, model.ten_dt, model.ten_tt, model.ten_pd, model.ten_dy, model.adid_dy, model.mail_dy, model.ten_xk, model.adid_xk, model.mail_xk, model.adidnguoitao, model.mailnguoitao);

            // gửi mail
            string body = $@"Xin chào <br />
                 Đơn yêu cầu mã : {rq_detail} đã được tạo <br /><br />
                 Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
                 <a href='http://172.26.248.62:8075/Approval/ListData'> Link </a> <br />
                 ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                 ※このメールは自動的に送付されたので、返事をしないでください。 ";

                    string body1 = $@"Xin chào <br />
                 Đơn yêu cầu mã : {rq_detail} đã được gửi đến bên xin phê duyệt <br /><br />
                 Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
                 <a href='http://172.26.248.62:8075/Approval/ListData'> Link </a> <br />
                 ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                 ※このメールは自動的に送付されたので、返事をしないでください。 ";

            string subject = "Xác nhận phê duyệt đơn yêu cầu hàng hóa";
            if (model.Urgent == "True")
            {
                subject = "[Gấp] Xác nhận phê duyệt đơn yêu cầu hàng hóa";
            }
         
            var sendmail = REQUEST_PROCESS._sendmail(body, model.mailnguoitao, subject);
            var sendmail1 = REQUEST_PROCESS._sendmail(body1, model.mail_dt, subject);

            return Json("OK");
        }
        public double USD(double NT, double dongia)
        {
            double USD = 0;
            USD = NT / dongia;
            return USD;
        }
        [HttpPost]
        public JsonResult _Insert_request_GA([FromBody] Insertt_GA model)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
           
            string _macoderq = _db.ReturnString($@"
                                DECLARE @Prefix NVARCHAR(10) = '{model.Cost_Center}.';
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
            var data = Common.CostManage.Tinhdutoanconlai(model.Cost_Center, model.Dealine, model.Declaration, _macoderq);
            for (int i = 0; i < model.rq.Count; i++)
            {
                if (model.rq[i].Amount == 0) continue;
                string phongchiuchipheCheck = model.rq[i].Phongchiuchiphi!;
                phongchiuchipheCheck = phongchiuchipheCheck.Length > 0 ? phongchiuchipheCheck : model.Cost_Center;

                Checksotien = Checksotien | phongchiuchipheCheck.Contains(model.Cost_Center);

                Sotienphaichiuchiphi += phongchiuchipheCheck.Contains(model.Cost_Center) ? USD(Convert.ToDouble(model.rq[i].Total_exchange), Convert.ToDouble(model.rq[i].Total_exchange)) : 0;
            }
            if (Checksotien)
            {
                if (Convert.ToDouble(Common.CostManage.Ketqua_Chiphi["Tong"].ToString()) < Sotienphaichiuchiphi)
                {
                    if (_db.GET_DATA_FROM_SQL("SELECT * FROM [TM_LOAIHINHTOKHIA] WHERE [Value] = '" + model.Declaration + "' AND [CoverChiPhi] = 'False' ").Rows.Count > 0)
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
            if (model.Urgent == "True")
            {
                subject = "[Gấp] Xác nhận phê duyệt đơn yêu cầu hàng hóa";
            }
            if (string.IsNullOrEmpty(model.mailnguoitao?.Replace("\n", "").Replace(" ", "")) || model.mailnguoitao.Replace("\n", "").Replace(" ", "") == "")
            {
                model.mailnguoitao = model.adidnguoitao + "@brothergroup.net";
            }
            var rq_detail = REQUEST_PROCESS_GA.Insert_request_GA(model.Cost_Center, model.Declaration, model.Dealine, model.Total_exchange, model.Exchange_rate, model.Currency, model.Total, model.Kind, model.Typee, model.Status, model.Place, model.Loaihinhtokhai, model.Group_Code, model.Chophepin, model.Urgent, model.User_Create, model.rq, model.adid_dt, model.adid_tt, model.adid_pd, model.mail_dt, model.mail_tt, model.mail_pd, model.ten_dt, model.ten_tt, model.ten_pd, model.ten_qlsc, model.adid_qlsc, model.mail_qlsc, model.ten_xk, model.adid_xk, model.mail_xk, model.adidnguoitao, model.mailnguoitao, model.ten_qltc, model.adid_qltc, model.mail_qltc);

            // gửi mail
            string body = $@"Xin chào <br />
               Đơn yêu cầu mã : {rq_detail} đã được tạo <br /><br />
               Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
               <a href='http://172.26.248.62:8075/Approval/ListData_GA'> Link </a> <br />
               ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
               ※このメールは自動的に送付されたので、返事をしないでください。 ";

            string body1 = $@"Xin chào <br />
               Đơn yêu cầu mã : {rq_detail} đã được gửi đến bạn phê duyệt <br /><br />
               Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
               <a href='http://172.26.248.62:8075/Approval/ListData_GA'> Link </a> <br />
               ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
               ※このメールは自動的に送付されたので、返事をしないでください。 ";

         
            var sendmail = REQUEST_PROCESS._sendmail(body, model.mailnguoitao.Replace("\n", "").Replace(" ", ""), subject);
            var sendmail1 = REQUEST_PROCESS._sendmail(body1, model.mail_dt.Replace("\n", "").Replace(" ", ""), subject);

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
               <a href='http://172.26.248.62:8075/Approval/ListData'> Link </a> <br />
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
                     <a href='http://172.26.248.62:8075/Approval/ListData'> Link </a> <br />
                     ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                     ※このメールは自動的に送付されたので, 返事をしないでください。";
            }
            REQUEST_PROCESS._sendmail(body, mailTo, subject);
            var up = REQUEST_PROCESS._update_request(id_request, regency, buoc.ToString());
            // gửi đến người tạo đơn
            if (step == "4")
            {
                string guiden = mail_send.Rows[0]["CHR_MAIL_NGUOITAO"].ToString()!;
                body = $@"Xin chào <br />
                     Đơn yêu cầu mã : {get_ma} của bạn ở trạng thái đã đến kho <br /><br />
                     Bạn vui lòng đến kho để nhận hàng <br /><br />
                     Kiểm tra thông tin đơn tại : <a href='http://172.26.248.62:8075/Approval/Condition'> Link </a> <br />
                     ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                     ※このメールは自動的に送付されたので, 返事をしないでください。";
                REQUEST_PROCESS._sendmail(body, guiden, subject);
            }
            // Gửi mail 
          
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
               <a href='http://172.26.248.62:8075/Approval/ListData_GA'> Link </a> <br />
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
                     <a href='http://172.26.248.62:8075/Approval/ListData_GA'> Link </a> <br />
                     ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                     ※このメールは自動的に送付されたので, 返事をしないでください。";
            }
           
            var up = REQUEST_PROCESS_GA._update_request(id_request, regency, buoc.ToString());
              
            
            // Gửi mail 
           // REQUEST_PROCESS_GA._sendmail(body, mailTo, subject);
            if (step == "5")
            {
                string guiden = mail_send.Rows[0]["CHR_MAIL_NGUOITAO"].ToString()!;
                body = $@"Xin chào <br />
                     Đơn yêu cầu mã : {get_ma} của bạn ở trạng thái đã đến kho <br /><br />
                     Bạn vui lòng đến kho để nhận hàng <br /><br />
                     Kiểm tra thông tin đơn tại : <a href='http://172.26.248.62:8075/Approval/Condition'> Link </a> <br />
                     ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                     ※このメールは自動的に送付されたので, 返事をしないでください。";
                //REQUEST_PROCESS_GA._sendmail(body, guiden, subject);
            }
            return Json("OK");
        }
        public JsonResult _update_dongytatca(string us, string madon)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            //var kq = REQUEST_PROCESS._update_all(us, madon);
            
            string body = $@"Xin chào <br />
                    Đơn yêu cầu mã : {madon.Split("_")[0]} của bạn ở trạng thái ĐỒNG Ý phê duyệt <br /><br />
                    Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
                    <a href='http://172.26.248.62:8075/Approval/ListData'> Link </a> <br />
                    ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                    ※このメールは自動的に送付されたので, 返事をしないでください。";
             
            var get_if = db.GET_DATA_FROM_SQL("select INT_STEP, CHR_MAIL_NGUOITAO, Urgent from [PE_REQUEST_CONFIRM] as a left join REQUEST as b on a.ID_REQUEST = b.Id_Request where b.Code_Request = '" + madon.Split("_")[0] + "' ");
         
            string subject = "Xác nhận phê duyệt đơn yêu cầu hàng hóa";

            if (get_if.Rows[0][2].ToString() == "True")
            {
                subject = "[Gấp] Xác nhận phê duyệt đơn yêu cầu hàng hóa";
            }
            // Gửi mail
            var catmadon = madon.Split("_")[0];
            var get_id = db.ReturnString("select [Id_Request] from [REQUEST] where [Code_Request] = '" + catmadon + "'");

            int buoc = (int.Parse(get_if.Rows[0][0].ToString()!) + 1);
            var mail_send = db.GET_DATA_FROM_SQL("select * from PE_REQUEST_CONFIRM where ID_REQUEST = '" + get_id + "'");
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
                     Đơn yêu cầu mã : {catmadon} của bạn ở trạng thái ĐỒNG Ý phê duyệt <br /><br />
                     Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
                     <a href='http://172.26.248.62:8075/Approval/ListData'> Link </a> <br />
                     ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                     ※このメールは自動的に送付されたので, 返事をしないでください。";
            }

            var up = REQUEST_PROCESS._update_request(get_id, columnName.Split('_')[2], buoc.ToString());
            REQUEST_PROCESS._sendmail(body, mailTo!, subject);            
             
            return Json(up);
        }
        public JsonResult _update_dongytatca_GA(string us, string madon)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            //var kq = REQUEST_PROCESS_GA._update_all(us, madon);

            string body = $@"Xin chào <br />
                    Đơn yêu cầu mã : {madon.Split("_")[0]} của bạn ở trạng thái ĐỒNG Ý phê duyệt <br /><br />
                    Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
                    <a href='http://172.26.248.62:8075/Approval/ListData_GA'> Link </a> <br />
                    ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                    ※このメールは自動的に送付されたので, 返事をしないでください。";

            var get_if = db.GET_DATA_FROM_SQL("select INT_STEP, CHR_MAIL_NGUOITAO, Urgent from [PE_REQUEST_CONFIRM_GA] as a left join REQUEST as b on a.ID_REQUEST = b.Id_Request where b.Code_Request = '" + madon.Split("_")[0] + "' ");

            string subject = "Xác nhận phê duyệt đơn yêu cầu hàng hóa";

            if (get_if.Rows[0][2].ToString() == "True")
            {
                subject = "[Gấp] Xác nhận phê duyệt đơn yêu cầu hàng hóa";
            }
            // Gửi mail

            var catmadon = madon.Split("_")[0];
            var get_id = db.ReturnString("select [Id_Request] from [REQUEST] where [Code_Request] = '" + catmadon + "'");

            int buoc = int.Parse(get_if.Rows[0][0].ToString()!) + 1;
            if (buoc == 5)
            {
                body = $@"Xin chào <br />
                     Đơn yêu cầu mã : {catmadon} của bạn ở trạng thái ĐỒNG Ý phê duyệt <br /><br />
                     Bạn vui lòng click vào đường link dưới đây để xác nhận <br /><br />
                     <a href='http://172.26.248.62:8075/Approval/ListData_GA'> Link </a> <br />
                     ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
                     ※このメールは自動的に送付されたので, 返事をしないでください。";
            }

            var mail_send = db.GET_DATA_FROM_SQL("select * from PE_REQUEST_CONFIRM_GA where ID_REQUEST = '" + get_id + "'");
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
          
            // update đơn
            var up = REQUEST_PROCESS_GA._update_request(get_id, columnName.Split('_')[2], buoc.ToString());
            // gửi mail
            REQUEST_PROCESS_GA._sendmail(body, mailTo!, subject);

            return Json(up);
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
              <a href='http://172.26.248.62:8075/Approval/Condition'> Link </a> <br />
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
        public JsonResult _huydon_prod(string id_request, string reason)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            //string subject = "Hủy đơn yêu cầu hàng hóa ";

            var madon = _db.ReturnString("select Code_Request from REQUEST where [Id_Request] = '" + id_request + "'");

            //string body = @"Xin chào <br />
            //  Đơn yêu cầu mã : " + madon + @" bị hủy bỏ <br /><br />
            //  Lý do : " + reason + @"<br /><br />            

            //  ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
            //  ※このメールは自動的に送付されたので、返事をしないでください。";
           
            var up = _db.GET_DATA_FROM_SQL("update PE_REQUEST_CONFIRM set INT_STEP = '15' where [ID_REQUEST] = '" + id_request+"'");
                    
            var mail_send = _db.ReturnString("select CHR_MAIL_NGUOITAO from PE_REQUEST_CONFIRM where ID_REQUEST = '" + id_request + "'");

            _db.GET_DATA_FROM_SQL("Insert into PE_LOGHUYDON ([ID_REQUEST],[Lydo],[Madon]) values ('" + id_request + "',N'" + reason + "','" + madon + "')");
            _db.GET_DATA_FROM_SQL("update [REQUEST] set [Status] = 'REFUSE' where [Id_Request] = '" + id_request + "'");
            // Gửi về người tạo đơn
            // mail_nguoidat = mail_send.Trim();
            //REQUEST_PROCESS._sendmail(body, mail_nguoidat, subject);

            return Json("Hoàn thành !");
        }
        public JsonResult _huydon_GA(string id_request, string reason)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            string subject = "Hủy đơn yêu cầu hàng hóa ";

            var madon = _db.ReturnString("select Code_Request from REQUEST where [Id_Request] = '" + id_request + "'");

            string body = @"Xin chào <br />
              Đơn yêu cầu mã : " + madon + @" bị hủy bỏ <br /><br />
              Lý do : " + reason + @"<br /><br />            

              ※Email này được gửi một cách tự động, xin vui lòng không trả lời.<br />
              ※このメールは自動的に送付されたので、返事をしないでください。";

            var up = _db.GET_DATA_FROM_SQL("update PE_REQUEST_CONFIRM_GA set INT_STEP = '15' where [ID_REQUEST] = '" + id_request + "'");

            var mail_send = _db.ReturnString("select CHR_MAIL_NGUOITAO from PE_REQUEST_CONFIRM_GA where ID_REQUEST = '" + id_request + "'");

            _db.GET_DATA_FROM_SQL("Insert into PE_LOGHUYDON ([ID_REQUEST],[Lydo],[Madon]) values ('" + id_request + "',N'" + reason + "','" + madon + "')");
            _db.GET_DATA_FROM_SQL("update [REQUEST] set [Status] = 'REFUSE' where [Id_Request] = '" + id_request + "'");
            // Gửi về người tạo đơn
            string mail_nguoidat = mail_send.Trim();
            REQUEST_PROCESS_GA._sendmail(body, mail_nguoidat, subject);

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
              <a href='http://172.26.248.62:8075/Approval/Condition'> Link </a> <br />
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
        public JsonResult get_requestcondition(string loaicp, string ngayyc,string Group_Code, string Code_Request, string INT_STEP, string Cost_Center, string Request_Date, string Total, string Urgent, string us, string costt_ct)
        {
            //Urgent = (Urgent == "Gấp") ? "1" : (Urgent == "Thông thường" ? "0" : Urgent);
            //var a =  (Total == "Dưới 3000") ? "2999"                  
            //       : (Total == "Dưới 10,000") ? "3000"
            //       : (Total == "Trên 10,000") ? "10000"
            //       : "0";
            double aa = double.Parse(Total);
            var cf = REQUEST_PROCESS.get_requestcondition(loaicp, ngayyc,us,Group_Code, Code_Request, INT_STEP, Cost_Center, Request_Date, aa, Urgent,costt_ct);
            
            return Json(cf);
        }
        public JsonResult get_requestcondition_GA(string loaicp, string ngayyc,string us, string Group_Code, string Code_Request, string INT_STEP, string Cost_Center, string Request_Date, string Total, string Urgent, string costt_ct)
        {
            //Urgent = (Urgent == "Gấp") ? "1" : (Urgent == "Thông thường" ? "0" : Urgent);
            //var a =  (Total == "Dưới 3000") ? "2999"                  
            //       : (Total == "Dưới 10,000") ? "3000"
            //       : (Total == "Trên 10,000") ? "10000"
            //       : "0";
            double aa = double.Parse(Total);
            var cf = REQUEST_PROCESS_GA.get_requestcondition(loaicp, ngayyc, us, Group_Code, Code_Request, INT_STEP, Cost_Center, Request_Date, aa, Urgent, costt_ct);
            return Json(cf);
        }
        public JsonResult _layphongban(string ph)
        {
            if(ph == null)
            {
                return Json("Chưa nhập phòng ban");
            }
            else
            {
                SQL_Connect_DB20 db = new SQL_Connect_DB20();
                var phongban = db.ReturnString("select [CHR_Section_Code] from [DEPARTMENT] where [Cost_Center] = '" + ph.Split(':')[0] + "' ");

                return Json(phongban);
            }
         
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
                                Total_exchange = '{rq[i].Total_exchange}',
                                Phongchiuchiphi = '{rq[i].Phongchiuchiphi!.Split(':')[0]}',
                                [Status] = '',

                            WHERE Code_Request = '{Code_Request}' AND Material_Code = '{rq[i].Material_Code}'
                        END
                        ELSE
                        BEGIN
                            -- Thực hiện Insert
                            INSERT INTO REQUEST_DETAIL (Code_Request,Material_Name, Material_Code, Account_Name,Account_Code,Unit, Amount, Price, Total, Vitri, Poisition,Currency, Aim, Id_Request,Total_exchange,Rate,Material_Name_EN,Material_Name_ENJP, Phongchiuchiphi)
                            VALUES ('{Code_Request}',N'{rq[i].Material_Name}','{rq[i].Material_Code}','{rq[i].Account_Name}','{rq[i].Account_Code}',N'{rq[i].Unit}','{rq[i].Amount}','{rq[i].Price}','{rq[i].Total_exchange}','{rq[i].Vitri}',N'{rq[i].Poisition}',N'{rq[i].Currency!.Trim()}',N'{rq[i].Aim}','{iD_REQUEST}','{rq[i].Total_exchange}','1','','','{rq[i].Phongchiuchiphi!.Split(':')[0]}')
                        END");
                    }
                    db.GET_DATA_FROM_SQL($"Update [REQUEST] set [Declaration] = '{Declaration}', [Total_exchange] = '{Total_exchange}', Exchange_rate = '{Exchange_rate}', [Dealine] = '{Dealine}', Total = '{Total}',Place = N'{Place}',Urgent = '{Urgent}', User_Create = '{User_Create}', Create_Date = GETDATE() where Id_Request = '{iD_REQUEST}'");

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
        public ActionResult RequestAll()
        {
            return View();
        }
        public JsonResult view_rqall(string StartDate, string EndDate, string us, string Group_Code, string loaicp, string Code_Request, string Urgent,string Cost_Center, string Tinhtrang, double Total)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            // 1. Lấy dữ liệu chính
            var Truongdulieu = "REQUEST.[Id_Request],[Code_Request],[Group_Code],[Cost_Center],[Request_Date],[Declaration],[Dealine],[Dealine_Real],[Total],[Total_Real],[Kind],[Type],[Status],[Create_Date],[User_Create],[Last_Update],[User_Update],[Reason],[Action],[Note] AS Chitiet,[Chophepin],[INT_STEP] ";
            string gia = "";
           
            if (Total > 0 && Total < 3000)
            {
                gia = "and b.Total < '3000'";
            }
            if (Total >= 3000 && Total < 10000)
            {
                gia = "and b.Total >= '3000' and b.Total < '10000'";
            }
            if (Total >= 10000)
            {
                gia = "and b.Total >= '10000'";
            }
            string bang = "PE_REQUEST_CONFIRM";
            if (Group_Code == "GA")
            {
                bang = "PE_REQUEST_CONFIRM_GA";
            }
            string sql = $@"SELECT TOP (300) {Truongdulieu} FROM REQUEST  left join {bang} on REQUEST.Id_Request = {bang}.ID_REQUEST
                    WHERE Request_Date like '%{StartDate}%' 
                    AND Dealine like '%{EndDate}%'
                    AND KIND = 'IN'
                    AND Urgent like N'%%'
	                AND Group_Code like N'%{Group_Code}%'
					AND Declaration like N'%{loaicp}%'
                    AND [Status] like N'%{Tinhtrang}%'
					AND Code_Request like N'%{Code_Request}%'
					AND Cost_Center like N'%{Cost_Center}%'
                    {gia}
                    AND Group_Code like N'%{Group_Code}%'
                    AND Cost_Center in (SELECT Cost_Center FROM USER_DEPT WHERE CHR_USERID = '{us}') 
                    ORDER BY REQUEST.[Id_Request] DESC";

            DataTable dataTable = _db.GET_DATA_FROM_SQL(sql);

            var resultList = new List<Dictionary<string, object>>();
            foreach (DataRow row in dataTable.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dataTable.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                resultList.Add(dict);
            }

            return Json(resultList);
        }

    }
}
