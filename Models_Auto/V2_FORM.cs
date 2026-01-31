using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class V2_FORM
{
    public int Id_Madon { get; set; }

    public string MaDon { get; set; } = null!;

    public string? LoaiDon { get; set; }

    public string? Cost_Center { get; set; }

    public string? TenPhong { get; set; }

    public string? LoaiChiPhi { get; set; }

    public string? NguoiYeuCau { get; set; }

    public string? MucDich { get; set; }

    public DateOnly? NgayYeuCau { get; set; }

    public DateOnly? ThangTinhChiPhi { get; set; }

    public double? TongTienUSD { get; set; }

    public double? TongTienNT { get; set; }

    public string? LoaiTien { get; set; }

    public double? TyGia { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? TinhTrang { get; set; }

    public string? Note { get; set; }

    public string? NguoiTao { get; set; }

    public string? Nguoicapnhat { get; set; }

    public DateTime? Ngaycapnhat { get; set; }

    public string? Group_Code { get; set; }

    public bool? Tygiangoai { get; set; }

    public virtual DEPARTMENT? Cost_CenterNavigation { get; set; }

    public virtual ICollection<V2_FORM_CHITIET> V2_FORM_CHITIETs { get; set; } = new List<V2_FORM_CHITIET>();
}
