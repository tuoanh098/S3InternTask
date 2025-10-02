using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Shared.Contracts;

public class CheckoutServiceTests
{
    [Fact]
    public async Task QuoteAsync_returns_total_when_valid()
    {
        // Arrange
        var catalog = new Mock<ICatalogClient>();
        catalog.Setup(x => x.GetBookAsync(2, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new BookDto(2, "Deep Work", 18.75m, 20));
        var sut = new CheckoutService(catalog.Object);

        // Act
        var total = await sut.QuoteAsync(2, 3);

        // Assert
        total.Should().Be(56.25m); // 18.75 * 3
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task QuoteAsync_throws_when_qty_not_positive(int qty)
    {
        var sut = new CheckoutService(Mock.Of<ICatalogClient>());
        var act = async () => await sut.QuoteAsync(1, qty);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
                 .WithMessage("*Quantity must be > 0*"); // theo thông điệp trong service
    }

    [Fact]
    public async Task QuoteAsync_throws_when_book_not_found()
    {
        var catalog = new Mock<ICatalogClient>();
        catalog.Setup(x => x.GetBookAsync(999, It.IsAny<CancellationToken>()))
               .ReturnsAsync((BookDto?)null);
        var sut = new CheckoutService(catalog.Object);

        var act = async () => await sut.QuoteAsync(999, 1);
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Book not found");
    }

    [Fact]
    public async Task QuoteAsync_throws_when_insufficient_stock()
    {
        var catalog = new Mock<ICatalogClient>();
        catalog.Setup(x => x.GetBookAsync(1, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new BookDto(1, "Clean Architecture", 39.90m, 2));
        var sut = new CheckoutService(catalog.Object);

        var act = async () => await sut.QuoteAsync(1, 3);
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Insufficient stock");
    }
}
