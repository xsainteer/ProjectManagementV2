using Application.DTOs.Employee;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Common;
using Domain.Entities;

namespace Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<Result<EmployeeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, true, cancellationToken);
        if (employee == null)
            return Result<EmployeeDto>.Failure("Employee not found");

        return Result<EmployeeDto>.Success(new EmployeeDto(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.MiddleName,
            employee.Email
        ));
    }

    public async Task<Result<IEnumerable<EmployeeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.GetAllAsync(true, cancellationToken);
        var dtos = employees.Select(e => new EmployeeDto(
            e.Id,
            e.FirstName,
            e.LastName,
            e.MiddleName,
            e.Email
        ));

        return Result<IEnumerable<EmployeeDto>>.Success(dtos);
    }

    public async Task<Result<EmployeeDto>> CreateAsync(CreateEmployeeDto createDto, CancellationToken cancellationToken = default)
    {
        var employee = new Employee
        {
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            MiddleName = createDto.MiddleName,
            Email = createDto.Email
        };

        await _employeeRepository.AddAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        return Result<EmployeeDto>.Success(new EmployeeDto(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.MiddleName,
            employee.Email
        ));
    }

    public async Task<Result> UpdateAsync(UpdateEmployeeDto updateDto, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(updateDto.Id, false, cancellationToken);
        if (employee == null)
            return Result.Failure("Employee not found");

        employee.FirstName = updateDto.FirstName;
        employee.LastName = updateDto.LastName;
        employee.MiddleName = updateDto.MiddleName;
        employee.Email = updateDto.Email;

        _employeeRepository.Update(employee);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, false, cancellationToken);
        if (employee == null)
            return Result.Failure("Employee not found");

        _employeeRepository.Delete(employee);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
