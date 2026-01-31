using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class VIEWPO_NCC
{
    public int PO_Detail_Id { get; set; }

    public int? Id_Goc { get; set; }

    public string SoPO { get; set; } = null!;

    public string? Code_Request { get; set; }

    public int? Id_RequestDetail { get; set; }

    public string? Tentiengviet { get; set; }

    public string? Tentienganh { get; set; }

    public string Mahang { get; set; } = null!;

    public double? Soluong { get; set; }

    public string? Dovi { get; set; }

    public decimal? Dongia { get; set; }

    public string? Dieukiengiaohang { get; set; }

    public string? Diadiemgiaohang { get; set; }

    public string? Phuongthucvanchuyen { get; set; }

    public decimal? Sotien { get; set; }

    public double? Vat { get; set; }

    public string? Maphongyeucau { get; set; }

    public string? Tenphongyeucau { get; set; }

    public DateOnly? Ngaygiaohangdukien { get; set; }

    public string? Noigiaodukien { get; set; }

    public string? Thoigianthanhtoan { get; set; }

    public string? Loaitien { get; set; }

    public decimal? Tygia { get; set; }

    public decimal? DoisangUSD { get; set; }

    public string? Danhmuc { get; set; }

    public string? Invoice { get; set; }

    public DateTime? InvoiceNgaynhap { get; set; }

    public string? InvoiceNguoinhap { get; set; }

    public double? Luongvethucte { get; set; }

    public DateTime? LuongvethucteNgaynhap { get; set; }

    public string? LuongvethucteNguoinhap { get; set; }

    public double? Luongvekho { get; set; }

    public DateTime? LuongvekhoNgaynhap { get; set; }

    public string? LuongvekhoNguoinhap { get; set; }

    public string? LuongvekhoKhonhap { get; set; }

    public bool? LuongvekhoDanhap { get; set; }

    public string? Sotokhai { get; set; }

    public DateTime? Ngaydangkytk { get; set; }

    public string? Kiemtratk { get; set; }

    public DateTime? SotokhaiNgaynhap { get; set; }

    public string? SotokhaiNguoinhap { get; set; }

    public string? Tinhtrangtokhai { get; set; }

    public int? Hienthi { get; set; }

    public string? Benxacnhantruoc { get; set; }

    public string? InvoicePO { get; set; }

    public string? InvoicePODenghithanhtoan { get; set; }

    public DateTime? InvoicePONgaynhap { get; set; }

    public string? InvoicePONguoinhap { get; set; }

    public string? Id_LichsuNhap { get; set; }

    public int? Id_PO { get; set; }

    public string? Expr1 { get; set; }

    public string? Loaichiphi { get; set; }

    public DateOnly? Ngayphathanh { get; set; }

    public string? Maphongban { get; set; }

    public string? Tenphongban { get; set; }

    public string? Bophan { get; set; }

    public string? Mucdich { get; set; }

    public string? MaNCC { get; set; }

    public string? TenNCC { get; set; }

    public string? KhuvucNCC { get; set; }

    public string? DiachiNCC { get; set; }

    public string? SodienthoaiNCC { get; set; }

    public string? SofaxNCC { get; set; }

    public DateTime? Ngaytao { get; set; }

    public string? Nguoilamdon { get; set; }

    public string? Hinhthuc { get; set; }

    public string? Group_Code { get; set; }

    public string? Expr2 { get; set; }

    public string? TinhtrangPO { get; set; }

    public string? TinhtranghaiquanPO { get; set; }

    public string? TinhtranghaiquanPONguoinhap { get; set; }

    public DateTime? TinhtranghaiquanPONgaynhap { get; set; }

    public string? Expr3 { get; set; }

    public string? Lydo { get; set; }

    public string? Nguoixacnhan { get; set; }

    public DateTime? Thoigianxacnhan { get; set; }

    public bool? NNCNEW { get; set; }
}
