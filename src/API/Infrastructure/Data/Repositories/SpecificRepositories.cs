using Application.DTOs.Project;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Employee>> GetAllAsync(string? searchTerm, bool asNoTracking = true, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (asNoTracking)
            query = query.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(e => 
                e.FirstName.ToLower().Contains(searchTerm) || 
                e.LastName.ToLower().Contains(searchTerm) || 
                e.Email.ToLower().Contains(searchTerm) ||
                (e.FirstName + " " + e.LastName).ToLower().Contains(searchTerm));
        }

        return await query.ToListAsync(cancellationToken);
    }
}

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    private readonly IFileService _fileService;

    public ProjectRepository(AppDbContext context, IFileService fileService) : base(context)
    {
        _fileService = fileService;
    }

    public async Task<(IEnumerable<Project> Items, int TotalCount)> GetProjectsAsync(
        DateTime? startDateFrom,
        DateTime? startDateTo,
        int? priority,
        string? sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (asNoTracking)
            query = query.AsNoTracking();

        // Filtering
        if (startDateFrom.HasValue)
            query = query.Where(p => p.StartDate >= startDateFrom.Value);

        if (startDateTo.HasValue)
            query = query.Where(p => p.StartDate <= startDateTo.Value);

        if (priority.HasValue)
            query = query.Where(p => p.Priority == priority.Value);

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "name" => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "customercompany" => sortDescending ? query.OrderByDescending(p => p.CustomerCompany) : query.OrderBy(p => p.CustomerCompany),
            "performercompany" => sortDescending ? query.OrderByDescending(p => p.PerformerCompany) : query.OrderBy(p => p.PerformerCompany),
            "priority" => sortDescending ? query.OrderByDescending(p => p.Priority) : query.OrderBy(p => p.Priority),
            "startdate" => sortDescending ? query.OrderByDescending(p => p.StartDate) : query.OrderBy(p => p.StartDate),
            _ => query.OrderBy(p => p.Id) // Default sorting
        };

        var totalCount = await query.CountAsync(cancellationToken);

        // Pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Project> CreateFullAsync(Project project, List<int> executorIds, List<FileData> files, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var savedFilePaths = new List<string>();

        try
        {
            // 2. Add Executors
            project.Employees = new List<Employee>();
            foreach (var executorId in executorIds)
            {
                var executor = await _context.Employees.FindAsync(new object[] { executorId }, cancellationToken);
                if (executor == null)
                {
                    throw new Exception($"Executor with ID {executorId} was not found.");
                }
                project.Employees.Add(executor);
            }

            // 3. Save Project to get ID
            await _dbSet.AddAsync(project, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Handle Files
            foreach (var file in files)
            {
                var saveResult = await _fileService.SaveFileAsync(file.Stream, file.FileName, $"project_{project.Id}", cancellationToken);
                if (saveResult.IsFailure)
                {
                    throw new Exception(saveResult.Error.Message);
                }
                
                savedFilePaths.Add(saveResult.Value);
                
                project.Documents.Add(new ProjectDocument
                {
                    FileName = file.FileName,
                    FilePath = saveResult.Value,
                    ProjectId = project.Id
                });
            }

            // 5. Final save (for documents)
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return project;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            foreach (var path in savedFilePaths)
            {
                _fileService.DeleteFile(path);
            }
            throw;
        }
    }

    public async Task<Project?> GetWithEmployeesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Employees)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
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
