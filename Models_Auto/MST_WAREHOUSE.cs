using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

/// <summary>
/// Quản lý danh sách kho tại BIVN Được quản lý bởi từng phòng ban.
/// </summary>
public partial class MST_WAREHOUSE
{
    public int ID { get; set; }

    /// <summary>
    /// Tên kho Thực hiện lưu theo format : [Nhà máy]_[Phòng ban]_[Số thứ tự]
    /// </summary>
    public string CHR_WAREHOUSE { get; set; } = null!;

    /// <summary>
    /// Phòng quản lý kho ( Đối với kho sử dụng cho nhiều phòng thì cần 1 phòng đại diện quản lý)
    /// </summary>
    public string CHR_DEPT_USE { get; set; } = null!;

    /// <summary>
    /// Nhà máy đặt kho
    /// </summary>
    public string CHR_FACTORY { get; set; } = null!;

    /// <summary>
    /// Thời gian thay đổi hoặc thêm mới
    /// </summary>
    public DateTime DTM_UPDATE { get; set; }

    /// <summary>
    /// Người thêm mới hoặc chỉnh sửa gần nhất
    /// </summary>
    public string CHR_USER { get; set; } = null!;

    /// <summary>
    /// Thông tin thêm ( giải thích về mục đích sử dụng kho, cách sử dụng kho,...)
    /// </summary>
    public string CHR_NOTE { get; set; } = null!;
}
