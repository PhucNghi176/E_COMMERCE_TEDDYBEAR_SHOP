using CONTRACT.CONTRACT.CONTRACT.Attributes;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Entities;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.PERSISTENCE.DependencyInjection.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace QUERY.PERSISTENCE.Repositories;
public sealed class MongoRepository<TDocument> : IMongoRepository<TDocument>
    where TDocument : IDocument
{
    private readonly IMongoCollection<TDocument> _collection;

    public MongoRepository(IMongoDbSettings settings)
    {
        var database = new MongoClient(settings.ConnectionString).GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
    }

    public IMongoQueryable<TDocument> AsQueryable(Expression<Func<TDocument, bool>>? filterExpression = null)
    {
        return filterExpression is not null
            ? _collection.AsQueryable().Where(filterExpression)
            : _collection.AsQueryable();
    }

    public async Task<UpdateResult> UpdateOneAsync(
        FilterDefinition<TDocument> filter,
        UpdateDefinition<TDocument> update,
        UpdateOptions options = null,
        CancellationToken cancellationToken = default)
    {
        // Perform the update operation
        return await _collection.UpdateOneAsync(filter, update, options, cancellationToken);
    }

    public IEnumerable<TDocument> FilterBy(
        Expression<Func<TDocument, bool>> filterExpression)
    {
        return _collection.Find(filterExpression).ToEnumerable();
    }

    public IEnumerable<TProjected> FilterBy<TProjected>(
        Expression<Func<TDocument, bool>> filterExpression,
        Expression<Func<TDocument, TProjected>> projectionExpression)
    {
        return _collection.Find(filterExpression).Project(projectionExpression).ToEnumerable();
    }

    public TDocument FindOne(Expression<Func<TDocument, bool>> filterExpression)
    {
        return _collection.Find(filterExpression).FirstOrDefault();
    }

    public Task<TDocument?> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression)
    {
        return Task.Run(() => _collection.Find(filterExpression).FirstOrDefaultAsync());
    }

    public TDocument FindById(string id)
    {
        var objectId = new ObjectId(id);
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.DocumentId, objectId);
        return _collection.Find(filter).SingleOrDefault();
    }

    public Task<TDocument> FindByIdAsync(string id)
    {
        return Task.Run(() =>
        {
            var objectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.DocumentId, objectId);
            return _collection.Find(filter).SingleOrDefaultAsync();
        });
    }


    public void InsertOne(TDocument document)
    {
        _collection.InsertOne(document);
    }

    public Task InsertOneAsync(TDocument document)
    {
        return Task.Run(() => _collection.InsertOneAsync(document));
    }

    public void InsertMany(ICollection<TDocument> documents)
    {
        _collection.InsertMany(documents);
    }


    public async Task InsertManyAsync(ICollection<TDocument> documents)
    {
        await _collection.InsertManyAsync(documents);
    }

    public void ReplaceOne(TDocument document)
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.DocumentId, document.DocumentId);
        _ = _collection.FindOneAndReplace(filter, document);
    }

    public async Task ReplaceOneAsync(TDocument document)
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.DocumentId, document.DocumentId);
        _ = await _collection.FindOneAndReplaceAsync(filter, document);
    }

    public void DeleteOne(Expression<Func<TDocument, bool>> filterExpression)
    {
        _ = _collection.FindOneAndDelete(filterExpression);
    }

    public Task DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression)
    {
        return Task.Run(() => _collection.FindOneAndDeleteAsync(filterExpression));
    }

    public void DeleteById(string id)
    {
        var objectId = new ObjectId(id);
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.DocumentId, objectId);
        _ = _collection.FindOneAndDelete(filter);
    }

    public Task DeleteByIdAsync(string id)
    {
        return Task.Run(() =>
        {
            var objectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.DocumentId, objectId);
            _ = _collection.FindOneAndDeleteAsync(filter);
        });
    }

    public void DeleteMany(Expression<Func<TDocument, bool>> filterExpression)
    {
        _ = _collection.DeleteMany(filterExpression);
    }

    public Task DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression)
    {
        return Task.Run(() => _collection.DeleteManyAsync(filterExpression));
    }

    private static string GetCollectionName(Type documentType)
    {
        return ((BsonCollectionAttribute)documentType.GetCustomAttributes(
                typeof(BsonCollectionAttribute), true)
            .FirstOrDefault())?.CollectionName;
    }
}