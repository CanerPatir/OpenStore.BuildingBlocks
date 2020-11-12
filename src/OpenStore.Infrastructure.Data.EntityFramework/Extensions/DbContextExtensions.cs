using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OpenStore.Infrastructure.Data.EntityFramework.Extensions
{
    public static class DbContextExtensions
    {
        //see https://blogs.msdn.microsoft.com/dotnet/2016/09/29/implementing-seeding-custom-conventions-and-interceptors-in-ef-core-1-0/
        //for why I call DetectChanges before ChangeTracker, and why I then turn ChangeTracker.AutoDetectChangesEnabled off/on around SaveChanges

        /// <summary>
        /// This SaveChangesAsync, with a boolean to decide whether to validate or not
        /// </summary>
        /// <param name="context"></param>
        /// <param name="shouldValidate"></param>
        /// <param name="config"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<IStatusGeneric> SaveChangesWithOptionalValidationAsync(this DbContext context, bool shouldValidate, IGenericServicesConfig config, CancellationToken token = default)
        {
            if (shouldValidate)
            {
                return await context.SaveChangesWithValidationAsync(config, token: token);
            }
            else
            {
                return await context.SaveChangesWithExtrasAsync(config, token: token);
            }
        }

        /// <summary>
        /// This will validate any entity classes that will be added or updated
        /// If the validation does not produce any errors then SaveChangesAsync will be called 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="config"></param>
        /// <param name="token"></param>
        /// <returns>List of errors, empty if there were no errors</returns>
        public static async Task<IStatusGeneric> SaveChangesWithValidationAsync(this DbContext context, IGenericServicesConfig config = null, CancellationToken token = default)
        {
            var status = context.ExecuteValidation();
            if (!status.IsValid) return status;

            // var changedEntityNames = context.GetChangedEntityNames();
            var result = await context.SaveChangesWithExtrasAsync(config, true, token: token);
            // context.InvalidateCache(changedEntityNames);
            // context.ClearCache();
            return result;
        }

        // public static void InvalidateCache(this ApplicationDbContext context, string[] changedEntityNames) => context.GetService<IEFCacheServiceProvider>().InvalidateCacheDependencies(changedEntityNames);
        // public static void ClearCache(this DbContext context)
        // {
        //     // context.GetService<IEFCacheServiceProvider>().ClearAllCachedEntries();
        // }

        private static async Task<IStatusGeneric> SaveChangesWithExtrasAsync(this DbContext context, IGenericServicesConfig config, bool turnOffChangeTracker = false, CancellationToken token = default)
        {
            var status = config?.BeforeSaveChanges != null
                ? config.BeforeSaveChanges(context)
                : new StatusGenericHandler();
            if (!status.IsValid)
                return status;

            if (turnOffChangeTracker)
                context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                await context.SaveChangesAsync(token);
            }
            catch (Exception e)
            {
                var exStatus = config?.SaveChangesExceptionHandler(e, context);
                if (exStatus == null) throw; //error wasn't handled, so rethrow
                status.CombineStatuses(exStatus);
            }
            finally
            {
                context.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            return status;
        }

        private static IStatusGeneric ExecuteValidation(this DbContext context)
        {
            var status = new StatusGenericHandler();
            var entriesToCheck = context.ChangeTracker.Entries()
                .Where(e =>
                    (e.State == EntityState.Added) ||
                    (e.State == EntityState.Modified)).ToList(); //This is needed, otherwise you get a "collection has changed" exception
            foreach (var entity in entriesToCheck.Select(entry => entry.Entity))
            {
                status.Header = entity.GetType().GetNameForClass();
                var valProvider = new ValidationDbContextServiceProvider(context);
                var valContext = new ValidationContext(entity, valProvider, null);
                var entityErrors = new List<ValidationResult>();
                if (!Validator.TryValidateObject(entity, valContext, entityErrors, true))
                {
                    status.AddValidationResults(entityErrors);
                }
            }

            status.Header = null; //reset the header, as could cause incorrect error message
            return status;
        }
    }
}