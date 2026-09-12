namespace PRJ_WAREHOUSE_BIVN.Models_Auto
{
    public class BaoGia_RoleUser
    {
        public int ID { get; set; }

        public string UserAdid { get; set; } = null!;

        public string Role { get; set; } = null!;

        public bool IsUsing { get; set; }
    }
}
