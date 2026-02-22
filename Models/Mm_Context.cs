using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Med_Map.Models
{
    public class Mm_Context : IdentityDbContext<ApplicationUser>
    {
        #region SETS
        public DbSet<AiChatRequest> AiChatRequest { get; set; }
        public DbSet<AiChatResponse> AiChatResponse { get; set; }
        public DbSet<AiChatSession> AiChatSession { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<DoctorRequest> DoctorRequest { get; set; }
        public DbSet<MedicineMaster> MedicineMaster { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<Pharmacy> Pharmacy { get; set; }
        public DbSet<PharmacyInventory> PharmacyInventory { get; set; }
        public DbSet<Recommendation> Recommendation { get; set; }
        public DbSet<Wallet> Wallet { get; set; }
        public DbSet<WithdrawalRequest> WithdrawalRequest { get; set; }
        #endregion
        #region constructor
        public Mm_Context(): base()
        {
            
        }
        public Mm_Context(DbContextOptions options):base(options)
        {
            
        }
        #endregion
    
    }
}
