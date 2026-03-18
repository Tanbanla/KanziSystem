using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using PRJ_WAREHOUSE_BIVN.Models;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class ExportController : Controller
    {
        [HttpGet]
        public IActionResult Export_material()
        {
            return View();
        }
        public byte[] ExportToExcel<T>(List<T> data, string sheetName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(sheetName);

                // Load dữ liệu từ List vào sheet, bắt đầu từ ô A1, tự động tạo Header
                worksheet.Cells["A1"].LoadFromCollection(data, true);

                // Format Header (Bôi đậm)
                using (var range = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                {
                    range.Style.Font.Bold = true;
                }

                // Tự động căn chỉnh độ rộng cột
                worksheet.Cells.AutoFitColumns();

                return package.GetAsByteArray();
            }
        }
        public List<KHO_NHAPXUAT> ImportExcel(Stream fileStream)
        {
            var list = new List<KHO_NHAPXUAT>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(fileStream))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Lấy sheet đầu tiên
                var rowCount = worksheet.Dimension.Rows;

                // Chạy từ dòng 2 (vì dòng 1 là Header)
                for (int row = 2; row <= rowCount; row++)
                {
                    list.Add(new KHO_NHAPXUAT
                    {
                    //    Name = worksheet.Cells[row, 1].Value?.ToString(),
                    //    Age = int.Parse(worksheet.Cells[row, 2].Value?.ToString() ?? "0"),
                    //    Email = worksheet.Cells[row, 3].Value?.ToString()
                    });
                }
            }
            return list;
        }
        [HttpGet("export")]
        public IActionResult Export()
        {
            var data = new List<KHO_NHAPXUAT> {
            new KHO_NHAPXUAT { /*Name = "Nguyen Van A", Age = 20, Email = "a@gmail.com"*/ }
        };
            var fileContents = ExportToExcel(data, "Students");

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "StudentList.xlsx"
            );
        }

        [HttpPost("import")]
        public IActionResult Import(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File trống");

            using (var stream = file.OpenReadStream())
            {
                var result = ImportExcel(stream);
                return Ok(result);
            }
        }
    }
}
