using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CONTRACT.CONTRACT.DOMAIN.Abstractions.Entities;
public interface IDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    ObjectId DocumentId { get; set; }

    DateTime CreatedOnUtc { get; set; }
    DateTime? ModifiedOnUtc { get; }


}