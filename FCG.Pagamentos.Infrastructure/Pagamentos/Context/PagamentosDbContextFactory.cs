using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FCG.Pagamentos.Infrastructure
{
    public class PagamentosDbContextFactory : IDesignTimeDbContextFactory<PagamentosDbContext>
    {
        public PagamentosDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PagamentosDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=FCG_Pagamentos;Trusted_Connection=true;");

            return new PagamentosDbContext(optionsBuilder.Options);
        }
    }
}

