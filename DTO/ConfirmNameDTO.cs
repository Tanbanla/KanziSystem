namespace PRJ_WAREHOUSE_BIVN.DTO
{
    public class ConfirmNameDTO
    {
        public int Id { get; set; }
        public string? TenHaiQuan { get; set; }
        public string? MaHangNoiBo { get; set; }
        public string? LyDo { get; set; }
        public bool? pheDuyet { get; set; }
        public string? PicShip { get; set; }
    }
    public class CountCofirmName
    {
        public int? countCofirmed { get; set; }
        public int? countConfirming { get; set; }
        public int? countRejected { get; set; }
        public int? sum { get; set; }
    }
}
