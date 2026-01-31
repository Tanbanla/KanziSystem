using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

/// <summary>
/// Quản lý tổng chung toàn bộ linh kiện trong nhà máy
/// </summary>
public partial class TOTAL_WAREHOUSE
{
    public int ID { get; set; }

    /// <summary>
    /// Mã linh kiện đang được quản lý
    /// </summary>
    public string CHR_MATERIAL_CODE { get; set; } = null!;

    /// <summary>
    /// Số lượng linh kiện còn lại trong kho đang được quản lý
    /// </summary>
    public double QTY { get; set; }

    /// <summary>
    /// Số lượng linh kiện ít nhất cần lưu kho ( đối với kho của phòng ban)
    /// </summary>
    public double QTY_MINIMUM { get; set; }

    /// <summary>
    /// Số lượng nhiều nhất được cho phép lưu kho
    /// </summary>
    public double QTY_MAXIMUM { get; set; }

    /// <summary>
    /// ID của tên Unit (lấy từ bảng MST_UNIT)
    /// </summary>
    public int ID_UNIT { get; set; }

    /// <summary>
    /// Vị trí lưu trữ ( vị trí trong kho theo giá, vị trí được phân biệt)
    /// </summary>
    public string CHR_LOCATION { get; set; } = null!;

    /// <summary>
    /// ID của kho đã được khai báo quản lý trong bảng MST_WAREHOUSE
    /// </summary>
    public int ID_WAREHOUSE { get; set; }

    /// <summary>
    /// Mã cost chi phí phòng ban ( bao gồm cả các cost dự án)
    /// </summary>
    public string CHR_COST { get; set; } = null!;

    /// <summary>
    /// Các giải thích thêm, chú thích về linh kiện)
    /// </summary>
    public string CHR_DESCRIPTION { get; set; } = null!;

    public DateTime DTM_UPDATED { get; set; }

    /// <summary>
    /// Cho phép lưu trữ True : Cho phép lưu trữ tại kho False : Không cho phép lưu trữ
    /// </summary>
    public bool IS_SAVE_WH { get; set; }
}
