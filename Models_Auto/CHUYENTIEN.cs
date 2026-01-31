using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class CHUYENTIEN
{
    public int Id_ChuyenTien { get; set; }

    public string MaChuyenTien { get; set; } = null!;

    public string PhongChuyen { get; set; } = null!;

    public double? SotienChuyen { get; set; }

    public string LoaiChiPhiChuyen { get; set; } = null!;

    public DateTime Ngaychuyen { get; set; }

    public string Nguoichuyen { get; set; } = null!;

    public string? Nguoixuly { get; set; }

    public string Tinhtrang { get; set; } = null!;

    public string? Namtaichinh { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<CHUYENTIEN_CHITIET> CHUYENTIEN_CHITIETs { get; set; } = new List<CHUYENTIEN_CHITIET>();
}
