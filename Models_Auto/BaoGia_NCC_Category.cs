using System.ComponentModel.DataAnnotations;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto
{
    public partial class BaoGia_NCC_Category
    {
        public int Id { get; set; }

        public string? CHR_MaNCC { get; set; }

        public string? NVCHR_TenNCC { get; set; }

        public string? NVCHR_ChungLoai { get; set; }


        public string? NVCHR_SanXuat { get; set; }

        public string? CHR_Status { get; set; } 

        public string? CHR_CreateBy { get; set; }

        public DateTime? DTM_CreateBy { get; set; }

        public string? CHR_PIC { get; set; }

        public string? CHR_Mail { get; set; }
    }
}
