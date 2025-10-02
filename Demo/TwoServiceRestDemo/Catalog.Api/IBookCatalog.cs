namespace Catalog.Api;

using Shared.Contracts;
using System.Threading;

public interface IBookCatalog
{
    Task<IReadOnlyList<BookDto>> GetAllAsync(CancellationToken ct = default);
    Task<BookDto?> GetByIdAsync(long id, CancellationToken ct = default);
}
