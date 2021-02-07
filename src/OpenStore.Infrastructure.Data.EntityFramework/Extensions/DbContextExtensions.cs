using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OpenStore.Domain;

namespace OpenStore.Infrastructure.Data.EntityFramework.Extensions
{
    public static class DbContextExtensions
    {
        //see https://blogs.msdn.microsoft.com/dotnet/2016/09/29/implementing-seeding-custom-conventions-and-interceptors-in-ef-core-1-0/
        //for why I call DetectChanges before ChangeTracker, and why I then turn ChangeTracker.AutoDetectChangesEnabled off/on around SaveChanges

        // /// <summary>
        // /// This SaveChangesAsync, with a boolean to decide whether to validate or not
        // /// </summary>
        // /// <param name="context"></param>
        // /// <param name="shouldValidate"></param>
        // /// <param name="config"></param>
        // /// <param name="token"></param>
        // /// <returns></returns>
        // public static async Task<IStatusGeneric> SaveChangesWithOptionalValidationAsync(this DbContext context, bool shouldValidate, IGenericServicesConfig config,
        //     CancellationToken token = default)
        // {
        //     if (shouldValidate)
        //     {
        //         return await context.SaveChangesWithValidationAsync(config, token);
        //     }
        //     else
        //     {
        //         return await context.SaveChangesWithExtrasAsync(config, false, token);
        //     }
        // }

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
            
            context.ApplyAuditableEntities();
            
            // var changedEntityNames = context.GetChangedEntityNames();
            var result = await context.SaveChangesWithExtrasAsync(config, true, token);
            // context.InvalidateCache(changedEntityNames);
            // context.ClearCache();
            return result;
        }

        // public static void InvalidateCache(this ApplicationDbContext context, string[] changedEntityNames) => context.GetService<IEFCacheServiceProvider>().InvalidateCacheDependencies(changedEntityNames);
        // public static void ClearCache(this DbContext context)
        // {
        //     // context.GetService<IEFCacheServiceProvider>().ClearAllCachedEntries();
        // }

        private static async Task<IStatusGeneric> SaveChangesWithExtrasAsync(this DbContext context, IGenericServicesConfig config, bool turnOffChangeTracker, CancellationToken token)
        {
            var status = config?.BeforeSaveChanges != null ? config.BeforeSaveChanges(context) : new StatusGenericHandler();
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

        private static void ApplyAuditableEntities(this DbContext context)
        {
            // context.ChangeTracker.DetectChanges();
            var auditableEntityEntries = context.ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified)
                .Where(x => x.Entity is IAuditableEntity)
                .ToList();

            string userInfo = null;
            var httpContextAccessor =  context.GetService<IHttpContextAccessor>();
            if (httpContextAccessor != null)
            {
               userInfo = httpContextAccessor
                   .HttpContext?
                   .User?.Claims
                   .FirstOrDefault(x => x.Type == "email" || x.Type == ClaimTypes.Email)?
                   .Value;
            }

            foreach (var item in auditableEntityEntries)
            {
                if (item.State == EntityState.Added)
                {
                    item.CurrentValues[nameof(IAuditableEntity.CreatedAt)] = DateTime.UtcNow;
                    item.CurrentValues[nameof(IAuditableEntity.CreatedBy)] = userInfo;
                }
                
                if (item.State == EntityState.Modified)
                {
                    item.CurrentValues[nameof(IAuditableEntity.UpdatedAt)] = DateTime.UtcNow;
                    item.CurrentValues[nameof(IAuditableEntity.UpdatedBy)] = userInfo;
                }
            }
        }

        private static IStatusGeneric ExecuteValidation(this DbContext context)
        {
            var status = new StatusGenericHandler();
            var entriesToCheck = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList(); //This is needed, otherwise you get a "collection has changed" exception
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