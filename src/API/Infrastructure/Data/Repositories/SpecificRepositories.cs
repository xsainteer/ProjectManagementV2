using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext context) : base(context) { }
}

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(AppDbContext context) : base(context) { }

    public async Task<(IEnumerable<Project> Items, int TotalCount)> GetProjectsAsync(
        DateTime? startDateFrom,
        DateTime? startDateTo,
        int? priority,
        string? sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize)
    {
        var query = _dbSet.AsQueryable();

        // Filtering
        if (startDateFrom.HasValue)
            query = query.Where(p => p.StartDate >= startDateFrom.Value);

        if (startDateTo.HasValue)
            query = query.Where(p => p.StartDate <= startDateTo.Value);

        if (priority.HasValue)
            query = query.Where(p => p.Priority == priority.Value);

        // Sorting (can be more dynamic with reflection if needed, but this is straightforward for known fields)
        query = sortBy?.ToLower() switch
        {
            "name" => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "customercompany" => sortDescending ? query.OrderByDescending(p => p.CustomerCompany) : query.OrderBy(p => p.CustomerCompany),
            "performercompany" => sortDescending ? query.OrderByDescending(p => p.PerformerCompany) : query.OrderBy(p => p.PerformerCompany),
            "priority" => sortDescending ? query.OrderByDescending(p => p.Priority) : query.OrderBy(p => p.Priority),
            "startdate" => sortDescending ? query.OrderByDescending(p => p.StartDate) : query.OrderBy(p => p.StartDate),
            _ => query.OrderBy(p => p.Id) // Default sorting
        };

        var totalCount = await query.CountAsync();

        // Pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}

public class ProjectTaskRepository : Repository<ProjectTask>, IProjectTaskRepository
{
    public ProjectTaskRepository(AppDbContext context) : base(context) { }
}

public class ProjectDocumentRepository : Repository<ProjectDocument>, IProjectDocumentRepository
{
    public ProjectDocumentRepository(AppDbContext context) : base(context) { }
}
