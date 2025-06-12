namespace CONTRACT.CONTRACT.DOMAIN.Exceptions;
public static class TagException
{
    public class TagAlreadyExistsException(string tagName)
        : AlreadyExistedException("Tag already exists", $"Tag with name '{tagName}' already exists.");

    public class TagNotFoundException : NotFoundException
    {
        // Constructor với tham số tagName
        public TagNotFoundException(string tagName)
            : base("Tag not found", $"Tag with name '{tagName}' was not found.")
        {
        }

        // Constructor không tham số
        public TagNotFoundException()
            : base("Tag not found", "Tag was not found.")
        {
        }
    }
}