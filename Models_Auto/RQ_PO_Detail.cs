using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class RQ_PO_Detail
{
    public int Id_RequestDetail { get; set; }

    public string Code_Request { get; set; } = null!;

    public string? Account_Code { get; set; }

    public string? Account_Name { get; set; }

    public string? Aim { get; set; }

    public string? Phongchiuchiphi { get; set; }

    public string? Brand { get; set; }

    public string? Loaihinhtokhai { get; set; }

    public int? PO_Detail_Id { get; set; }

    public string? Good_Code { get; set; }

    public string? Tentienganh { get; set; }

    public string? Tentiengviet { get; set; }

    public string? Mahang { get; set; }

    public string? Material_Code { get; set; }

    public string? Material_Name { get; set; }

    public string? Material_Name_EN { get; set; }

    public double? Soluong { get; set; }

    public double? Amount { get; set; }

    public string? Dovi { get; set; }

    public string? Unit { get; set; }

    public decimal? Dongia { get; set; }

    public double? Price { get; set; }

    public decimal? Sotien { get; set; }

    public double? Price_Real { get; set; }

    public double? Total { get; set; }

    public double? Total_exchange_real { get; set; }

    public double? Vat { get; set; }

    public DateOnly? Ngaygiaohangdukien { get; set; }

    public DateOnly? Dealine { get; set; }

    public string? Noigiaodukien { get; set; }

    public string? Thoigianthanhtoan { get; set; }

    public decimal? Tygia { get; set; }

    public double? Rate { get; set; }

    public string? Loaitien { get; set; }

    public string? Currency { get; set; }

    public decimal? DoisangUSD { get; set; }

    public double? Total_Real { get; set; }

    public double? Total_exchange { get; set; }

    public string? Danhmuc { get; set; }

    public string? Invoice { get; set; }

    public DateTime? InvoiceNgaynhap { get; set; }

    public string? InvoiceNguoinhap { get; set; }

    public double? Luongvethucte { get; set; }

    public double? Luongvekho { get; set; }

    public double? Amount_Real { get; set; }

    public DateOnly? Dealine_Real { get; set; }

    public DateTime? Last_Update { get; set; }

    public string? User_Update { get; set; }

    public string? Status { get; set; }

    public string? Sotokhai { get; set; }

    public DateTime? Ngaydangkytk { get; set; }

    public string? Kiemtratk { get; set; }

    public string? Tinhtrangtokhai { get; set; }

    public int? Hienthi { get; set; }

    public string? SoPO { get; set; }

    public DateOnly? Ngayphathanh { get; set; }

    public string? TinhtrangPO { get; set; }

    public string? TinhtranghaiquanPO { get; set; }

    public string? MaNCC { get; set; }

    public string? TenNCC { get; set; }

    public string? Maphongban { get; set; }

    public string? Nguoixacnhan { get; set; }

    public DateTime? Thoigianxacnhan { get; set; }

    public string? Group_Code { get; set; }

    public string? Nguoilamdon { get; set; }

    public DateTime? Ngaytao { get; set; }

    public string? TinhtranghaiquanPONguoinhap { get; set; }

    public DateTime? TinhtranghaiquanPONgaynhap { get; set; }

    public string? Loaichiphi { get; set; }
}
