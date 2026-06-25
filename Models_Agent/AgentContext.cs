using Microsoft.EntityFrameworkCore;

namespace PRJ_WAREHOUSE_BIVN.Models_Agent
{
    public class AgentContext : DbContext
    {
        public AgentContext()
        {

        }
        public AgentContext(DbContextOptions<AgentContext> options)
            : base(options)
        {
        }
        public DbSet<TM_EMPLOYEE> TM_EMPLOYEE { get; set; } = null!;
        public DbSet<TM_MASTER_MAIL_KEY> TM_MASTER_MAIL_KEYs { get; set; } = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                 optionsBuilder.UseSqlServer("Server=APBIVNDB14;Database=AGENTDB;User Id=agent;Password=agent;TrustServerCertificate=true;MultipleActiveResultSets=true;Connection Timeout=300;Max Pool Size=200;Min Pool Size=10;");
            }
        }
    }
}
