using Microsoft.AspNetCore.Http.HttpResults;
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
        public decimal? Amount { get; set; }
        public decimal? Price { get; set; }
        public decimal? Price_Real { get; set; }
        public decimal? Total_exchange { get; set; }
        public int? Rate { get; set; }
        public string? Currency { get; set; }
        public decimal? Total { get; set; }
        public string? Amount_Real { get; set; }
        public decimal? VAT { get; set; }
        public decimal? Total_exchange_real { get; set; }
        public int? Rate_Real { get; set; }
        public string? Currency_Real { get; set; }
        public decimal? Total_Real { get; set; }
        public DateTime? Dealine_Real { get; set; }
        public string? Poisition { get; set; }
        public string? Aim { get; set; }
        public string? Status { get; set; }
        public DateTime? Last_Update { get; set; }
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
        public string? Urgent { get; set; }

    }
    public class REQUEST
    {
        public int Id_Request { get; set; }
        public string? Code_Request { get; set; }
        public string? Cost_Center { get; set; }
        public DateTime? Request_Date { get; set; }
        public string? Declaration { get; set; }
        public DateTime? Dealine { get; set; }
        public DateTime? Dealine_Real { get; set; }
        public decimal? Total_exchange { get; set; }
        public decimal? Exchange_rate { get; set; }
        public string? Currency { get; set; }
        public decimal? Total { get; set; }
        public decimal? Total_exchange_real { get; set; }
        public decimal? Exchange_rate_Real { get; set; }
        public string? Currency_Real { get; set; }
        public decimal? Total_Real { get; set; }
        public string? Kind { get; set; }
        public string? Type { get; set; }
        public int? Status { get; set; }
        public DateTime? Create_Date { get; set; }
        public string? User_Create { get; set; }
        public DateTime? Last_Update { get; set; }
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
        public bool? Urgent { get; set; }
        public string? CostCenter { get; set; }
        public List<REQUEST_DETAIL>? rq_dt { get;set; }

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
        public DateTime? Request_Date { get; set; }
        public string? Declaration { get; set; }
        public string? Dealine { get; set; }
        public DateTime? Dealine_Real { get; set; }
        public decimal? Total_exchange { get; set; }
        public decimal? Exchange_rate { get; set; }
        public string? Currency { get; set; }
        public decimal? Total { get; set; }
        public decimal? Total_exchange_real { get; set; }
        public decimal? Exchange_rate_Real { get; set; }
        public string? Currency_Real { get; set; }
        public decimal? Total_Real { get; set; }
        public string? Kind { get; set; }
        public string? Type { get; set; }
        public int? Status { get; set; }
        public string? Create_Date { get; set; }
        public string? User_Create { get; set; }
        public DateTime? Last_Update { get; set; }
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
    public class REQUEST_PROCESS
    {
        public static string Insert_request(string Cost_Center, string Declaration, string Dealine, string Total_exchange, string Exchange_rate, string Currency, string Total, string Kind, string Type, string Status, string Place, string Loaihinhtokhai, string Group_Code, string Chophepin, string Urgent, string User_Create, List<REQUEST_DETAIL>? rq_dt, string adid_dt, string adid_tt, string adid_pd, string mail_dt, string mail_tt, string mail_pd, string ten_dt, string ten_tt, string ten_pd, string ten_dy, string adid_dy, string mail_dy, string ten_xk, string adid_xk, string mail_xk)
        {
            try
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
                        @NewCode, '{Cost_Center}', '{DateTime.Now}', N'{Declaration}', '{Dealine}', ROUND({Total_exchange},2), '{Exchange_rate}', '{Currency}', ROUND({Total},2),  
                        '{Kind}', '{Type}', '{Status}','{DateTime.Now}', '{User_Create}', '{Place}', '{Loaihinhtokhai}', '{Group_Code}', '{Chophepin}', '{Urgent}' 
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
                _insert_request_confirm(newId, adid_dt, adid_tt, adid_pd, mail_dt, mail_tt, mail_pd, ten_dt, ten_tt, ten_pd, ten_dy.Split('_')[1], adid_dy, mail_dy, ten_xk.Split('_')[1], adid_xk, mail_xk);

                return "Thêm thành công !";
            }
            catch (Exception ex)
            {
                // Log lỗi ex.Message nếu cần
                return "NG";
            }
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
        public static string _insert_request_confirm(string id_request, string adid_dt, string adid_tt, string adid_pheduyet, string mail_dt, string mail_tt, string mail_pd, string ten_dt, string ten_tt, string ten_pd, string ten_dy, string adid_dy, string mail_dy, string ten_xk, string adid_xk, string mail_xk)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            _db.GET_DATA_FROM_SQL("insert into [PE_REQUEST_CONFIRM] (ID_REQUEST,CHR_ADID_NGUOIYEUCAU,CHR_ADID_NGUOITHAMTRA,CHR_ADID_NGUOIPHEDUYET,CHR_ADID_XACNHAN, DTM_XACNHAN, INT_STEP,CHR_MAIL_NGUOIYEUCAU,CHR_MAIL_NGUOITHAMTRA,CHR_MAIL_NGUOIPHEDUYET,CHR_TEN_NGUOIYEUCAU,CHR_TEN_NGUOITHAMTRA,CHR_TEN_NGUOIPHEDUYET,CHR_ADID_XUATKHO,CHR_MAIL_XUATKHO,CHR_TEN_XUATKHO,CHR_TEN_XACNHAN,CHR_MAIL_XACNHAN,CONFIRM_NGUOIYEUCAU,CONFIRM_NGUOITHAMTRA,CONFIRM_NGUOIPHEDUYET,CONFIRM_XACNHAN) " +
                "VALUES ('" + id_request + "','" + adid_dt + "','" + adid_tt + "','" + adid_pheduyet + "','" + adid_dy + "','" + DateTime.Now + "','0','" + mail_dt + "','" + mail_tt + "','" + mail_pd + "',N'" + ten_dt + "',N'" + ten_tt + "',N'" + ten_pd + "','" + adid_xk + "','" + mail_xk + "',N'" + ten_xk + "',N'" + ten_dy + "','" + mail_dy + "','0','0','0','0')");
            return "OK";
        }
        public static List<PE_REQUEST_CONFIRM> get_requestconfirm( string us)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            List<PE_REQUEST_CONFIRM> pe_ = new List<PE_REQUEST_CONFIRM>();
            var list = _db.GET_DATA_FROM_SQL(" select top (500) * from [PE_REQUEST_CONFIRM] as a left join REQUEST as b on a.ID_REQUEST = b.Id_Request left join DEPARTMENT as c on b.Cost_Center = c.Cost_Center where ((CHR_ADID_NGUOIYEUCAU = '" + us +"' and CONFIRM_NGUOIYEUCAU = '0') OR (CHR_ADID_NGUOITHAMTRA = '" + us +"' and CONFIRM_NGUOITHAMTRA = '0') OR (CHR_ADID_NGUOIPHEDUYET = '" + us +"' and CONFIRM_NGUOIPHEDUYET = '0') OR (CHR_ADID_XACNHAN = '" + us +"' and CONFIRM_XACNHAN = '0')) and INT_STEP < 5 ");
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
                    Total = decimal.Parse(list.Rows[i]["Total"].ToString()!),
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
        public static List<PE_REQUEST_CONFIRM> get_requestcondition()
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            List<PE_REQUEST_CONFIRM> pe_ = new List<PE_REQUEST_CONFIRM>();
            var list = _db.GET_DATA_FROM_SQL("select top (1000) * from [PE_REQUEST_CONFIRM] as a left join REQUEST as b on a.ID_REQUEST = b.Id_Request left join DEPARTMENT as c on b.Cost_Center = c.Cost_Center");
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
                    Dealine = list.Rows[i]["Dealine"].ToString()!,
                    Total = decimal.Parse(list.Rows[i]["Total"].ToString()!),
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
            var get_if = _db.GET_DATA_FROM_SQL("  select * from REQUEST_DETAIL as a left join DEPARTMENT as b on a.Phongchiuchiphi = b.Cost_Center where a.Code_Request = '" + cost_request + "'");
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
                    Amount = decimal.Parse(get_if.Rows[i]["Amount"].ToString()!),
                    Price = decimal.Parse(get_if.Rows[i]["Price"].ToString()!),
                    Total_exchange = decimal.Parse(get_if.Rows[i]["Total_exchange"].ToString()!),
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
            _db.GET_DATA_FROM_SQL("update PE_REQUEST_CONFIRM set INT_STEP = '" + step + "' , CONFIRM_" + regency + " = '1', DTM_" + regency + " = '" + DateTime.Now + "' where ID_REQUEST = '" + id_request + "'");
            return "Xác nhận thành công !";
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
        public static List<CHITIET_XUATKHO> ct_xk()
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var lst = _db.GET_DATA_FROM_SQL(@"SELECT TOP (2000) * FROM PE_REQUEST_CONFIRM AS a
                        LEFT JOIN REQUEST_DETAIL AS b ON a.ID_REQUEST = b.Id_Request
                        LEFT JOIN PE_REQUEST_INFORMATION AS c ON b.Id_RequestDetail = c.NCHR_REQUEST_CODE
                        WHERE 
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
        public static string _xuatkho(string code_request, string adid_nx, string manguyenlieu, string soluong, string donvi, string kho, string us)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var ins = _db.GET_DATA_FROM_SQL($"Insert into [PE_REQUEST_INFORMATION] (NCHR_REQUEST_CODE,NCHR_EMPLOYEE_ADID,NCHR_MATERIAL_CODE,QTY_NEED,NCHR_UNIT,NCHR_WAREHOUSE_NAME,DTM_UPDATE,NCHAR_USER) " +
                $"values ('{code_request}','{adid_nx}', '{manguyenlieu}','{soluong}',N'{donvi}', '{kho}','{DateTime.Now}', N'{us}')");
            _db.GET_DATA_FROM_SQL("Update PE_REQUEST_CONFIRM set INT_STEP = '5' where ID_REQUEST = '" + code_request + "'");

            _db.GET_DATA_FROM_SQL($"Insert into (MaNguyenLieu,Hanhdong,Soluong,Loai,Ngaynhaokho,Thoigian,Nguoicapnhat,Kho,Khoi,Phong,Vitri) " +
                $"values ('{manguyenlieu}', 'Xuất kho {kho} cho request')");
            return "Thêm thành công !";
        }
    }
}
