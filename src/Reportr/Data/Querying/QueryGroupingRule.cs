﻿namespace Reportr.Data.Querying
{
    using Reportr.Data;

    /// <summary>
    /// Represents a single query grouping rule
    /// </summary>
    public sealed class QueryGroupingRule
    {
        /// <summary>
        /// Constructs the grouping rule with the details
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <param name="direction">The sorting direction</param>
        public QueryGroupingRule
            (
                string columnName,
                SortDirection direction
            )
        {
            Validate.IsNotEmpty(columnName);

            this.ColumnName = columnName;
            this.Direction = direction;
        }

        /// <summary>
        /// Gets the column name
        /// </summary>
        public string ColumnName { get; private set; }

        /// <summary>
        /// Gets the sorting direction
        /// </summary>
        public SortDirection Direction { get; private set; }
    }
}
