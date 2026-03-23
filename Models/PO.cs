using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PRJ_WAREHOUSE_BIVN.Models
{
    public class PO
    {
        public string? PO_Detail_Id { get; set; }
        public string? Id_Goc { get; set; }
        public string? SoPO { get; set; }
        public string? Code_Request { get; set; }
        public string? Id_RequestDetail { get; set; }
        public string? Good_Code { get; set; }
        public string? Tentienganh { get; set; }
        public string? Tentiengviet { get; set; }
        public string? Mahang { get; set; }
        public string? Soluong { get; set; }
        public string? Dovi { get; set; }
        public double? Dongia { get; set; }
        public string? Dieukiengiaohang { get; set; }
        public string? Diadiemgiaohang { get; set; }
        public string? Phuongthucvanchuyen { get; set; }
        public double? Sotien { get; set; }
        public string? Vat { get; set; }
        public string?  Maphongyeucau { get; set; }
        public string? Tenphongyeucau { get; set; }
        public string? Ngaygiaohangdukien { get; set; }
        public string? Noigiaodukien { get; set; }
        public string? Thoigianthanhtoan { get; set; }
        public string? Loaitien { get; set; }
        public double? Tygia { get; set; }
        public double? DoisangUSD { get; set; }
        public string? Danhmuc { get; set; }
        public string? Invoice { get; set; }
        public string? InvoiceNgaynhap { get; set; }
        public string? InvoiceNguoinhap { get; set; }
        public string? Luongvethucte { get; set; }
        public string? LuongvethucteNgaynhap { get; set; }
        public string? LuongvethucteNguoinhap { get; set; }
        public string? Luongvekho { get; set; }
        public string? LuongvekhoNgaynhap { get; set; }
        public string? LuongvekhoNguoinhap { get; set; }
        public string? Sotokhai { get; set; }
        public string? Ngaydangkytk { get; set; }
        public string? Kiemtratk { get; set; }
        public string? SotokhaiNgaynhap { get; set; }
        public string? SotokhaiNguoinhap { get; set; }
        public string? Tinhtrangtokhai { get; set; }
        public string? Hienthi { get; set; }
        public string? Benxacnhantruoc { get; set; }
        public string? Ngayphathanh { get; set; }
        public string? TinhtrangPO { get; set; }
        public string? TinhtranghaiquanPO { get; set; }
        public string? MaNCC { get; set; }
        public string? TenNCC { get; set; }
        public string? Maphongban { get; set; }
        public string? Nguoixacnhan { get; set; }
        public string? Thoigianxacnhan { get; set; }
        public string? Group_Code { get; set; }
        public string? InvoicePO { get; set; }
        public string? InvoicePODenghithanhtoan { get; set; }
        public string? InvoicePONgaynhap { get; set; }
        public string? InvoicePONguoinhap { get; set; }
        public string? Nguoilamdon { get; set; }
        public string? Ngaytao { get; set; }
        public string? TinhtranghaiquanPONguoinhap { get; set; }
        public string? TinhtranghaiquanPONgaynhap { get; set; }
        public string? Id_LichsuNhap { get; set; }
        public string? LuongvekhoDanhap { get; set; }
        public string? Loaichiphi { get; set; }
        public string? LuongvekhoKhonhap { get; set; }
        public string? Aim { get; set; }
        public string? Loaihinhtokhai { get; set; }
        public string? Account_Code { get; set; }
        public string? Phongchiuchiphi { get; set; }

        public static List<PO> GetPoByPoNumber(string cmd)
        {
            SQL_Connect_DB20 _context = new SQL_Connect_DB20();
            var data = _context.GET_DATA_FROM_SQL(cmd);
            List<PO> result = new List<PO>();
            for (int idx = 0; idx < data.Rows.Count; idx++)
            {
                PO po = new PO()
                {
                    Account_Code = data.Rows[idx]["Account_Code"] is null ? "" : data.Rows[idx]["Account_Code"].ToString()!,
                    Aim = data.Rows[idx]["Aim"] is null ? "" : data.Rows[idx]["Aim"].ToString()!,
                    Benxacnhantruoc = data.Rows[idx]["Benxacnhantruoc"] is null ? "" : data.Rows[idx]["Benxacnhantruoc"].ToString()!,
                    Code_Request = data.Rows[idx]["Code_Request"] is null ? "" : data.Rows[idx]["Code_Request"].ToString()!,
                    Danhmuc = data.Rows[idx]["Danhmuc"] is null ? "" : data.Rows[idx]["Danhmuc"].ToString()!,
                    Diadiemgiaohang = data.Rows[idx]["Diadiemgiaohang"] is null ? "" : data.Rows[idx]["Diadiemgiaohang"].ToString()!,
                    Dieukiengiaohang = data.Rows[idx]["Dieukiengiaohang"] is null ? "" : data.Rows[idx]["Dieukiengiaohang"].ToString()!,
                    DoisangUSD = double.Parse(data.Rows[idx]["DoisangUSD"] is null ? "0" : data.Rows[idx]["DoisangUSD"].ToString()!),
                    Dongia = double.Parse(data.Rows[idx]["Dongia"] is null ? "0" : data.Rows[idx]["Dongia"].ToString()!),
                    Dovi = data.Rows[idx]["Dovi"] is null ? "" : data.Rows[idx]["Dovi"].ToString()!,
                    Good_Code = data.Rows[idx]["Good_Code"] is null ? "" : data.Rows[idx]["Good_Code"].ToString()!,
                    Group_Code = data.Rows[idx]["Group_Code"] is null ? "" : data.Rows[idx]["Group_Code"].ToString()!,
                    Hienthi = data.Rows[idx]["Hienthi"] is null ? "" : data.Rows[idx]["Hienthi"].ToString()!,
                    Id_Goc = data.Rows[idx]["Id_Goc"] is null ? "" : data.Rows[idx]["Id_Goc"].ToString()!,
                    Id_LichsuNhap = data.Rows[idx]["Id_LichsuNhap"] is null ? "" : data.Rows[idx]["Id_LichsuNhap"].ToString()!,
                    Id_RequestDetail = data.Rows[idx]["Id_RequestDetail"] is null ? "" : data.Rows[idx]["Id_RequestDetail"].ToString()!,
                    Invoice = data.Rows[idx]["Invoice"] is null ? "" : data.Rows[idx]["Invoice"].ToString()!,
                    InvoiceNgaynhap = data.Rows[idx]["InvoiceNgaynhap"] is null ? "" : data.Rows[idx]["InvoiceNgaynhap"].ToString()!,

                    InvoiceNguoinhap = data.Rows[idx]["InvoiceNguoinhap"] is null ? "" : data.Rows[idx]["InvoiceNguoinhap"].ToString()!,
                    InvoicePO = data.Rows[idx]["InvoicePO"] is null ? "" : data.Rows[idx]["InvoicePO"].ToString()!,
                    InvoicePODenghithanhtoan = data.Rows[idx]["InvoicePODenghithanhtoan"] is null ? "" : data.Rows[idx]["InvoicePODenghithanhtoan"].ToString()!,
                    InvoicePONgaynhap = data.Rows[idx]["InvoicePONgaynhap"] is null ? "" : data.Rows[idx]["InvoicePONgaynhap"].ToString()!,
                    InvoicePONguoinhap = data.Rows[idx]["InvoicePONguoinhap"] is null ? "" : data.Rows[idx]["InvoicePONguoinhap"].ToString()!,
                    Kiemtratk = data.Rows[idx]["Kiemtratk"] is null ? "" : data.Rows[idx]["Kiemtratk"].ToString()!,
                    Loaichiphi = data.Rows[idx]["Loaichiphi"] is null ? "" : data.Rows[idx]["Loaichiphi"].ToString()!,
                    Loaihinhtokhai = data.Rows[idx]["Loaihinhtokhai"] is null ? "" : data.Rows[idx]["Loaihinhtokhai"].ToString()!,
                    Loaitien = data.Rows[idx]["Loaitien"] is null ? "" : data.Rows[idx]["Loaitien"].ToString()!,
                    Luongvekho = data.Rows[idx]["Luongvekho"] is null ? "" : data.Rows[idx]["Luongvekho"].ToString()!,
                    LuongvekhoDanhap = data.Rows[idx]["LuongvekhoDanhap"] is null ? "" : data.Rows[idx]["LuongvekhoDanhap"].ToString()!,
                    LuongvekhoKhonhap = data.Rows[idx]["LuongvekhoKhonhap"] is null ? "" : data.Rows[idx]["LuongvekhoKhonhap"].ToString()!,

                    LuongvekhoNgaynhap = data.Rows[idx]["LuongvekhoNgaynhap"] is null ? "" : data.Rows[idx]["LuongvekhoNgaynhap"].ToString()!,
                    LuongvekhoNguoinhap = data.Rows[idx]["LuongvekhoNguoinhap"] is null ? "" : data.Rows[idx]["LuongvekhoNguoinhap"].ToString()!,
                    Luongvethucte = data.Rows[idx]["Luongvethucte"] is null ? "" : data.Rows[idx]["Luongvethucte"].ToString()!,
                    LuongvethucteNgaynhap = data.Rows[idx]["LuongvethucteNgaynhap"] is null ? "" : data.Rows[idx]["LuongvethucteNgaynhap"].ToString()!,
                    LuongvethucteNguoinhap = data.Rows[idx]["LuongvethucteNguoinhap"] is null ? "" : data.Rows[idx]["LuongvethucteNguoinhap"].ToString()!,

                    Mahang = data.Rows[idx]["Mahang"] is null ? "" : data.Rows[idx]["Mahang"].ToString()!,
                    MaNCC = data.Rows[idx]["MaNCC"] is null ? "" : data.Rows[idx]["MaNCC"].ToString()!,
                    Maphongban = data.Rows[idx]["Maphongban"] is null ? "" : data.Rows[idx]["Maphongban"].ToString()!,
                    Maphongyeucau = data.Rows[idx]["Maphongban"] is null ? "" : data.Rows[idx]["Maphongyeucau"].ToString()!,
                    Ngaydangkytk = data.Rows[idx]["Ngaydangkytk"] is null ? "" : data.Rows[idx]["Ngaydangkytk"].ToString()!,

                    Ngaygiaohangdukien = data.Rows[idx]["Ngaygiaohangdukien"] is null ? "" : data.Rows[idx]["Ngaygiaohangdukien"].ToString()!,
                    Ngayphathanh = data.Rows[idx]["Ngayphathanh"] is null ? "" : data.Rows[idx]["Ngayphathanh"].ToString()!,
                    Ngaytao = data.Rows[idx]["Ngaytao"] is null ? "" : data.Rows[idx]["Ngaytao"].ToString()!,
                    Nguoilamdon = data.Rows[idx]["Nguoilamdon"] is null ? "" : data.Rows[idx]["Nguoilamdon"].ToString()!,
                    Nguoixacnhan = data.Rows[idx]["Nguoixacnhan"] is null ? "" : data.Rows[idx]["Nguoixacnhan"].ToString()!,
                    Noigiaodukien = data.Rows[idx]["Noigiaodukien"] is null ? "" : data.Rows[idx]["Noigiaodukien"].ToString()!,
                    Phongchiuchiphi = data.Rows[idx]["Phongchiuchiphi"] is null ? "" : data.Rows[idx]["Phongchiuchiphi"].ToString()!,
                    Phuongthucvanchuyen = data.Rows[idx]["Phuongthucvanchuyen"] is null ? "" : data.Rows[idx]["Phuongthucvanchuyen"].ToString()!,
                    PO_Detail_Id = data.Rows[idx]["PO_Detail_Id"] is null ? "" : data.Rows[idx]["PO_Detail_Id"].ToString()!,
                    Soluong = data.Rows[idx]["Soluong"] is null ? "" : data.Rows[idx]["Soluong"].ToString()!,
                    SoPO = data.Rows[idx]["SoPO"] is null ? "" : data.Rows[idx]["SoPO"].ToString()!,
                    Sotien = double.Parse(data.Rows[idx]["Sotien"] is null ? "0" : data.Rows[idx]["Sotien"].ToString()!),
                    Sotokhai = data.Rows[idx]["Sotokhai"] is null ? "" : data.Rows[idx]["Sotokhai"].ToString()!,
                    SotokhaiNgaynhap = data.Rows[idx]["SotokhaiNgaynhap"] is null ? "" : data.Rows[idx]["SotokhaiNgaynhap"].ToString()!,
                    SotokhaiNguoinhap = data.Rows[idx]["SotokhaiNguoinhap"] is null ? "" : data.Rows[idx]["SotokhaiNguoinhap"].ToString()!,
                    TenNCC = data.Rows[idx]["TenNCC"] is null ? "" : data.Rows[idx]["TenNCC"].ToString()!,
                    Tenphongyeucau = data.Rows[idx]["Tenphongyeucau"] is null ? "" : data.Rows[idx]["Tenphongyeucau"].ToString()!,
                    Tentienganh = data.Rows[idx]["Tentienganh"] is null ? "" : data.Rows[idx]["Tentienganh"].ToString()!,
                    Tentiengviet = data.Rows[idx]["Tentiengviet"] is null ? "" : data.Rows[idx]["Tentiengviet"].ToString()!,
                    Thoigianthanhtoan = data.Rows[idx]["Thoigianthanhtoan"] is null ? "" : data.Rows[idx]["Thoigianthanhtoan"].ToString()!,
                    Thoigianxacnhan = data.Rows[idx]["Thoigianxacnhan"] is null ? "" : data.Rows[idx]["Thoigianxacnhan"].ToString()!,
                    TinhtranghaiquanPO = data.Rows[idx]["TinhtranghaiquanPO"] is null ? "" : data.Rows[idx]["TinhtranghaiquanPO"].ToString()!,
                    TinhtranghaiquanPONgaynhap = data.Rows[idx]["TinhtranghaiquanPONgaynhap"] is null ? "" : data.Rows[idx]["TinhtranghaiquanPONgaynhap"].ToString()!,
                    TinhtranghaiquanPONguoinhap = data.Rows[idx]["TinhtranghaiquanPONguoinhap"] is null ? "" : data.Rows[idx]["TinhtranghaiquanPONguoinhap"].ToString()!,

                    TinhtrangPO = data.Rows[idx]["TinhtrangPO"] is null ? "" : data.Rows[idx]["TinhtrangPO"].ToString()!,
                    Tinhtrangtokhai = data.Rows[idx]["Tinhtrangtokhai"] is null ? "" : data.Rows[idx]["Tinhtrangtokhai"].ToString()!,
                    Tygia = double.Parse(data.Rows[idx]["Tygia"] is null ? "0" : data.Rows[idx]["Tygia"].ToString()!),
                    Vat = data.Rows[idx]["Vat"] is null ? "" : data.Rows[idx]["Vat"].ToString()!
                };


                result.Add(po);
            }
            return result;
        }

        public static List<PO> GetPoByPoIdentify(string cmd)
        {
            SQL_Connect_DB20 _context = new SQL_Connect_DB20();
            var data = _context.GET_DATA_FROM_SQL(cmd);
            List<PO> result = new List<PO>();
            for (int idx = 0; idx < data.Rows.Count; idx++)
            {
                PO po = new PO()
                {
                    Account_Code = data.Rows[idx]["Account_Code"] is null ? "" : data.Rows[idx]["Account_Code"].ToString()!,
                    Aim = data.Rows[idx]["Aim"] is null ? "" : data.Rows[idx]["Aim"].ToString()!,
                    Benxacnhantruoc = data.Rows[idx]["Benxacnhantruoc"] is null ? "" : data.Rows[idx]["Benxacnhantruoc"].ToString()!,
                    Code_Request = data.Rows[idx]["Code_Request"] is null ? "" : data.Rows[idx]["Code_Request"].ToString()!,
                    Danhmuc = data.Rows[idx]["Danhmuc"] is null ? "" : data.Rows[idx]["Danhmuc"].ToString()!,
                    Diadiemgiaohang = data.Rows[idx]["Diadiemgiaohang"] is null ? "" : data.Rows[idx]["Diadiemgiaohang"].ToString()!,
                    Dieukiengiaohang = data.Rows[idx]["Dieukiengiaohang"] is null ? "" : data.Rows[idx]["Dieukiengiaohang"].ToString()!,
                    DoisangUSD = double.Parse(data.Rows[idx]["DoisangUSD"] is null ? "0" : data.Rows[idx]["DoisangUSD"].ToString()!),
                    Dongia = double.Parse(data.Rows[idx]["Dongia"] is null ? "0" : data.Rows[idx]["Dongia"].ToString()!),
                    Dovi = data.Rows[idx]["Dovi"] is null ? "" : data.Rows[idx]["Dovi"].ToString()!,
                    Good_Code = data.Rows[idx]["Good_Code"] is null ? "" : data.Rows[idx]["Good_Code"].ToString()!,
                    Group_Code = data.Rows[idx]["Group_Code"] is null ? "" : data.Rows[idx]["Group_Code"].ToString()!,
                    Hienthi = data.Rows[idx]["Hienthi"] is null ? "" : data.Rows[idx]["Hienthi"].ToString()!,
                    Id_Goc = data.Rows[idx]["Id_Goc"] is null ? "" : data.Rows[idx]["Id_Goc"].ToString()!,
                    Id_LichsuNhap = data.Rows[idx]["Id_LichsuNhap"] is null ? "" : data.Rows[idx]["Id_LichsuNhap"].ToString()!,
                    Id_RequestDetail = data.Rows[idx]["Id_RequestDetail"] is null ? "" : data.Rows[idx]["Id_RequestDetail"].ToString()!,
                    Invoice = data.Rows[idx]["Invoice"] is null ? "" : data.Rows[idx]["Invoice"].ToString()!,
                    InvoiceNgaynhap = data.Rows[idx]["InvoiceNgaynhap"] is null ? "" : data.Rows[idx]["InvoiceNgaynhap"].ToString()!,

                    InvoiceNguoinhap = data.Rows[idx]["InvoiceNguoinhap"] is null ? "" : data.Rows[idx]["InvoiceNguoinhap"].ToString()!,
                    InvoicePO = data.Rows[idx]["InvoicePO"] is null ? "" : data.Rows[idx]["InvoicePO"].ToString()!,
                    InvoicePODenghithanhtoan = data.Rows[idx]["InvoicePODenghithanhtoan"] is null ? "" : data.Rows[idx]["InvoicePODenghithanhtoan"].ToString()!,
                    InvoicePONgaynhap = data.Rows[idx]["InvoicePONgaynhap"] is null ? "" : data.Rows[idx]["InvoicePONgaynhap"].ToString()!,
                    InvoicePONguoinhap = data.Rows[idx]["InvoicePONguoinhap"] is null ? "" : data.Rows[idx]["InvoicePONguoinhap"].ToString()!,
                    Kiemtratk = data.Rows[idx]["Kiemtratk"] is null ? "" : data.Rows[idx]["Kiemtratk"].ToString()!,
                    Loaichiphi = data.Rows[idx]["Loaichiphi"] is null ? "" : data.Rows[idx]["Loaichiphi"].ToString()!,
                    Loaihinhtokhai = data.Rows[idx]["Loaihinhtokhai"] is null ? "" : data.Rows[idx]["Loaihinhtokhai"].ToString()!,
                    Loaitien = data.Rows[idx]["Loaitien"] is null ? "" : data.Rows[idx]["Loaitien"].ToString()!,
                    Luongvekho = data.Rows[idx]["Luongvekho"] is null ? "" : data.Rows[idx]["Luongvekho"].ToString()!,
                    LuongvekhoDanhap = data.Rows[idx]["LuongvekhoDanhap"] is null ? "" : data.Rows[idx]["LuongvekhoDanhap"].ToString()!,
                    LuongvekhoKhonhap = data.Rows[idx]["LuongvekhoKhonhap"] is null ? "" : data.Rows[idx]["LuongvekhoKhonhap"].ToString()!,

                    LuongvekhoNgaynhap = data.Rows[idx]["LuongvekhoNgaynhap"] is null ? "" : data.Rows[idx]["LuongvekhoNgaynhap"].ToString()!,
                    LuongvekhoNguoinhap = data.Rows[idx]["LuongvekhoNguoinhap"] is null ? "" : data.Rows[idx]["LuongvekhoNguoinhap"].ToString()!,
                    Luongvethucte = data.Rows[idx]["Luongvethucte"] is null ? "" : data.Rows[idx]["Luongvethucte"].ToString()!,
                    LuongvethucteNgaynhap = data.Rows[idx]["LuongvethucteNgaynhap"] is null ? "" : data.Rows[idx]["LuongvethucteNgaynhap"].ToString()!,
                    LuongvethucteNguoinhap = data.Rows[idx]["LuongvethucteNguoinhap"] is null ? "" : data.Rows[idx]["LuongvethucteNguoinhap"].ToString()!,

                    Mahang = data.Rows[idx]["Mahang"] is null ? "" : data.Rows[idx]["Mahang"].ToString()!,
                    MaNCC = data.Rows[idx]["MaNCC"] is null ? "" : data.Rows[idx]["MaNCC"].ToString()!,
                    Maphongban = data.Rows[idx]["Maphongban"] is null ? "" : data.Rows[idx]["Maphongban"].ToString()!,
                    Maphongyeucau = data.Rows[idx]["Maphongban"] is null ? "" : data.Rows[idx]["Maphongyeucau"].ToString()!,
                    Ngaydangkytk = data.Rows[idx]["Ngaydangkytk"] is null ? "" : data.Rows[idx]["Ngaydangkytk"].ToString()!,

                    Ngaygiaohangdukien = data.Rows[idx]["Ngaygiaohangdukien"] is null ? "" : data.Rows[idx]["Ngaygiaohangdukien"].ToString()!,
                    Ngayphathanh = data.Rows[idx]["Ngayphathanh"] is null ? "" : data.Rows[idx]["Ngayphathanh"].ToString()!,
                    Ngaytao = data.Rows[idx]["Ngaytao"] is null ? "" : data.Rows[idx]["Ngaytao"].ToString()!,
                    Nguoilamdon = data.Rows[idx]["Nguoilamdon"] is null ? "" : data.Rows[idx]["Nguoilamdon"].ToString()!,
                    Nguoixacnhan = data.Rows[idx]["Nguoixacnhan"] is null ? "" : data.Rows[idx]["Nguoixacnhan"].ToString()!,
                    Noigiaodukien = data.Rows[idx]["Noigiaodukien"] is null ? "" : data.Rows[idx]["Noigiaodukien"].ToString()!,
                    Phongchiuchiphi = data.Rows[idx]["Phongchiuchiphi"] is null ? "" : data.Rows[idx]["Phongchiuchiphi"].ToString()!,
                    Phuongthucvanchuyen = data.Rows[idx]["Phuongthucvanchuyen"] is null ? "" : data.Rows[idx]["Phuongthucvanchuyen"].ToString()!,
                    PO_Detail_Id = data.Rows[idx]["PO_Detail_Id"] is null ? "" : data.Rows[idx]["PO_Detail_Id"].ToString()!,
                    Soluong = data.Rows[idx]["Soluong"] is null ? "" : data.Rows[idx]["Soluong"].ToString()!,
                    SoPO = data.Rows[idx]["SoPO"] is null ? "" : data.Rows[idx]["SoPO"].ToString()!,
                    Sotien = double.Parse(data.Rows[idx]["Sotien"] is null ? "0" : data.Rows[idx]["Sotien"].ToString()!),
                    Sotokhai = data.Rows[idx]["Sotokhai"] is null ? "" : data.Rows[idx]["Sotokhai"].ToString()!,
                    SotokhaiNgaynhap = data.Rows[idx]["SotokhaiNgaynhap"] is null ? "" : data.Rows[idx]["SotokhaiNgaynhap"].ToString()!,
                    SotokhaiNguoinhap = data.Rows[idx]["SotokhaiNguoinhap"] is null ? "" : data.Rows[idx]["SotokhaiNguoinhap"].ToString()!,
                    TenNCC = data.Rows[idx]["TenNCC"] is null ? "" : data.Rows[idx]["TenNCC"].ToString()!,
                    Tenphongyeucau = data.Rows[idx]["Tenphongyeucau"] is null ? "" : data.Rows[idx]["Tenphongyeucau"].ToString()!,
                    Tentienganh = data.Rows[idx]["Tentienganh"] is null ? "" : data.Rows[idx]["Tentienganh"].ToString()!,
                    Tentiengviet = data.Rows[idx]["Tentiengviet"] is null ? "" : data.Rows[idx]["Tentiengviet"].ToString()!,
                    Thoigianthanhtoan = data.Rows[idx]["Thoigianthanhtoan"] is null ? "" : data.Rows[idx]["Thoigianthanhtoan"].ToString()!,
                    Thoigianxacnhan = data.Rows[idx]["Thoigianxacnhan"] is null ? "" : data.Rows[idx]["Thoigianxacnhan"].ToString()!,
                    TinhtranghaiquanPO = data.Rows[idx]["TinhtranghaiquanPO"] is null ? "" : data.Rows[idx]["TinhtranghaiquanPO"].ToString()!,
                    TinhtranghaiquanPONgaynhap = data.Rows[idx]["TinhtranghaiquanPONgaynhap"] is null ? "" : data.Rows[idx]["TinhtranghaiquanPONgaynhap"].ToString()!,
                    TinhtranghaiquanPONguoinhap = data.Rows[idx]["TinhtranghaiquanPONguoinhap"] is null ? "" : data.Rows[idx]["TinhtranghaiquanPONguoinhap"].ToString()!,

                    TinhtrangPO = data.Rows[idx]["TinhtrangPO"] is null ? "" : data.Rows[idx]["TinhtrangPO"].ToString()!,
                    Tinhtrangtokhai = data.Rows[idx]["Tinhtrangtokhai"] is null ? "" : data.Rows[idx]["Tinhtrangtokhai"].ToString()!,
                    Tygia = double.Parse(data.Rows[idx]["Tygia"] is null ? "0" : data.Rows[idx]["Tygia"].ToString()!),
                    Vat = data.Rows[idx]["Vat"] is null ? "" : data.Rows[idx]["Vat"].ToString()!
                };


                result.Add(po);
            }
            return result;
        }
    }
}
