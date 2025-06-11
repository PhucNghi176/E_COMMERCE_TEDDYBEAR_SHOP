namespace CONTRACT.CONTRACT.DOMAIN.Exceptions;
public static class TagException
{
    public class TagAlreadyExistsException(string tagName)
        : AlreadyExistedException("Tag already exists", $"Tag with name '{tagName}' already exists.");
}