namespace CONTRACT.CONTRACT.DOMAIN.Exceptions;
public static class ProductException
{
    public class ProductAlreadyExistsException(string productName)
        : AlreadyExistedException("Product already exists", $"Product with name '{productName}' already exists.");

    public class ProductNotFoundException : NotFoundException
    {
        // Constructor with productId parameter
        public ProductNotFoundException(int productId)
            : base("Product not found", $"Product with ID '{productId}' was not found.")
        {
        }

        // Default constructor
        public ProductNotFoundException()
            : base("Product not found", "Product was not found.")
        {
        }
    }
}