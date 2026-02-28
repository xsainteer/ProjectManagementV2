using Application.DTOs.Employee;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Endpoints;

public static class EmployeeEndpoints
{
    public static void MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/employees").WithTags("Employees");

        group.MapGet("/", GetEmployees);
        group.MapGet("/{id:int}", GetEmployeeById);
        group.MapPost("/", CreateEmployee);
        group.MapPut("/{id:int}", UpdateEmployee);
        group.MapDelete("/{id:int}", DeleteEmployee);
    }

    private static async Task<IResult> GetEmployees(
        [FromQuery] string? search,
        [FromServices] IEmployeeService employeeService,
        CancellationToken cancellationToken)
    {
        var result = await employeeService.GetAllAsync(search, cancellationToken);
        return ResultMapper.ToActionResult(result);
    }

    private static async Task<IResult> GetEmployeeById(
        int id,
        [FromServices] IEmployeeService employeeService,
        CancellationToken cancellationToken)
    {
        var result = await employeeService.GetByIdAsync(id, cancellationToken);
        return ResultMapper.ToActionResult(result);
    }

    private static async Task<IResult> CreateEmployee(
        CreateEmployeeDto createDto,
        [FromServices] IEmployeeService employeeService,
        CancellationToken cancellationToken)
    {
        var result = await employeeService.CreateAsync(createDto, cancellationToken);
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status201Created);
    }

    private static async Task<IResult> UpdateEmployee(
        int id,
        UpdateEmployeeDto updateDto,
        [FromServices] IEmployeeService employeeService,
        CancellationToken cancellationToken)
    {
        if (id != updateDto.Id)
            return Results.BadRequest("ID mismatch");

        var result = await employeeService.UpdateAsync(updateDto, cancellationToken);
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> DeleteEmployee(
        int id,
        [FromServices] IEmployeeService employeeService,
        CancellationToken cancellationToken)
    {
        var result = await employeeService.DeleteAsync(id, cancellationToken);
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status204NoContent);
    }
}