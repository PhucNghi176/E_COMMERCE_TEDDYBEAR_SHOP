using COMMAND.APPLICATION.UseCases.Commands.Products;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.DOMAIN.Entities;
using Moq;
using Xunit;

namespace COMMAND.APPLICATION.TEST.UseCases.Commands.Products;
public class CreateProductCommandHandlerTests
{
    private readonly Mock<IRepositoryBase<Product, int>> _mockRepository;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepositoryBase<Product, int>>();
        _handler = new CreateProductCommandHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidCommandWithoutTags_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var command = new CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Test Teddy Bear",
            Size: "Large",
            Color: new[] { "Brown", "White" },
            ImgUrl: new[] { "https://example.com/image1.jpg" },
            TagIds: null,
            Quantity: 5,
            Price: 29.99m
        );

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepository.Verify(r => r.Add(It.Is<Product>(p =>
            p.Name == "Test Teddy Bear" &&
            p.Size == "Large" &&
            p.Color.SequenceEqual(new[] { "Brown", "White" }) &&
            p.ImgUrl!.SequenceEqual(new[] { "https://example.com/image1.jpg" }) &&
            p.Quantity == 5 &&
            p.Price == 29.99m &&
            p.ProductTags.Count == 0
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommandWithTags_ShouldCreateProductWithTagsSuccessfully()
    {
        // Arrange
        var command = new CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Premium Teddy Bear",
            Size: "Medium",
            Color: new[] { "Blue" },
            ImgUrl: new[] { "https://example.com/image2.jpg", "https://example.com/image3.jpg" },
            TagIds: new[] { 1, 2, 3 },
            Quantity: 10,
            Price: 49.99m
        );

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepository.Verify(r => r.Add(It.Is<Product>(p =>
            p.Name == "Premium Teddy Bear" &&
            p.Size == "Medium" &&
            p.Color.SequenceEqual(new[] { "Blue" }) &&
            p.ImgUrl!.SequenceEqual(new[] { "https://example.com/image2.jpg", "https://example.com/image3.jpg" }) &&
            p.Quantity == 10 &&
            p.Price == 49.99m &&
            p.ProductTags.Count == 3 &&
            p.ProductTags.Any(pt => pt.TagId == 1) &&
            p.ProductTags.Any(pt => pt.TagId == 2) &&
            p.ProductTags.Any(pt => pt.TagId == 3)
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_CommandWithEmptyTagIds_ShouldCreateProductWithoutTags()
    {
        // Arrange
        var command = new CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Simple Teddy Bear",
            Size: "Small",
            Color: new[] { "Red" },
            ImgUrl: new[] { "https://example.com/image4.jpg" },
            TagIds: Array.Empty<int>(),
            Quantity: 3,
            Price: 19.99m
        );

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepository.Verify(r => r.Add(It.Is<Product>(p =>
            p.Name == "Simple Teddy Bear" &&
            p.ProductTags.Count == 0
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_CommandWithDefaultValues_ShouldCreateProductWithDefaults()
    {
        // Arrange
        var command = new CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Default Teddy Bear",
            Size: "Medium",
            Color: new[] { "Brown" },
            ImgUrl: null,
            TagIds: null
        ); // Using default Quantity = 1, Price = 0.1m

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepository.Verify(r => r.Add(It.Is<Product>(p =>
            p.Name == "Default Teddy Bear" &&
            p.Size == "Medium" &&
            p.Color.SequenceEqual(new[] { "Brown" }) &&
            p.ImgUrl == null &&
            p.Quantity == 1 &&
            p.Price == 0.1m &&
            p.ProductTags.Count == 0
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_CommandWithSingleTag_ShouldCreateProductWithOneTag()
    {
        // Arrange
        var command = new CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Tagged Teddy Bear",
            Size: "Large",
            Color: new[] { "Green", "Yellow" },
            ImgUrl: new[] { "https://example.com/image5.jpg" },
            TagIds: new[] { 42 },
            Quantity: 7,
            Price: 35.50m
        );

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepository.Verify(r => r.Add(It.Is<Product>(p =>
            p.ProductTags.Count == 1 &&
            p.ProductTags.First().TagId == 42 &&
            p.ProductTags.First().Product == p
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSetProductTagNavigationPropertyCorrectly()
    {
        // Arrange
        var command = new CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Navigation Test Bear",
            Size: "Medium",
            Color: new[] { "Purple" },
            ImgUrl: new[] { "https://example.com/image6.jpg" },
            TagIds: new[] { 10, 20 },
            Quantity: 2,
            Price: 25.00m
        );

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepository.Verify(r => r.Add(It.Is<Product>(p =>
            p.ProductTags.All(pt => pt.Product == p) &&
            p.ProductTags.Count == 2
        )), Times.Once);
    }

    [Theory]
    [InlineData("Mini Bear", "XS", 1, 9.99)]
    [InlineData("Giant Bear", "XXL", 50, 199.99)]
    [InlineData("Standard Bear", "M", 25, 39.99)]
    public async Task Handle_VariousValidInputs_ShouldCreateProductSuccessfully(
        string name, string size, int quantity, decimal price)
    {
        // Arrange
        var command = new CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: name,
            Size: size,
            Color: new[] { "Brown" },
            ImgUrl: new[] { "https://example.com/test.jpg" },
            TagIds: null,
            Quantity: quantity,
            Price: price
        );

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepository.Verify(r => r.Add(It.Is<Product>(p =>
            p.Name == name &&
            p.Size == size &&
            p.Quantity == quantity &&
            p.Price == price
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_CommandWithMultipleColors_ShouldCreateProductWithAllColors()
    {
        // Arrange
        var colors = new[] { "Red", "Blue", "Green", "Yellow", "Purple" };
        var command = new CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Rainbow Bear",
            Size: "Large",
            Color: colors,
            ImgUrl: new[] { "https://example.com/rainbow.jpg" },
            TagIds: null,
            Quantity: 1,
            Price: 59.99m
        );

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepository.Verify(r => r.Add(It.Is<Product>(p =>
            p.Color.SequenceEqual(colors)
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_CommandWithMultipleImages_ShouldCreateProductWithAllImages()
    {
        // Arrange
        var images = new[]
        {
            "https://example.com/front.jpg",
            "https://example.com/back.jpg",
            "https://example.com/side.jpg"
        };
        var command = new CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Multi-Image Bear",
            Size: "Medium",
            Color: ["Brown"],
            ImgUrl: images,
            TagIds: null,
            Quantity: 5,
            Price: 45.00m
        );

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepository.Verify(r => r.Add(It.Is<Product>(p =>
            p.ImgUrl!.SequenceEqual(images)
        )), Times.Once);
    }
}