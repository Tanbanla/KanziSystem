using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.View_Models.Login;
using System.Text;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class TmUserRepository: BaseRepository<TM_USER, int>, ITmUserRepository  
    {
        private readonly COST_MANAGEMENTContext _context;
        public TmUserRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
            : base(context, options, configuration)
        {
            _context = context;
        }
        // Login
        public async Task<TM_USER> Login(string username, string password)
        {
            try
            {
                var user = await _context.TM_USERs
                    .FirstOrDefaultAsync(u => u.CHR_USERID == username && u.VCHR_PASSWORD == password);
                return user!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Login: {ex.Message}");
                return null!;
            }
        }
        // Lấy thông tin user theo ADID
        public async Task<TM_USER> GetUserByAdId(string adId)
        {
            try
            {
                var user = await _context.TM_USERs
                    .FirstOrDefaultAsync(u => u.CHR_USERID == adId);
                return user!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserByAdId: {ex.Message}");
                return null!;
            }
        }
        // lấy quyền user
        public async Task<string> GetRoleAsync(string adId)
        {
            try
            {
                var result = await _context.BaoGia_RoleUsers.Where(x => x.UserAdid == adId && x.IsUsing).Select(x => x.Role).FirstOrDefaultAsync();
                return result ?? "User";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserByAdId: {ex.Message}");
                return null!;
            }
        }
        // Insert thông tin và đăng ký user đăng nhập
        public async Task<bool> InsertListUserAsync(List<TM_USER> users)
        {
            if (users == null || users.Count == 0)
                return false;
            var existingUserIds = await _context.TM_USERs.Select(u => u.CHR_USERID.ToLower().Trim()).ToListAsync();
            var newUsers = users.Where(u => !existingUserIds.Contains(u.CHR_USERID.ToLower().Trim())).ToList();
            if (newUsers.Count == 0)
                return false;
            await _context.TM_USERs.AddRangeAsync(newUsers);
            await _context.SaveChangesAsync();
            return true;
        }
        // Search thông tin user
        public async Task<List<TM_USER>> SearchUserAsync(UserSearchModel searchModel)
        {
            if (searchModel == null) return new List<TM_USER>();

            var sql = new StringBuilder(@"SELECT
                   u.[ID]
                  ,[CHR_USERID]
                  ,[VCHR_PASSWORD]
                  ,[FULLNAME]
                  ,[CHR_CRT_USERID]
                  ,[DTM_CREATE]
                  ,[Lancuoicungdangnhap]
                  ,[CHR_EMPLOYEE_ID]
                  ,[CHR_ADID_GROUPUSER]
                  ,[DTM_LAST_LOGIN]
                  ,[INT_LOCK]
                  ,[INT_LOCK_DAY]
                  ,[CHR_SECTION]
                  ,[INT_USERID_COMMON]
                  ,[dia_chi_mail]
                  ,[phan_quyen]
                  ,[phong_ban]
                  ,[thoi_gian_cap_nhat]
                  ,[cho_phep_hoat_dong]
              FROM TM_USER as u 
              left join BaoGia_RoleUser as br
              on u.CHR_CRT_USERID = br.UserAdid
            WHERE 1=1");

            if (!string.IsNullOrEmpty(searchModel.adid))
            {
                sql.Append($" AND u.CHR_USERID LIKE '%{searchModel.adid}%'");
            }
            if (!string.IsNullOrEmpty(searchModel.fullname))
            {
                sql.Append($" AND u.FULLNAME LIKE '%{searchModel.fullname}%'");
            }
            if (!string.IsNullOrEmpty(searchModel.section))
            {
                sql.Append($" AND u.CHR_SECTION LIKE '%{searchModel.section}%'");
            }
            if (!string.IsNullOrEmpty(searchModel.role))
            {
                sql.Append($" AND br.Role = '{searchModel.role}'");
            }
            if (searchModel.status.HasValue)
            {
                sql.Append($" AND u.INT_LOCK = {searchModel.status.Value}");
            }

            if (searchModel.pageIndex.HasValue && searchModel.pageSize.HasValue)
            {
                int offset = (searchModel.pageIndex.Value - 1) * searchModel.pageSize.Value;
                sql.Append($" ORDER BY u.CHR_USERID OFFSET {offset} ROWS FETCH NEXT {searchModel.pageSize.Value} ROWS ONLY");
            }

            var users = await _context.TM_USERs.FromSqlRaw(sql.ToString()).ToListAsync();
            return users;
        }
        // Đăng ký user mới
        public async Task<bool> RegisterUserAsync(UserInsertModel userInsert)
        {
            if (userInsert == null || userInsert.infor == null)
                return false;
            var existingUser = await _context.TM_USERs
                .FirstOrDefaultAsync(u => u.CHR_USERID == userInsert.infor.CHR_USERID);
            if (existingUser != null)
                return false;

            var roleUser = new BaoGia_RoleUser
            {
                UserAdid = userInsert.infor.CHR_USERID,
                Role = userInsert.role ?? "User",
                IsUsing = true
            };
            await _context.TM_USERs.AddAsync(userInsert.infor);
            await _context.BaoGia_RoleUsers.AddAsync(roleUser);
            await _context.SaveChangesAsync();
            return true;
        }
        // Update thông tin user
        public async Task<bool> UpdateUserAsync(UserInsertModel userUpdate)
        {
            if (userUpdate == null || userUpdate.infor == null)
                return false;
            var existingUser = await _context.TM_USERs
                .FirstOrDefaultAsync(u => u.CHR_USERID == userUpdate.infor.CHR_USERID);
            if (existingUser == null)
                return false;
            existingUser.FULLNAME = userUpdate.infor.FULLNAME;
            existingUser.CHR_SECTION = userUpdate.infor.CHR_SECTION;
            existingUser.INT_LOCK = userUpdate.infor.INT_LOCK;
            existingUser.dia_chi_mail = userUpdate.infor.dia_chi_mail;
            existingUser.phan_quyen = userUpdate.infor.phan_quyen;
            existingUser.phong_ban = userUpdate.infor.phong_ban;
            existingUser.thoi_gian_cap_nhat = DateTime.Now;
            existingUser.cho_phep_hoat_dong = userUpdate.infor.cho_phep_hoat_dong;

            var roleUser = await _context.BaoGia_RoleUsers
                .FirstOrDefaultAsync(r => r.UserAdid == userUpdate.infor.CHR_USERID);
            if (roleUser != null)
            {
                roleUser.Role = userUpdate.role ?? "User";
                roleUser.IsUsing = true;
            }
            else
            {
                var newRoleUser = new BaoGia_RoleUser
                {
                    UserAdid = userUpdate.infor.CHR_USERID,
                    Role = userUpdate.role ?? "User",
                    IsUsing = true
                };
                await _context.BaoGia_RoleUsers.AddAsync(newRoleUser);
            }
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteUserAsync(string userId)
        {
            var existingUser = await _context.TM_USERs
                .FirstOrDefaultAsync(u => u.CHR_USERID == userId);
            if (existingUser == null)
                return false;

            existingUser.INT_LOCK = 2; // Lock the user instead of deleting

            var roleUser = await _context.BaoGia_RoleUsers
                .FirstOrDefaultAsync(r => r.UserAdid == userId);
            if (roleUser != null)
            {
                roleUser.IsUsing = false; // Disable the role instead of deleting
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
