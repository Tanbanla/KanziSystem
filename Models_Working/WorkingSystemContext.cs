using Microsoft.EntityFrameworkCore;

namespace PRJ_WAREHOUSE_BIVN.Models_Working
{
    public class WorkingSystemContext: DbContext
    {
        public WorkingSystemContext()
        {

        }
        public WorkingSystemContext(DbContextOptions<WorkingSystemContext> options)
            : base(options)
        {
        }
        public DbSet<TM_SECTION> TM_SECTION { get; set; } = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=ADMIN\\MYSQLKANGZANG;Database=WORKING_CONTROL;User Id=WorkingControl;Password=WorkingControl;TrustServerCertificate=true;");
            }
        }
    }
}
