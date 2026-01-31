using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

/// <summary>
/// Dữ liệu xuất hàng
/// </summary>
public partial class WF_ORDER_EXPORT
{
    public int ID { get; set; }

    /// <summary>
    /// User lấy theo MST_USER
    /// </summary>
    public int ID_USER_ORDER { get; set; }

    /// <summary>
    /// Chứa ID của Request hàng hóa ( xuất hàng từ kho)
    /// </summary>
    public int ID_REQUEST { get; set; }

    /// <summary>
    /// Thể hiện trạng thái đơn: 0 - Được lưu lại nhưng chưa phát hành 1,2,3,4 ( xác nhận quy trình xin đơn hàng)
    /// </summary>
    public int STATUS { get; set; }

    /// <summary>
    /// Thông tin chi tiết về người phê duyệt, thời gian phê duyệt, nội dung từ chối nếu có.
    /// </summary>
    public string INFORMATION { get; set; } = null!;

    /// <summary>
    /// Thời gian cập nhật cuối
    /// </summary>
    public DateTime DTM_UPDATE { get; set; }

    /// <summary>
    /// Người cập nhật cuối
    /// </summary>
    public string CHR_USER { get; set; } = null!;
}
