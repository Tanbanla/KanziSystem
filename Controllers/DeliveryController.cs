using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using PRJ_WAREHOUSE_BIVN.Models;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Data;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Globalization;
using System.Transactions;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class SearchPoPayload
    {
        public string? PoNumber { get; set; }
        public string? Department { get; set; }
        public string? Mayeucau { get; set; }
        public string? Mahang { get; set; }
        public string? Phongbanyeucau { get; set; }
        public string? UserName { get; set; }
    }

    public class ExchangeUnit
    {
        public string? materialCode { get; set; }
        public string? poUnit { get; set; }
        public string? convertedUnit { get; set; }
        public string? conversionQty { get; set; }
        public string? deptUnit { get; set; }
    }

    public class ConfirmImport
    {
        public string? benXacNhanTruoc { get; set; }
        public string? PO_Detail_Id { get; set; }
        public string? luongvethuctekho { get; set; }
        public string? NgayNhap { get; set; }
        public string? KhoNhan { get; set; }
        public string? Id_nhapkho { get; set; }
        public string? Mahang { get; set; }
        public string? Soluong { get; set; }
        public string? Group_Code { get; set; }
        public string? UserName { get; set; }
        public string? Donvi { get; set; }
        public string? Id_Lichsu { get; set; }
        public string? Id_Goc { get; set; }
    }

    public class DeliveryController : Controller
    {
        public IActionResult DeliveryPO()
        {
            return View();
        }
        public IActionResult DeliverySection()
        {
            return View();
        }
        public IActionResult PO_Info()
        {
            return View();
        }
        public IActionResult Giaohang()
        {
            return View();
        }
        private void UpdateTinhTrangPO(string PO)
        {
            Models.SQL_Connect_DB20 db = new Models.SQL_Connect_DB20();
            bool Codong_Chuanhapkho = false;
            bool Codong_Danhapkho = false;

            DataTable DanhmuchangPO = db.GET_DATA_FROM_SQL($"SELECT * FROM [IM_PO_DETAIL] WHERE [SoPO] = '{PO}'");
            foreach (DataRow r in DanhmuchangPO.Rows)
            {
                if (r["Luongvekho"].ToString()!.Trim().Equals(""))
                {
                    Codong_Chuanhapkho = true;
                }
                else if (!r["Luongvekho"].ToString()!.Trim().Equals(""))
                {
                    Codong_Danhapkho = true;
                }
            }
            //Về 1 phần.
            if (Codong_Chuanhapkho == true & Codong_Danhapkho == true)
            {
                db.ReturnString($"UPDATE [IM_PO] SET [TinhtrangPO] = 'VE1PHAN' WHERE [SoPO] = '{PO}'");
            }
            // Chưa nhập kho tý nào.
            else if (Codong_Chuanhapkho == true & Codong_Danhapkho == false)
            {

            }
            // Đã nhập kho hết.
            else if (Codong_Chuanhapkho == false & Codong_Danhapkho == true)
            {
                db.ReturnString($"UPDATE [IM_PO] SET [TinhtrangPO] = 'HOANTHANH' WHERE [SoPO] = '{PO}'");
            }
        }

        [HttpPost]
        public JsonResult GetWareHouse()
        {
            var data = Models.MST_WAREHOUSE.warehouse_process();
            return Json(data.Select(n => n.CHR_WAREHOUSE).ToArray());
        }

        //public JsonResult GetWareHouseSection()
        //{
        //    string ADID = User.Identity?.Name?.ToString() ?? "dev";
        //    //Lấy dữ liệu cho phiên làm việc ....

        //}

        public JsonResult SearchDataPo([FromBody] SearchPoPayload data)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            var khoi = db.ReturnString("SELECT [Group_Code] FROM [COST_MANAGEMENT].[dbo].[GROUP_MEMBER] WHERE [CHR_USERID] = '" + data.UserName + "'");

            string WhereCmd = string.Empty;
            if (data.PoNumber!.Contains(','))
            {
                string[] lstPO = data.PoNumber.Split(',');
                WhereCmd = $"[SoPO] in ('{String.Join("','", lstPO)}') AND ";
            }
            else
            {
                WhereCmd = $"[SoPO] like '%{data.PoNumber}%' AND ";
            }
            string sqlColumn = "[PO_Detail_Id],[Id_Goc],[SoPO],[Code_Request]";
            sqlColumn += ",[Id_RequestDetail],[Good_Code],[Tentienganh]";
            sqlColumn += ",[Tentiengviet],[Mahang],[Soluong]";
            sqlColumn += ",[Dovi],[Dongia],[Dieukiengiaohang]";
            sqlColumn += ",[Diadiemgiaohang],[Phuongthucvanchuyen],[Sotien]";
            sqlColumn += ",[Vat],[Maphongyeucau],[Tenphongyeucau]";
            sqlColumn += ",[Ngaygiaohangdukien],[Noigiaodukien],[Thoigianthanhtoan]";
            sqlColumn += ",[Loaitien],[Tygia],[DoisangUSD]";
            sqlColumn += ",[Danhmuc],[Invoice],[InvoiceNgaynhap],[InvoiceNguoinhap],[Luongvethucte]";
            sqlColumn += ",[LuongvethucteNgaynhap],[LuongvethucteNguoinhap],[Luongvekho],[LuongvekhoNgaynhap] ";
            sqlColumn += ",[LuongvekhoNguoinhap],[Sotokhai],[Ngaydangkytk]";
            sqlColumn += ",[Kiemtratk],[SotokhaiNgaynhap],[SotokhaiNguoinhap]";
            sqlColumn += ",[Tinhtrangtokhai],[Hienthi],[Benxacnhantruoc]";
            sqlColumn += ",[Ngayphathanh],[TinhtrangPO],[TinhtranghaiquanPO]";
            sqlColumn += ",[MaNCC],[TenNCC],[Maphongban]";
            sqlColumn += ",[Nguoixacnhan],[Thoigianxacnhan],[Group_Code]";
            sqlColumn += ",[InvoicePO],[InvoicePODenghithanhtoan],[InvoicePONgaynhap],[InvoicePONguoinhap]";
            sqlColumn += ",[Nguoilamdon],[Ngaytao],[TinhtranghaiquanPONguoinhap],[TinhtranghaiquanPONgaynhap]";
            sqlColumn += ",[Id_LichsuNhap],[LuongvekhoDanhap],[Loaichiphi]";
            sqlColumn += ",[LuongvekhoKhonhap],[Aim],[Loaihinhtokhai],[Account_Code],[Phongchiuchiphi]";

            string cmdQry = $"SELECT TOP 200 {sqlColumn} FROM [PO] WHERE {WhereCmd} [TinhtrangPO] Not in ('DANGCHOXACNHAN','HUY') AND [Group_Code] = '{khoi}' and Code_Request like '%{data.Mayeucau}%' and Mahang like '%{data.Mahang}%' and Tenphongyeucau like N'%{data.Phongbanyeucau}%'  ORDER BY [SoPO] DESC, Hienthi ASC";

            var dataResult = Models.PO.GetPoByPoNumber(cmdQry);
            return Json(dataResult);
        }
        public JsonResult LoadDataPo(string us)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            string khoi = db.ReturnString("SELECT [Group_Code] FROM [COST_MANAGEMENT].[dbo].[GROUP_MEMBER] WHERE [CHR_USERID] = '" + us + "'");

            string sqlColumn = "[PO_Detail_Id],[Id_Goc],[SoPO],[Code_Request]";
            sqlColumn += ",[Id_RequestDetail],[Good_Code],[Tentienganh]";
            sqlColumn += ",[Tentiengviet],[Mahang],[Soluong]";
            sqlColumn += ",[Dovi],[Dongia],[Dieukiengiaohang]";
            sqlColumn += ",[Diadiemgiaohang],[Phuongthucvanchuyen],[Sotien]";
            sqlColumn += ",[Vat],[Maphongyeucau],[Tenphongyeucau]";
            sqlColumn += ",[Ngaygiaohangdukien],[Noigiaodukien],[Thoigianthanhtoan]";
            sqlColumn += ",[Loaitien],[Tygia],[DoisangUSD]";
            sqlColumn += ",[Danhmuc],[Invoice],[InvoiceNgaynhap],[InvoiceNguoinhap],[Luongvethucte]";
            sqlColumn += ",[LuongvethucteNgaynhap],[LuongvethucteNguoinhap],[Luongvekho],[LuongvekhoNgaynhap] ";
            sqlColumn += ",[LuongvekhoNguoinhap],[Sotokhai],[Ngaydangkytk]";
            sqlColumn += ",[Kiemtratk],[SotokhaiNgaynhap],[SotokhaiNguoinhap]";
            sqlColumn += ",[Tinhtrangtokhai],[Hienthi],[Benxacnhantruoc]";
            sqlColumn += ",[Ngayphathanh],[TinhtrangPO],[TinhtranghaiquanPO]";
            sqlColumn += ",[MaNCC],[TenNCC],[Maphongban]";
            sqlColumn += ",[Nguoixacnhan],[Thoigianxacnhan],[Group_Code]";
            sqlColumn += ",[InvoicePO],[InvoicePODenghithanhtoan],[InvoicePONgaynhap],[InvoicePONguoinhap]";
            sqlColumn += ",[Nguoilamdon],[Ngaytao],[TinhtranghaiquanPONguoinhap],[TinhtranghaiquanPONgaynhap]";
            sqlColumn += ",[Id_LichsuNhap],[LuongvekhoDanhap],[Loaichiphi]";
            sqlColumn += ",[LuongvekhoKhonhap],[Aim],[Loaihinhtokhai],[Account_Code],[Phongchiuchiphi]";

            string cmdQry = $"SELECT TOP (300) {sqlColumn} FROM [PO] WHERE [TinhtrangPO] Not in ('DANGCHOXACNHAN','HUY') AND [Group_Code] = '{khoi}' ORDER BY [SoPO] DESC, Hienthi ASC";

            var dataResult = Models.PO.GetPoByPoNumber(cmdQry);
            return Json(dataResult);
        }

        public JsonResult GetExchangeUnit()
        {
            Models.SQL_Connect_DB20 db = new Models.SQL_Connect_DB20();
            string cmd = "SELECT [MaNguyenLieu],[DonviPO],[DonviRequest],[Soluongquydoi],[Unit],[Khoi],[Id_Quydoi] ";
            cmd += "FROM [COST_MANAGEMENT].[dbo].[DONVIQUYDOI] ";
            DataTable gData = db.GET_DATA_FROM_SQL(cmd);
            List<ExchangeUnit> values = new List<ExchangeUnit>();
            for (int idx = 0; idx < gData.Rows.Count; idx++)
            {
                values.Add(new ExchangeUnit()
                {
                    poUnit = gData.Rows[idx]["DonviPO"].ToString()!,
                    conversionQty = gData.Rows[idx]["Soluongquydoi"].ToString()!,
                    convertedUnit = gData.Rows[idx]["DonviRequest"].ToString()!,
                    deptUnit = gData.Rows[idx]["Unit"].ToString()!,
                    materialCode = gData.Rows[idx]["MaNguyenLieu"].ToString()!
                });
            }
            return Json(values);
        }
        public JsonResult ImportWarehouse([FromBody] ConfirmImport data)
        {
            data.NgayNhap = string.IsNullOrWhiteSpace(data.NgayNhap)
                ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                : data.NgayNhap;
            data.luongvethuctekho = string.IsNullOrWhiteSpace(data.luongvethuctekho) ? data.Soluong : data.luongvethuctekho;
            Models.SQL_Connect_DB20 db = new Models.SQL_Connect_DB20();

            // lấy ra khối và set kho
            var get_khoi = db.GET_DATA_FROM_SQL("SELECT [Group_Code] FROM [COST_MANAGEMENT].[dbo].[PO] WHERE  PO_Detail_Id = '" + data.Id_nhapkho + "'");
            // nếu khối Prod về kho F2, GA về GA, IT về IT, PUR về PUR
            string khoi = get_khoi.Rows[0][0].ToString()!;
            data.KhoNhan = data.Mahang switch
            {
                var s when s!.Contains("E") || s.Contains("A") => "F2",
                var s when s!.Contains("I") => "IT",
                var s when s!.Contains("B") || s.Contains("C") => "F1",
                _ => khoi switch // không có mã hàng sẽ gán theo khối
                {
                    "PROD" => "F2",
                    "GA" => "F1",
                    _ => khoi
                }
            };

            string sqlColumn = "[PO_Detail_Id],[Id_Goc],[SoPO],[Code_Request]";
            sqlColumn += ",[Id_RequestDetail],[Good_Code],[Tentienganh]";
            sqlColumn += ",[Tentiengviet],[Mahang],[Soluong]";
            sqlColumn += ",[Dovi],[Dongia],[Dieukiengiaohang]";
            sqlColumn += ",[Diadiemgiaohang],[Phuongthucvanchuyen],[Sotien]";
            sqlColumn += ",[Vat],[Maphongyeucau],[Tenphongyeucau]";
            sqlColumn += ",[Ngaygiaohangdukien],[Noigiaodukien],[Thoigianthanhtoan]";
            sqlColumn += ",[Loaitien],[Tygia],[DoisangUSD]";
            sqlColumn += ",[Danhmuc],[Invoice],[InvoiceNgaynhap],[InvoiceNguoinhap],[Luongvethucte]";
            sqlColumn += ",[LuongvethucteNgaynhap],[LuongvethucteNguoinhap],[Luongvekho],[LuongvekhoNgaynhap] ";
            sqlColumn += ",[LuongvekhoNguoinhap],[Sotokhai],[Ngaydangkytk]";
            sqlColumn += ",[Kiemtratk],[SotokhaiNgaynhap],[SotokhaiNguoinhap]";
            sqlColumn += ",[Tinhtrangtokhai],[Hienthi],[Benxacnhantruoc]";
            sqlColumn += ",[Ngayphathanh],[TinhtrangPO],[TinhtranghaiquanPO]";
            sqlColumn += ",[MaNCC],[TenNCC],[Maphongban]";
            sqlColumn += ",[Nguoixacnhan],[Thoigianxacnhan],[Group_Code]";
            sqlColumn += ",[InvoicePO],[InvoicePODenghithanhtoan],[InvoicePONgaynhap],[InvoicePONguoinhap]";
            sqlColumn += ",[Nguoilamdon],[Ngaytao],[TinhtranghaiquanPONguoinhap],[TinhtranghaiquanPONgaynhap]";
            sqlColumn += ",[Id_LichsuNhap],[LuongvekhoDanhap],[Loaichiphi]";
            sqlColumn += ",[LuongvekhoKhonhap],[Aim],[Loaihinhtokhai],[Account_Code],[Phongchiuchiphi]";

            string cmdQry = $"SELECT {sqlColumn} FROM [PO] WHERE [PO_Detail_Id] = '{data.Id_nhapkho}'";

            var dataPO = Models.PO.GetPoByPoIdentify(cmdQry);
            if (dataPO.Count == 0) return Json($"Không tìm thấy ID {data.Id_nhapkho} của mục PO");
            //string Khoi = db.ReturnString("SELECT [Group_Code] FROM [COST_MANAGEMENT].[dbo].[GROUP_MEMBER] WHERE [CHR_USERID] = '" + data.UserName + "'");
            //if (Khoi == "") Khoi = "PROD";
            if (data.benXacNhanTruoc!.Equals("STOCK")) // Hàng trong kho cũ của hệ thống cost (PR1-MC, IT, GA)
            {
                bool IsExists = db.GET_DATA_FROM_SQL($"SELECT * FROM [IM_PO_DETAIL] WHERE [Id_Goc] ='{data.Id_nhapkho}'").Rows.Count > 0;
                if (IsExists)
                {
                    return Json("Danh mục hàng đã được tách nên không thể thay đổi số lượng, \nMuốn thay đổi số lượng thì phải Reset dòng hàng");
                }
            }

            if (dataPO[0].LuongvekhoDanhap!.Trim().Equals("True"))
            {
                return Json("Danh mục hàng đã được nhập kho, vui lòng reset lại rồi mới nhập số mới");
            }

            if (dataPO[0].TinhtrangPO!.Trim().Equals("DANGCHOXACNHAN"))
            {
                return Json($"Danh mục hàng số: {data.Id_nhapkho} đang trong tình trạng chờ xác nhận Need/No Need của shipping nên không thể nhập kho");
            }

            if (dataPO[0].Luongvekho!.Trim() == "")
            {
                dataPO[0].Luongvekho = dataPO[0].Soluong;
            }

            if (dataPO[0].Benxacnhantruoc == "" || dataPO[0].Benxacnhantruoc!.Equals("STOCK"))
            {
                double dbluongvekho = 0.0;
                double.TryParse(data.luongvethuctekho, out dbluongvekho);

                double soluong = 0;
                double.TryParse(dataPO[0].Soluong, out soluong);
                if (dbluongvekho < soluong && dbluongvekho != 0)
                {
                    double Soluongmoi = soluong - dbluongvekho;
                    double Dongia = 0.0;
                    Dongia = dataPO[0].Dongia ?? 0.0;
                    double Sotien = Soluongmoi * Dongia;
                    double VAT_Unit = 0.0;
                    double.TryParse(dataPO[0].Vat, out VAT_Unit);
                    double VAT = (Sotien * VAT_Unit) / 100;

                    Sotien = Sotien + VAT;
                    double DoisangUSD = 0.0;
                    DoisangUSD = dataPO[0].Tygia ?? 0.0;
                    //double.TryParse(dataPO[0].Tygia, out DoisangUSD);

                    string Insert = "INSERT INTO IM_PO_DETAIL([SoPO],[Tentienganh],[Tentiengviet],[Mahang],[Soluong],[Dovi],[Dongia],[Dieukiengiaohang],[Diadiemgiaohang],[Phuongthucvanchuyen],[Sotien],[Vat],[Maphongyeucau],[Tenphongyeucau],[Ngaygiaohangdukien],[Noigiaodukien],[Thoigianthanhtoan],[Code_Request],[Id_RequestDetail],[Loaitien],[Tygia],[DoisangUSD],[Danhmuc],[Id_Goc],[Hienthi],[Benxacnhantruoc],[Good_Code]) ";
                    Insert += $" SELECT [SoPO],[Tentienganh],[Tentiengviet],[Mahang],'{Soluongmoi}',[Dovi],[Dongia],[Dieukiengiaohang],[Diadiemgiaohang],[Phuongthucvanchuyen],'{Sotien}',[Vat],[Maphongyeucau],[Tenphongyeucau],[Ngaygiaohangdukien],[Noigiaodukien],[Thoigianthanhtoan],[Code_Request],[Id_RequestDetail],[Loaitien],[Tygia],'{DoisangUSD}',[Danhmuc],[PO_Detail_Id],[Hienthi] + 1,'STOCK',[Good_Code]";
                    Insert += $" FROM IM_PO_DETAIL WHERE PO_Detail_Id = '{data.Id_nhapkho}' ";
                    db.GET_DATA_FROM_SQL(Insert);
                }

                double Luongvekho = 0.0;
                double.TryParse(data.luongvethuctekho, out Luongvekho);

                double Dongiaroot = 0.0;
                Dongiaroot = dataPO[0].Dongia ?? 0.0;

                double Sotienroot = Luongvekho * Dongiaroot;
                double VAT_Unit_2 = 0.0;
                double.TryParse(dataPO[0].Vat, out VAT_Unit_2);
                double VAT_2 = Sotienroot * VAT_Unit_2 / 100;
                Sotienroot += VAT_2;

                double DoisangUSDroot = dataPO[0].Tygia ?? 0.0;

                DoisangUSDroot = Math.Round(Sotienroot / DoisangUSDroot);

                db.GET_DATA_FROM_SQL($"UPDATE [IM_PO_DETAIL] SET [Luongvekho] = '{data.luongvethuctekho}', LuongvekhoNgaynhap = '{data.NgayNhap}', [LuongvekhoNguoinhap] = '{data.UserName}', LuongvekhoKhonhap = '{data.KhoNhan}', Sotien = '{Sotienroot}', DoisangUSD = '{DoisangUSDroot}', [Benxacnhantruoc] = 'STOCK', LuongvekhoDanhap = 'True' WHERE [PO_Detail_Id] = '{data.Id_nhapkho}' ");
            }
            else // trường hợp = "SHIP"
            {
                double Luongvekho = 0.0;
                double.TryParse(data.luongvethuctekho, out Luongvekho);

                double Dongiaroot = 0.0;
                Dongiaroot = dataPO[0].Dongia ?? 0.0;

                double Sotienroot = Luongvekho * Dongiaroot;
                double DoisangUSDroot = dataPO[0].Tygia ?? 0.0;

                DoisangUSDroot = Math.Round(Sotienroot / DoisangUSDroot, 2);

                db.GET_DATA_FROM_SQL($"UPDATE [IM_PO_DETAIL] SET [Luongvekho] = '{data.luongvethuctekho}', LuongvekhoNgaynhap = '{data.NgayNhap}', [LuongvekhoNguoinhap] = '{data.UserName}', LuongvekhoKhonhap = '{data.KhoNhan}', Sotien = '{Sotienroot}', DoisangUSD = '{DoisangUSDroot}', LuongvekhoDanhap = 'True' WHERE [PO_Detail_Id] = '{data.Id_nhapkho}' ");
            }

            string Lydo = "";
            if (data.Mahang!.Trim().Equals("")) // Không có mã hàng là hàng ngoài danh mục ....
            {
                double Luongnhapkho = 0.0;
                double.TryParse(data.luongvethuctekho!.Trim(), out Luongnhapkho);

                string DonviRequest = db.ReturnString($"SELECT [Unit] FROM [MATERIAL] WHERE [Material_Code] = N'{data.Mahang}'");
                string Quydoi = db.ReturnString($"SELECT [Soluongquydoi] FROM [KHO_DONVIQUYDOI] WHERE [MaNguyenLieu] = '{data.Mahang}' AND [DonviRequest] = '{DonviRequest}' AND [DonviPO] = N'{dataPO[0].Dovi}' ");

                if (Quydoi != "")
                {
                    Luongnhapkho = double.Parse(Quydoi) * Luongnhapkho;
                }
                //Nhập kho
                string Soluonghientai = db.ReturnString($"SELECT [Hientai] FROM KHO WHERE [MaNguyenLieu] =  N'{data.Mahang}' AND [Kho] = '{data.KhoNhan}' AND [Group_Code] = '{khoi}'");
                double SoluongTruocthaydoi = 0;

                if (dataPO[0].Benxacnhantruoc == "" || dataPO[0].Benxacnhantruoc!.Equals("STOCK")) // Thực hiện với hàng kho của PR1-MC/GA
                {
                    if (Soluonghientai.Trim() == "")
                    {
                        db.ReturnString($"INSERT INTO KHO(MaNguyenLieu,Hientai,Group_Code,Kho, QTY_NEW) VALUES (N'{data.Mahang}','{Luongnhapkho}','{khoi}','{data.KhoNhan}','{Luongnhapkho}')");
                    }
                    else
                    {
                        db.ReturnString($"UPDATE KHO SET [Hientai] = [Hientai] + {Luongnhapkho}, [QTY_NEW] = [QTY_NEW] + {Luongnhapkho} WHERE [MaNguyenLieu] =  N'{data.Mahang}' AND  [Kho] = '{data.KhoNhan}' AND [Group_Code] = '{khoi}'");
                    }
                }
                else // Hàng trong kho của các phòng ban khác
                {
                    // Thực hiện việc quản lý lượng nhập vào bảng tạm. Chờ khi người đảm nhiệm kho xác nhận đã nhận hàng mới nhập vào kho chưa?
                    Models.KHO_TMP tmp = new Models.KHO_TMP()
                    {
                        QUANTITY = Luongnhapkho,
                        CHR_WAREHOUSE = data.KhoNhan,
                        CHR_CODE_MATERIAL = dataPO[0].Mahang,
                        CHR_GROUP_CODE = khoi
                    };
                    Models.KHO_TMP.InsertDataImport(tmp);
                }

                string Manhanvien = db.ReturnString($"SELECT CHR_CRT_USERID FROM [TM_USER] WHERE [CHR_USERID] = '{data.UserName}'");
                double Soluongconlai = 0;
                double SoluongPO = 0;
                if (Quydoi != "")
                {
                    double.TryParse(data.Soluong, out SoluongPO);
                    Soluongconlai = SoluongPO - Luongnhapkho;
                }

                db.ReturnString($"DELETE FROM [KHO_NHAPXUAT] WHERE [Id_Lichsu] = '{dataPO[0].Id_LichsuNhap}'");

                string Id_Lichsu = db.ReturnString($"INSERT INTO [KHO_NHAPXUAT]([MaNguyenLieu],[Hanhdong],[Soluong],[Loai],[Thoigian],[Nguoicapnhat],[Kho],[Khoi],[TenNguyenlieu],[NCC],[Donvi],[MaNguoinhap],[Gia],[SoPO],[SoluongPO],[DonviPO],[Soluongconlai],[Ngaynhaokho],[Soluongtruocthaydoi],[Soluongsauthaydoi]) OUTPUT Inserted.Id_Lichsu VALUES(N'{dataPO[0].Mahang}',N'Nhập kho {data.KhoNhan} từ PO: {dataPO[0].SoPO} -> {Lydo}','{Luongnhapkho}','NHAP',GETDATE(),'{data.UserName}','{data.KhoNhan}','{khoi}',N'{dataPO[0].Tentienganh}',N'{dataPO[0].TenNCC}',N'{DonviRequest}','{Manhanvien}','{dataPO[0].Dongia}','{dataPO[0].SoPO}','{SoluongPO}',N'{dataPO[0].Dovi}','{Soluongconlai}','{data.NgayNhap}','{SoluongTruocthaydoi}','{Luongnhapkho + SoluongTruocthaydoi}')");
                db.ReturnString($"UPDATE [IM_PO_DETAIL] SET [Id_LichsuNhap] = '{Id_Lichsu}' WHERE [PO_Detail_Id] = '{data.Id_nhapkho}'");
            }

            db.ReturnString($"INSERT INTO [IM_LOG]([Loai],[SoPO],[PO_Detail_Id],[Hanhdong],[Thogian],[Nguoicapnhat]) VALUES  ('DM','{data.PO_Detail_Id}','{data.Id_nhapkho}',N'Nhập kho',Getdate(),'{data.UserName}')");
            db.ReturnString($"UPDATE [IM_PO] SET [Nguoixacnhan] = '{data.UserName}',[Thoigianxacnhan] = GETDATE() WHERE [SoPO] = '{data.PO_Detail_Id}'");

            UpdateTinhTrangPO(data.PO_Detail_Id!);
            return Json("OK");
        }
        [HttpPost]
        public JsonResult Sudungngay([FromBody] ConfirmImport data)
        {
            Models.SQL_Connect_DB20 db = new Models.SQL_Connect_DB20();

            // dữ liệu đầu vào ---
            string ngayNhap = string.IsNullOrWhiteSpace(data.NgayNhap)
                              ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : data.NgayNhap;

            double luongVeThucTe = 0;
            if (!double.TryParse(data.luongvethuctekho, out luongVeThucTe))
            {
                double.TryParse(data.Soluong, out luongVeThucTe); // Mặc định lấy theo SL PO nếu trống
            }

            // Lấy thông tin PO và Khối 
            string sqlColumn = "[PO_Detail_Id],[Id_Goc],[SoPO],[Code_Request]";
            sqlColumn += ",[Id_RequestDetail],[Good_Code],[Tentienganh]";
            sqlColumn += ",[Tentiengviet],[Mahang],[Soluong]";
            sqlColumn += ",[Dovi],[Dongia],[Dieukiengiaohang]";
            sqlColumn += ",[Diadiemgiaohang],[Phuongthucvanchuyen],[Sotien]";
            sqlColumn += ",[Vat],[Maphongyeucau],[Tenphongyeucau]";
            sqlColumn += ",[Ngaygiaohangdukien],[Noigiaodukien],[Thoigianthanhtoan]";
            sqlColumn += ",[Loaitien],[Tygia],[DoisangUSD]";
            sqlColumn += ",[Danhmuc],[Invoice],[InvoiceNgaynhap],[InvoiceNguoinhap],[Luongvethucte]";
            sqlColumn += ",[LuongvethucteNgaynhap],[LuongvethucteNguoinhap],[Luongvekho],[LuongvekhoNgaynhap] ";
            sqlColumn += ",[LuongvekhoNguoinhap],[Sotokhai],[Ngaydangkytk]";
            sqlColumn += ",[Kiemtratk],[SotokhaiNgaynhap],[SotokhaiNguoinhap]";
            sqlColumn += ",[Tinhtrangtokhai],[Hienthi],[Benxacnhantruoc]";
            sqlColumn += ",[Ngayphathanh],[TinhtrangPO],[TinhtranghaiquanPO]";
            sqlColumn += ",[MaNCC],[TenNCC],[Maphongban]";
            sqlColumn += ",[Nguoixacnhan],[Thoigianxacnhan],[Group_Code]";
            sqlColumn += ",[InvoicePO],[InvoicePODenghithanhtoan],[InvoicePONgaynhap],[InvoicePONguoinhap]";
            sqlColumn += ",[Nguoilamdon],[Ngaytao],[TinhtranghaiquanPONguoinhap],[TinhtranghaiquanPONgaynhap]";
            sqlColumn += ",[Id_LichsuNhap],[LuongvekhoDanhap],[Loaichiphi]";
            sqlColumn += ",[LuongvekhoKhonhap],[Aim],[Loaihinhtokhai],[Account_Code],[Phongchiuchiphi]";

            string cmdQry = $"SELECT {sqlColumn} FROM [PO] WHERE [PO_Detail_Id] = '{data.Id_nhapkho}'";

            var dataPO = Models.PO.GetPoByPoIdentify(cmdQry);
            if (dataPO.Count == 0) return Json("Lỗi: Không tìm thấy ID PO trong hệ thống.");
            var poRow = dataPO[0];

            // lấy ra khối và set kho
            var get_khoi = db.GET_DATA_FROM_SQL("SELECT [Group_Code] FROM [COST_MANAGEMENT].[dbo].[PO] WHERE  PO_Detail_Id = '" + data.Id_nhapkho + "'");
            // nếu khối Prod về kho F2, GA về GA, IT về IT, PUR về PUR
            string khoi = get_khoi.Rows[0][0].ToString()!;


            //Kiểm tra điều kiện
            if (poRow.Benxacnhantruoc == "STOCK")
            {
                bool daTach = db.GET_DATA_FROM_SQL($"SELECT 1 FROM [IM_PO_DETAIL] WHERE [Id_Goc] = '{data.Id_nhapkho}'").Rows.Count > 0;
                if (daTach) return Json("Lỗi: Dòng hàng đã tách, không thể đổi số lượng. Hãy Reset dòng này.");
            }
            if (poRow.LuongvekhoDanhap?.Trim() == "True") return Json("Lỗi: Mục này đã được nhập kho rồi.");
            if (poRow.TinhtrangPO?.Trim() == "DANGCHOXACNHAN") return Json("Lỗi: PO đang chờ Shipping xác nhận.");


            //Tách Dòng (Nếu nhập thiếu số lượng)
            double soLuongGoc = 0; double.TryParse(poRow.Soluong, out soLuongGoc);
            double donGia = poRow.Dongia ?? 0;
            double tyGia = poRow.Tygia ?? 1;
            double vatPhanTram = 0; double.TryParse(poRow.Vat, out vatPhanTram);

            if ((string.IsNullOrEmpty(poRow.Benxacnhantruoc) || poRow.Benxacnhantruoc == "STOCK")
                && luongVeThucTe < soLuongGoc && luongVeThucTe > 0)
            {
                double slMoi = soLuongGoc - luongVeThucTe;
                double tienMoi = (slMoi * donGia) * (1 + vatPhanTram / 100);
                double usdMoi = Math.Round(tienMoi / tyGia, 2);

                string Insert = "INSERT INTO IM_PO_DETAIL([SoPO],[Tentienganh],[Tentiengviet],[Mahang],[Soluong],[Dovi],[Dongia],[Dieukiengiaohang],[Diadiemgiaohang],[Phuongthucvanchuyen],[Sotien],[Vat],[Maphongyeucau],[Tenphongyeucau],[Ngaygiaohangdukien],[Noigiaodukien],[Thoigianthanhtoan],[Code_Request],[Id_RequestDetail],[Loaitien],[Tygia],[DoisangUSD],[Danhmuc],[Id_Goc],[Hienthi],[Benxacnhantruoc],[Good_Code]) ";
                Insert += $" SELECT [SoPO],[Tentienganh],[Tentiengviet],[Mahang],'{slMoi}',[Dovi],[Dongia],[Dieukiengiaohang],[Diadiemgiaohang],[Phuongthucvanchuyen],'{tienMoi}',[Vat],[Maphongyeucau],[Tenphongyeucau],[Ngaygiaohangdukien],[Noigiaodukien],[Thoigianthanhtoan],[Code_Request],[Id_RequestDetail],[Loaitien],[Tygia],'{usdMoi}',[Danhmuc],[PO_Detail_Id],[Hienthi] + 1,'STOCK',[Good_Code]";
                Insert += $" FROM IM_PO_DETAIL WHERE PO_Detail_Id = '{data.Id_nhapkho}' ";
                db.GET_DATA_FROM_SQL(Insert);

            }

            //Cập nhật trạng thái dòng hiện tại 
            double tienHienTai = (luongVeThucTe * donGia) * (1 + vatPhanTram / 100);
            double usdHienTai = Math.Round(tienHienTai / tyGia, 2);

            db.GET_DATA_FROM_SQL($@"UPDATE [IM_PO_DETAIL] SET 
                [Luongvekho] = '{luongVeThucTe}', [LuongvekhoNgaynhap] = '{ngayNhap}', 
                [LuongvekhoNguoinhap] = '{data.UserName}', [LuongvekhoKhonhap] = '{data.KhoNhan}', 
                [Sotien] = '{tienHienTai}', [DoisangUSD] = '{usdHienTai}', 
                [Benxacnhantruoc] = 'STOCK', [LuongvekhoDanhap] = 'True' 
                WHERE [PO_Detail_Id] = '{data.Id_nhapkho}'");

            // Log và Hoàn tất 
            db.GET_DATA_FROM_SQL($"INSERT INTO [IM_LOG]([Loai],[SoPO],[PO_Detail_Id],[Hanhdong],[Thogian],[Nguoicapnhat]) VALUES ('DM','{poRow.SoPO}','{data.Id_nhapkho}',N'Nhập kho',Getdate(),'{data.UserName}')");
            db.GET_DATA_FROM_SQL($"UPDATE [IM_PO] SET [Nguoixacnhan] = '{data.UserName}', [Thoigianxacnhan] = GETDATE() WHERE [SoPO] = '{poRow.SoPO}'");

            UpdateTinhTrangPO(poRow.SoPO!);
            return Json("OK");

        }
        [HttpPost]
        public JsonResult NhapKhoAction([FromBody] ConfirmImport data)
        {
            Models.SQL_Connect_DB20 db = new Models.SQL_Connect_DB20();

            // dữ liệu đầu vào ---
            string ngayNhap = string.IsNullOrWhiteSpace(data.NgayNhap)
                              ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : data.NgayNhap;

            double luongVeThucTe = 0;
            if (!double.TryParse(data.luongvethuctekho, out luongVeThucTe))
            {
                double.TryParse(data.Soluong, out luongVeThucTe); // Mặc định lấy theo SL PO nếu trống
            }

            // Lấy thông tin PO và Khối 
            string sqlColumn = "[PO_Detail_Id],[Id_Goc],[SoPO],[Code_Request]";
            sqlColumn += ",[Id_RequestDetail],[Good_Code],[Tentienganh]";
            sqlColumn += ",[Tentiengviet],[Mahang],[Soluong]";
            sqlColumn += ",[Dovi],[Dongia],[Dieukiengiaohang]";
            sqlColumn += ",[Diadiemgiaohang],[Phuongthucvanchuyen],[Sotien]";
            sqlColumn += ",[Vat],[Maphongyeucau],[Tenphongyeucau]";
            sqlColumn += ",[Ngaygiaohangdukien],[Noigiaodukien],[Thoigianthanhtoan]";
            sqlColumn += ",[Loaitien],[Tygia],[DoisangUSD]";
            sqlColumn += ",[Danhmuc],[Invoice],[InvoiceNgaynhap],[InvoiceNguoinhap],[Luongvethucte]";
            sqlColumn += ",[LuongvethucteNgaynhap],[LuongvethucteNguoinhap],[Luongvekho],[LuongvekhoNgaynhap] ";
            sqlColumn += ",[LuongvekhoNguoinhap],[Sotokhai],[Ngaydangkytk]";
            sqlColumn += ",[Kiemtratk],[SotokhaiNgaynhap],[SotokhaiNguoinhap]";
            sqlColumn += ",[Tinhtrangtokhai],[Hienthi],[Benxacnhantruoc]";
            sqlColumn += ",[Ngayphathanh],[TinhtrangPO],[TinhtranghaiquanPO]";
            sqlColumn += ",[MaNCC],[TenNCC],[Maphongban]";
            sqlColumn += ",[Nguoixacnhan],[Thoigianxacnhan],[Group_Code]";
            sqlColumn += ",[InvoicePO],[InvoicePODenghithanhtoan],[InvoicePONgaynhap],[InvoicePONguoinhap]";
            sqlColumn += ",[Nguoilamdon],[Ngaytao],[TinhtranghaiquanPONguoinhap],[TinhtranghaiquanPONgaynhap]";
            sqlColumn += ",[Id_LichsuNhap],[LuongvekhoDanhap],[Loaichiphi]";
            sqlColumn += ",[LuongvekhoKhonhap],[Aim],[Loaihinhtokhai],[Account_Code],[Phongchiuchiphi]";

            string cmdQry = $"SELECT {sqlColumn} FROM [PO] WHERE [PO_Detail_Id] = '{data.Id_nhapkho}'";

            var dataPO = Models.PO.GetPoByPoIdentify(cmdQry);
            if (dataPO.Count == 0) return Json("Lỗi: Không tìm thấy ID PO trong hệ thống.");
            var poRow = dataPO[0];

            // lấy ra khối và set kho
            var get_khoi = db.GET_DATA_FROM_SQL("SELECT [Group_Code] FROM [COST_MANAGEMENT].[dbo].[PO] WHERE  PO_Detail_Id = '" + data.Id_nhapkho + "'");
            // nếu khối Prod về kho F2, GA về GA, IT về IT, PUR về PUR
            string khoi = get_khoi.Rows[0][0].ToString()!;
            data.KhoNhan = data.Mahang switch
            {
                var s when s!.Contains("E") || s!.Contains("A") => "F2",
                var s when s!.Contains("I") => "IT",
                var s when s!.Contains("B") || s!.Contains("C") => "F1",
                _ => khoi switch // không có mã hàng sẽ gán theo khối
                {
                    "PROD" => "F2",
                    "GA" => "F1",
                    _ => khoi
                }
            };

            //Kiểm tra điều kiện
            if (poRow.Benxacnhantruoc == "STOCK")
            {
                bool daTach = db.GET_DATA_FROM_SQL($"SELECT 1 FROM [IM_PO_DETAIL] WHERE [Id_Goc] = '{data.Id_nhapkho}'").Rows.Count > 0;
                if (daTach) return Json("Lỗi: Dòng hàng đã tách, không thể đổi số lượng. Hãy Reset dòng này.");
            }
            if (poRow.LuongvekhoDanhap?.Trim() == "True") return Json("Lỗi: Mục này đã được nhập kho rồi.");
            if (poRow.TinhtrangPO?.Trim() == "DANGCHOXACNHAN") return Json("Lỗi: PO đang chờ Shipping xác nhận.");

            try
            {
                //Tách Dòng (Nếu nhập thiếu số lượng)
                double soLuongGoc = 0; double.TryParse(poRow.Soluong, out soLuongGoc);
                double donGia = poRow.Dongia ?? 0;
                double tyGia = poRow.Tygia ?? 1;
                double vatPhanTram = 0; double.TryParse(poRow.Vat, out vatPhanTram);

                if ((string.IsNullOrEmpty(poRow.Benxacnhantruoc) || poRow.Benxacnhantruoc == "STOCK")
                    && luongVeThucTe < soLuongGoc && luongVeThucTe > 0)
                {
                    double slMoi = soLuongGoc - luongVeThucTe;
                    double tienMoi = (slMoi * donGia) * (1 + vatPhanTram / 100);
                    double usdMoi = Math.Round(tienMoi / tyGia, 2);

                    string Insert = "INSERT INTO IM_PO_DETAIL([SoPO],[Tentienganh],[Tentiengviet],[Mahang],[Soluong],[Dovi],[Dongia],[Dieukiengiaohang],[Diadiemgiaohang],[Phuongthucvanchuyen],[Sotien],[Vat],[Maphongyeucau],[Tenphongyeucau],[Ngaygiaohangdukien],[Noigiaodukien],[Thoigianthanhtoan],[Code_Request],[Id_RequestDetail],[Loaitien],[Tygia],[DoisangUSD],[Danhmuc],[Id_Goc],[Hienthi],[Benxacnhantruoc],[Good_Code]) ";
                    Insert += $" SELECT [SoPO],[Tentienganh],[Tentiengviet],[Mahang],'{slMoi.ToString(CultureInfo.InvariantCulture)}',[Dovi],[Dongia],[Dieukiengiaohang],[Diadiemgiaohang],[Phuongthucvanchuyen],'{tienMoi.ToString(CultureInfo.InvariantCulture)}',[Vat],[Maphongyeucau],[Tenphongyeucau],[Ngaygiaohangdukien],[Noigiaodukien],[Thoigianthanhtoan],[Code_Request],[Id_RequestDetail],[Loaitien],[Tygia],'{usdMoi.ToString(CultureInfo.InvariantCulture)}',[Danhmuc],[PO_Detail_Id],[Hienthi] + 1,'STOCK',[Good_Code]";
                    Insert += $" FROM IM_PO_DETAIL WHERE PO_Detail_Id = '{data.Id_nhapkho}' ";
                    db.GET_DATA_FROM_SQL(Insert);

                }

                //Cập nhật trạng thái dòng hiện tại 
                double tienHienTai = (luongVeThucTe * donGia) * (1 + vatPhanTram / 100);
                double usdHienTai = Math.Round(tienHienTai / tyGia, 2);

                db.GET_DATA_FROM_SQL($@"UPDATE [IM_PO_DETAIL] SET 
                    [Luongvekho] = '{luongVeThucTe.ToString(CultureInfo.InvariantCulture)}', [LuongvekhoNgaynhap] = '{ngayNhap}', 
                    [LuongvekhoNguoinhap] = '{data.UserName}', [LuongvekhoKhonhap] = '{data.KhoNhan}', 
                    [Sotien] = '{tienHienTai.ToString(CultureInfo.InvariantCulture)}', [DoisangUSD] = '{usdHienTai.ToString(CultureInfo.InvariantCulture)}', 
                    [Benxacnhantruoc] = 'STOCK', [LuongvekhoDanhap] = 'True' 
                    WHERE [PO_Detail_Id] = '{data.Id_nhapkho}'");

                // Xử lý tồn kho 
                if (!string.IsNullOrWhiteSpace(poRow.Mahang))
                {
                    double slNhapKho = luongVeThucTe;
                    // Quy đổi đơn vị 
                    string donViPO = poRow.Dovi!;
                    string donViGoc = db.ReturnString($"SELECT [Unit] FROM [MATERIAL] WHERE [Material_Code] = N'{poRow.Mahang}'");
                    string rate = db.ReturnString($"SELECT [Soluongquydoi] FROM [KHO_DONVIQUYDOI] WHERE [MaNguyenLieu] = '{poRow.Mahang}' AND [DonviRequest] = N'{donViGoc}' AND [DonviPO] = N'{donViPO}'");

                    if (!string.IsNullOrEmpty(rate)) slNhapKho = double.Parse(rate) * luongVeThucTe;

                    // Cập nhật bảng KHO
                    string slHienTaiStr = db.ReturnString($"SELECT [Hientai] FROM KHO WHERE [MaNguyenLieu] = N'{poRow.Mahang}' AND [Kho] = '{data.KhoNhan}' AND [Group_Code] = '{khoi}'");
                    double slTruocThayDoi = 0;
                    if (string.IsNullOrEmpty(slHienTaiStr))
                    {
                        db.GET_DATA_FROM_SQL($"INSERT INTO KHO(MaNguyenLieu,Hientai,Group_Code,Kho) VALUES (N'{poRow.Mahang}','{slNhapKho.ToString(CultureInfo.InvariantCulture)}','{khoi}','{data.KhoNhan}')");
                    }
                    else
                    {
                        slTruocThayDoi = double.Parse(slHienTaiStr);
                        db.GET_DATA_FROM_SQL($"UPDATE KHO SET [Hientai] = [Hientai] + {slNhapKho.ToString(CultureInfo.InvariantCulture)} WHERE [MaNguyenLieu] = N'{poRow.Mahang}' AND [Kho] = '{data.KhoNhan}' AND [Group_Code] = '{khoi}'");
                    }

                    // Ghi lịch sử Nhập Xuất
                    string maNV = db.ReturnString($"SELECT CHR_CRT_USERID FROM [TM_USER] WHERE [CHR_USERID] = '{data.UserName}'");
                    string sqlLichSu = $@"INSERT INTO [KHO_NHAPXUAT]([MaNguyenLieu],[Hanhdong],[Soluong],[Loai],[Thoigian],[Nguoicapnhat],[Kho],[Khoi],[TenNguyenlieu],[NCC],[Donvi],[MaNguoinhap],[Gia],[SoPO],[SoluongPO],[DonviPO],[Soluongconlai],[Ngaynhaokho],[Soluongtruocthaydoi],[Soluongsauthaydoi]) 
                                 OUTPUT Inserted.Id_Lichsu
                                 VALUES(N'{poRow.Mahang}', N'Nhập từ PO: {poRow.SoPO}', '{slNhapKho.ToString(CultureInfo.InvariantCulture)}', 'NHAP', GETDATE(), '{data.UserName}', '{data.KhoNhan}', '{khoi}', N'{poRow.Tentienganh}', N'{poRow.TenNCC}', N'{donViGoc.ToString(CultureInfo.InvariantCulture)}', '{maNV}', '{donGia.ToString(CultureInfo.InvariantCulture)}', '{poRow.SoPO}', '{soLuongGoc.ToString(CultureInfo.InvariantCulture)}', N'{donViPO.ToString(CultureInfo.InvariantCulture)}', '{(soLuongGoc - slNhapKho).ToString(CultureInfo.InvariantCulture)}', '{ngayNhap}', '{slTruocThayDoi.ToString(CultureInfo.InvariantCulture)}', '{(slTruocThayDoi + slNhapKho).ToString(CultureInfo.InvariantCulture)}')";
                    string idLichSu = db.ReturnString(sqlLichSu);
                    db.GET_DATA_FROM_SQL($"UPDATE [IM_PO_DETAIL] SET [Id_LichsuNhap] = '{idLichSu}' WHERE [PO_Detail_Id] = '{data.Id_nhapkho}'");
                }

                // Log và Hoàn tất 
                db.GET_DATA_FROM_SQL($"INSERT INTO [IM_LOG]([Loai],[SoPO],[PO_Detail_Id],[Hanhdong],[Thogian],[Nguoicapnhat]) VALUES ('DM','{poRow.SoPO}','{data.Id_nhapkho}',N'Nhập kho',Getdate(),'{data.UserName}')");
                db.GET_DATA_FROM_SQL($"UPDATE [IM_PO] SET [Nguoixacnhan] = '{data.UserName}', [Thoigianxacnhan] = GETDATE() WHERE [SoPO] = '{poRow.SoPO}'");

                UpdateTinhTrangPO(poRow.SoPO!);
                return Json("OK");

            }
            catch (Exception ex)
            {
                return Json("Lỗi hệ thống: " + ex.Message);
            }
        }
        [HttpPost]
        public JsonResult ResetImportRow([FromBody] ConfirmImport item)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            try { 
                // lấy ra khối và set kho
                var get_khoi = _db.GET_DATA_FROM_SQL("SELECT [Group_Code] FROM [COST_MANAGEMENT].[dbo].[PO] WHERE  PO_Detail_Id = '" + item.Id_nhapkho + "'");

                // nếu khối Prod về kho F2, GA về GA, IT về IT, PUR về PUR
                string khoi = get_khoi.Rows[0][0].ToString()!;
                item.KhoNhan = item.Mahang switch
                {
                    var s when s!.Contains("E") || s!.Contains("A") => "F2",
                    var s when s!.Contains("I") => "IT",
                    var s when s!.Contains("B") || s!.Contains("C") => "F1",
                    _ => khoi switch // không có mã hàng sẽ gán theo khối
                    {
                        "PROD" => "F2",
                        "GA" => "F1",
                        _ => khoi
                    }
                };
                if (!string.IsNullOrEmpty(item.Id_Goc))
                {
                    _db.ReturnString("DELETE FROM [IM_PO_DETAIL] WHERE [PO_Detail_Id] IN (" + item.Id_nhapkho + ") ");
                }
                if (item.benXacNhanTruoc!.Trim().Equals("STOCK"))
                {
                    string Con = item.Id_nhapkho!;
                    string Idcaccon = "";
                    while (Con != "")
                    {
                        Con = _db.ReturnString("SELECT [PO_Detail_Id] FROM [IM_PO_DETAIL] WHERE [Id_Goc] = '" + Con + "' ");
                        if (Con != "")
                        {
                            Idcaccon = Idcaccon + ",'" + Con + "'";
                        }
                    }
                    var Donglienquan = _db.GET_DATA_FROM_SQL("SELECT * FROM [IM_PO_DETAIL] WHERE [PO_Detail_Id] IN ('" + item.PO_Detail_Id + "'" + Idcaccon + ") ");
                    foreach (DataRow rDonglienquan in Donglienquan.Rows)
                    {
                        if (item.luongvethuctekho!.Trim() != "")
                        {
                            if (item.Mahang!.Trim().Equals(""))
                            {
                                double Luongnhapkho = Convert.ToDouble(Convert.ToDouble(rDonglienquan["Luongvekho"].ToString()!.Trim()));
                                string DonviRequest = _db.ReturnString("SELECT [Unit] FROM [MATERIAL] WHERE [Material_Code] = '" + item.Mahang.Trim() + "' ");
                                string Quydoi = _db.ReturnString("SELECT [Soluongquydoi] FROM [KHO_DONVIQUYDOI] WHERE [MaNguyenLieu] = '" + item.Mahang.ToString().Trim() + "' AND [DonviRequest] = N'" + DonviRequest + "' AND [DonviPO] = N'" + item.Donvi + "' ");
                                if (Quydoi != "")
                                {
                                    Luongnhapkho = Convert.ToDouble(Quydoi) * Luongnhapkho;
                                }
                                string Kho = "";
                                if (rDonglienquan["LuongvekhoKhonhap"].ToString()!.Trim() != "")
                                {
                                    Kho = rDonglienquan["LuongvekhoKhonhap"].ToString()!.Trim();
                                }
                                else
                                {
                                    Kho = item.KhoNhan;
                                }
                                _db.GET_DATA_FROM_SQL("UPDATE KHO SET [Hientai] = [Hientai] - " + Luongnhapkho + " WHERE [MaNguyenLieu] =  N'" + item.Mahang.Trim() + "' AND [Kho] = '" + Kho + "' AND [Group_Code] = '" + khoi + "'");
                                _db.GET_DATA_FROM_SQL("INSERT INTO [KHO_XOA] SELECT * FROM [KHO_NHAPXUAT] WHERE [Id_Lichsu] = '" + rDonglienquan["Id_LichsuNhap"].ToString()!.Trim() + "' ");
                                _db.GET_DATA_FROM_SQL("DELETE FROM [KHO_NHAPXUAT] WHERE [Id_Lichsu] = '" + rDonglienquan["Id_LichsuNhap"].ToString()!.Trim() + "' ");
                            }
                        }
                    }
                    _db.GET_DATA_FROM_SQL("INSERT INTO [KHO_XOA] SELECT * FROM [KHO_NHAPXUAT] WHERE [Id_Lichsu] = '" + item.Id_Lichsu + "' ");
                    _db.GET_DATA_FROM_SQL("DELETE FROM [KHO_NHAPXUAT] WHERE [Id_Lichsu] = '" + item.Id_Lichsu!.Trim() + "' ");
                    _db.GET_DATA_FROM_SQL("UPDATE [IM_PO_DETAIL] SET Sotien=Soluong*Dongia,DoisangUSD=(Soluong*Dongia)/Tygia, [Luongvekho] = NULL,[LuongvekhoNguoinhap] = NULL,[LuongvekhoNgaynhap] = NULL,[LuongvekhoKhonhap] = NULL,[Benxacnhantruoc] = NULL, LuongvekhoDanhap = NULL WHERE [PO_Detail_Id] = '" + item.Id_nhapkho + "' ");
                  
                }
                else
                {
                    _db.GET_DATA_FROM_SQL("UPDATE [IM_PO_DETAIL] SET Sotien=Soluong*Dongia,DoisangUSD=(Soluong*Dongia)/Tygia, [Luongvekho] = NULL,[LuongvekhoKhonhap] = NULL,[LuongvekhoNguoinhap] = NULL,[LuongvekhoNgaynhap] = NULL, LuongvekhoDanhap = NULL WHERE [PO_Detail_Id] = '" + item.Id_nhapkho + "' "); //16/5/2025 mai sửa : [LuongvekhoNgaynhap] = Getdate(), [LuongvekhoNguoinhap] = '" + User + "'
                    double Luongnhapkho = Convert.ToDouble(item.luongvethuctekho!.ToString().Trim());

                    _db.GET_DATA_FROM_SQL("UPDATE KHO SET [Hientai] = [Hientai] - " + Luongnhapkho + " WHERE [MaNguyenLieu] =  N'" + item.Mahang!.Trim() + "' AND [Kho] = '" + item.KhoNhan + "' AND [Group_Code] = '" + khoi + "'");
                    _db.GET_DATA_FROM_SQL("INSERT INTO [KHO_XOA] SELECT * FROM [KHO_NHAPXUAT] WHERE [Id_Lichsu] = '" + item.Id_Lichsu!.Trim() + "' ");
                    _db.GET_DATA_FROM_SQL("DELETE FROM [KHO_NHAPXUAT] WHERE [Id_Lichsu] = '" + item.Id_Lichsu.Trim() + "' ");
                }
                _db.GET_DATA_FROM_SQL("INSERT INTO [IM_LOG]([Loai],[SoPO],[PO_Detail_Id],[Hanhdong],[Thogian],[Nguoicapnhat]) VALUES ('DM','" + item.PO_Detail_Id + "','" + item.Id_nhapkho + "',N'Kho reset số lượng về',Getdate(),'" + item.UserName + "')");
                _db.GET_DATA_FROM_SQL("UPDATE [IM_PO] SET [Nguoixacnhan] = '" + item.UserName + "',[Thoigianxacnhan] = GETDATE() WHERE [SoPO] = '" + item.PO_Detail_Id + "' ");
                UpdateTinhTrangPO(item.PO_Detail_Id!);

                return Json("Thành công !");
                
            }
            catch (Exception ex)
            {
                return Json("Lỗi :" + ex);
            }
        }
    }
}
