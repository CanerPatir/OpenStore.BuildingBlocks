using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace OpenStore.Infrastructure.Data.NoSql.MongoDb
{
    public class MongoUnitOfWork : IMongoUnitOfWork
    {
        private readonly bool _transactionSupported;

        public MongoUnitOfWork(IMongoDatabase databaseBase)
        {
            DatabaseBase = databaseBase;
            Session = databaseBase.Client.StartSession();
            try
            {
                Session.StartTransaction();
                _transactionSupported = true;
            }
            catch (NotSupportedException e)
            {
            }
        }

        public IMongoDatabase DatabaseBase { get; }

        public IClientSessionHandle Session { get; }

        public async Task SaveChangesAsync(CancellationToken token = default)
        {
            if (!_transactionSupported) return;
            await Session.CommitTransactionAsync(token);
        }

        public Task BeginTransactionAsync(CancellationToken token = default)
        {
            // Session.StartTransaction();
            return Task.CompletedTask;
        }
    }
}