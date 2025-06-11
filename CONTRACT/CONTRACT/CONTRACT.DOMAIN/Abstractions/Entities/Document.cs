using MongoDB.Bson;

namespace CONTRACT.CONTRACT.DOMAIN.Abstractions.Entities;
public abstract class Document : IDocument
{
    public Guid Id { get; set; } // Id cua SourceMessage: ProductID, CustomerID, OrderI
    public ObjectId DocumentId { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }
}