using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PrintConnect.domain.entities;
using PrintConnect.domain.Entities;

namespace PrintConnect.infrastructure.Data
{
    public class MasterCrmDbContext : IdentityDbContext<IdentityUser>
    {
        public MasterCrmDbContext(DbContextOptions<MasterCrmDbContext> options) : base(options) { }

        public DbSet<Company> Companies => Set<Company>();
        public DbSet<CompanyDatabase> CompanyDatabases => Set<CompanyDatabase>();
        public DbSet<Device> Devices => Set<Device>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Company>(entity =>
            {
                entity.HasKey(x => x.CompanyId);
                entity.Property(x => x.CompanyCode).HasMaxLength(50).IsRequired();
                entity.HasIndex(x => x.CompanyCode).IsUnique();
            });

            builder.Entity<CompanyDatabase>(entity =>
            {
                entity.HasKey(x => x.CompanyDatabaseId);
                entity.HasOne(x => x.Company)
                      .WithMany()
                      .HasForeignKey(x => x.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Device>(entity =>
            {
                entity.HasKey(x => x.DeviceId);
                entity.Property(x => x.DeviceCode).HasMaxLength(50).IsRequired();
                entity.Property(x => x.DeviceName).HasMaxLength(200).IsRequired();
                entity.HasOne(x => x.Company)
                      .WithMany(x => x.Devices)
                      .HasForeignKey(x => x.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }

    // --- FACTORY FOR MASTER CLOUD MIGRATION ---
    public class MasterCrmDbContextDesignTimeFactory : IDesignTimeDbContextFactory<MasterCrmDbContext>
    {
        public MasterCrmDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MasterCrmDbContext>();

            optionsBuilder.UseSqlServer("Server=db81003.public.databaseasp.net;Database=db81003;User Id=db81003;Password=7Rg@!2NxE+b8;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;");

            return new MasterCrmDbContext(optionsBuilder.Options);
        }
    }
}