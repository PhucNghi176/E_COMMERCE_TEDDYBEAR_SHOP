using COMMAND.APPLICATION.UseCases.Commands.Products;
using COMMAND.CONTRACT.Services.Products;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.DOMAIN.Entities;
using CONTRACT.CONTRACT.DOMAIN.Exceptions;
using Moq;
using Xunit;

namespace COMMAND.APPLICATION.TEST.UseCases.Commands.Products;
public class CreateProductCommandHandlerTests
{
    private readonly Mock<IRepositoryBase<Product, int>> _mockProductRepository;
    private readonly Mock<IRepositoryBase<Tag, int>> _mockTagRepository;
    private readonly CreateProductCommandHandler _handler;

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
        var command = new COMMAND.CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Test Teddy Bear",
            Size: "Large",
            Color: ["Brown", "White"],
            ImgUrl: ["https://example.com/image1.jpg"],
            TagIds: null,
            PrimaryImgUrl: "", // PrimaryImgUrl is not used in this test
            Quantity: 5,
            Price: 29.99m
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
        var command = new COMMAND.CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Premium Teddy Bear",
            Size: "Medium",
            Color: new[] { "Blue" },
            ImgUrl: new[] { "https://example.com/image2.jpg", "https://example.com/image3.jpg" },
            TagIds: new[] { 1, 2, 3 },
            PrimaryImgUrl: "",
            Quantity: 10,
            Price: 49.99m
        );

        // Setup tag repository to return valid tags using FindSingleAsync
        _mockTagRepository.Setup(r =>
                r.FindSingleAsync(It.Is<System.Linq.Expressions.Expression<Func<Tag, bool>>>(expr => true),
                    It.IsAny<CancellationToken>(), It.IsAny<System.Linq.Expressions.Expression<Func<Tag, object>>[]>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<Tag, bool>> predicate, CancellationToken ct,
                System.Linq.Expressions.Expression<Func<Tag, object>>[] includes) =>
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
            r => r.FindSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(),
                It.IsAny<CancellationToken>(), It.IsAny<System.Linq.Expressions.Expression<Func<Tag, object>>[]>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task Handle_CommandWithEmptyTagIds_ShouldCreateProductWithoutTags()
    {
        // Arrange
        var command = new COMMAND.CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Simple Teddy Bear",
            Size: "Small",
            Color: new[] { "Red" },
            ImgUrl: new[] { "https://example.com/image4.jpg" },
            TagIds: Array.Empty<int>(),
            PrimaryImgUrl: "",
            Quantity: 3,
            Price: 19.99m
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
        var command = new COMMAND.CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Default Teddy Bear",
            Size: "Medium",
            Color: new[] { "Brown" },
            ImgUrl: null,
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
        var command = new COMMAND.CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Tagged Teddy Bear",
            Size: "Large",
            Color: new[] { "Green", "Yellow" },
            ImgUrl: new[] { "https://example.com/image5.jpg" },
            TagIds: new[] { 42 },
            Quantity: 7,
            PrimaryImgUrl: "",
            Price: 35.50m
        );

        // Setup tag repository to return valid tag using FindSingleAsync
        _mockTagRepository.Setup(r => r.FindSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(),
                It.IsAny<CancellationToken>(), It.IsAny<System.Linq.Expressions.Expression<Func<Tag, object>>[]>()))
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
            r => r.FindSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(),
                It.IsAny<CancellationToken>(), It.IsAny<System.Linq.Expressions.Expression<Func<Tag, object>>[]>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSetProductTagNavigationPropertyCorrectly()
    {
        // Arrange
        var command = new COMMAND.CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Navigation Test Bear",
            Size: "Medium",
            Color: new[] { "Purple" },
            ImgUrl: new[] { "https://example.com/image6.jpg" },
            TagIds: new[] { 10, 20 },
            Quantity: 2,
            PrimaryImgUrl: "",
            Price: 25.00m
        );

        // Setup tag repository to return valid tags using FindSingleAsync
        _mockTagRepository.Setup(r => r.FindSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(),
                It.IsAny<CancellationToken>(), It.IsAny<System.Linq.Expressions.Expression<Func<Tag, object>>[]>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<Tag, bool>> predicate, CancellationToken ct,
                System.Linq.Expressions.Expression<Func<Tag, object>>[] includes) =>
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
        var command = new COMMAND.CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: name,
            Size: size,
            Color: new[] { "Brown" },
            ImgUrl: new[] { "https://example.com/test.jpg" },
            TagIds: null,
            PrimaryImgUrl: "",
            Quantity: quantity,
            Price: price
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
        var command = new COMMAND.CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Rainbow Bear",
            Size: "Large",
            Color: colors,
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
        var command = new COMMAND.CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Multi-Image Bear",
            Size: "Medium",
            Color: new[] { "Brown" },
            ImgUrl: images,
            TagIds: null,
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
        var command = new COMMAND.CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Test Bear",
            Size: "Medium",
            Color: new[] { "Brown" },
            ImgUrl: null,
            PrimaryImgUrl: "",
            TagIds: new[] { 1, 2 },
            Quantity: 1,
            Price: 25.00m
        );

        // Setup tag repository to return valid tags using FindSingleAsync
        _mockTagRepository.Setup(r => r.FindSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(),
                It.IsAny<CancellationToken>(), It.IsAny<System.Linq.Expressions.Expression<Func<Tag, object>>[]>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<Tag, bool>> predicate, CancellationToken ct,
                System.Linq.Expressions.Expression<Func<Tag, object>>[] includes) =>
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
            r => r.FindSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(),
                It.IsAny<CancellationToken>(), It.IsAny<System.Linq.Expressions.Expression<Func<Tag, object>>[]>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_CommandWithNonExistentTag_ShouldThrowTagNotFoundException()
    {
        // Arrange
        var command = new COMMAND.CONTRACT.Services.Products.Commands.CreateProductCommand(
            Name: "Test Bear",
            Size: "Medium",
            Color: new[] { "Brown" },
            PrimaryImgUrl: "",
            ImgUrl: null,
            TagIds: new[] { 999 }, // Non-existent tag
            Quantity: 1,
            Price: 25.00m
        );

        // Setup tag repository to return null for non-existent tag
        _mockTagRepository.Setup(r => r.FindSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(),
                It.IsAny<CancellationToken>(), It.IsAny<System.Linq.Expressions.Expression<Func<Tag, object>>[]>()))
            .ReturnsAsync((Tag?)null);

        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await Assert.ThrowsAsync<TagException.TagNotFoundException>(() => _handler.Handle(command, cancellationToken));

        // Verify that tag repository was called
        _mockTagRepository.Verify(
            r => r.FindSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(),
                It.IsAny<CancellationToken>(), It.IsAny<System.Linq.Expressions.Expression<Func<Tag, object>>[]>()),
            Times.Once);

        // Verify that product repository was NOT called since validation failed
        _mockProductRepository.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
    }
}