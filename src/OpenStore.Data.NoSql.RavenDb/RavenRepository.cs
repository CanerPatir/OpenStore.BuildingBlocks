using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenStore.Data.OutBox;
using OpenStore.Domain;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace OpenStore.Data.NoSql.RavenDb;

public class RavenRepository<TAggregateRoot> : Repository<TAggregateRoot>, IRavenRepository<TAggregateRoot>
    where TAggregateRoot : AggregateRoot<string>, ISavingChanges
{
    private readonly IOutBoxStoreService _outBoxStoreService;
    public IRavenUnitOfWork RavenUow { get; }
    public IQueryable<TAggregateRoot> Query => RavenQuery();
    public IAsyncDocumentSession RavenSession => RavenUow.Session;

    public IUnitOfWork Uow => RavenUow;

    public RavenRepository(IRavenUnitOfWork uow, IOutBoxStoreService outBoxStoreService)
    {
        _outBoxStoreService = outBoxStoreService;
        RavenUow = uow;
    }

    public IRavenQueryable<TAggregateRoot> RavenQuery(string indexName = null, string collectionName = null, bool isMapReduce = false) =>
        RavenSession.Query<TAggregateRoot>(indexName, collectionName, isMapReduce);

    public override async Task<TAggregateRoot> GetAsync(object id, CancellationToken token = default)
    {
        var entity = await RavenSession.LoadAsync<TAggregateRoot>(id.ToString(), token);
        return entity;
    }

    public override async Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken token = default)
    {
        if (aggregateRoot == null) throw new ArgumentNullException(nameof(aggregateRoot));

        if (aggregateRoot.HasUncommittedChanges())
        {
            var events = aggregateRoot.GetUncommittedChanges();
            await _outBoxStoreService.StoreMessages(events, token);
        }

        var documentId = GetDocumentId(aggregateRoot.Id);

        if (IsCreating(aggregateRoot, documentId))
        {
            await RavenSession.StoreAsync(aggregateRoot, documentId, token);
        }

        try
        {
            aggregateRoot.OnSavingChanges();
            await Uow.SaveChangesAsync(token);
        }
        catch (Raven.Client.Exceptions.ConcurrencyException ex)
        {
            throw new ConcurrencyException(ex.Message, ex);
        }

        aggregateRoot.Commit();
    }

    private bool IsCreating(TAggregateRoot entity, string documentId)
    {
        if (!RavenSession.Advanced.HasChanged(entity))
        {
            return true;
        }

        var changes = RavenSession.Advanced.WhatChanged()[documentId];
        return changes.Any(x => x.Change == DocumentsChanges.ChangeType.DocumentAdded);
    }

    private string GetDocumentId(string entityId)
    {
        if (entityId == null)
        {
            return null;
        }

        var strEntityId = (string) entityId;

        var collectionName = RavenSession.Advanced.DocumentStore.Conventions.FindCollectionName(typeof(TAggregateRoot));
        var idPrefix = RavenSession.Advanced.DocumentStore.Conventions.TransformTypeCollectionNameToDocumentIdPrefix(collectionName);

        return strEntityId.StartsWith(idPrefix) ? strEntityId : $"{idPrefix}/{strEntityId}";
    }

    public override Task Delete(TAggregateRoot aggregateRoot, CancellationToken token = default)
    {
        RavenSession.Delete(aggregateRoot);
        return Task.CompletedTask;
    }
}