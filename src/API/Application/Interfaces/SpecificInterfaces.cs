using Domain.Entities;

namespace Application.Interfaces;

// No need to separate these repositories by files, as they are simple and only inherit from the generic IRepository interface.
public interface IEmployeeRepository : IRepository<Employee>
{
}

public interface IProjectRepository : IRepository<Project>
{
    Task<(IEnumerable<Project> Items, int TotalCount)> GetProjectsAsync(
        DateTime? startDateFrom,
        DateTime? startDateTo,
        int? priority,
        string? sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default);
}

public interface IProjectTaskRepository : IRepository<ProjectTask>
{
}

public interface IProjectDocumentRepository : IRepository<ProjectDocument>
{
}
