using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRJ_WAREHOUSE_BIVN.Models;
using System.Collections.Generic;
using System.Data;

namespace PRJ_WAREHOUSE_BIVN.Pages.UserManagement
{
    public class IndexModel : PageModel
    {
        public List<PE_USERNAME> Users { get; set; } = new List<PE_USERNAME>();

        [BindProperty]
        public PE_USERNAME Input { get; set; } = new PE_USERNAME();

        private SQL_Connect_DB20 _db = new SQL_Connect_DB20();

        public void OnGet()
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            Users.Clear();
            var dt = _db.GET_DATA_FROM_SQL("SELECT Id_User, User_Name, Mail, Adid, Group_Code, Role FROM PE_USERNAME ORDER BY Id_User DESC");
            if (dt == null) return;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Users.Add(new PE_USERNAME
                {
                    Id_User = int.Parse(dt.Rows[i]["Id_User"].ToString()!),
                    User_Name = dt.Rows[i]["User_Name"].ToString(),
                    Mail = dt.Rows[i]["Mail"].ToString(),
                    Adid = dt.Rows[i]["Adid"].ToString(),
                    Group_Code = dt.Rows[i]["Group_Code"].ToString(),
                    Role = dt.Rows[i]["Role"].ToString()
                });
            }
        }

        public IActionResult OnPostCreate()
        {
            // Basic null/empty handling
            var name = Input.User_Name?.Replace("'", "''") ?? "";
            var mail = Input.Mail?.Replace("'", "''") ?? "";
            var adid = Input.Adid?.Replace("'", "''") ?? "";
            var group = Input.Group_Code?.Replace("'", "''") ?? "";
            var role = Input.Role?.Replace("'", "''") ?? "";

            string sql = $@"INSERT INTO PE_USERNAME (User_Name, Mail, Adid, Group_Code, Role)
                            VALUES (N'{name}', '{mail}', '{adid}', '{group}', '{role}')";
            _db.GET_DATA_FROM_SQL(sql);
            return RedirectToPage();
        }

        public IActionResult OnPostEdit()
        {
            var id = Input.Id_User;
            if (id <= 0) return RedirectToPage();

            var name = Input.User_Name?.Replace("'", "''") ?? "";
            var mail = Input.Mail?.Replace("'", "''") ?? "";
            var adid = Input.Adid?.Replace("'", "''") ?? "";
            var group = Input.Group_Code?.Replace("'", "''") ?? "";
            var role = Input.Role?.Replace("'", "''") ?? "";

            string sql = $@"UPDATE PE_USERNAME SET 
                                User_Name = N'{name}',
                                Mail = '{mail}',
                                Adid = '{adid}',
                                Group_Code = '{group}',
                                Role = '{role}'
                            WHERE Id_User = {id}";
            _db.GET_DATA_FROM_SQL(sql);
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            if (id <= 0) return RedirectToPage();
            _db.GET_DATA_FROM_SQL($"DELETE FROM PE_USERNAME WHERE Id_User = {id}");
            return RedirectToPage();
        }
    }
}
