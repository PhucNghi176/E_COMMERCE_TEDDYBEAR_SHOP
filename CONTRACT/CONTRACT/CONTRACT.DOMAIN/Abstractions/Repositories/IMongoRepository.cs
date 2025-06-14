﻿using CONTRACT.CONTRACT.DOMAIN.Abstractions.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
public interface IMongoRepository<TDocument>
    where TDocument : IDocument
{
    IMongoQueryable<TDocument> AsQueryable(Expression<Func<TDocument, bool>>? filterExpression = null);

    IEnumerable<TDocument> FilterBy(
        Expression<Func<TDocument, bool>> filterExpression);

    IEnumerable<TProjected> FilterBy<TProjected>(
        Expression<Func<TDocument, bool>> filterExpression,
        Expression<Func<TDocument, TProjected>> projectionExpression);

    TDocument FindOne(Expression<Func<TDocument, bool>> filterExpression);

    Task<TDocument?> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression);

    TDocument FindById(string id);

    Task<TDocument> FindByIdAsync(string id);

    void InsertOne(TDocument document);

    Task InsertOneAsync(TDocument document);

    Task<UpdateResult> UpdateOneAsync(
        FilterDefinition<TDocument> filter,
        UpdateDefinition<TDocument> update,
        UpdateOptions? options = null,
        CancellationToken cancellationToken = default);

    void InsertMany(ICollection<TDocument> documents);

    Task InsertManyAsync(ICollection<TDocument> documents);

    void ReplaceOne(TDocument document);

    Task ReplaceOneAsync(TDocument document);

    void DeleteOne(Expression<Func<TDocument, bool>> filterExpression);

    Task DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression);

    void DeleteById(string id);

    Task DeleteByIdAsync(string id);

    void DeleteMany(Expression<Func<TDocument, bool>> filterExpression);

    Task DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression);
}