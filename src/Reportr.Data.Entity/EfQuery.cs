﻿namespace Reportr.Data.Entity
{
    using Reportr.Data.Querying;
    using Reportr.Filtering;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a base class for generic Entity Framework queries
    /// </summary>
    /// <typeparam name="T">The query output type</typeparam>
    public abstract class EfQuery<T> : QueryBase
    {
        private QueryColumnInfo[] _columns;

        /// <summary>
        /// Constructs the query with an Entity Framework data source
        /// </summary>
        /// <param name="dataSource">The EF data source</param>
        public EfQuery
            (
                EfDataSource dataSource
            )

            : base(dataSource)
        {
            ResolveColumns();
        }

        /// <summary>
        /// Resolves the queries columns from the output type
        /// </summary>
        private void ResolveColumns()
        {
            var entityType = typeof(T);
            var dataSource = this.DataSource;

            var tableSchema = dataSource.Schema.FirstOrDefault
            (
                dts => dts.Name == entityType.Name
            );

            if (tableSchema == null)
            {
                var message = "The type {0} is not valid for the data source {1}.";

                throw new InvalidOperationException
                (
                    String.Format
                    (
                        message,
                        entityType.Name,
                        dataSource.Name
                    )
                );
            }

            var properties = entityType.GetProperties
            (
                BindingFlags.Public | BindingFlags.GetProperty
            );

            var columnInfos = new List<QueryColumnInfo>();

            foreach (var property in properties)
            {
                columnInfos.Add
                (
                    new QueryColumnInfo
                    (
                        tableSchema,
                        new DataColumnSchema
                        (
                            property.Name,
                            property.PropertyType
                        )
                    )
                );
            }

            _columns = columnInfos.ToArray();
        }

        /// <summary>
        /// Gets an array of the columns generated by the query
        /// </summary>
        public override QueryColumnInfo[] Columns
        {
            get
            {
                return _columns;
            }
        }

        /// <summary>
        /// Gets the database context from the data source
        /// </summary>
        /// <returns>The database context</returns>
        protected DbContext GetContext()
        {
            return ((EfDataSource)this.DataSource).Context;
        }

        /// <summary>
        /// Generates a queryable for the database context specified
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="parameterValues">The parameter values</param>
        /// <returns>The query generated</returns>
        protected abstract IQueryable<T> GenerateQuerable
        (
            DbContext context,
            params ParameterValue[] parameterValues
        );

        /// <summary>
        /// Asynchronously fetches the query data using the parameter values
        /// </summary>
        /// <param name="parameterValues">The parameter values</param>
        /// <returns>The query data in the form of an array of rows</returns>
        protected override async Task<IEnumerable<QueryRow>> FetchDataAsync
            (
                params ParameterValue[] parameterValues
            )
        {
            Validate.IsNotNull(parameterValues);

            var context = GetContext();

            var queryable = GenerateQuerable
            (
                context,
                parameterValues
            );

            var queryResults = await queryable.ToListAsync();
            var rows = new List<QueryRow>();
            var entityType = typeof(T);

            foreach (var item in queryResults)
            {
                var cells = new List<QueryCell>();

                foreach (var info in _columns)
                {
                    var property = entityType.GetProperty
                    (
                        info.Column.Name
                    );

                    var propertyValue = property.GetValue
                    (
                        item
                    );

                    cells.Add
                    (
                        new QueryCell
                        (
                            info.Column,
                            propertyValue
                        )
                    );
                }

                rows.Add
                (
                    new QueryRow
                    (
                        cells.ToArray()
                    )
                );
            }

            return rows;
        }
    }
}
