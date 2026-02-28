namespace Application.DTOs.Employee;

public record EmployeeDto(
    int Id,
    string FirstName,
    string LastName,
    string MiddleName,
    string Email
);

public record CreateEmployeeDto(
    string FirstName,
    string LastName,
    string MiddleName,
    string Email
);

public record UpdateEmployeeDto(
    int Id,
    string FirstName,
    string LastName,
    string MiddleName,
    string Email
);
