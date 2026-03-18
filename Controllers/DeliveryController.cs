using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Data;
using System.Transactions;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{

    public class SearchPoPayload
    {
        public string? PoNumber { get; set; }
        public string? Department { get; set; }
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
            string WhereCmd = string.Empty;

            if (data.PoNumber!.Trim() == "") return Json(null);
            if (data.PoNumber.Contains(','))
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

            string cmdQry = $"SELECT {sqlColumn} FROM [PO] WHERE {WhereCmd} [TinhtrangPO] Not in ('DANGCHOXACNHAN','HUY') AND [Group_Code] = '{data.Department}' ORDER BY [SoPO] DESC, Hienthi ASC";

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
            Models.SQL_Connect_DB20 db = new Models.SQL_Connect_DB20();
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

            string cmdQry = $"SELECT {sqlColumn} FROM [PO] WHERE [SoPO] = '{data.PO_Detail_Id}'";

            //Console.WriteLine("abc");
            var dataPO = Models.PO.GetPoByPoIdentify(cmdQry);
            if (dataPO.Count == 0) return Json($"Không tìm thấy ID {data.Id_nhapkho} của mục PO");
            string UserName = User.Identity?.Name is not null ? User.Identity?.Name!.Split('\\').Last() : "luannd";

            string Khoi = db.ReturnString("SELECT [Group_Code] FROM [COST_MANAGEMENT].[dbo].[GROUP_MEMBER] WHERE [CHR_USERID] = '" + UserName + "'");
            if (Khoi == "") Khoi = "PROD";
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
                    double.TryParse(dataPO[0].Dongia, out Dongia);
                    double Sotien = Soluongmoi * Dongia;
                    double VAT_Unit = 0.0;
                    double.TryParse(dataPO[0].Vat, out VAT_Unit);
                    double VAT = (Sotien * VAT_Unit) / 100;

                    Sotien = Sotien + VAT;
                    double DoisangUSD = 0.0;
                    double.TryParse(dataPO[0].Tygia, out DoisangUSD);

                    string Insert = "INSERT INTO IM_PO_DETAIL([SoPO],[Tentienganh],[Tentiengviet],[Mahang],[Soluong],[Dovi],[Dongia],[Dieukiengiaohang],[Diadiemgiaohang],[Phuongthucvanchuyen],[Sotien],[Vat],[Maphongyeucau],[Tenphongyeucau],[Ngaygiaohangdukien],[Noigiaodukien],[Thoigianthanhtoan],[Code_Request],[Id_RequestDetail],[Loaitien],[Tygia],[DoisangUSD],[Danhmuc],[Id_Goc],[Hienthi],[Benxacnhantruoc],[Good_Code]) ";
                    Insert += $" SELECT [SoPO],[Tentienganh],[Tentiengviet],[Mahang],'{Soluongmoi}',[Dovi],[Dongia],[Dieukiengiaohang],[Diadiemgiaohang],[Phuongthucvanchuyen],'{Sotien}',[Vat],[Maphongyeucau],[Tenphongyeucau],[Ngaygiaohangdukien],[Noigiaodukien],[Thoigianthanhtoan],[Code_Request],[Id_RequestDetail],[Loaitien],[Tygia],'{DoisangUSD}',[Danhmuc],[PO_Detail_Id],[Hienthi] + 1,'STOCK',[Good_Code]";
                    Insert += $" FROM IM_PO_DETAIL WHERE PO_Detail_Id = '{data.Id_nhapkho}' ";
                    db.GET_DATA_FROM_SQL(Insert);
                }

                double Luongvekho = 0.0;
                double.TryParse(dataPO[0].Luongvekho, out Luongvekho);

                double Dongiaroot = 0.0;
                double.TryParse(dataPO[0].Dongia, out Dongiaroot);

                double Sotienroot = Luongvekho * Dongiaroot;
                double VAT_Unit_2 = 0.0;
                double.TryParse(dataPO[0].Vat, out VAT_Unit_2);
                double VAT_2 = Sotienroot * VAT_Unit_2 / 100;
                Sotienroot += VAT_2;

                double DoisangUSDroot = 0.0;
                double.TryParse(dataPO[0].Tygia, out DoisangUSDroot);
                DoisangUSDroot = Sotienroot / DoisangUSDroot;

                db.GET_DATA_FROM_SQL($"UPDATE [IM_PO_DETAIL] SET [Luongvekho] = '{dataPO[0].Luongvekho}', LuongvekhoNgaynhap = '{data.NgayNhap}', [LuongvekhoNguoinhap] = '{UserName}', LuongvekhoKhonhap = '{data.KhoNhan}', Sotien = '{Sotienroot}', DoisangUSD = '{DoisangUSDroot}', [Benxacnhantruoc] = 'STOCK', LuongvekhoDanhap = 'True' WHERE [PO_Detail_Id] = '{data.PO_Detail_Id}' ");
            }
            else
            {
                double Luongvekho = 0.0;
                double.TryParse(dataPO[0].Luongvekho, out Luongvekho);

                double Dongiaroot = 0.0;
                double.TryParse(dataPO[0].Dongia, out Dongiaroot);

                double Sotienroot = Luongvekho * Dongiaroot;
                double DoisangUSDroot = 0.0;
                double.TryParse(dataPO[0].Tygia, out DoisangUSDroot);
                DoisangUSDroot = Sotienroot / DoisangUSDroot;

                db.GET_DATA_FROM_SQL($"UPDATE [IM_PO_DETAIL] SET [Luongvekho] = '{dataPO[0].Luongvekho}', LuongvekhoNgaynhap = '{data.NgayNhap}', [LuongvekhoNguoinhap] = '{UserName}', LuongvekhoKhonhap = '{data.KhoNhan}', Sotien = '{Sotienroot}', DoisangUSD = '{DoisangUSDroot}', LuongvekhoDanhap = 'True' WHERE [PO_Detail_Id] = '{data.PO_Detail_Id}' ");
            }

            string Lydo = "";
            if (!data.Mahang.Trim().Equals("")) // Không có mã hàng là hàng ngoài danh mục ....
            {
                double Luongnhapkho = 0.0;
                double.TryParse(data.luongvethuctekho.Trim(), out Luongnhapkho);

                string DonviRequest = db.ReturnString($"SELECT [Unit] FROM [MATERIAL] WHERE [Material_Code] = N'{data.Mahang}'");
                string Quydoi = db.ReturnString($"SELECT [Soluongquydoi] FROM [KHO_DONVIQUYDOI] WHERE [MaNguyenLieu] = '{data.Mahang}' AND [DonviRequest] = '{DonviRequest}' AND [DonviPO] = N'{dataPO[0].Dovi}' ");

                if (Quydoi != "")
                {
                    Luongnhapkho = double.Parse(Quydoi) * Luongnhapkho;
                }
                //Nhập kho
                string Soluonghientai = db.ReturnString($"SELECT [Hientai] FROM KHO WHERE [MaNguyenLieu] =  N'{data.Mahang}' AND [Kho] = '{data.KhoNhan}' AND [Group_Code] = '{Khoi}'");
                double SoluongTruocthaydoi = 0;

                if (dataPO[0].Benxacnhantruoc == "" || dataPO[0].Benxacnhantruoc!.Equals("STOCK")) // Thực hiện với hàng kho của PR1-MC/GA
                {
                    if (Soluonghientai.Trim() == "")
                    {
                        db.ReturnString($"INSERT INTO KHO(MaNguyenLieu,Hientai,Group_Code,Kho) VALUES (N'{data.Mahang}','{data.luongvethuctekho}','{Khoi}','{data.KhoNhan}')");
                    }
                    else
                    {
                        db.ReturnString($"UPDATE KHO SET [Hientai] = [Hientai] + {data.luongvethuctekho} WHERE [MaNguyenLieu] =  N'{data.Mahang}' AND  [Kho] = '{data.KhoNhan}' AND [Group_Code] = '{Khoi}'");
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
                        CHR_GROUP_CODE = Khoi
                    };
                    Models.KHO_TMP.InsertDataImport(tmp);
                }

                string Manhanvien = db.ReturnString($"SELECT CHR_CRT_USERID FROM [TM_USER] WHERE [CHR_USERID] = '{UserName}'");
                double Soluongconlai = 0;
                double SoluongPO = 0;
                if (Quydoi != "")
                {
                    double.TryParse(data.Soluong, out SoluongPO);
                    Soluongconlai = SoluongPO - Luongnhapkho;
                }

                db.ReturnString($"DELETE FROM [KHO_NHAPXUAT] WHERE [Id_Lichsu] = '{dataPO[0].Id_LichsuNhap}'");
                string Id_Lichsu = db.ReturnString($"INSERT INTO [KHO_NHAPXUAT]([MaNguyenLieu],[Hanhdong],[Soluong],[Loai],[Thoigian],[Nguoicapnhat],[Kho],[Khoi],[TenNguyenlieu],[NCC],[Donvi],[MaNguoinhap],[Gia],[SoPO],[SoluongPO],[DonviPO],[Soluongconlai],[Ngaynhaokho],[Soluongtruocthaydoi],[Soluongsauthaydoi]) OUTPUT Inserted.Id_Lichsu VALUES(N'{dataPO[0].Mahang}',N'Nhập kho {data.KhoNhan} từ PO: {dataPO[0].SoPO} -> {Lydo}','{Luongnhapkho}','NHAP',GETDATE(),'{UserName}','{data.KhoNhan}','{Khoi}',N'{dataPO[0].Tentienganh}',N'{dataPO[0].TenNCC}',N'{DonviRequest}','{Manhanvien}','{dataPO[0].Dongia}','{dataPO[0].SoPO}','{SoluongPO}',N'{dataPO[0].Dovi}','{Soluongconlai}','{data.NgayNhap}','{SoluongTruocthaydoi}','{Luongnhapkho + SoluongTruocthaydoi}')");
                db.ReturnString($"UPDATE [IM_PO_DETAIL] SET [Id_LichsuNhap] = '{Id_Lichsu}' WHERE [PO_Detail_Id] = '{dataPO[0].PO_Detail_Id}'");
            }

            db.ReturnString($"INSERT INTO [IM_LOG]([Loai],[SoPO],[PO_Detail_Id],[Hanhdong],[Thogian],[Nguoicapnhat]) VALUES  ('DM','{data.PO_Detail_Id}','{data.Id_nhapkho}',N'Nhập kho',Getdate(),'{UserName}')");
            db.ReturnString($"UPDATE [IM_PO] SET [Nguoixacnhan] = '{UserName}',[Thoigianxacnhan] = GETDATE() WHERE [SoPO] = '{data.PO_Detail_Id}'");

            UpdateTinhTrangPO(data.PO_Detail_Id!);
            return Json("OK");
        }
    }
}
