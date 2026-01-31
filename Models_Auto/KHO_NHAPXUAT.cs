using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class KHO_NHAPXUAT
{
    public int Id_Lichsu { get; set; }

    public string MaNguyenLieu { get; set; } = null!;

    public string? Hanhdong { get; set; }

    public double? Soluong { get; set; }

    public string? Loai { get; set; }

    public DateOnly? Ngaynhaokho { get; set; }

    public DateTime? Thoigian { get; set; }

    public string? Nguoicapnhat { get; set; }

    public string? Kho { get; set; }

    public string? Khoi { get; set; }

    public string? Phong { get; set; }

    public string? Vitri { get; set; }

    public string? TenNguyenlieu { get; set; }

    public string? NCC { get; set; }

    public string? Donvi { get; set; }

    public string? MaNguoinhap { get; set; }

    public double? Gia { get; set; }

    public string? SoPO { get; set; }

    public double? SoluongPO { get; set; }

    public string? DonviPO { get; set; }

    public double? Soluongconlai { get; set; }

    public string? Sotaikhoan { get; set; }

    public double? Soluongtruocthaydoi { get; set; }

    public double? Soluongsauthaydoi { get; set; }
}
