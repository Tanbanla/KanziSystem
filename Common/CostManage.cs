using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace PRJ_WAREHOUSE_BIVN.Common
{
    public static class CostManage
    {
        public static Dictionary<string, double> Ketqua_Chiphi;

        public static Models.SQL_Connect_DB20 conn = new Models.SQL_Connect_DB20();
        public static DateTime DateTimeParse(string datetime, string format)
        {
            if (datetime.Length > 10)
                datetime = datetime.Substring(0, 10);
            format = "yyyy-MM-dd";
            return DateTime.ParseExact(datetime, format, (IFormatProvider)CultureInfo.InvariantCulture);
        }

        public static double Tinhdutoanconlai(string Cost_Center, string Caldate, string KindofDeclaration, string Code_Request)
        {
            string str1 = "";
            string str2 = "";
            if (Code_Request != "")
            {
                str1 = " AND [Code_Request] <> '" + Code_Request + "' ";
                str2 = " AND [MaDon] <> '" + Code_Request + "' ";
            }
            CostManage.Ketqua_Chiphi = new Dictionary<string, double>();
            double num1 = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = 0.0;
            double num5 = 0.0;
            double num6 = 0.0;
            double num7 = 0.0;
            double num8 = 0.0;
            if (Cost_Center != "" & KindofDeclaration != "")
            {
                string[] strArray1 = new string[9]
          {
        "SELECT Money_ACC FROM [ESTIMATE] WHERE [Cost_Center] = '",
        Cost_Center,
        "' AND Month([Month]) = '",
        CostManage.DateTimeParse(Caldate, "dd/MM/yyyy").ToString("MM"),
        "' AND Year([Month]) = '",
        null,
        null,
        null,
        null
          };
                string[] strArray2 = strArray1;
                int index1 = 5;
                DateTime dateTime = CostManage.DateTimeParse(Caldate, "dd/MM/yyyy");
                string str3 = dateTime.ToString("yyyy");
                strArray2[index1] = str3;
                strArray1[6] = "' AND Kind = '";
                strArray1[7] = KindofDeclaration;
                strArray1[8] = "' ";
                string s = conn.ReturnString(string.Concat(strArray1));
                string[] strArray3 = new string[9];
                strArray3[0] = "SELECT SUM([Money]) FROM [ESTIMATE_CHANGE] WHERE Cost_Center = '";
                strArray3[1] = Cost_Center;
                strArray3[2] = "' AND Month([Month]) = '";
                string[] strArray4 = strArray3;
                int index2 = 3;
                dateTime = CostManage.DateTimeParse(Caldate, "dd/MM/yyyy");
                string str4 = dateTime.ToString("MM");
                strArray4[index2] = str4;
                strArray3[4] = "' AND Year([Month]) = '";
                string[] strArray5 = strArray3;
                int index3 = 5;
                dateTime = CostManage.DateTimeParse(Caldate, "dd/MM/yyyy");
                string str5 = dateTime.ToString("yyyy");
                strArray5[index3] = str5;
                strArray3[6] = "' AND Kind = '";
                strArray3[7] = KindofDeclaration;
                strArray3[8] = "' AND [NamThang] = 'PBDC'";
                string str6 = conn.ReturnString(string.Concat(strArray3));
                if (str6 != "")
                    num2 = Convert.ToDouble(str6);
                if (s != "")
                    num1 = Convert.ToDouble(double.Parse(s).ToString("N2"));
                string[] strArray6 = new string[9];
                strArray6[0] = " SELECT SUM(Total_Real) FROM [COSTCENTER_MONTH_TOTAL_ESTIMATE] WHERE [Phongchiuchiphi] = '";
                strArray6[1] = Cost_Center;
                strArray6[2] = "' AND Year(Dealine_Real) = '";
                string[] strArray7 = strArray6;
                int index4 = 3;
                dateTime = DateTimeParse(Caldate, "dd/MM/yyyy");
                string str7 = dateTime.ToString("yyyy");
                strArray7[index4] = str7;
                strArray6[4] = "' AND Month(Dealine_Real) = '";
                string[] strArray8 = strArray6;
                int index5 = 5;
                dateTime = DateTimeParse(Caldate, "dd/MM/yyyy");
                string str8 = dateTime.ToString("MM");
                strArray8[index5] = str8;
                strArray6[6] = "' AND [Declaration] = '";
                strArray6[7] = KindofDeclaration;
                strArray6[8] = "' AND ([Tinhchiphichi] <> 'False' Or Tinhchiphichi is null) AND [StatusTotal] IN ('PROGRESS','DONE')   ";
                string str9 = conn.ReturnString(string.Concat(strArray6));
                if (str9 != "")
                    num3 = Convert.ToDouble(str9);
                string[] strArray9 = new string[9];
                strArray9[0] = "SELECT SUM([First]) as [First],SUM([Last]) as [Last] FROM [REMAINDER] WHERE [Dept] = '";
                strArray9[1] = Cost_Center;
                strArray9[2] = "' AND Year([Month]) = '";
                string[] strArray10 = strArray9;
                int index6 = 3;
                dateTime = DateTimeParse(Caldate, "dd/MM/yyyy");
                string str10 = dateTime.ToString("yyyy");
                strArray10[index6] = str10;
                strArray9[4] = "' AND Month([Month]) = '";
                string[] strArray11 = strArray9;
                int index7 = 5;
                dateTime = DateTimeParse(Caldate, "dd/MM/yyyy");
                string str11 = dateTime.ToString("MM");
                strArray11[index7] = str11;
                strArray9[6] = "'  AND [Kind] = '";
                strArray9[7] = KindofDeclaration;
                strArray9[8] = "' ";
                System.Data.DataTable dataTable = conn.Getdatatable(string.Concat(strArray9), "tk");
                if (dataTable.Rows.Count > 0)
                {
                    double num9 = !(dataTable.Rows[0]["First"].ToString() != "") ? num3 : Convert.ToDouble(dataTable.Rows[0]["First"].ToString()) + num3;
                    if (dataTable.Rows[0]["Last"].ToString() != "")
                        num9 -= Convert.ToDouble(dataTable.Rows[0]["Last"].ToString());
                    num3 = num9;
                }
                string[] strArray12 = new string[11];
                strArray12[0] = "SELECT SUM(Total - ISNULL(Total_Real,0)) as [Order] FROM [COSTCENTER_MONTH_TOTAL_ESTIMATE] WHERE [Phongchiuchiphi] = '";
                strArray12[1] = Cost_Center;
                strArray12[2] = "' ";
                strArray12[3] = str1;
                strArray12[4] = " AND Month([Dealine]) = '";
                string[] strArray13 = strArray12;
                int index8 = 5;
                dateTime = DateTimeParse(Caldate, "dd/MM/yyyy");
                string str12 = dateTime.ToString("MM");
                strArray13[index8] = str12;
                strArray12[6] = "' AND Year([Dealine]) = '";
                string[] strArray14 = strArray12;
                int index9 = 7;
                dateTime = DateTimeParse(Caldate, "dd/MM/yyyy");
                string str13 = dateTime.ToString("yyyy");
                strArray14[index9] = str13;
                strArray12[8] = "' AND [Declaration] = '";
                strArray12[9] = KindofDeclaration;
                strArray12[10] = "' AND ([Tinhchiphichi] <> 'False' Or Tinhchiphichi is null) AND [StatusTotal]  = 'PROGRESS' ";
                string str14 = conn.ReturnString(string.Concat(strArray12));
                if (str14 != "")
                    num4 = Convert.ToDouble(str14);
                string[] strArray15 = new string[11];
                strArray15[0] = "SELECT SUM(Total) as [Order] FROM [COSTCENTER_MONTH_TOTAL_ESTIMATE] WHERE [Phongchiuchiphi] = '";
                strArray15[1] = Cost_Center;
                strArray15[2] = "' ";
                strArray15[3] = str1;
                strArray15[4] = "  AND Month([Dealine]) = '";
                string[] strArray16 = strArray15;
                int index10 = 5;
                dateTime = DateTimeParse(Caldate, "dd/MM/yyyy");
                string str15 = dateTime.ToString("MM");
                strArray16[index10] = str15;
                strArray15[6] = "' AND Year([Dealine]) = '";
                string[] strArray17 = strArray15;
                int index11 = 7;
                dateTime = DateTimeParse(Caldate, "dd/MM/yyyy");
                string str16 = dateTime.ToString("yyyy");
                strArray17[index11] = str16;
                strArray15[8] = "' AND [Declaration] = '";
                strArray15[9] = KindofDeclaration;
                strArray15[10] = "' AND ([Tinhchiphichi] <> 'False' Or Tinhchiphichi is null) AND [StatusTotal] = 'ACCEPT'   ";
                string str17 = conn.ReturnString(string.Concat(strArray15));
                if (str17 != "")
                    num5 = Convert.ToDouble(str17);
                string[] strArray18 = new string[11];
                strArray18[0] = "SELECT SUM(Total) as [Order] FROM [COSTCENTER_MONTH_TOTAL_ESTIMATE] WHERE [Phongchiuchiphi] = '";
                strArray18[1] = Cost_Center;
                strArray18[2] = "' ";
                strArray18[3] = str1;
                strArray18[4] = " AND Month([Dealine]) = '";
                string[] strArray19 = strArray18;
                int index12 = 5;
                dateTime = DateTimeParse(Caldate, "dd/MM/yyyy");
                string str18 = dateTime.ToString("MM");
                strArray19[index12] = str18;
                strArray18[6] = "' AND Year([Dealine]) = '";
                string[] strArray20 = strArray18;
                int index13 = 7;
                dateTime = DateTimeParse(Caldate, "dd/MM/yyyy");
                string str19 = dateTime.ToString("yyyy");
                strArray20[index13] = str19;
                strArray18[8] = "' AND [Declaration] = '";
                strArray18[9] = KindofDeclaration;
                strArray18[10] = "' AND ([Tinhchiphichi] <> 'False' Or Tinhchiphichi is null) AND [StatusTotal] in ('WAITCONFIRM','EXPORT')   ";
                string str20 = conn.ReturnString(string.Concat(strArray18));
                if (str20 != "")
                    num6 = Convert.ToDouble(str20);
                string[] strArray21 = new string[9]
          {
        "SELECT SUM(Total) FROM [OUT_INPUT_ACCOUNT] WHERE Cost_Center = '",
        Cost_Center,
        "' AND Declaration = '",
        KindofDeclaration,
        "' AND Year(Request_Date) = '",
        null,
        null,
        null,
        null
          };
                string[] strArray22 = strArray21;
                int index14 = 5;
                dateTime = DateTimeParse(Caldate, "dd/MM/yyyy");
                string str21 = dateTime.ToString("yyyy");
                strArray22[index14] = str21;
                strArray21[6] = "' AND Month(Request_Date) = '";
                string[] strArray23 = strArray21;
                int index15 = 7;
                dateTime = DateTimeParse(Caldate, "dd/MM/yyyy");
                string str22 = dateTime.ToString("MM");
                strArray23[index15] = str22;
                strArray21[8] = "' AND ([Tinhchiphichi] <> 'False' Or Tinhchiphichi is null) ";
                string str23 = conn.ReturnString(string.Concat(strArray21));
                if (str23 != "")
                    num7 = Convert.ToDouble(str23);
                string[] strArray24 = new string[11]
          {
        "SELECT SUM(SoTienUSD) FROM [V2_FORM_ALL] WHERE [TinhTrang] <> 'REFUSE' AND [Phongchiuchiphi] = '",
        Cost_Center,
        "' ",
        str2,
        " AND LoaiChiPhi = '",
        KindofDeclaration,
        "' AND Year(ThangTinhChiPhi) = '",
        null,
        null,
        null,
        null
          };
                string[] strArray25 = strArray24;
                int index16 = 7;
                dateTime = DateTimeParse(Caldate, "dd/MM/yyyy");
                string str24 = dateTime.ToString("yyyy");
                strArray25[index16] = str24;
                strArray24[8] = "' AND Month(ThangTinhChiPhi) = '";
                string[] strArray26 = strArray24;
                int index17 = 9;
                dateTime = DateTimeParse(Caldate, "dd/MM/yyyy");
                string str25 = dateTime.ToString("MM");
                strArray26[index17] = str25;
                strArray24[10] = "' AND ([Tinhchiphichi] <> 'False' Or Tinhchiphichi is null)";
                string str26 = conn.ReturnString(string.Concat(strArray24));
                if (str26 != "")
                    num8 = Convert.ToDouble(str26);
            }
            Ketqua_Chiphi.Add("Dutoan", num1);
            Ketqua_Chiphi.Add("Phongbandieuchinh", num2);
            Ketqua_Chiphi.Add("Dasudung", num3);
            Ketqua_Chiphi.Add("Chuaxuatkhohet", num4);
            Ketqua_Chiphi.Add("Choxuatkho", num5);
            Ketqua_Chiphi.Add("Choxacnhan", num6);
            Ketqua_Chiphi.Add("Outinput", num7);
            Ketqua_Chiphi.Add("Xinchiphi", num8);
            double num10 = Math.Round(num1 - num3 - num4 - num5 - num7 - num6 - num8 + num2, 2);
            Ketqua_Chiphi.Add("Tong", num10);
            return num10;
        }

    }
}
