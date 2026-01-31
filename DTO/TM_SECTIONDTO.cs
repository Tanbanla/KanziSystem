using System.ComponentModel.DataAnnotations;

namespace PRJ_WAREHOUSE_BIVN.DTO
{
    public class TM_SECTIONDTO
    {
        [Key]
        public string CHR_CODE_SEC { get; set; } = null!;
        public string NVCHR_SEC { get; set; } = null!;
    }
}
