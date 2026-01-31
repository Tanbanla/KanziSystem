using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

/// <summary>
/// Bảng lưu dữ liệu lịch sử thay đổi bảng MST_VENDER
/// </summary>
public partial class HISTORY_CHANGE_VENDER
{
    public int ID { get; set; }

    /// <summary>
    /// Lưu dữ liệu vender được thay đổi dạng JSON ( sử dụng điều tra lỗi khi có vấn đề)
    /// </summary>
    public string NCHR_CHANGE_VENDER { get; set; } = null!;

    /// <summary>
    /// Thời gian chỉnh sửa
    /// </summary>
    public DateTime DTM_EDITED { get; set; }

    public string CHR_USER_EDIT { get; set; } = null!;
}
