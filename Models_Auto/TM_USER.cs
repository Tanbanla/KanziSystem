using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TM_USER
{
    public int ID { get; set; }

    public string CHR_USERID { get; set; } = null!;

    public string? VCHR_PASSWORD { get; set; }

    public string? FULLNAME { get; set; }

    public string? CHR_CRT_USERID { get; set; }

    public DateTime? DTM_CREATE { get; set; }

    public DateTime? Lancuoicungdangnhap { get; set; }

    public string CHR_EMPLOYEE_ID { get; set; } = null!;

    public string? CHR_ADID_GROUPUSER { get; set; }

    public DateTime? DTM_LAST_LOGIN { get; set; }

    public decimal? INT_LOCK { get; set; }

    public decimal? INT_LOCK_DAY { get; set; }

    public string? CHR_SECTION { get; set; }

    public int INT_USERID_COMMON { get; set; }

    public string? dia_chi_mail { get; set; }

    public int phan_quyen { get; set; }

    public string? phong_ban { get; set; }

    public DateTime thoi_gian_cap_nhat { get; set; }

    public bool cho_phep_hoat_dong { get; set; }

    public virtual ICollection<EMAIL> EMAILs { get; set; } = new List<EMAIL>();

    public virtual ICollection<GROUP_MEMBER> GROUP_MEMBERs { get; set; } = new List<GROUP_MEMBER>();

    public virtual ICollection<TM_AUTHORITY_MENU> TM_AUTHORITY_MENUs { get; set; } = new List<TM_AUTHORITY_MENU>();

    public virtual ICollection<USER_DEPT> USER_DEPTs { get; set; } = new List<USER_DEPT>();

    public virtual ICollection<TM_AUTHORITY_THEOCHUCNANG> CHR_CODE_FUNCTIONs { get; set; } = new List<TM_AUTHORITY_THEOCHUCNANG>();
}
