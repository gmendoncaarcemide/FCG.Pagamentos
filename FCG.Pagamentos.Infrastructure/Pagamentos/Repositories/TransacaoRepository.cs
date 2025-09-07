using FCG.Pagamentos.Domain.Pagamentos.Entities;
using FCG.Pagamentos.Domain.Pagamentos.Interfaces;
using FCG.Pagamentos.Infrastructure.Base;
using Microsoft.EntityFrameworkCore;

namespace FCG.Pagamentos.Infrastructure.Pagamentos.Repositories;

public class TransacaoRepository : Repository<Transacao>, ITransacaoRepository
{
    public TransacaoRepository(PagamentosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Transacao>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        return await _dbSet
            .Where(t => t.UsuarioId == usuarioId && t.Ativo)
            .OrderByDescending(t => t.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transacao>> ObterPorJogoAsync(Guid jogoId)
    {
        return await _dbSet
            .Where(t => t.JogoId == jogoId && t.Ativo)
            .OrderByDescending(t => t.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transacao>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await _dbSet
            .Where(t => t.DataCriacao >= dataInicio && t.DataCriacao <= dataFim && t.Ativo)
            .OrderByDescending(t => t.DataCriacao)
            .ToListAsync();
    }
} 