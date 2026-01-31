using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class ACC_NHOMVITRI
{
    public int Id_Nhomvitri { get; set; }

    public string? LoaiVitri { get; set; }

    public string Mahangmuctheovitri { get; set; } = null!;

    public string? Tenhangmuctheovitri { get; set; }

    public string? Model { get; set; }

    public virtual ICollection<ACC_NHOMVITRI_DEPARTMENT_VITRI> ACC_NHOMVITRI_DEPARTMENT_VITRIs { get; set; } = new List<ACC_NHOMVITRI_DEPARTMENT_VITRI>();
}
