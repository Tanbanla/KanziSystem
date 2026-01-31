using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class ACC_NHOMVITRI_DEPARTMENT_VITRI
{
    public int Id_Nhom { get; set; }

    public string Mahangmuctheovitri { get; set; } = null!;

    public string MaChuyen { get; set; } = null!;

    public string Cost { get; set; } = null!;

    public virtual ACC_NHOMVITRI MahangmuctheovitriNavigation { get; set; } = null!;
}
