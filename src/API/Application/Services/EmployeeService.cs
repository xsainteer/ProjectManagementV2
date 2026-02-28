using Application.DTOs.Employee;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Common;
using Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<EmployeeService> _logger;
    private readonly IValidator<CreateEmployeeDto> _createValidator;
    private readonly IValidator<UpdateEmployeeDto> _updateValidator;

    public EmployeeService(
        IEmployeeRepository employeeRepository, 
        ILogger<EmployeeService> logger,
        IValidator<CreateEmployeeDto> createValidator,
        IValidator<UpdateEmployeeDto> updateValidator)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<EmployeeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdAsync(id, true, cancellationToken);
            if (employee == null)
                return Result<EmployeeDto>.Failure(Error.NotFound($"Employee with ID {id} was not found."));

            return Result<EmployeeDto>.Success(new EmployeeDto(
                employee.Id,
                employee.FirstName,
                employee.LastName,
                employee.MiddleName,
                employee.Email
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting employee with ID {EmployeeId}", id);
            return Result<EmployeeDto>.Failure(Error.Unexpected("An internal error occurred"));
        }
    }

    public async Task<Result<IEnumerable<EmployeeDto>>> GetAllAsync(string? searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            var employees = await _employeeRepository.GetAllAsync(searchTerm, true, cancellationToken);
            var dtos = employees.Select(e => new EmployeeDto(
                e.Id,
                e.FirstName,
                e.LastName,
                e.MiddleName,
                e.Email
            ));

            return Result<IEnumerable<EmployeeDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all employees with search term: {SearchTerm}", searchTerm);
            return Result<IEnumerable<EmployeeDto>>.Failure(Error.Unexpected("An internal error occurred"));
        }
    }

    public async Task<Result<IEnumerable<EmployeeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetAllAsync(null, cancellationToken);
    }

    public async Task<Result<EmployeeDto>> CreateAsync(CreateEmployeeDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = await _createValidator.ValidateAsync(createDto, cancellationToken);
            if (!validationResult.IsValid)
                return Result<EmployeeDto>.Failure(Error.Validation(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating employee");
            return Result<EmployeeDto>.Failure(Error.Unexpected("An internal error occurred"));
        }
    }

    public async Task<Result> UpdateAsync(UpdateEmployeeDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = await _updateValidator.ValidateAsync(updateDto, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Failure(Error.Validation(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating employee with ID {EmployeeId}", updateDto.Id);
            return Result.Failure(Error.Unexpected("An internal error occurred"));
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdAsync(id, false, cancellationToken);
            if (employee == null)
                return Result.Failure(Error.NotFound($"Employee with ID {id} was not found."));

            _employeeRepository.Delete(employee);
            await _employeeRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting employee with ID {EmployeeId}", id);
            return Result.Failure(Error.Unexpected("An internal error occurred"));
        }
    }
}