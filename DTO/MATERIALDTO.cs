using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class MATERIALDTO
{
    public int Id_Material { get; set; }

    public string Material_Code { get; set; } = null!;

    public string? Material_Name_VN { get; set; }

    public string? Material_Name_EN { get; set; }

    public string? Material_Name_JP { get; set; }

    public string? Account_Code { get; set; }

    public string? Account_Name_EN { get; set; }

    public string? Account_Name_VN { get; set; }

    public string? Unit { get; set; }

    public string? Unit_Note { get; set; }

    public double? Price { get; set; }

    public string? Currency { get; set; }

    public string? Group_Code { get; set; }

    public string? GoodKind { get; set; }

    public string? Category_VN { get; set; }

    public string? Category_JP { get; set; }

    public string? Shape { get; set; }

    public string? Material { get; set; }

    public string? Composition { get; set; }

    public string? Dimension { get; set; }

    public string? UsedFor { get; set; }

    public string? Purpose { get; set; }

    public string? Category_EN { get; set; }

    public string? Code_Suppiler { get; set; }

    public string? GetLoaiHang()
    {
        if (string.IsNullOrEmpty(Material_Code))
            return null;
        switch(Material_Code.Substring(0, 1))
        {
            case "A":
                return "A";
            case "B":
                return "B";
            case "C":
                return "C";
            case "E":
                return "E";
            default:
                return "NO LIST";
        }
    }
    // Serialized convenience properties so computed values are available to client-side JavaScript
    public string? LoaiHang => GetLoaiHang();

    public string? GetTenMoThuTuc()
    {
        return Category_VN +" có hình dáng dạng " +Shape + " chất liệu " 
            + Material + " thành phần hóa chất " + Composition + " có kích thước " + Dimension + " dung để " + UsedFor + " cho " + Purpose;
    }

    // Expose TenMoThuTuc as a property so it will be included in JSON responses
    public string? TenMoThuTuc => GetTenMoThuTuc();
    // Name Vi
    public string? NameVI => GetNameVi();
    public string? GetNameVi()
    {
        if (string.IsNullOrEmpty(Material_Code))
            return null;
        switch (Material_Code.Substring(0, 1))
        {
            case "O": return "Có hình dáng dạng " + Shape + " & " + UsedFor + " & " +Purpose;
            default:
                return Material_Name_VN;
        }
    }
}
