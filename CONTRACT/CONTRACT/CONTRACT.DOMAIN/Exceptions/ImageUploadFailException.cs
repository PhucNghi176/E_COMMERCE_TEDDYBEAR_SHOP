namespace CONTRACT.CONTRACT.DOMAIN.Exceptions;
public class ImageUploadFailException(string? message = null) : DomainException("Image upload failed", message);