using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.View_Models.Login
{
    public class UserManagerModel
    {
        public List<BaoGia_WorkflowRoleDTO>? Roles { get; set; } = new List<BaoGia_WorkflowRoleDTO>();
    }

    // search thông tin người dùng
    public class UserSearchModel
    {
        public string? adid { get; set; }
        public string? fullname { get; set; }
        public string? section { get; set; }
        public string? role { get; set; }
        public int? status { get; set; }

        public int? pageIndex { get; set; }
        public int? pageSize { get; set; }
    }
    // Insert thông tin và đăng ký user đăng nhập
    public class UserInsertModel
    {
        public TM_USER infor { get; set; } = null!;
        public string? role { get; set; }

    }
}
