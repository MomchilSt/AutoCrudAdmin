﻿namespace AutoCrudAdmin.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoCrudAdmin.Attributes;
    using AutoCrudAdmin.Extensions;
    using AutoCrudAdmin.Helpers;
    using AutoCrudAdmin.ViewModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NonFactors.Mvc.Grid;

    [AutoCrudAdminControllerNameConvention]
    public class AutoCrudAdminController<TEntity>
        : Controller
        where TEntity : class
    {
        private static readonly Type EntityType = typeof(TEntity);

        private IQueryable<TEntity> set;
        private DbContext db;
        private IFormControlsHelper formControlsHelper;

        private IQueryable<TEntity> Set
            => this.set ??= this.DbContext
                ?.Set<TEntity>();

        protected virtual IEnumerable<string> ShownColumnNames
            => Enumerable.Empty<string>();

        protected virtual IEnumerable<string> HiddenColumnNames
            => Enumerable.Empty<string>();

        protected virtual IEnumerable<Func<TEntity, ValidatorResult>> EntityValidators
            => Array.Empty<Func<TEntity, ValidatorResult>>();

        protected virtual IEnumerable<GridAction> CustomActions
            => Enumerable.Empty<GridAction>();

        protected virtual int RowsPerPage
            => this.PageSizes.Any()
                ? this.PageSizes.First().Item1
                : 20;

        protected virtual bool ShowPageSizes
            => this.PageSizes.Any();

        protected virtual IEnumerable<Tuple<int, string>> PageSizes
            => Enumerable.Empty<Tuple<int, string>>();

        private static MethodInfo GenerateColumnExpressionMethod =>
            typeof(AutoCrudAdminController<TEntity>)
                .GetMethod(
                    nameof(GenerateColumnConfiguration),
                    BindingFlags.NonPublic | BindingFlags.Static);

        private static IEnumerable<GridAction> DefaultActions
            => new[] { new GridAction { Action = nameof(Edit) }, new GridAction { Action = nameof(Delete) }, };

        private IEnumerable<GridAction> Actions
            => DefaultActions.Concat(this.CustomActions);

        private DbContext DbContext
            => this.db ??= this.HttpContext
                .RequestServices
                .GetService<DbContext>();

        private IFormControlsHelper FormControlsHelper
            => this.formControlsHelper ??= this.HttpContext
                .RequestServices
                .GetService<IFormControlsHelper>();

        protected virtual IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> set) => set;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            this.ViewBag.LayoutName = context.HttpContext.Items["layout_name"];
            this.ViewBag.ApplicationName = context.HttpContext.Items["application_name"];
            base.OnActionExecuting(context);
        }

        [HttpGet]
        public virtual IActionResult Index()
        {
            this.PrepareErrorMessageViewData();

            return this.View(
                "../AutoCrudAdmin/Index",
                new AutoCrudAdminIndexViewModel { GenerateGrid = this.GenerateGrid, });
        }

        [HttpGet]
        public virtual IActionResult Create()
            => this.GetEntityForm(
                ExpressionsBuilder.ForCreateInstance<TEntity>()(),
                EntityAction.Create);

        [HttpGet]
        public virtual IActionResult Edit([FromQuery] IDictionary<string, string> complexId)
            => this.GetEntityForm(
                this.Set
                    .FirstOrDefault(ExpressionsBuilder.ForByEntityPrimaryKey<TEntity>(complexId)),
                EntityAction.Edit,
                complexId);

        [HttpGet]
        public virtual IActionResult Delete([FromQuery] IDictionary<string, string> complexId)
            => this.GetEntityForm(
                this.Set
                    .FirstOrDefault(ExpressionsBuilder.ForByEntityPrimaryKey<TEntity>(complexId)),
                EntityAction.Delete,
                complexId);

        [HttpPost]
        public virtual IActionResult PostCreate(IDictionary<string, string> entityDict)
            => this.PostEntityForm(entityDict, EntityAction.Create);

        [HttpPost]
        public virtual IActionResult PostEdit(IDictionary<string, string> entityDict)
            => this.PostEntityForm(entityDict, EntityAction.Edit);

        [HttpPost]
        public virtual IActionResult PostDelete(IDictionary<string, string> entityDict)
            => this.PostEntityForm(entityDict, EntityAction.Delete);

        protected virtual IActionResult GetEntityForm(
            TEntity entity,
            EntityAction action,
            IDictionary<string, string> complexId = null)
        {
            var formControls = this.GenerateFormControls(entity, action).ToList();

            if (action == EntityAction.Delete)
            {
                formControls.ForEach(fc => fc.IsReadOnly = true);
            }

            return this.View(
                "../AutoCrudAdmin/EntityForm",
                new AutoCrudAdminEntityFormViewModel { FormControls = formControls, Action = action, });
        }

        protected virtual IEnumerable<FormControlViewModel> GenerateFormControls(TEntity entity, EntityAction action)
            => this.FormControlsHelper.GenerateFormControls(entity, action);

        protected virtual IActionResult PostEntityForm(IDictionary<string, string> entityDict, EntityAction action)
        {
            var entity = this.CreateEntityFromFormData(entityDict);
            this.ValidateBeforeSave(entity);
            this.BeforeEntitySave(entity, entityDict);
            this.DbContext.Entry(entity).State = action switch
            {
                EntityAction.Create => EntityState.Added,
                EntityAction.Edit => EntityState.Modified,
                var _ => EntityState.Deleted
            };

            this.DbContext.SaveChanges();
            this.AfterEntitySave(entity, entityDict);

            return this.RedirectToAction("Index");
        }

        protected virtual void AfterEntitySave(TEntity entity, IDictionary<string, string> entityDict)
        {
        }

        protected virtual void BeforeEntitySave(TEntity entity, IDictionary<string, string> entityDict)
        {
        }

        protected virtual IHtmlGrid<TEntity> GenerateGrid(IHtmlHelper<AutoCrudAdminIndexViewModel> htmlHelper)
        {
            var types = ReflectionHelper.DbSetProperties
                .Select(p => p.PropertyType)
                .Select(t => t.GetGenericArguments())
                .Select(a => a.FirstOrDefault())
                .ToHashSet();

            var names = this.Set.GetType()
                .GetGenericArguments()
                .FirstOrDefault()
                ?.GetProperties()
                .Where(prop => types.Contains(prop.PropertyType))
                .Select(prop => prop.Name);

            var setForGrid = names
                !.Aggregate(this.Set, (current, name) => current.Include(name));

            setForGrid = this.ApplyIncludes(setForGrid);

            return htmlHelper
                .Grid(setForGrid)
                .Build(columns =>
                {
                    this.BuildGridColumns(columns);
                    this.BuildGridActions(columns, htmlHelper);
                })
                .Using(GridFilterMode.Header)
                .Empty("No data found")
                .Pageable(pager =>
                {
                    pager.PageSizes = this.PageSizes.ToDictionary(
                        x => x.Item1,
                        y => y.Item2);
                    pager.ShowPageSizes = this.ShowPageSizes;
                    pager.RowsPerPage = this.RowsPerPage;
                });
        }

        protected virtual IGridColumnsOf<TEntity> BuildGridColumns(IGridColumnsOf<TEntity> columns)
        {
            if (this.ShownColumnNames.Any() && this.HiddenColumnNames.Any())
            {
                throw new Exception("Both shown and hidden column names are declared. Leave only one of them");
            }

            Func<PropertyInfo, bool> filter = this.ShownColumnNames.Any()
                ? x => this.ShownColumnNames.Contains(x.Name)
                : this.HiddenColumnNames.Any()
                    ? x => !this.HiddenColumnNames.Contains(x.Name)
                    : x => !typeof(IEnumerable).IsAssignableFrom(x.PropertyType);

            var primaryKeys = EntityType.GetPrimaryKeyPropertyInfos();

            var properties = EntityType
                .GetProperties()
                .Where(filter)
                .OrderBy(property => property != primaryKeys.FirstOrDefault());

            properties = primaryKeys.Skip(1)
                .Aggregate(
                    properties,
                    (current, pk)
                        => current.ThenBy(property => property != pk))
                .ThenBy(property => property.Name);

            return properties
                .Aggregate(
                    columns,
                    (currentColumns, prop) => (IGridColumnsOf<TEntity>)GenerateColumnExpressionMethod
                        .MakeGenericMethod(prop.PropertyType)
                        .Invoke(null, new object[] { currentColumns, prop }));
        }

        protected virtual IGridColumnsOf<TEntity> BuildGridActions(
            IGridColumnsOf<TEntity> columns,
            IHtmlHelper htmlHelper)
        {
            this.Actions
                .ToList()
                .ForEach(action =>
                {
                    columns.Add(model => htmlHelper.ActionLink(
                            action.Name,
                            action.Action,
                            this.RouteData.Values["controller"].ToString(),
                            RouteValueDictionary.FromArray(
                                EntityType.GetPrimaryKeyValue(model).ToArray()),
                            new { }))
                        .Titled("Action");
                });

            return columns;
        }

        private static IGridColumnsOf<TEntity> GenerateColumnConfiguration<TProperty>(
            IGridColumnsOf<TEntity> columns,
            MemberInfo property)
        {
            var lambda = ExpressionsBuilder.ForGetProperty<TEntity, TProperty>(property);
            columns
                .Add(lambda)
                .Titled(property.Name)
                .Filterable(true)
                .Sortable(true);

            return columns;
        }

        private void ValidateBeforeSave(TEntity entity)
        {
            var errors = this.EntityValidators
                .Select(v => v(entity))
                .Where(x => !x.IsValid)
                .Select(x => x.Message)
                .ToList();

            if (errors.Any())
            {
                throw new Exception(string.Join(", ", errors));
            }
        }

        private TEntity CreateEntityFromFormData(IDictionary<string, string> entityDict)
        {
            var entity = Activator.CreateInstance<TEntity>();
            EntityType.GetProperties()
                .Where(prop => prop.CanWrite)
                .ToList()
                .ForEach(prop =>
                {
                    if (!entityDict.ContainsKey(prop.Name))
                    {
                        return;
                    }

                    Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    var strValue = entityDict[prop.Name];
                    object safeValue;

                    if (propType.IsEnum)
                    {
                        safeValue = Enum.Parse(propType, strValue);
                    }
                    else
                    {
                        safeValue = (strValue == null) ? null : Convert.ChangeType(strValue, propType);
                    }

                    prop.SetValue(entity, safeValue);
                });

            return entity;
        }
    }
}
