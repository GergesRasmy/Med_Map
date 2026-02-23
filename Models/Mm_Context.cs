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
        public DbSet<PharmacyInventory> PharmacyInventory { get; set; }
        public DbSet<Recommendation> Recommendation { get; set; }
        public DbSet<Wallet> Wallet { get; set; }
        public DbSet<WithdrawalRequest> WithdrawalRequest { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<UserSession> UserSession { get; set; }
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

            // Enforce unique email at the database level by adding a unique index
            // on NormalizedEmail (Identity uses Normalized values for lookups).
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.NormalizedEmail)
                .IsUnique();
            builder.Entity<ApplicationUser>()
                 .HasIndex(u => u.PhoneNumber)
                 .IsUnique();

            builder.Entity<OtpCode>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Index SessionId for faster lookup during VerifyOTP action
                entity.HasIndex(e => e.SessionId).IsUnique();

                entity.Property(e => e.Code).IsRequired().HasMaxLength(6);

                // Relationship: One User can have many OTP attempts
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);


                builder.Entity<UserSession>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.HasIndex(e => e.JwtId).IsUnique();

                    entity.HasOne(e => e.User)
                          .WithMany()
                          .HasForeignKey(e => e.UserId)
                          .OnDelete(DeleteBehavior.Cascade);
                });
            });
        }
        #endregion
    
    }
}
