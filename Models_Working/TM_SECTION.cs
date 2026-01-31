using System.ComponentModel.DataAnnotations;

namespace PRJ_WAREHOUSE_BIVN.Models_Working
{
    public class TM_SECTION
    {
        [Key]
        [StringLength(50)] // Tùy độ dài thực tế của cột
        public string CHR_CODE_SEC { get; set; } = null!;

        [StringLength(255)] // NVARCHAR thường dùng cho Unicode text
        public string? NVCHR_SEC { get; set; }

        [StringLength(50)]
        public string? CHR_CODE_DEPT { get; set; }

        [StringLength(50)]
        public string? CHR_CRT_USERID { get; set; }

        public DateTime? DTM_CREATE { get; set; }

        [StringLength(50)]
        public string? CHR_UPD_USERID { get; set; }

        public DateTime? DTM_UPDATE { get; set; }

        public int? INT_ODERBY { get; set; }

        public bool? BIT_USE_LEAVE_RATE { get; set; }
    }
}
