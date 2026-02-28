using Application.DTOs.Employee;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Common;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(IEmployeeRepository employeeRepository, ILogger<EmployeeService> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public async Task<Result<EmployeeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdAsync(id, true, cancellationToken);
            if (employee == null)
                return Result<EmployeeDto>.Failure("Employee not found", 404);

            return Result<EmployeeDto>.Success(new EmployeeDto(
                employee.Id,
                employee.FirstName,
                employee.LastName,
                employee.MiddleName,
                employee.Email
            ), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting employee with ID {EmployeeId}", id);
            return Result<EmployeeDto>.Failure("An internal error occurred", 500);
        }
    }

    public async Task<Result<IEnumerable<EmployeeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var employees = await _employeeRepository.GetAllAsync(true, cancellationToken);
            var dtos = employees.Select(e => new EmployeeDto(
                e.Id,
                e.FirstName,
                e.LastName,
                e.MiddleName,
                e.Email
            ));

            return Result<IEnumerable<EmployeeDto>>.Success(dtos, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all employees");
            return Result<IEnumerable<EmployeeDto>>.Failure("An internal error occurred", 500);
        }
    }

    public async Task<Result<EmployeeDto>> CreateAsync(CreateEmployeeDto createDto, CancellationToken cancellationToken = default)
    {
        try
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
            ), 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating employee");
            return Result<EmployeeDto>.Failure("An internal error occurred", 500);
        }
    }

    public async Task<Result> UpdateAsync(UpdateEmployeeDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdAsync(updateDto.Id, false, cancellationToken);
            if (employee == null)
                return Result.Failure("Employee not found", 404);

            employee.FirstName = updateDto.FirstName;
            employee.LastName = updateDto.LastName;
            employee.MiddleName = updateDto.MiddleName;
            employee.Email = updateDto.Email;

            _employeeRepository.Update(employee);
            await _employeeRepository.SaveChangesAsync(cancellationToken);

            return Result.Success(204);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating employee with ID {EmployeeId}", updateDto.Id);
            return Result.Failure("An internal error occurred", 500);
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdAsync(id, false, cancellationToken);
            if (employee == null)
                return Result.Failure("Employee not found", 404);

            _employeeRepository.Delete(employee);
            await _employeeRepository.SaveChangesAsync(cancellationToken);

            return Result.Success(204);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting employee with ID {EmployeeId}", id);
            return Result.Failure("An internal error occurred", 500);
        }
    }
}
