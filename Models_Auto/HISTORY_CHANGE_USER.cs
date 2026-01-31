using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class HISTORY_CHANGE_USER
{
    public int ID { get; set; }

    /// <summary>
    /// Lưu dữ liệu lịch sử cài đặt, thay đổi user bằng dạng JSON
    /// </summary>
    public string VCHR_DATA { get; set; } = null!;

    /// <summary>
    /// Thời gian thay đổi
    /// </summary>
    public DateTime DTM_SAVED { get; set; }

    /// <summary>
    /// Người lưu thay đổi
    /// </summary>
    public string CHR_USER_CHANGE { get; set; } = null!;
}
