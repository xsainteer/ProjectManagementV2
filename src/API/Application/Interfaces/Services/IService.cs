using Domain.Common;

namespace Application.Interfaces.Services;

public interface IService<T, TDto, TCreateDto, TUpdateDto> where T : class, IEntity
{
    Task<Result<TDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<TDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<TDto>> CreateAsync(TCreateDto createDto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(TUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
