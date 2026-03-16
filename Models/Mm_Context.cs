using Med_Map.Models.ordersANDmedicine;
using Med_Map.Models.pharmacy;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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
        public DbSet<PharmacyProfile> PharmacyProfille { get; set; }
        public DbSet<PharmacyInventory> PharmacyInventory { get; set; }
        public DbSet<Recommendation> Recommendation { get; set; }
        public DbSet<Wallet> Wallet { get; set; }
        public DbSet<WithdrawalRequest> WithdrawalRequest { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<UserSession> UserSession { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<PaymentLog> PaymentLog { get; set; }

        #endregion
        #region constructor
        public Mm_Context(): base()
        {
            
        }
        public Mm_Context(DbContextOptions options):base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. User Indexes
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.NormalizedEmail).IsUnique();
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.PhoneNumber).IsUnique();

            // 2. OTP Configuration
            builder.Entity<OtpCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SessionId).IsUnique();
                entity.Property(e => e.Code).IsRequired().HasMaxLength(6);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.NoAction);
            });

            // 3. Session Configuration
            builder.Entity<UserSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.JwtId).IsUnique();
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.NoAction);
            });

            // 4. Customer Configuration
            builder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.ApplicationUserId);

            // 5. Pharmacy Configuration 
            builder.Entity<Pharmacy>(entity =>
            {
                entity.HasKey(p => p.ApplicationUserId);
                entity.HasOne(p => p.User)
                      .WithOne(u => u.Pharmacy)
                      .HasForeignKey<Pharmacy>(p => p.ApplicationUserId);
                entity.HasOne(p => p.ActiveProfile)
                      .WithMany()
                      .HasForeignKey(p => p.ActiveProfileId) 
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.PendingProfile)
                      .WithMany()
                      .HasForeignKey(p => p.PendingProfileId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
        #endregion

    }
}
