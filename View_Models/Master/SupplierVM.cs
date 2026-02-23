namespace PRJ_WAREHOUSE_BIVN.View_Models.Master
{
    public class SupplierVM
    {
    }
    public class SearchSupplierRequestDTO
    {
        public string? CodeNcc { get; set; }
        public string? NameNcc { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
    public class DeleteSupplierRequestDTO
    {
        public int Id { get; set; }
    }
    public class InsertFileExcelSupplierRequestDTO
    {
        public IFormFile? FileExcel { get; set; }
        public string? maNCC { get; set; }
        public string? tenNCC { get; set; }
    }
    public class ImportSupplierDetailDTO
    {
        public IFormFile? FileExcel { get; set; }
    }
}
