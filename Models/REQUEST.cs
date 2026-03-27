using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;
using Microsoft.AspNetCore.Http.HttpResults;
using OfficeOpenXml.Utils;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Net;
using System.Net.Mail;
using System.Net.WebSockets;
using System.Security.Cryptography;

namespace PRJ_WAREHOUSE_BIVN.Models
{
    public class PE_USERNAME
    {
        public int Id_User { get; set; }
        public string? User_Name { get; set; }
        public string? Mail { get; set; }
        public string? Adid { get; set; }
        public string? Group_Code { get; set; }
        public string? Role { get; set; }
    }
    public class REQUEST_DETAIL
    {
        public int Id_Request { get; set; }
        public string? Code_Request { get; set; }
        public string? Material_Code { get; set; }
        public int Id_RequestDetail { get; set; }
        public string? Material_Name { get; set; }
        public string? Material_Name_EN { get; set; }
        public string? Material_Name_ENJP { get; set; }
        public string? Account_Code { get; set; }
        public string? Account_Name { get; set; }
        public string? Unit { get; set; }
        public string? Unit_Real { get; set; }
        public double? Amount { get; set; }
        public double? Price { get; set; }
        public string? Price_Real { get; set; }
        public double? Total_exchange { get; set; }
        public int? Rate { get; set; }
        public string? Currency { get; set; }
        public double? Total { get; set; }
        public double? Amount_Real { get; set; }
        public string? VAT { get; set; }
        public string? Total_exchange_real { get; set; }
        public double? Rate_Real { get; set; }
        public string? Currency_Real { get; set; }
        public double? Total_Real { get; set; }
        public string? Dealine_Real { get; set; }
        public string? Poisition { get; set; }
        public string? Aim { get; set; }
        public string? Brand { get; set; }
        public string? Status { get; set; }
        public string? Last_Update { get; set; }
        public string? User_Update { get; set; }
        public string? PO { get; set; }
        public string? Unit_Note { get; set; }
        public string? Phongchiuchiphi { get; set; }
        public string? Vitri { get; set; }
        public int? Id_LichsuXuat { get; set; }
        public string? Kho { get; set; }
        public string? Cost_Center_Group { get; set; }
        public string? Name_Dept { get; set; }
        public string? Creat_Date { get; set; }
        public string? Dealine { get; set; }
        public string? Cost_Center { get; set; }
        public string? Group_Code { get; set; }
        public string? Good_Code { get; set; }
        public string? Urgent { get; set; }
        public string? MaHangTem { get; set; }
        public List<SOLUONGKHO>? slk { get; set; }


    }
    public class REQUEST
    {
        public int Id_Request { get; set; }
        public string? Code_Request { get; set; }
        public string? Cost_Center { get; set; }
        public string? Request_Date { get; set; }
        public string? Declaration { get; set; }
        public string? Dealine { get; set; }
        public string? Dealine_Real { get; set; }
        public double? Total_exchange { get; set; }
        public string? Exchange_rate { get; set; }
        public string? Currency { get; set; }
        public double? Total { get; set; }
        public string? Total_exchange_real { get; set; }
        public string? Exchange_rate_Real { get; set; }
        public string? Currency_Real { get; set; }
        public string? Total_Real { get; set; }
        public string? Kind { get; set; }
        public string? Typee { get; set; }
        public string? Status { get; set; }
        public string? Create_Date { get; set; }
        public string? User_Create { get; set; }
        public string? Last_Update { get; set; }
        public string? User_Update { get; set; }
        public string? Reason { get; set; }
        public string? Action { get; set; }
        public string? Place { get; set; }
        public bool? Freeze { get; set; }
        public string? Note { get; set; }
        public string? Loaihinhtokhai { get; set; }
        public string? Phuongthucvanchuyen { get; set; }
        public string? Group_Code { get; set; }
        public bool? Chophepin { get; set; }
        public string? KindofRQ { get; set; }
        public string? Urgent { get; set; }
        public string? CostCenter { get; set; }
        public string? Name { get; set; }
        public string? Name_Jp { get; set; }
        public string? Cost_Center_Group { get; set; }

        // Tạo mã đơn vào bảng request và request_detail
    }
    public class PE_REQUEST_CONFIRM
    {
        public int ID { get; set; }
        public int ID_REQUEST { get; set; }
        public string? CHR_ADID_NGUOIYEUCAU { get; set; }
        public string? CHR_ADID_NGUOITHAMTRA { get; set; }
        public string? CHR_ADID_NGUOIPHEDUYET { get; set; }
        public string? CHR_ADID_XACNHAN { get; set; }
        public string? CHR_ADID_XUATKHO { get; set; }
        public string? CHR_ADID_ { get; set; }
        public string? CONFIRM_NGUOITHAMTRA { get; set; }
        public string? CONFIRM_NGUOIPHEDUYET { get; set; }
        public string? CONFIRM_XACNHAN { get; set; }
        public string? DTM_XACNHAN { get; set; }
        public string? DTM_NGUOITHAMTRA { get; set; }
        public string? DTM_NGUOIPHEDUYET { get; set; }
        public string? INT_STEP { get; set; }
        public string? Code_Request { get; set; }
        public string? Cost_Center { get; set; }
        public string? Request_Date { get; set; }
        public string? Declaration { get; set; }
        public string? Dealine { get; set; }
        public DateTime? Dealine_Real { get; set; }
        public double? Total_exchange { get; set; }
        public double? Exchange_rate { get; set; }
        public string? Currency { get; set; }
        public double? Total { get; set; }
        public double? Total_exchange_real { get; set; }
        public double? Exchange_rate_Real { get; set; }
        public string? Currency_Real { get; set; }
        public double? Total_Real { get; set; }
        public string? Kind { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string? Create_Date { get; set; }
        public string? User_Create { get; set; }
        public string? Last_Update { get; set; }
        public string? User_Update { get; set; }
        public string? Reason { get; set; }
        public string? Action { get; set; }
        public string? Place { get; set; }
        public bool? Freeze { get; set; }
        public string? Note { get; set; }
        public string? Loaihinhtokhai { get; set; }
        public string? Phuongthucvanchuyen { get; set; }
        public string? Group_Code { get; set; }
        public bool? Chophepin { get; set; }
        public string? KindofRQ { get; set; }
        public string? Urgent { get; set; }
        public string? CostCenter { get; set; }
        public string? CHR_MAIL_NGUOIYEUCAU { get; set; }
        public string? CHR_MAIL_NGUOITHAMTRA { get; set; }
        public string? CHR_MAIL_NGUOIPHEDUYET { get; set; }
        public string? CHR_TEN_NGUOIYEUCAU { get; set; }
        public string? CHR_TEN_NGUOITHAMTRA { get; set; }
        public string? CHR_TEN_NGUOIPHEDUYET { get; set; }
        public string? CHR_TEN_XACNHAN { get; set; }
        public string? CHR_TEN_XUATKHO { get; set; }
        public string? Cost_Center_Group { get; set; }
    }
    public class CHITIET_XUATKHO
    {
        public int? Id_RequestDetail { get; set; }
        public string ? Code_Request { get; set; }
        public int? Id_Request { get; set; }
        public string? Material_Code { get; set; }
        public string? Material_Name { get; set; }
        public string? Account_Code { get; set; }
        public string? Unit { get; set; }
        public string? Amount { get; set; }
        public decimal? Price { get; set; }
        public string? Currency { get; set; }
        public string? Phongchiuchiphi { get; set; }
        public int ID { get; set; }
        public string? CHR_ADID_NGUOIYEUCAU { get; set; }
        public string? CHR_ADID_NGUOITHAMTRA { get; set; }
        public string? CHR_ADID_NGUOIPHEDUYET { get; set; }
        public string? CHR_ADID_XACNHAN { get; set; }
        public string? CHR_ADID_XUATKHO { get; set; }
        public string? CONFIRM_NGUOITHAMTRA { get; set; }
        public string? CONFIRM_NGUOIPHEDUYET { get; set; }
        public string? CONFIRM_XACNHAN { get; set; }
        public string? DTM_XACNHAN { get; set; }
        public string? DTM_NGUOITHAMTRA { get; set; }
        public string? DTM_NGUOIPHEDUYET { get; set; }
        public string? INT_STEP { get; set; }
        public string? QTY_NEED { get; set; }
        public string? DTM_UPDATE { get; set; }
    }
    public class SOLUONGKHO
    {
        public string? tenkho { get; set; }
        public string? soluong { get; set; }
    }
    public class REQUEST_PROCESS
    {
        public static string Insert_request(string Cost_Center, string Declaration, string Dealine, string Total_exchange, string Exchange_rate, string Currency, string Total, string Kind, string Type, string Status, string Place, string Loaihinhtokhai, string Group_Code, string Chophepin, string Urgent, string User_Create, List<REQUEST_DETAIL>? rq_dt, string adid_dt, string adid_tt, string adid_pd, string mail_dt, string mail_tt, string mail_pd, string ten_dt, string ten_tt, string ten_pd, string ten_dy, string adid_dy, string mail_dy, string ten_xk, string adid_xk, string mail_xk,string adidnguoitao, string mailnguoitao)
        {
            
                SQL_Connect_DB20 _db = new SQL_Connect_DB20();
                //  TẠO MÃ REQUEST TĂNG TỰ ĐỘNG VÀ INSERT BẢNG REQUEST ---
                string _cmdRequest = $@"
                    DECLARE @Prefix NVARCHAR(10) = '{Cost_Center}.';
                    DECLARE @Today NVARCHAR(8) = FORMAT(GETDATE(), 'yyyyMMdd');
                    DECLARE @SearchPattern NVARCHAR(20) = @Prefix + @Today + '-%';
                    DECLARE @NewCode NVARCHAR(30);
                    DECLARE @MaxNum INT = 0;
                    SELECT @MaxNum = MAX(CAST(REPLACE([Code_Request], @Prefix + @Today + '-', '') AS INT))
                    FROM [dbo].[REQUEST] WHERE [Code_Request] LIKE @SearchPattern;
                    SET @NewCode = @Prefix + @Today + '-' + CAST(ISNULL(@MaxNum, 0) + 1 AS NVARCHAR(10));
                    WHILE EXISTS (SELECT 1 FROM [dbo].[REQUEST] WHERE [Code_Request] = @NewCode)
                    BEGIN 
                        SET @MaxNum = @MaxNum + 1;
                        SET @NewCode = @Prefix + @Today + '-' + CAST(@MaxNum + 1 AS NVARCHAR(10));
                    END
                    INSERT INTO [dbo].[REQUEST] (
                        [Code_Request], [Cost_Center], [Request_Date], [Declaration], [Dealine], [Total_exchange], [Exchange_rate], [Currency], [Total],  
                        [Kind], [Type], [Status], [Create_Date], [User_Create],[Place], [Loaihinhtokhai], [Group_Code],  [Chophepin], [Urgent] 
                    ) 
                    VALUES (
                         
                        @NewCode, '{Cost_Center}', '{DateTime.Now.ToString("yyyy-MM-dd HH:ss:mm")}', N'{Declaration}', '{Dealine}', ROUND({Total_exchange},2), '{Exchange_rate}', '{Currency}', ROUND({Total},2),  
                        '{Kind}', '{Type}', '{Status}','{DateTime.Now.ToString("yyyy-MM-dd HH:ss:mm")}', '{User_Create}', '{Place}', '{Loaihinhtokhai}', '{Group_Code}', '{Chophepin}', '{Urgent}' 
                    );
                    SELECT @NewCode AS NextCode, SCOPE_IDENTITY() AS NewID;";
                var dtBase = _db.GET_DATA_FROM_SQL(_cmdRequest);
                if (dtBase == null || dtBase.Rows.Count == 0) return "NG";

                string newCode = dtBase.Rows[0]["NextCode"].ToString()!;
                string newId = dtBase.Rows[0]["NewID"].ToString()!;
             
                // INSERT BẢNG REQUEST_DETAIL ---
                foreach (var item in rq_dt!)
                {
                     string _cmdDetail = $@"
                                        INSERT INTO [REQUEST_DETAIL] (
                                        [Code_Request], [Id_Request], [Material_Code], [Material_Name], [Material_Name_EN], [Material_Name_ENJP], 
                                        [Account_Code], [Account_Name], [Unit], [Unit_Real], [Amount], [Price], [Total_exchange], [Rate], 
                                        [Currency], [Total], [Amount_Real], [Price_Real], [VAT], [Total_exchange_real], [Rate_Real], 
                                        [Currency_Real], [Total_Real], [Dealine_Real], [Poisition], [Aim], [Status], [Last_Update], 
                                        [User_Update], [PO], [Unit_Note], [Phongchiuchiphi], [Vitri], [Id_LichsuXuat], [Kho] )
                                        VALUES (
                                            '{newCode}', {newId}, '{item.Material_Code}', N'{item.Material_Name}', N'{item.Material_Name_EN}', N'{item.Material_Name_ENJP}', 
                                            '{item.Account_Code}', N'{item.Account_Name}', N'{item.Unit}', '{item.Unit_Real}', '{item.Amount}', '{item.Price}', 
                                             ROUND({item.Total_exchange},2), '{item.Rate}', '{item.Currency}', ROUND({item.Total},2), '{item.Amount_Real}', '{item.Price_Real}', 
                                            '{item.VAT}', '{item.Total_exchange_real}', '{item.Rate_Real}', '{item.Currency_Real}', '{item.Total_Real}', 
                                            '{item.Dealine_Real}', N'{item.Poisition}', N'{item.Aim}', '{item.Status}', GETDATE(), 
                                            '{item.User_Update}', '{item.PO}', N'{item.Unit_Note}', N'{item.Phongchiuchiphi}', N'{item.Vitri}', '{item.Id_LichsuXuat}', '{item.Kho}'
                                        )";
                    _db.GET_DATA_FROM_SQL(_cmdDetail);
                }
                //ten_dy = ten_dy.Split('_')[1];
                //ten_xk = ten_xk.Split('_')[1];
                _insert_request_confirm(newId, adid_dt, adid_tt, adid_pd, mail_dt, mail_tt, mail_pd, ten_dt, ten_tt, ten_pd, ten_dy , adid_dy, mail_dy, ten_xk, adid_xk, mail_xk, adidnguoitao, mailnguoitao);

                return newCode;   
        }
        public static string get_rate()
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var rate = _db.GET_DATA_FROM_SQL("SELECT TOP 1 Rate FROM [COST_MANAGEMENT].[dbo].[EXCHANGE_RATE] WHERE Currency = 'VND' ORDER BY Id DESC;");
            return rate.Rows[0][0].ToString()!;
        }
        public static List<string> phongchiuphi()
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var phongchiuchi = _db.GET_DATA_FROM_SQL("select * from [DEPARTMENT]");
            List<string> pcp = new List<string>();
            for (int i = 0; i < phongchiuchi.Rows.Count; i++)
            {
                pcp.Add(phongchiuchi.Rows[i]["Cost_Center"].ToString() + ":" + phongchiuchi.Rows[i]["Name"].ToString());
            }
            return pcp;
        }
        public static string _insert_request_confirm(string id_request, string adid_dt, string adid_tt, string adid_pheduyet, string mail_dt, string mail_tt, string mail_pd, string ten_dt, string ten_tt, string ten_pd, string ten_dy, string adid_dy, string mail_dy, string ten_xk, string adid_xk, string mail_xk, string nguoitao, string mailnguoitao)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            _db.GET_DATA_FROM_SQL("insert into [PE_REQUEST_CONFIRM] (ID_REQUEST,CHR_ADID_NGUOIYEUCAU,CHR_ADID_NGUOITHAMTRA,CHR_ADID_NGUOIPHEDUYET,CHR_ADID_XACNHAN, DTM_XACNHAN, INT_STEP,CHR_MAIL_NGUOIYEUCAU,CHR_MAIL_NGUOITHAMTRA,CHR_MAIL_NGUOIPHEDUYET,CHR_TEN_NGUOIYEUCAU,CHR_TEN_NGUOITHAMTRA,CHR_TEN_NGUOIPHEDUYET,CHR_ADID_XUATKHO,CHR_MAIL_XUATKHO,CHR_TEN_XUATKHO,CHR_TEN_XACNHAN,CHR_MAIL_XACNHAN,CONFIRM_NGUOIYEUCAU,CONFIRM_NGUOITHAMTRA,CONFIRM_NGUOIPHEDUYET,CONFIRM_XACNHAN,CHR_ADID_NGUOITAO,CHR_MAIL_NGUOITAO) " +
                "VALUES ('" + id_request + "','" + adid_dt + "','" + adid_tt + "','" + adid_pheduyet + "','" + adid_dy + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','0','" + mail_dt + "','" + mail_tt + "','" + mail_pd + "',N'" + ten_dt + "',N'" + ten_tt + "',N'" + ten_pd + "','" + adid_xk + "','" + mail_xk + "',N'" + ten_xk + "',N'" + ten_dy + "','" + mail_dy + "','0','0','0','0','" + nguoitao + "','"  + mailnguoitao + "')");
            return "OK";
        }
        public static string _insert_request_confirm_GA(string id_request, string adid_dt, string adid_tt, string adid_pheduyet, string mail_dt, string mail_tt, string mail_pd, string ten_dt, string ten_tt, string ten_pd, string ten_qlsc, string adid_qlsc, string mail_qlsc, string ten_xk, string adid_xk, string mail_xk, string nguoitao, string mailnguoitao, string adid_qltc, string mail_qltc, string ten_qltc)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            _db.GET_DATA_FROM_SQL("insert into [PE_REQUEST_CONFIRM_GA] (ID_REQUEST,CHR_ADID_NGUOIYEUCAU,CHR_ADID_NGUOITHAMTRA,CHR_ADID_NGUOIPHEDUYET,CHR_ADID_QLSC, DTM_QLSC, INT_STEP,CHR_MAIL_NGUOIYEUCAU,CHR_MAIL_NGUOITHAMTRA,CHR_MAIL_NGUOIPHEDUYET,CHR_TEN_NGUOIYEUCAU,CHR_TEN_NGUOITHAMTRA,CHR_TEN_NGUOIPHEDUYET,CHR_ADID_XUATKHO,CHR_MAIL_XUATKHO,CHR_TEN_XUATKHO,CHR_TEN_QLSC,CHR_MAIL_QLSC,CONFIRM_NGUOIYEUCAU,CONFIRM_NGUOITHAMTRA,CONFIRM_NGUOIPHEDUYET,CONFIRM_QLSC,CHR_ADID_NGUOITAO,CHR_MAIL_NGUOITAO,CHR_ADID_QLTC,CHR_TEN_QLTC,CHR_MAIL_QLTC,CONFIRM_QLTC) " +
                "VALUES ('" + id_request + "','" + adid_dt + "','" + adid_tt + "','" + adid_pheduyet + "','" + adid_qlsc + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','0','" + mail_dt + "','" + mail_tt + "','" + mail_pd + "',N'" + ten_dt + "',N'" + ten_tt + "',N'" + ten_pd + "','" + adid_xk + "','" + mail_xk + "',N'" + ten_xk + "',N'" + ten_qlsc + "','" + mail_qlsc + "','0','0','0','0','" + nguoitao + "','" + mailnguoitao + "','" + adid_qltc + "','" + ten_qltc + "','" + mail_qltc + "','0')");
            return "OK";
        }
        public static List<PE_REQUEST_CONFIRM> get_requestconfirm( string us, string Urgent, double Total, string Code_Request, string INT_STEP)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            List<PE_REQUEST_CONFIRM> pe_ = new List<PE_REQUEST_CONFIRM>();
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
            var list = _db.GET_DATA_FROM_SQL(" select top (200) * from [PE_REQUEST_CONFIRM] as a left join REQUEST as b on a.ID_REQUEST = b.Id_Request left join DEPARTMENT as c on b.Cost_Center = c.Cost_Center " +
                "where ((CHR_ADID_NGUOIYEUCAU = '" + us +"' and CONFIRM_NGUOIYEUCAU = '0') OR (CHR_ADID_NGUOITHAMTRA = '" + us +"' and CONFIRM_NGUOITHAMTRA = '0') OR (CHR_ADID_NGUOIPHEDUYET = '" + us +"' and CONFIRM_NGUOIPHEDUYET = '0') OR (CHR_ADID_XACNHAN = '" + us +"' and CONFIRM_XACNHAN = '0')) and INT_STEP < 5 " +
                $"and Urgent like '%{Urgent}%' {gia} and b.Code_Request like '%{Code_Request}%' and a.INT_STEP like '%{INT_STEP}%'");
            for (int i = 0; i < list.Rows.Count; i++)
            {
                pe_.Add(new PE_REQUEST_CONFIRM {
                    ID = int.Parse(list.Rows[i]["ID"].ToString()!),
                    ID_REQUEST = int.Parse(list.Rows [i]["ID_REQUEST"].ToString()!),
                    CHR_ADID_NGUOIYEUCAU = list.Rows [i]["CHR_ADID_NGUOIYEUCAU"].ToString()!,
                    CHR_ADID_NGUOITHAMTRA = list.Rows [i]["CHR_ADID_NGUOITHAMTRA"].ToString()!,
                    CHR_ADID_NGUOIPHEDUYET = list.Rows [i]["CHR_ADID_NGUOIPHEDUYET"].ToString()!,
                    CHR_ADID_XACNHAN = list.Rows [i]["CHR_ADID_XACNHAN"].ToString()!,                  
                    CONFIRM_NGUOITHAMTRA = list.Rows [i]["CONFIRM_NGUOITHAMTRA"].ToString()!,
                    CONFIRM_NGUOIPHEDUYET = list.Rows [i]["CONFIRM_NGUOIPHEDUYET"].ToString()!,
                    DTM_XACNHAN = list.Rows [i]["DTM_XACNHAN"].ToString()!,
                    DTM_NGUOITHAMTRA = list.Rows [i]["DTM_NGUOITHAMTRA"].ToString()!,
                    DTM_NGUOIPHEDUYET = list.Rows [i]["DTM_NGUOIPHEDUYET"].ToString()!,
                    INT_STEP = list.Rows [i]["INT_STEP"].ToString()!,
                    Code_Request = list.Rows[i]["Code_Request"].ToString()!,
                    Cost_Center = list.Rows[i]["Cost_Center"].ToString()!,
                    Dealine = list.Rows[i]["Dealine"].ToString()!,
                    Total = Math.Round(double.Parse(list.Rows[i]["Total"].ToString()!),2),
                    User_Create = list.Rows[i]["User_Create"].ToString()!,
                    Create_Date = list.Rows[i]["Create_Date"].ToString()!,
                    CHR_TEN_NGUOIYEUCAU = list.Rows[i]["CHR_TEN_NGUOIYEUCAU"].ToString()!,
                    CHR_TEN_NGUOITHAMTRA = list.Rows[i]["CHR_TEN_NGUOITHAMTRA"].ToString()!,
                    CHR_TEN_NGUOIPHEDUYET = list.Rows[i]["CHR_TEN_NGUOIPHEDUYET"].ToString()!,
                    CHR_TEN_XACNHAN = list.Rows[i]["CHR_TEN_XACNHAN"].ToString()!,
                    CHR_TEN_XUATKHO = list.Rows[i]["CHR_TEN_XUATKHO"].ToString()!,
                    CHR_MAIL_NGUOIPHEDUYET = list.Rows[i]["CHR_MAIL_NGUOIPHEDUYET"].ToString()!,
                    CHR_MAIL_NGUOIYEUCAU = list.Rows[i]["CHR_MAIL_NGUOIYEUCAU"].ToString()!,
                    CHR_MAIL_NGUOITHAMTRA = list.Rows[i]["CHR_MAIL_NGUOITHAMTRA"].ToString()!,
                    CHR_ADID_XUATKHO = list.Rows[i]["CHR_ADID_XUATKHO"].ToString()!,
                    Cost_Center_Group = list.Rows[i]["Cost_Center_Group"].ToString()!,
                    Urgent = list.Rows[i]["Urgent"].ToString()!                 
                });
            }
            return pe_;
        }
        public static List<PE_REQUEST_CONFIRM> get_requestcondition(string Group_Code, string Code_Request, string INT_STEP, string Cost_Center, string Request_Date, double Total, string Urgent)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            List<PE_REQUEST_CONFIRM> pe_ = new List<PE_REQUEST_CONFIRM>();
            string gia = "";
            if(Total > 0 && Total < 3000)
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
            var list = _db.GET_DATA_FROM_SQL($@"select top (1000) * from [PE_REQUEST_CONFIRM] as a 
                        left join REQUEST as b on a.ID_REQUEST = b.Id_Request 
                        left join DEPARTMENT as c on b.Cost_Center = c.Cost_Center 
                        WHERE b.Group_Code like '%{Group_Code}%' and b.Code_Request like '%{Code_Request}%' and a.INT_STEP like '%{INT_STEP}%' and b.Cost_Center like '%{Cost_Center}%' and b.Request_Date like '%{Request_Date}%' and Urgent like '%{Urgent}%' {gia}  
                        order by ID desc");

            for (int i = 0; i < list.Rows.Count; i++)
            {
                pe_.Add(new PE_REQUEST_CONFIRM
                {
                    ID = int.Parse(list.Rows[i]["ID"].ToString()!),
                    ID_REQUEST = int.Parse(list.Rows[i]["ID_REQUEST"].ToString()!),
                    CHR_ADID_NGUOIYEUCAU = list.Rows[i]["CHR_ADID_NGUOIYEUCAU"].ToString()!,
                    CHR_ADID_NGUOITHAMTRA = list.Rows[i]["CHR_ADID_NGUOITHAMTRA"].ToString()!,
                    CHR_ADID_NGUOIPHEDUYET = list.Rows[i]["CHR_ADID_NGUOIPHEDUYET"].ToString()!,
                    CHR_ADID_XACNHAN = list.Rows[i]["CHR_ADID_XACNHAN"].ToString()!,
                    CONFIRM_NGUOITHAMTRA = list.Rows[i]["CONFIRM_NGUOITHAMTRA"].ToString()!,
                    CONFIRM_NGUOIPHEDUYET = list.Rows[i]["CONFIRM_NGUOIPHEDUYET"].ToString()!,
                    DTM_XACNHAN = list.Rows[i]["DTM_XACNHAN"].ToString()!,
                    DTM_NGUOITHAMTRA = list.Rows[i]["DTM_NGUOITHAMTRA"].ToString()!,
                    DTM_NGUOIPHEDUYET = list.Rows[i]["DTM_NGUOIPHEDUYET"].ToString()!,
                    INT_STEP = list.Rows[i]["INT_STEP"].ToString()!,
                    Code_Request = list.Rows[i]["Code_Request"].ToString()!,
                    Cost_Center = list.Rows[i]["Cost_Center"].ToString()!,
                    Dealine = list.Rows[i]["Dealine"].ToString()!.Split(" ")[0],
                    Total = Math.Round(double.Parse(list.Rows[i]["Total"].ToString()!),2),
                    User_Create = list.Rows[i]["User_Create"].ToString()!,
                    Create_Date = list.Rows[i]["Create_Date"].ToString()!,
                    CHR_TEN_NGUOIYEUCAU = list.Rows[i]["CHR_TEN_NGUOIYEUCAU"].ToString()!,
                    CHR_TEN_NGUOITHAMTRA = list.Rows[i]["CHR_TEN_NGUOITHAMTRA"].ToString()!,
                    CHR_TEN_NGUOIPHEDUYET = list.Rows[i]["CHR_TEN_NGUOIPHEDUYET"].ToString()!,
                    CHR_TEN_XACNHAN = list.Rows[i]["CHR_TEN_XACNHAN"].ToString()!,
                    CHR_TEN_XUATKHO = list.Rows[i]["CHR_TEN_XUATKHO"].ToString()!,
                    CHR_MAIL_NGUOIPHEDUYET = list.Rows[i]["CHR_MAIL_NGUOIPHEDUYET"].ToString()!,
                    CHR_MAIL_NGUOIYEUCAU = list.Rows[i]["CHR_MAIL_NGUOIYEUCAU"].ToString()!,
                    CHR_MAIL_NGUOITHAMTRA = list.Rows[i]["CHR_MAIL_NGUOITHAMTRA"].ToString()!,
                    CHR_ADID_XUATKHO = list.Rows[i]["CHR_ADID_XUATKHO"].ToString()!,
                    Cost_Center_Group = list.Rows[i]["Cost_Center_Group"].ToString()!,
                    Urgent = list.Rows[i]["Urgent"].ToString()!
                });
            }
            return pe_;
        }
        public static List<REQUEST_DETAIL> _get_info_dtrq(string cost_request)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var get_if = _db.GET_DATA_FROM_SQL("select * from REQUEST_DETAIL as a left join DEPARTMENT as b on a.Phongchiuchiphi = b.Cost_Center where a.Code_Request = '" + cost_request + "'");
            var dtm = _db.GET_DATA_FROM_SQL("SELECT * FROM [COST_MANAGEMENT].[dbo].[REQUEST] where Code_Request = '" + cost_request + "'");
            List<REQUEST_DETAIL> rq = new List<REQUEST_DETAIL>();
            for (int i = 0; i < get_if.Rows.Count; i++)
            {
                rq.Add(new REQUEST_DETAIL
                {
                    Id_RequestDetail = int.Parse(get_if.Rows[i]["Id_RequestDetail"].ToString()!),
                    Id_Request = int.Parse(get_if.Rows[i]["Id_Request"].ToString()!),
                    Code_Request = get_if.Rows[i]["Code_Request"].ToString(),
                    Material_Code = get_if.Rows[i]["Material_Code"].ToString(),
                    Material_Name = get_if.Rows[i]["Material_Name"].ToString(),
                    Account_Code = get_if.Rows[i]["Account_Code"].ToString(),
                    Account_Name = get_if.Rows[i]["Account_Name"].ToString(),
                    Unit = get_if.Rows[i]["Unit"].ToString(),
                    Amount = Math.Round(double.Parse(get_if.Rows[i]["Amount"].ToString()!)),
                    Price = Math.Round(double.Parse(get_if.Rows[i]["Price"].ToString()!),2),
                    Total_exchange = Math.Round(double.Parse(get_if.Rows[i]["Total_exchange"].ToString()!)),
                    Currency = get_if.Rows[i]["Currency"].ToString(),
                    Cost_Center_Group = get_if.Rows[i]["Cost_Center_Group"].ToString(),
                    Name_Dept = get_if.Rows[i]["Name"].ToString(),
                    Creat_Date = dtm.Rows[0]["Request_Date"].ToString(),              
                    Dealine = dtm.Rows[0]["Dealine"].ToString(),
                    Cost_Center = dtm.Rows[0]["Cost_Center"].ToString(),
                    Group_Code = dtm.Rows[0]["Group_Code"].ToString(),
                    Urgent = dtm.Rows[0]["Urgent"].ToString(),
                    Aim = get_if.Rows[0]["Aim"].ToString(),
                });
            }
            return rq;
        }
        public static string _update_request(string id_request, string regency, string step)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            _db.GET_DATA_FROM_SQL("update PE_REQUEST_CONFIRM set INT_STEP = '" + step + "' , CONFIRM_" + regency + " = '1', DTM_" + regency + " = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where ID_REQUEST = '" + id_request + "'");
            return "Xác nhận thành công !";
        }
        public static string _update_all(string us)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            db.GET_DATA_FROM_SQL($@"
                                UPDATE [COST_MANAGEMENT].[dbo].[PE_REQUEST_CONFIRM]
                                SET 
                                    -- Cập nhật trạng thái xác nhận: Chỉ chuyển từ '0' sang '1'
                                    [CONFIRM_NGUOIYEUCAU]   = CASE WHEN [CHR_ADID_NGUOIYEUCAU]   = '{us}' AND [CONFIRM_NGUOIYEUCAU]   = '0' THEN '1' ELSE [CONFIRM_NGUOIYEUCAU] END,
                                    [CONFIRM_NGUOITHAMTRA]  = CASE WHEN [CHR_ADID_NGUOITHAMTRA]  = '{us}' AND [CONFIRM_NGUOITHAMTRA]  = '0' THEN '1' ELSE [CONFIRM_NGUOITHAMTRA] END,
                                    [CONFIRM_NGUOIPHEDUYET] = CASE WHEN [CHR_ADID_NGUOIPHEDUYET] = '{us}' AND [CONFIRM_NGUOIPHEDUYET] = '0' THEN '1' ELSE [CONFIRM_NGUOIPHEDUYET] END,
                                    [CONFIRM_XACNHAN]       = CASE WHEN [CHR_ADID_XACNHAN]       = '{us}' AND [CONFIRM_XACNHAN]       = '0' THEN '1' ELSE [CONFIRM_XACNHAN] END,
                                    [CONFIRM_XUATKHO]       = CASE WHEN [CHR_ADID_XUATKHO]       = '{us}' AND [CONFIRM_XUATKHO]       = '0' THEN '1' ELSE [CONFIRM_XUATKHO] END,
    
                                    -- Chỉ tăng step nếu có ít nhất một cột thực sự được cập nhật từ '0' thành '1'
                                    [INT_STEP] = ISNULL([INT_STEP], 0) + 1,

                                    -- Cập nhật ngày giờ xác nhận: Chỉ cập nhật nếu cột confirm tương ứng đang là '0'
                                    [DTM_NGUOIYEUCAU]   = CASE WHEN [CHR_ADID_NGUOIYEUCAU]   = '{us}' AND [CONFIRM_NGUOIYEUCAU]   = '0' THEN GETDATE() ELSE [DTM_NGUOIYEUCAU] END,
                                    [DTM_NGUOITHAMTRA]  = CASE WHEN [CHR_ADID_NGUOITHAMTRA]  = '{us}' AND [CONFIRM_NGUOITHAMTRA]  = '0' THEN GETDATE() ELSE [DTM_NGUOITHAMTRA] END,
                                    [DTM_NGUOIPHEDUYET] = CASE WHEN [CHR_ADID_NGUOIPHEDUYET] = '{us}' AND [CONFIRM_NGUOIPHEDUYET] = '0' THEN GETDATE() ELSE [DTM_NGUOIPHEDUYET] END,
                                    [DTM_XACNHAN]       = CASE WHEN [CHR_ADID_XACNHAN]       = '{us}' AND [CONFIRM_XACNHAN]       = '0' THEN GETDATE() ELSE [DTM_XACNHAN] END,
                                    [DTM_XUATKHO]       = CASE WHEN [CHR_ADID_XUATKHO]       = '{us}' AND [CONFIRM_XUATKHO]       = '0' THEN GETDATE() ELSE [DTM_XUATKHO] END

                                WHERE 
                                    -- Điều kiện lọc: User có tên trong danh sách và cột đó phải đang ở trạng thái '0'
                                    ( [CHR_ADID_NGUOIYEUCAU]   = '{us}' AND [CONFIRM_NGUOIYEUCAU]   = '0' ) OR
                                    ( [CHR_ADID_NGUOITHAMTRA]  = '{us}' AND [CONFIRM_NGUOITHAMTRA]  = '0' ) OR
                                    ( [CHR_ADID_NGUOIPHEDUYET] = '{us}' AND [CONFIRM_NGUOIPHEDUYET] = '0' ) OR
                                    ( [CHR_ADID_XACNHAN]       = '{us}' AND [CONFIRM_XACNHAN]       = '0' ) OR
                                    ( [CHR_ADID_XUATKHO]       = '{us}' AND [CONFIRM_XUATKHO]       = '0' );");
            return "Update thành công !";
        }
        public static List<PE_USERNAME> _load_userinventory(string group_code, string id)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var _get_if = _db.GET_DATA_FROM_SQL("select * from PE_USERNAME where Group_Code like '%" + group_code + "%' and Id_User like N'%" + id + "%' ");
            List<PE_USERNAME> _if = new List<PE_USERNAME>();
            for(int i = 0; i < _get_if.Rows.Count; i++)
            {
                _if.Add(new PE_USERNAME
                {
                    Id_User = int.Parse(_get_if.Rows[i]["Id_User"].ToString()!),
                    User_Name = _get_if.Rows[i]["User_Name"].ToString()!,
                    Mail = _get_if.Rows[i]["Mail"].ToString()!,
                    Adid = _get_if.Rows[i]["Adid"].ToString()!,
                    Group_Code = _get_if.Rows[i]["Group_Code"].ToString()!,
                    Role = _get_if.Rows[i]["Role"].ToString()!,
                });
            }
            return _if;
        }
        public static string _sendmail(string body, string mail_to, string subject)
        {      
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("BIVNWarehouse.sys@brother-bivn.com.vn");
            mail.Subject = subject;
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.brother.co.jp";
            //smtp.EnableSsl = true;
            NetworkCredential networkCredential = new NetworkCredential();
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = networkCredential;
            smtp.Port = 25;
            mail.Body = body;
            mail.To.Add(mail_to);
            smtp.Send(mail);
            return "Gửi mail thành công !";
        }
        public static List<CHITIET_XUATKHO> ct_xk(string mayeucau, string nguoitao)
        { 
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var lst = _db.GET_DATA_FROM_SQL($@"SELECT TOP (2000) * FROM PE_REQUEST_CONFIRM AS a
                        LEFT JOIN REQUEST_DETAIL AS b ON a.ID_REQUEST = b.Id_Request
                        LEFT JOIN PE_REQUEST_INFORMATION AS c ON b.Id_RequestDetail = c.NCHR_REQUEST_CODE
                        WHERE b.Code_Request like '%{mayeucau}%' and CHR_ADID_NGUOITAO like '%{nguoitao}%' and 
                        (a.INT_STEP = '4' OR a.INT_STEP = '5') 
                        AND c.NCHR_REQUEST_CODE IS NULL;");
            List<CHITIET_XUATKHO> ctxk = new List<CHITIET_XUATKHO>();
            for(int i = 0; i < lst.Rows.Count; i++)
            {               
                ctxk.Add(new CHITIET_XUATKHO
                {
                    Id_RequestDetail = int.Parse(lst.Rows[i]["Id_RequestDetail"].ToString()!),
                    Code_Request = lst.Rows[i]["Code_Request"].ToString()!,
                    Id_Request = int.Parse(lst.Rows[i]["Id_Request"].ToString()!),
                    Material_Code = lst.Rows[i]["Material_Code"].ToString()!,
                    Material_Name = lst.Rows[i]["Material_Name"].ToString()!,
                    Account_Code = lst.Rows[i]["Account_Code"].ToString()!,
                    Unit = lst.Rows[i]["Unit"].ToString()!,
                    Amount = lst.Rows[i]["Amount"].ToString()!,
                    Price = Math.Round(decimal.Parse(lst.Rows[i]["Price"].ToString()!),2),
                    Currency = lst.Rows[i]["Currency"].ToString()!,
                    Phongchiuchiphi = lst.Rows[i]["Phongchiuchiphi"].ToString()!,
                    ID = int.Parse(lst.Rows[i]["ID"].ToString()!),
                    CHR_ADID_NGUOIYEUCAU = lst.Rows[i]["CHR_ADID_NGUOIYEUCAU"].ToString()!,         
                    CHR_ADID_XUATKHO = lst.Rows[i]["CHR_ADID_XUATKHO"].ToString()!,
                    QTY_NEED = lst.Rows[i]["CHR_ADID_XUATKHO"].ToString()!,
                    DTM_UPDATE = lst.Rows[i]["CHR_ADID_XUATKHO"].ToString()!,                                                       
                });
            }
            return ctxk;
        }
        public static string _xuatkho(string code_request, string adid_nx, string nguoinhan, string nguoixuatkho, string thoigian, string manguyenlieu, string soluong, string giathucte, string donvi, string kho, string tongchiphi, string vitri, string phong, string khoi, string id_rq)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            try
            {
                // Kiểm tra tồn kho hiện tại trước khi xuất
                string sqlCheckKho = $"SELECT [Hientai] FROM [KHO] WHERE [MaNguyenLieu] = '{manguyenlieu}' AND [Kho] = '{kho}' AND [Group_Code] = '{khoi}'";
                DataTable dtKho = _db.GET_DATA_FROM_SQL(sqlCheckKho);

                if (dtKho.Rows.Count == 0)
                    return " Mã hàng không tồn tại trong kho này.";

                double slHienTai = Convert.ToDouble(dtKho.Rows[0]["Hientai"]);
                double slXuat = Convert.ToDouble(soluong);

                if (slXuat > slHienTai)
                    return $" Kho không đủ. Hiện có: {slHienTai}";

                // Thực hiện trừ kho
                string sqlUpdateKho = $"UPDATE [KHO] SET [Hientai] = [Hientai] - {slXuat} WHERE [MaNguyenLieu] = '{manguyenlieu}' AND [Kho] = '{kho}' AND [Group_Code] = '{khoi}'";
                _db.GET_DATA_FROM_SQL(sqlUpdateKho);

                // Ghi log vào bảng lịch sử KHO_NHAPXUAT

                string hanhdong = $"Xuất kho {kho} cho request: {code_request}";
                string sqlLog = $@"INSERT INTO [KHO_NHAPXUAT] 
                    ([MaNguyenLieu], [Hanhdong], [Soluong], [Loai], [Thoigian], [Nguoicapnhat], [Kho], [Khoi], [Phong], [Vitri], [Ngaynhaokho], [Soluongtruocthaydoi], [Soluongsauthaydoi])
                    VALUES ('{manguyenlieu}', '{hanhdong}', '{slXuat}', 'XUAT', GETDATE(), '{nguoixuatkho}', '{kho}', '{khoi}', '{phong}', N'{vitri}', '{thoigian}', '{slHienTai}', '{slHienTai - slXuat}')";

                _db.GET_DATA_FROM_SQL(sqlLog);
          
                // Cập nhật trạng thái trong REQUEST_DETAIL
                string sqlUpdateDetail = $@"UPDATE REQUEST_DETAIL SET 
                            [Amount_Real] = '{slXuat}', 
                            [Price_Real] = '{giathucte}',
                            [Total_exchange_real] = '{tongchiphi}',
                            [Status] = 'DONE',
                            [Last_Update] = GETDATE(),
                            [User_Update] = '{nguoixuatkho}'
                            WHERE [Code_Request] = '{code_request}' AND [Material_Code] = '{manguyenlieu}'";

                _db.GET_DATA_FROM_SQL(sqlUpdateDetail);

                string UpdateRequest = "";
                UpdateRequest = UpdateRequest + "UPDATE [REQUEST] SET [Total_exchange_real] = '" + tongchiphi + "'";
                UpdateRequest = UpdateRequest + ",[Exchange_rate_Real] = '" + giathucte +"'";
                UpdateRequest = UpdateRequest + ",[Currency_Real] = 'USD'";
                UpdateRequest = UpdateRequest + ",[Total_Real] = '" + tongchiphi + "' ,[Status] = 'PROGRESS' ";
                UpdateRequest = UpdateRequest + ",[Last_Update] = GETDATE(),[User_Update]='" + nguoinhan + "'";
                UpdateRequest = UpdateRequest + ",[Freeze] = NULL WHERE [Code_Request] = '" + code_request + "'";

                _db.GET_DATA_FROM_SQL(UpdateRequest);

                CheckDone(code_request, id_rq);
                return "OK";

            }
            catch (Exception ex)
            {
                return "ERR: " + ex.Message;
            }
        }
        public static void CheckDone(string Code_Request, string id_rq)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            bool check = true;
            var dte = _db.GET_DATA_FROM_SQL("SELECT [Status] FROM [REQUEST_DETAIL] WHERE [Code_Request] = '" + Code_Request + "' ");
            for(int i = 0; i < dte.Rows.Count; i++)
            {
                if (dte.Rows[i]["Status"].ToString()!.Trim() != "DONE")
                {
                    check = false;
                }
            }    
            if (check == true)
            {
                _db.GET_DATA_FROM_SQL("Update [PE_REQUEST_CONFIRM] set INT_STEP = '7' where ID_REQUEST = '" + id_rq + "'");
                _db.GET_DATA_FROM_SQL("UPDATE [REQUEST] SET [Status] = 'DONE' WHERE [Code_Request] = '" + Code_Request + "' ");
            }
        }
        public static List<PE_REQUEST_CONFIRM> _load_tonkhoxuathang(string madonhang, string nguoitao, string khoi)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            List<PE_REQUEST_CONFIRM> pe_ = new List<PE_REQUEST_CONFIRM>();
           
            var list = _db.GET_DATA_FROM_SQL($@"select b.*,a.ID_REQUEST from [PE_REQUEST_CONFIRM] as a 
                        left join REQUEST as b on a.ID_REQUEST = b.Id_Request 
                        left join DEPARTMENT as c on b.Cost_Center = c.Cost_Center 
                        WHERE (a.INT_STEP = '4' OR a.INT_STEP = '5')  and Code_Request like '%{madonhang}%' and b.Status <> 'DONE' and CHR_ADID_NGUOITAO like '%{nguoitao}%'
                        and b.Group_Code like '%{khoi}%' order by ID desc");

            for (int i = 0; i < list.Rows.Count; i++)
            {
                pe_.Add(new PE_REQUEST_CONFIRM
                {                 
                    Code_Request = list.Rows[i]["Code_Request"].ToString()!,
                    Cost_Center = list.Rows[i]["Cost_Center"].ToString()!,
                    Request_Date = list.Rows[i]["Request_Date"].ToString()!,
                    Declaration = list.Rows[i]["Declaration"].ToString()!,
                    Dealine = list.Rows[i]["Dealine"].ToString()!,
                    Total_exchange = Math.Round(double.Parse(list.Rows[i]["Total_exchange"].ToString()!),2),
                    Total = Math.Round(double.Parse(list.Rows[i]["Total"].ToString()!),2),
                    Kind = list.Rows[i]["Kind"].ToString()!,
                    Type = list.Rows[i]["Type"].ToString()!,
                    Status = list.Rows[i]["Status"].ToString()!,
                    Create_Date = list.Rows[i]["Create_Date"].ToString()!,
                    User_Create = list.Rows[i]["User_Create"].ToString()!,
                    Last_Update = list.Rows[i]["Last_Update"].ToString()!,
                    User_Update = list.Rows[i]["User_Update"].ToString()!,
                    Group_Code = list.Rows[i]["Group_Code"].ToString()!,
                    Urgent = list.Rows[i]["Urgent"].ToString()!,
                    ID_REQUEST = int.Parse(list.Rows[i]["ID_REQUEST"].ToString()!),
                });
            }
            return pe_;
        }
        public static List<REQUEST_DETAIL> _load_body_detail(string code_request)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            List<REQUEST_DETAIL> rq_lst = new List<REQUEST_DETAIL>();
            var lst = db.GET_DATA_FROM_SQL("SELECT * FROM [REQUEST_DETAIL] WHERE [Code_Request] = '" + code_request + "' and [Status] <> 'DONE'");
            for(int i = 0; i < lst.Rows.Count; i++)
            {
                var list = db.GET_DATA_FROM_SQL("select Hientai, Kho from KHO where MaNguyenLieu = '" + lst.Rows[i]["Material_Code"].ToString() + "' and Hientai <> '0' ");
                List<SOLUONGKHO> sl = new List<SOLUONGKHO>();
                for(int a  = 0; a < list.Rows.Count; a++)
                {
                    sl.Add(new SOLUONGKHO
                    {
                        tenkho = list.Rows[a]["Kho"].ToString(),
                        soluong = list.Rows[a]["Hientai"].ToString()
                    });
                }    
                rq_lst.Add(new REQUEST_DETAIL
                {
                    Id_RequestDetail = int.Parse(lst.Rows[i]["Id_RequestDetail"].ToString()!),
                    Code_Request = lst.Rows[i]["Code_Request"].ToString()!,
                    Material_Code = lst.Rows[i]["Material_Code"].ToString()!,
                    Material_Name = lst.Rows[i]["Material_Name"].ToString()!,
                    Material_Name_EN = lst.Rows[i]["Material_Name_EN"].ToString()!,
                    Material_Name_ENJP = lst.Rows[i]["Material_Name_ENJP"].ToString()!,
                    Brand = lst.Rows[i]["Brand"].ToString()!,
                    Good_Code = lst.Rows[i]["Good_Code"].ToString()!,
                    Account_Code = lst.Rows[i]["Account_Code"].ToString()!,
                    Account_Name = lst.Rows[i]["Account_Name"].ToString()!,
                    Amount = double.Parse(lst.Rows[i]["Amount"].ToString()!),
                    Amount_Real = double.Parse(lst.Rows[i]["Amount_Real"].ToString()!),
                    Unit = lst.Rows[i]["Unit"].ToString()!,
                    Unit_Note = lst.Rows[i]["Unit_Note"].ToString()!,
                    Price = Math.Round(double.Parse(lst.Rows[i]["Price"].ToString()!),2),
                    Price_Real = lst.Rows[i]["Price_Real"].ToString()!,
                    VAT = lst.Rows[i]["VAT"].ToString()!,
                    Total_exchange = double.Parse(lst.Rows[i]["Total_exchange"].ToString()!),
                    Total_exchange_real = lst.Rows[i]["Total_exchange_real"].ToString()!,
                    PO = lst.Rows[i]["PO"].ToString()!,
                    Dealine_Real = lst.Rows[i]["Dealine_Real"].ToString()!,
                    Aim = lst.Rows[i]["Aim"].ToString()!,
                    Phongchiuchiphi = lst.Rows[i]["Phongchiuchiphi"].ToString()!,
                    Vitri = lst.Rows[i]["Vitri"].ToString()!,
                    Last_Update = lst.Rows[i]["Last_Update"].ToString()!,
                    User_Update = lst.Rows[i]["User_Update"].ToString()!,
                    Status = lst.Rows[i]["Status"].ToString()!,
                    MaHangTem = lst.Rows[i]["MaHangTem"].ToString()!,
                    slk = sl
                }); 
            }
            return rq_lst;
        }
        public static List<REQUEST> _load_request(string code_request)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            List<REQUEST> rq_lst = new List<REQUEST>();
          
            var lst = db.GET_DATA_FROM_SQL("select Code_Request,a.Cost_Center, Request_Date, Declaration, Dealine,Dealine_Real, Total_exchange, Exchange_rate, Currency, Total, Total_exchange_real,Exchange_rate_Real,Currency_Real, Total_Real,Kind,[Type],Status,Create_Date, User_Create, Last_Update,User_Update,Reason,Place, Loaihinhtokhai, Phuongthucvanchuyen, Group_Code, b.[Name], Name_Jp,Cost_Center_Group,Note, Urgent from [REQUEST] as a left join DEPARTMENT as b on a.Cost_Center =  b.Cost_Center where a.Code_Request = '" + code_request + "'");
            for (int i = 0; i < lst.Rows.Count; i++)
            {
                var ten = db.GET_DATA_FROM_SQL(" SELECT [FULLNAME] FROM [TM_USER] WHERE [CHR_USERID] = '" + lst.Rows[i]["User_Create"].ToString() + "'");
                rq_lst.Add(new REQUEST
                {
                    Code_Request = lst.Rows[i]["Code_Request"].ToString(),
                    Cost_Center = lst.Rows[i]["Cost_Center"].ToString(),
                    Request_Date = lst.Rows[i]["Request_Date"].ToString(),
                    Declaration = lst.Rows[i]["Declaration"].ToString(),
                    Dealine = lst.Rows[i]["Dealine"].ToString(),
                    Dealine_Real = lst.Rows[i]["Dealine_Real"].ToString(),
                    Total_exchange = double.Parse(lst.Rows[i]["Total_exchange"].ToString()!),
                    Exchange_rate = lst.Rows[i]["Exchange_rate"].ToString()!,
                    Currency =  lst.Rows[i]["Currency"].ToString(),
                    Total = double.Parse(lst.Rows[i]["Total"].ToString()!),
                    Total_exchange_real = lst.Rows[i]["Total_exchange_real"].ToString()!,
                    Exchange_rate_Real = lst.Rows[i]["Exchange_rate_Real"].ToString()!,
                    Currency_Real = lst.Rows[i]["Currency_Real"].ToString(),
                    Total_Real = lst.Rows[i]["Total_Real"].ToString(),
                    Kind = lst.Rows[i]["Kind"].ToString(),
                    Typee = lst.Rows[i]["Type"].ToString(),
                    Status = lst.Rows[i]["Status"].ToString(),
                    Create_Date = lst.Rows[i]["Create_Date"].ToString(),
                    User_Create = lst.Rows[i]["User_Create"].ToString() + " (" + ten.Rows[0][0].ToString() + ")",
                    Last_Update = lst.Rows[i]["Last_Update"].ToString(),
                    User_Update = lst.Rows[i]["User_Update"].ToString(),
                    Reason = lst.Rows[i]["Reason"].ToString(),
                    Place = lst.Rows[i]["Place"].ToString(),
                    Loaihinhtokhai = lst.Rows[i]["Loaihinhtokhai"].ToString(),
                    Phuongthucvanchuyen = lst.Rows[i]["Phuongthucvanchuyen"].ToString(),
                    Group_Code = lst.Rows[i]["Group_Code"].ToString(),
                    Name = lst.Rows[i]["Name"].ToString(),
                    Name_Jp = lst.Rows[i]["Name_Jp"].ToString(),
                    Cost_Center_Group = lst.Rows[i]["Cost_Center_Group"].ToString(),
                    Note = lst.Rows[i]["Note"].ToString(),
                    Urgent = lst.Rows[i]["Urgent"].ToString(),
                });
            }
            return rq_lst;

        }
       
    }
}
