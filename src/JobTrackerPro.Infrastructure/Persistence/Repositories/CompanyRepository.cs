using JobTrackerPro.Domain.Entities;
using JobTrackerPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobTrackerPro.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of ICompanyRepository.</summary>
public class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Companies.FindAsync([id], cancellationToken);

    public async Task<Company?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => await _context.Companies
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower(), cancellationToken);

    public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Companies.OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public async Task AddAsync(Company company, CancellationToken cancellationToken = default)
        => await _context.Companies.AddAsync(company, cancellationToken);

    public async Task UpdateAsync(Company company, CancellationToken cancellationToken = default)
    {
        _context.Companies.Update(company);
        await Task.CompletedTask;
    }
}