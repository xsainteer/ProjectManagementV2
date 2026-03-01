using Domain.Common;
using FluentValidation;

namespace Application.Validators;

public static class ValidationExtensions
{
    public static async Task<Result<T>> ValidateToResultAsync<T>(this IValidator<T> validator, T instance, CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (result.IsValid) return Result<T>.Success(instance);

        var errors = string.Join(", ", result.Errors.Select(e => e.ErrorMessage));
        return Result<T>.Failure(Error.Validation(errors));
    }
}
