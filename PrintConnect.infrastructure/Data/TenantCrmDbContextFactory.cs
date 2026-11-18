using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Design;

namespace PrintConnect.infrastructure.Data
{
    public interface ITenantCrmDbContextFactory
    {
        Task<TenantCrmDbContext> CreateAsync(int companyId);
    }

    public class TenantCrmDbContextFactory : ITenantCrmDbContextFactory
    {
        private readonly MasterCrmDbContext _masterDb;
        private readonly IConfiguration _configuration;

        public TenantCrmDbContextFactory(MasterCrmDbContext masterDb, IConfiguration configuration)
        {
            _masterDb = masterDb;
            _configuration = configuration;
        }

        public async Task<TenantCrmDbContext> CreateAsync(int companyId)
        {
            var companyDb = await _masterDb.CompanyDatabases
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.IsActive);

            if (companyDb == null)
            {
                throw new Exception($"No active database found for Company ID {companyId}");
            }

            var userId = _configuration[$"TenantCredentials:{companyDb.CredentialKey}:UserId"];
            var password = _configuration[$"TenantCredentials:{companyDb.CredentialKey}:Password"];

            string connectionString;

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(password))
            {
                connectionString = $"Server={companyDb.ServerName};Database={companyDb.DatabaseName};User Id={userId};Password={password};Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;";
            }
            else
            {
                connectionString = $"Server={companyDb.ServerName};Database={companyDb.DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;";
            }

            var options = new DbContextOptionsBuilder<TenantCrmDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new TenantCrmDbContext(options);
        }
    }

    // --- FACTORY FOR TENANT CLOUD MIGRATION ---
    public class TenantCrmDbContextDesignTimeFactory : IDesignTimeDbContextFactory<TenantCrmDbContext>
    {
        public TenantCrmDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantCrmDbContext>();

            // Updated with the Tenant's specific public server and credentials
            optionsBuilder.UseSqlServer("Server=db81004.public.databaseasp.net;Database=db81004;User Id=db81004;Password=K?f3-2PzWd8@;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;");

            return new TenantCrmDbContext(optionsBuilder.Options);
        }
    }
}