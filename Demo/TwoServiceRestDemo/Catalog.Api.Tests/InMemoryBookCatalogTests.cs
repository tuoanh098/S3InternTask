using Catalog.Api;
using Shared.Contracts;
using FluentAssertions;

public class InMemoryBookCatalogTests
{
    private static InMemoryBookCatalog CreateSut() => new(new[]
    {
        new BookDto(1, "Clean Architecture", 39.90m, 25),
        new BookDto(2, "Deep Work",          18.75m, 20),
        new BookDto(3, "Kubernetes in Action", 49.00m, 10)
    });

    [Fact]
    public async Task GetAllAsync_returns_all_seeded_books()
    {
        var sut = CreateSut();

        var result = await sut.GetAllAsync();

        result.Should().HaveCount(3);
        result.Select(b => b.Id).Should().BeEquivalentTo(new long[] { 1, 2, 3 });
    }

    [Theory]
    [InlineData(1, "Clean Architecture")]
    [InlineData(2, "Deep Work")]
    [InlineData(3, "Kubernetes in Action")]
    public async Task GetByIdAsync_returns_correct_book(long id, string expectedTitle)
    {
        var sut = CreateSut();

        var book = await sut.GetByIdAsync(id);

        book.Should().NotBeNull();
        book!.Title.Should().Be(expectedTitle);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_not_found()
    {
        var sut = CreateSut();

        var book = await sut.GetByIdAsync(999);

        book.Should().BeNull();
    }
}
