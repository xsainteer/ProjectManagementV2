using Application.DTOs.Employee;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Validators;
using Domain.Common;
using Domain.Entities;
using FluentValidation;

namespace Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IValidator<CreateEmployeeDto> _createValidator;
    private readonly IValidator<UpdateEmployeeDto> _updateValidator;

    public EmployeeService(
        IEmployeeRepository employeeRepository, 
        IValidator<CreateEmployeeDto> createValidator,
        IValidator<UpdateEmployeeDto> updateValidator)
    {
        _employeeRepository = employeeRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<EmployeeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, true, cancellationToken);
        if (employee == null)
            return Result<EmployeeDto>.Failure(Error.NotFound($"Employee with ID {id} was not found."));

        return Result<EmployeeDto>.Success(MapToDto(employee));
    }

    public async Task<Result<IEnumerable<EmployeeDto>>> GetAllAsync(string? searchTerm, CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.GetAllAsync(searchTerm, true, cancellationToken);
        return Result<IEnumerable<EmployeeDto>>.Success(employees.Select(MapToDto));
    }

    public async Task<Result<IEnumerable<EmployeeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetAllAsync(null, cancellationToken);
    }

    public async Task<Result<EmployeeDto>> CreateAsync(CreateEmployeeDto createDto, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateToResultAsync(createDto, cancellationToken);
        if (validation.IsFailure) return Result<EmployeeDto>.Failure(validation.Error);

        var employee = new Employee
        {
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            MiddleName = createDto.MiddleName,
            Email = createDto.Email
        };

        await _employeeRepository.AddAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        return Result<EmployeeDto>.Success(MapToDto(employee));
    }

    public async Task<Result> UpdateAsync(UpdateEmployeeDto updateDto, CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateToResultAsync(updateDto, cancellationToken);
        if (validation.IsFailure) return validation;

        var employee = await _employeeRepository.GetByIdAsync(updateDto.Id, false, cancellationToken);
        if (employee == null)
            return Result.Failure(Error.NotFound($"Employee with ID {updateDto.Id} was not found."));

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
            return Result.Failure(Error.NotFound($"Employee with ID {id} was not found."));

        _employeeRepository.Delete(employee);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static EmployeeDto MapToDto(Employee employee) => new(
        employee.Id,
        employee.FirstName,
        employee.LastName,
        employee.MiddleName,
        employee.Email
    );
}
