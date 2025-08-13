using FCG.Pagamentos.Domain.Pagamentos.Entities;
using FCG.Pagamentos.Domain.Pagamentos.Interfaces;
using FCG.Pagamentos.Infrastructure.Base;
using Microsoft.EntityFrameworkCore;

namespace FCG.Pagamentos.Infrastructure.Pagamentos.Repositories;

public class ReembolsoRepository : Repository<Reembolso>, IReembolsoRepository
{
    public ReembolsoRepository(PagamentosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Reembolso>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        return await _dbSet
            .Where(r => r.UsuarioId == usuarioId && r.Ativo)
            .Include(r => r.Transacao)
            .OrderByDescending(r => r.DataSolicitacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reembolso>> ObterPorTransacaoAsync(Guid transacaoId)
    {
        return await _dbSet
            .Where(r => r.TransacaoId == transacaoId && r.Ativo)
            .Include(r => r.Transacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reembolso>> ObterPorStatusAsync(StatusReembolso status)
    {
        return await _dbSet
            .Where(r => r.Status == status && r.Ativo)
            .Include(r => r.Transacao)
            .OrderByDescending(r => r.DataSolicitacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reembolso>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await _dbSet
            .Where(r => r.DataSolicitacao >= dataInicio && r.DataSolicitacao <= dataFim && r.Ativo)
            .Include(r => r.Transacao)
            .OrderByDescending(r => r.DataSolicitacao)
            .ToListAsync();
    }
} 