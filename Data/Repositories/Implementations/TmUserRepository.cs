using Dapper;
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
        public async Task<ListRequest<dynamic>> SearchUserAsync(UserSearchModel searchModel)
        {
            var sql = new StringBuilder();
            var where = new List<string>();
            var parameters = new DynamicParameters();

            sql.Append(@"
                SELECT
                    u.ID
                    ,u.CHR_USERID
                    ,u.VCHR_PASSWORD
                    ,u.FULLNAME
                    ,u.CHR_CRT_USERID
                    ,u.DTM_CREATE
                    ,u.Lancuoicungdangnhap
                    ,u.CHR_EMPLOYEE_ID
                    ,u.CHR_ADID_GROUPUSER
                    ,u.DTM_LAST_LOGIN
                    ,u.INT_LOCK
                    ,u.INT_LOCK_DAY
                    ,u.CHR_SECTION
                    ,u.INT_USERID_COMMON
                    ,u.dia_chi_mail
                    ,u.phan_quyen
                    ,u.phong_ban
                    ,u.thoi_gian_cap_nhat
                    ,u.cho_phep_hoat_dong
                    ,br.Role
                FROM TM_USER u
                LEFT JOIN BaoGia_RoleUser br
                    ON u.CHR_USERID = br.UserAdid
            ");

            #region Filter

            if (!string.IsNullOrWhiteSpace(searchModel.adid))
            {
                where.Add("u.CHR_USERID LIKE '%' + @Adid + '%'");
                parameters.Add("Adid", searchModel.adid);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.fullname))
            {
                where.Add("u.FULLNAME LIKE '%' + @FullName + '%'");
                parameters.Add("FullName", searchModel.fullname);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.section))
            {
                where.Add("u.CHR_SECTION LIKE '%' + @Section + '%'");
                parameters.Add("Section", searchModel.section);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.role))
            {
                where.Add("br.Role = @Role");
                parameters.Add("Role", searchModel.role);
            }

            if (searchModel.status.HasValue)
            {
                where.Add("u.INT_LOCK = @Status");
                parameters.Add("Status", searchModel.status.Value);
            }

            if (where.Any())
            {
                sql.Append(" WHERE ");
                sql.Append(string.Join(" AND ", where));
            }

            #endregion

            #region Count

            var countSql = $@"
                SELECT COUNT(1)
                FROM TM_USER u
                LEFT JOIN BaoGia_RoleUser br
                    ON u.CHR_USERID = br.UserAdid
                {(where.Any() ? "WHERE " + string.Join(" AND ", where) : "")}
            ";

            var totalCount = await _conn.ExecuteScalarAsync<int>(
                countSql,
                parameters);

            #endregion

            #region Paging

            if (searchModel.pageSize.HasValue && searchModel.pageSize.Value > 0)
            {
                var page = searchModel.pageIndex.GetValueOrDefault(1);

                var offset = (page - 1) * searchModel.pageSize.Value;

                sql.Append(@"
                    ORDER BY u.CHR_USERID
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY
                ");

                parameters.Add("Offset", offset);
                parameters.Add("PageSize", searchModel.pageSize.Value);
            }
            else
            {
                sql.Append(" ORDER BY u.CHR_USERID");
            }

            #endregion

            var data = (
                await _conn.QueryAsync<dynamic>(
                    sql.ToString(),
                    parameters))
                .ToList();

            return new ListRequest<dynamic>
            {
                Data = data,
                TotalCount = totalCount
            };
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
            userInsert.infor.CHR_CRT_USERID = userInsert.infor.CHR_EMPLOYEE_ID;
            userInsert.infor.CHR_ADID_GROUPUSER = userInsert.infor.CHR_USERID;
            userInsert.infor.INT_LOCK_DAY = 30;
            userInsert.infor.DTM_CREATE = DateTime.Now;

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
