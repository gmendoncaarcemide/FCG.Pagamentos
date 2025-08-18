using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FCG.Pagamentos.Infrastructure
{
    public class PagamentosDbContextFactory : IDesignTimeDbContextFactory<PagamentosDbContext>
    {
        public PagamentosDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PagamentosDbContext>();
            optionsBuilder.UseNpgsql("Host=db.elcvczlnnzbgcpsbowkg.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=Fiap@1234");

            return new PagamentosDbContext(optionsBuilder.Options);
        }
    }
}

