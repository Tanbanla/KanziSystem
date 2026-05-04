namespace PRJ_WAREHOUSE_BIVN.Models
{
    public class PARAS
    {
        public string? Id_Material { get; set; }
        public string? Material_Code { get; set; }
        public string? Material_Name_VN { get; set; }
        public string? Material_Name_EN { get; set; }
        public string? Material_Name_JP { get; set; }
        public string? Account_Code { get; set; }
        public string? Account_Name_EN { get; set; }
        public string? Account_Name_VN { get; set; }
        public string? Unit { get; set; }
        public string? Unit_Note { get; set; }
        public decimal? Price { get; set; }
        public string? Currency { get; set; }
        public string? Group_Code { get; set; }
        public string? GoodKind { get; set; }
        public string? Num_Inventory { get; set; }
        public string? Inventory { get; set; }
        public string? UserName { get; set; }
    }
    public class MATERIA
    {
        public static List<PARAS> material_process(PARAS para)
        {
            SQL_Connect_DB20 _context = new SQL_Connect_DB20();
            string code_mt = para.Material_Code!;
            string timcode = "a.Material_Code like N'%%' and";
            if (para.Material_Code != null)
            {
                code_mt = para.Material_Code!.Split(":")[0].Length > 0 ? para.Material_Code!.Split(":")[0] : para.Material_Code!;
                timcode = "a.Material_Code = N'" + code_mt + "' and";
            }
                    
            var _cmd = _context.GET_DATA_FROM_SQL($"SELECT * FROM [MATERIAL_ACOUNTCODE] as a left join KHO as b on a.Material_Code = b.MaNguyenLieu WHERE {timcode} a.Material_Name_VN like N'%" + para.Material_Name_VN + "%' and a.Account_Name_VN like N'%" + para.Account_Name_VN + "%' and a.Group_Code like '%" + para.Group_Code + "%' ");
            List<PARAS> _material = new List<PARAS>();
            for (int i = 0; i < _cmd.Rows.Count; i++)
            {
                _material.Add(new PARAS
                {
                    Id_Material = _cmd.Rows[i]["Id_Material"].ToString(),
                    Material_Code = _cmd.Rows[i]["Material_Code"].ToString(),
                    Material_Name_VN = _cmd.Rows[i]["Material_Name_VN"].ToString(),
                    Material_Name_EN = _cmd.Rows[i]["Material_Name_EN"].ToString(),
                    Material_Name_JP = _cmd.Rows[i]["Material_Name_JP"].ToString(),
                    Account_Code = _cmd.Rows[i]["Account_Code"].ToString(),
                    Account_Name_EN = _cmd.Rows[i]["Account_Name_EN"].ToString(),
                    Account_Name_VN = _cmd.Rows[i]["Account_Name_VN"].ToString(),
                    Unit = _cmd.Rows[i]["Unit"].ToString(),
                    Unit_Note = _cmd.Rows[i]["Unit_Note"].ToString(),
                    Price = Math.Round(decimal.Parse(_cmd.Rows[i]["Price"].ToString()!),2),
                    Currency = _cmd.Rows[i]["Currency"].ToString(),
                    Group_Code = _cmd.Rows[i]["Group_Code"].ToString(),
                    GoodKind = _cmd.Rows[i]["GoodKind"].ToString(),
                    Num_Inventory = _cmd.Rows[i]["Hientai"].ToString(),
                    Inventory = _cmd.Rows[i]["Kho"].ToString(),
                });
            }
            return _material;
        }
    }
}

