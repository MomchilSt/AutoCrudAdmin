namespace AutoCrudAdmin.Helpers.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoCrudAdmin.Extensions;
    using AutoCrudAdmin.ViewModels;
    using Microsoft.EntityFrameworkCore;
    using static AutoCrudAdmin.Constants.Entity;

    public class FormControlsHelper
        : IFormControlsHelper
    {
        private static readonly ISet<Type> PrimitiveTypes = new HashSet<Type>
        {
            typeof(string),
            typeof(int),
            typeof(int?),
            typeof(short),
            typeof(short?),
            typeof(long),
            typeof(long?),
            typeof(double),
            typeof(double?),
            typeof(decimal),
            typeof(decimal?),
            typeof(bool),
            typeof(bool?),
            typeof(DateTime),
            typeof(DateTime?),
            typeof(TimeSpan),
            typeof(TimeSpan?),
        };

        private readonly DbContext dbContext;

        static FormControlsHelper()
            => Types = ReflectionHelper.DbSetProperties
                .Select(p => p.PropertyType)
                .Select(dt => dt.GetGenericArguments().FirstOrDefault())
                .ToHashSet();

        public FormControlsHelper(DbContext dbContext)
            => this.dbContext = dbContext;

        private static ISet<Type> Types { get; set; }

        public IEnumerable<FormControlViewModel> GenerateFormControls<TEntity>(
            TEntity entity,
            EntityAction entityAction,
            IDictionary<string, Expression<Func<object, bool>>> complexOptionFilters = null)
            => this.GeneratePrimaryKeyFormControls(entity, entityAction)
                .Concat(GeneratePrimitiveFormControls(entity))
                .Concat(this.GenerateComplexFormControls(entity, complexOptionFilters));

        public string GetComplexFormControlNameForEntityName(string entityName)
            => entityName + "Id";

        private IEnumerable<FormControlViewModel> GeneratePrimaryKeyFormControls<TEntity>(
            TEntity entity,
            EntityAction entityAction)
        {
            var entityType = typeof(TEntity);

            var primaryKeyValues = entityType.GetPrimaryKeyValue(entity)
                .ToList();

            if (entityAction == EntityAction.Create && primaryKeyValues.Count > 1)
            {
                return primaryKeyValues
                    .Select(pair =>
                    {
                        var name = pair.Key[..^2];
                        var property = entityType.GetProperty(name);

                        return new FormControlViewModel
                        {
                            Name = name,
                            Type = property.PropertyType,
                            Value = null,
                            Options = this.dbContext.Set(property.PropertyType),
                            IsDbSet = true,
                            IsReadOnly = false,
                        };
                    });
            }

            return primaryKeyValues
                .Select(pair => new FormControlViewModel
                {
                    Name = pair.Key == SinglePrimaryKeyName
                        ? typeof(TEntity).GetPrimaryKeyPropertyInfos()
                            .Select(pk => pk.Name)
                            .FirstOrDefault()
                        : pair.Key,
                    Type = pair.Value.GetType(),
                    Value = pair.Key == SinglePrimaryKeyName
                        ? ExpressionsBuilder.ForGetPropertyValue<TEntity>(
                            typeof(TEntity).GetPrimaryKeyPropertyInfos()
                                .FirstOrDefault())(
                            entity)
                        : ExpressionsBuilder.ForGetPropertyValue<TEntity>(
                            typeof(TEntity).GetProperty(pair.Key))(entity),
                    IsReadOnly = true,
                });
        }

        private static IEnumerable<FormControlViewModel> GeneratePrimitiveFormControls<TEntity>(TEntity entity)
        {
            var entityType = typeof(TEntity);

            return entityType.GetProperties()
                .Where(p => !p.GetCustomAttributes<NotMappedAttribute>().Any())
                .Where(property => IsPrimitiveProperty(property, entityType)
                                   && !IsComplexPrimaryKey(property, entityType))
                .OrderBy(p => p.MetadataToken)
                .Select(property => new FormControlViewModel
                {
                    Name = property.Name,
                    Type = property.PropertyType,
                    Value = ExpressionsBuilder.ForGetPropertyValue<TEntity>(property)(entity),
                });
        }

        private static bool IsDbContextEntity(PropertyInfo property)
            => Types.Contains(property.PropertyType);

        private static bool IsPrimitiveProperty(PropertyInfo property, Type entityType)
            => entityType
                   .GetPrimaryKeyPropertyInfos()
                   .Any(pk => pk == property)
               || property.PropertyType.IsEnum
               || (PrimitiveTypes.Contains(property.PropertyType) && !property.Name.ToLower().EndsWith("id"));

        private static bool IsComplexPrimaryKey(PropertyInfo property, Type entityType)
            => entityType
                .GetPrimaryKeyPropertyInfos()
                .Any(p => p == property);

        private bool IsPartOfPrimaryKey(PropertyInfo property, Type entityType)
            => entityType.GetPrimaryKeyPropertyInfos()
                .Any(pk => pk.Name == this.GetComplexFormControlNameForEntityName(property.Name));

        private IEnumerable<FormControlViewModel> GenerateComplexFormControls<TEntity>(
            TEntity entity,
            IDictionary<string, Expression<Func<object, bool>>> optionFilters = null)
        {
            var entityType = entity.GetType();

            return entityType.GetProperties()
                .Where(property => IsDbContextEntity(property) && !this.IsPartOfPrimaryKey(property, entityType))
                .Select(property =>
                {
                    var filter = optionFilters?.ContainsKey(property.Name) ?? false
                        ? optionFilters[property.Name]
                        : null;
                    return this.GenerateFormControlForComplexProperty(entity, property, filter);
                })
                .OrderBy(x => x.Name)
                .ToList();
        }

        private FormControlViewModel GenerateFormControlForComplexProperty<TEntity>(
            TEntity entity,
            PropertyInfo property,
            Expression<Func<object, bool>> optionsFilter = null)
        {
            var entityType = entity.GetType();

            var options = optionsFilter != null
                ? this.dbContext.Set(property.PropertyType).Where(optionsFilter)
                : this.dbContext.Set(property.PropertyType);

            var valueFunc = ExpressionsBuilder.ForGetPropertyValue<TEntity>(
                entityType.GetProperty(this.GetComplexFormControlNameForEntityName(property.Name)));

            return new FormControlViewModel
            {
                Name = property.Name,
                Type = property.PropertyType,
                Value = valueFunc(entity),
                Options = options,
                IsDbSet = true,
                IsReadOnly = false,
            };
        }
    }
}