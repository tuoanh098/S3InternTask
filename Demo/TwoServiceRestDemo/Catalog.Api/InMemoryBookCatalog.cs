namespace Catalog.Api;

using Shared.Contracts;

public sealed class InMemoryBookCatalog : IBookCatalog
{
    private readonly List<BookDto> _books;

    public InMemoryBookCatalog(IEnumerable<BookDto> seed)
        => _books = seed.ToList();

    public Task<IReadOnlyList<BookDto>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<BookDto>>(_books);

    public Task<BookDto?> GetByIdAsync(long id, CancellationToken ct = default)
        => Task.FromResult(_books.FirstOrDefault(x => x.Id == id));
}
