using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Net.Mail;
using System.Net.NetworkInformation;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class SendMailRepository: BaseRepository<TM_MASTER_MAIL, int>, ISendMailRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public SendMailRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration, IWebHostEnvironment env)
            : base(context, options, configuration)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }
        // lấy mail theo ID
        public async Task<TM_MASTER_MAIL?> GetMailByIdAsync(int id)
        {
            return await _context.TM_MASTER_MAILs.Where(c => c.INT_Mail == id).FirstOrDefaultAsync();
        }
        // Lay danh sach NCC can gui mail
        public async Task<List<string>> GetSuppliersToNotifyAsync()
        {

          var res =  await _context.BaoGia_Request_of_Quotations
                .Where(m => (m.BIT_IsTemplate == null || m.BIT_IsTemplate == false)
                && m.BIT_LayBaoGia == true && m.CHR_MaNCC != "" && m.ID_StepBaoGia == 6)//&& m.CHR_MaNCC != "L4AMIVN"
                .Select(m => m.CHR_MaNCC)
                .Distinct()
                .ToListAsync();
            //&& m.ID_Status == "WAIT_SEND_MAIL"
            if (res == null || res.Count == 0)
            {
                return new List<string>();
            }
           return res;
        }
        // Lay thong tin don bao gia cua nha cung cap
        public async Task<List<dynamic>> GetBaoGiaRequestBySupplierAsync(string supplierCode)
        {
            var sql = @"SELECT q.*, n.ShortName , n.Ten, n.Diachi
                FROM BaoGia_Request_of_Quotation AS q
                LEFT JOIN IM_NCC_NEW AS n ON q.CHR_MaNCC = n.Ma
                WHERE q.CHR_MaNCC = @SupplierCode
                  and ID_Status = 'WAIT_SEND_MAIL'
                  -- AND (q.BIT_IsTemplate IS NULL OR q.BIT_IsTemplate = 0) 
                  AND q.BIT_LayBaoGia = 1 and q.ID_StepBaoGia = 6";
            //and ID_Status = 'WAIT_SEND_MAIL'
            var parameter = new { SupplierCode = supplierCode };

            return (await _conn.QueryAsync<dynamic>(sql, parameter)).ToList();
        }
        // Lay email nha cung cap
        public async Task<string?> GetSupplierEmaiCategorylAsync(string supplierCode, string catergory)
        {
            var email = await _context.BaoGia_NCC_Categories
                .Where(m => m.CHR_MaNCC == supplierCode && m.NVCHR_ChungLoai == catergory)
                .Select(m => m.CHR_Mail)
                .FirstOrDefaultAsync();
            return email;
        }
        public async Task<string?> GetSupplierEmailAsync(string supplierCode)
        {
            var email = await _context.BaoGia_NCC_Categories
                .Where(m => m.CHR_MaNCC == supplierCode)
                .Select(m => m.CHR_Mail)
                .FirstOrDefaultAsync();
            return email;
        }
        public async Task<bool> UpdateMailSentStatusAsync(List<int> listRq)
        {
            var requests = await _context.BaoGia_Request_of_Quotations
                .Where(m => listRq.Contains(m.ID))
                .ToListAsync();

            if (requests == null || !requests.Any())
            {
                return false;
            }

            foreach (var item in requests)
            {
                item.ID_Status = "WAIT_NCC";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Lay thong tin nha cung cap theo ma don hang
        public async Task<List<dynamic>> GetNotifyRequestCodeAsync(string requestCode)
        {
            var sql = @"SELECT q.*, n.ShortName , n.Ten, n.Diachi
                FROM BaoGia_Request_of_Quotation AS q
                LEFT JOIN IM_NCC_NEW AS n ON q.CHR_MaNCC = n.Ma
                WHERE q.CHR_MaDon = @RequestCode 
                  AND (q.BIT_IsTemplate IS NULL OR q.BIT_IsTemplate = 0) 
                  AND q.BIT_LayBaoGia = 1";

            var parameter = new { RequestCode = requestCode };

            return (await _conn.QueryAsync<dynamic>(sql, parameter)).ToList();
        }
        // lay thông tin phê duyệt theo phòng ban
        public async Task<string> GetRequesterEmailAsync(string section, int step)
        {
            var res = await _context.BaoGia_Master_Approver_Send_Mails
                .Where(m => (m.CHR_CodeSection == section || string.IsNullOrEmpty(section)) && m.ID_BaoGiaStep == step && m.CHR_Status == "ON")
                .GroupBy(m => m.CHR_UserAdid) 
                .Select(g => g.Key) 
                .ToListAsync();

            if (res == null || res.Count == 0)
                return "";

            // Thêm đuôi @brothergroup.net và nối chuỗi
            var emails = res
                .Where(adid => !string.IsNullOrEmpty(adid))
                .Select(adid => adid.Trim() + "@brothergroup.net");

            return string.Join(";", emails);
        }
        // Inset thông tin vào bảng Báo giá detail 
        public async Task<bool> InsertBaoGiaDetailAsync(List<BaoGia_Detail_of_Quotation> dtos)
        {
            if (dtos == null || dtos.Count() == 0) return false;
            var listDetailOK = new List<BaoGia_Detail_of_Quotation>();
            foreach (var detail in dtos)
            {
                var rq = await _context.BaoGia_Request_of_Quotations
                .Where(c => c.BIT_LayBaoGia == true && c.ID == detail.ID_RequestQuote && c.ID_Status == "WAIT_NCC")
                .FirstOrDefaultAsync();
                if (rq == null) continue;
                rq.BIT_IsTemplate = true;

                // kiểm tra dữ liệu Insert
                var exists = await _context.BaoGia_Detail_of_Quotations
                    .AnyAsync(c => c.ID_RequestQuote == detail.ID_RequestQuote);
                if (exists) continue;
                listDetailOK.Add(detail);
            }
            await _context.BaoGia_Detail_of_Quotations.AddRangeAsync(listDetailOK);
            await _context.SaveChangesAsync();
            return true;
        }
        // Lấy file từ link 
        public async Task<GenericResponse<IFormFile>> GetFileToLinkAsync(string filePath)
        {
            var result = new GenericResponse<IFormFile>();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                result.Success = false;
                result.Message = "File path is empty.";
                return result;
            }

            try
            {
                var raw = filePath.Trim().Trim('"', '\'');
                string fileNameOnly = Path.GetFileName(raw);
                string uploadFolder = (_configuration["ApiSettings:BaseUpload"] ?? string.Empty).TrimEnd('/', '\\');

                string physicalPath = raw;

                if (raw.StartsWith("\\\\") || raw.StartsWith("//"))
                {
                    physicalPath = raw.Replace('/', Path.DirectorySeparatorChar);
                }
                else if (raw.StartsWith("/"))
                {
                    if (!string.IsNullOrWhiteSpace(uploadFolder))
                    {
                        physicalPath = Path.Combine(uploadFolder, fileNameOnly);
                    }
                    else
                    {
                        var webRoot = _env.WebRootPath ?? string.Empty;
                        var trimmed = raw.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                        physicalPath = Path.Combine(webRoot, trimmed);
                    }
                }

                physicalPath = physicalPath.Replace('/', Path.DirectorySeparatorChar);

                if (!File.Exists(physicalPath))
                {
                    result.Success = false;
                    result.Message = "File not found.";
                    return result;
                }

                var ms = new MemoryStream();
                using (var fs = File.OpenRead(physicalPath))
                {
                    await fs.CopyToAsync(ms);
                }
                ms.Position = 0;

                var localFileName = Path.GetFileName(physicalPath);
                var localFormFile = new FormFile(ms, 0, ms.Length, "file", localFileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/octet-stream"
                };

                result.Data = localFormFile;
                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
        }
        // lấy mail người tạo đơn
        public async Task<string> GetRequesterEmailByAdidAsync(string adid)
        {
            if (string.IsNullOrWhiteSpace(adid))
            {
                return string.Empty;
            }

            var sql = @"SELECT top 1 [CHR_EMPLOYEE_MAIL]
             FROM [AGENTDB].[dbo].[TM_EMPLOYEE] where CHR_EMPLOYEE_ADID =  @Adid";

            var parameter = new { Adid = adid };

            var email = await _conn.QueryFirstOrDefaultAsync<string>(sql, parameter);
            return string.IsNullOrWhiteSpace(email) ? string.Empty : email.Trim();
        }
        // The list vender not need to send mail
        public async Task<List<string>> SupplierNeedToSendMailAsync()
        {
            const string sql = @"SELECT DISTINCT CHR_MaNcc
                                  FROM BaoGia_Vender_NotConfirm
                                  WHERE CHR_Status = @Status";
            try
            {
                var res = (await _conn.QueryAsync<string>(sql, new { Status = "ON" }))?.ToList();
                if (res == null || res.Count == 0)
                {
                    return new List<string>();
                }
                return res;
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }
    }
}
