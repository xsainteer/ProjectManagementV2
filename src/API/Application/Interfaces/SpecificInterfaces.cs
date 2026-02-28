using Domain.Entities;

namespace Application.Interfaces;

// No need to separate these repositories by files, as they are simple and only inherit from the generic IRepository interface.
public interface IEmployeeRepository : IRepository<Employee>
{
}

public interface IProjectRepository : IRepository<Project>
{
}

public interface IProjectTaskRepository : IRepository<ProjectTask>
{
}

public interface IProjectDocumentRepository : IRepository<ProjectDocument>
{
}
