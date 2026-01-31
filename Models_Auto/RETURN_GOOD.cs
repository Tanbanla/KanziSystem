using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class RETURN_GOOD
{
    public int Id { get; set; }

    public string? MaSanPham { get; set; }

    public string? TenSanPham { get; set; }

    public string? TinhNang { get; set; }

    public double? SoLuong { get; set; }

    public string? NoiCatGiu { get; set; }

    public string? PhongBanCoHang { get; set; }

    public double? SoDienThoai { get; set; }

    public string? HinhAnh { get; set; }

    public string? NguoiUp { get; set; }

    public DateTime? ThoiGianUp { get; set; }

    public string? TinhTrang { get; set; }

    public string? Ghichu { get; set; }

    public string? Width { get; set; }
}
