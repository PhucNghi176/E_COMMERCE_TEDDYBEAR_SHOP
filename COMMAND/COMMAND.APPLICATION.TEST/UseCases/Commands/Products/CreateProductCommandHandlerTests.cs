using System.Linq.Expressions;
using COMMAND.APPLICATION.UseCases.Commands.Products;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.DOMAIN.Entities;
using CONTRACT.CONTRACT.DOMAIN.Exceptions;
using Moq;
using Xunit;

namespace COMMAND.APPLICATION.TEST.UseCases.Commands.Products;
public class CreateProductCommandHandlerTests
{
    private readonly CreateProductCommandHandler _handler;
    private readonly Mock<IRepositoryBase<Product, int>> _mockProductRepository;
    private readonly Mock<IRepositoryBase<Tag, int>> _mockTagRepository;

    public CreateProductCommandHandlerTests()
    {
        _mockProductRepository = new Mock<IRepositoryBase<Product, int>>();
        _mockTagRepository = new Mock<IRepositoryBase<Tag, int>>();
        _handler = new CreateProductCommandHandler(_mockProductRepository.Object, _mockTagRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidCommandWithoutTags_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var command = new CONTRACT.Services.Products.Commands.CreateProductCommand(
            "Test Teddy Bear",
            "Large",
            ["Brown", "White"],
            ["https://example.com/image1.jpg"],
            null,
            "", // PrimaryImgUrl is not used in this test
            5,
            29.99m
        );

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockProductRepository.Verify(r => r.Add(It.Is<Product>(p =>
            p.Name == "Test Teddy Bear" &&
            p.Size == "Large" &&
            p.Color!.SequenceEqual(new[] { "Brown", "White" }) &&
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
            "Premium Teddy Bear",
            "Medium",
            new[] { "Blue" },
            new[] { "https://example.com/image2.jpg", "https://example.com/image3.jpg" },
            new[] { 1, 2, 3 },
            "",
            10,
            49.99m
        );

        // Setup tag repository to return valid tags using FindSingleAsync
        _mockTagRepository.Setup(r =>
                r.FindSingleAsync(It.Is<Expression<Func<Tag, bool>>>(expr => true),
                    It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Tag, object>>[]>()))
            .ReturnsAsync((Expression<Func<Tag, bool>> predicate, CancellationToken ct,
                Expression<Func<Tag, object>>[] includes) =>
            {
                // Simulate finding tags by ID
                var compiled = predicate.Compile();
                if (compiled(new Tag { Id = 1, Name = "Tag1" })) return new Tag { Id = 1, Name = "Tag1" };
                if (compiled(new Tag { Id = 2, Name = "Tag2" })) return new Tag { Id = 2, Name = "Tag2" };
                if (compiled(new Tag { Id = 3, Name = "Tag3" })) return new Tag { Id = 3, Name = "Tag3" };
                return null;
            });

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockProductRepository.Verify(r => r.Add(It.Is<Product>(p =>
            p.Name == "Premium Teddy Bear" &&
            p.Size == "Medium" &&
            p.Color!.SequenceEqual(new[] { "Blue" }) &&
            p.ImgUrl!.SequenceEqual(new[] { "https://example.com/image2.jpg", "https://example.com/image3.jpg" }) &&
            p.Quantity == 10 &&
            p.Price == 49.99m &&
            p.ProductTags.Count == 3 &&
            p.ProductTags.Any(pt => pt.TagId == 1) &&
            p.ProductTags.Any(pt => pt.TagId == 2) &&
            p.ProductTags.Any(pt => pt.TagId == 3)
        )), Times.Once);

        // Verify that tag repository was called to validate tags
        _mockTagRepository.Verify(
            r => r.FindSingleAsync(It.IsAny<Expression<Func<Tag, bool>>>(),
                It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Tag, object>>[]>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task Handle_CommandWithEmptyTagIds_ShouldCreateProductWithoutTags()
    {
        // Arrange
        var command = new CONTRACT.Services.Products.Commands.CreateProductCommand(
            "Simple Teddy Bear",
            "Small",
            new[] { "Red" },
            new[] { "https://example.com/image4.jpg" },
            Array.Empty<int>(),
            "",
            3,
            19.99m
        );

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockProductRepository.Verify(r => r.Add(It.Is<Product>(p =>
            p.Name == "Simple Teddy Bear" &&
            p.ProductTags.Count == 0
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_CommandWithDefaultValues_ShouldCreateProductWithDefaults()
    {
        // Arrange
        var command = new CONTRACT.Services.Products.Commands.CreateProductCommand(
            "Default Teddy Bear",
            "Medium",
            new[] { "Brown" },
            null,
            PrimaryImgUrl: "",
            TagIds: null
        ); // Using default Quantity = 1, Price = 0.1m

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockProductRepository.Verify(r => r.Add(It.Is<Product>(p =>
            p.Name == "Default Teddy Bear" &&
            p.Size == "Medium" &&
            p.Color!.SequenceEqual(new[] { "Brown" }) &&
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
            "Tagged Teddy Bear",
            "Large",
            new[] { "Green", "Yellow" },
            new[] { "https://example.com/image5.jpg" },
            new[] { 42 },
            Quantity: 7,
            PrimaryImgUrl: "",
            Price: 35.50m
        );

        // Setup tag repository to return valid tag using FindSingleAsync
        _mockTagRepository.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<Tag, bool>>>(),
                It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Tag, object>>[]>()))
            .ReturnsAsync(new Tag { Id = 42, Name = "SpecialTag" });

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockProductRepository.Verify(r => r.Add(It.Is<Product>(p =>
            p.ProductTags.Count == 1 &&
            p.ProductTags.First().TagId == 42 &&
            p.ProductTags.First().Product == p
        )), Times.Once);

        // Verify that tag repository was called to validate tag
        _mockTagRepository.Verify(
            r => r.FindSingleAsync(It.IsAny<Expression<Func<Tag, bool>>>(),
                It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Tag, object>>[]>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSetProductTagNavigationPropertyCorrectly()
    {
        // Arrange
        var command = new CONTRACT.Services.Products.Commands.CreateProductCommand(
            "Navigation Test Bear",
            "Medium",
            new[] { "Purple" },
            new[] { "https://example.com/image6.jpg" },
            new[] { 10, 20 },
            Quantity: 2,
            PrimaryImgUrl: "",
            Price: 25.00m
        );

        // Setup tag repository to return valid tags using FindSingleAsync
        _mockTagRepository.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<Tag, bool>>>(),
                It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Tag, object>>[]>()))
            .ReturnsAsync((Expression<Func<Tag, bool>> predicate, CancellationToken ct,
                Expression<Func<Tag, object>>[] includes) =>
            {
                var compiled = predicate.Compile();
                if (compiled(new Tag { Id = 10, Name = "Tag10" })) return new Tag { Id = 10, Name = "Tag10" };
                if (compiled(new Tag { Id = 20, Name = "Tag20" })) return new Tag { Id = 20, Name = "Tag20" };
                return null;
            });

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockProductRepository.Verify(r => r.Add(It.Is<Product>(p =>
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
            name,
            size,
            new[] { "Brown" },
            new[] { "https://example.com/test.jpg" },
            null,
            "",
            quantity,
            price
        );

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockProductRepository.Verify(r => r.Add(It.Is<Product>(p =>
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
            "Rainbow Bear",
            "Large",
            colors,
            PrimaryImgUrl: "",
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
        _mockProductRepository.Verify(r => r.Add(It.Is<Product>(p =>
            p.Color!.SequenceEqual(colors)
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
            "Multi-Image Bear",
            "Medium",
            new[] { "Brown" },
            images,
            null,
            Quantity: 5,
            PrimaryImgUrl: "",
            Price: 45.00m
        );

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockProductRepository.Verify(r => r.Add(It.Is<Product>(p =>
            p.ImgUrl!.SequenceEqual(images)
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_CommandWithValidTags_ShouldCallTagRepositoryForValidation()
    {
        // Arrange
        var command = new CONTRACT.Services.Products.Commands.CreateProductCommand(
            "Test Bear",
            "Medium",
            new[] { "Brown" },
            null,
            PrimaryImgUrl: "",
            TagIds: new[] { 1, 2 },
            Quantity: 1,
            Price: 25.00m
        );

        // Setup tag repository to return valid tags using FindSingleAsync
        _mockTagRepository.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<Tag, bool>>>(),
                It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Tag, object>>[]>()))
            .ReturnsAsync((Expression<Func<Tag, bool>> predicate, CancellationToken ct,
                Expression<Func<Tag, object>>[] includes) =>
            {
                var compiled = predicate.Compile();
                if (compiled(new Tag { Id = 1, Name = "Tag1" })) return new Tag { Id = 1, Name = "Tag1" };
                if (compiled(new Tag { Id = 2, Name = "Tag2" })) return new Tag { Id = 2, Name = "Tag2" };
                return null;
            });

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify that tag repository was called to validate tags
        _mockTagRepository.Verify(
            r => r.FindSingleAsync(It.IsAny<Expression<Func<Tag, bool>>>(),
                It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Tag, object>>[]>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_CommandWithNonExistentTag_ShouldThrowTagNotFoundException()
    {
        // Arrange
        var command = new CONTRACT.Services.Products.Commands.CreateProductCommand(
            "Test Bear",
            "Medium",
            new[] { "Brown" },
            PrimaryImgUrl: "",
            ImgUrl: null,
            TagIds: new[] { 999 }, // Non-existent tag
            Quantity: 1,
            Price: 25.00m
        );

        // Setup tag repository to return null for non-existent tag
        _mockTagRepository.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<Tag, bool>>>(),
                It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Tag, object>>[]>()))
            .ReturnsAsync((Tag?)null);

        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await Assert.ThrowsAsync<TagException.TagNotFoundException>(() => _handler.Handle(command, cancellationToken));

        // Verify that tag repository was called
        _mockTagRepository.Verify(
            r => r.FindSingleAsync(It.IsAny<Expression<Func<Tag, bool>>>(),
                It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Tag, object>>[]>()),
            Times.Once);

        // Verify that product repository was NOT called since validation failed
        _mockProductRepository.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
    }
}