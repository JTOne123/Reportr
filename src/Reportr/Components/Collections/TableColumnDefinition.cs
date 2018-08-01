﻿namespace Reportr.Components.Collections
{
    using Reportr.Data;
    using Reportr.Data.Querying.Functions;
    using System;

    /// <summary>
    /// Represents a single report table column definition
    /// </summary>
    public class TableColumnDefinition
    {
        /// <summary>
        /// Constructs the column definition with the details
        /// </summary>
        /// <param name="name">The column name</param>
        /// <param name="binding">The data binding</param>
        /// <param name="totalAggregator">The total aggregator</param>
        public TableColumnDefinition
            (
                string name,
                DataBinding binding,
                IAggregateFunction totalAggregator = null
            )
        {
            Validate.IsNotEmpty(name);
            Validate.IsNotNull(binding);

            this.ColumnId = Guid.NewGuid();
            this.Name = name;
            this.Title = name;
            this.Binding = binding;

            if (totalAggregator != null)
            {
                this.TotalAggregator = totalAggregator;
                this.HasTotal = true;
            }
        }

        /// <summary>
        /// Gets the column ID
        /// </summary>
        public Guid ColumnId { get; protected set; }

        /// <summary>
        /// Gets the column name
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the columns data binding
        /// </summary>
        public DataBinding Binding { get; protected set; }

        /// <summary>
        /// Gets the total aggregator function
        /// </summary>
        public IAggregateFunction TotalAggregator { get; protected set; }

        /// <summary>
        /// Gets a flag indicating if the column has a total aggregator
        /// </summary>
        public bool HasTotal { get; protected set; }

        /// <summary>
        /// Gets the value formatting type override
        /// </summary>
        public DataValueFormattingType? FormattingTypeOverride
        {
            get;
            protected internal set;
        }

        /// <summary>
        /// Adds style details to the column
        /// </summary>
        /// <param name="title">The title</param>
        /// <param name="alignment">The text alignment</param>
        /// <param name="importance">The importance</param>
        /// <returns>The updated column</returns>
        public TableColumnDefinition WithStyle
            (
                string title,
                ColumnAlignment alignment,
                ColumnImportance importance = default(ColumnImportance)
            )
        {
            this.Title = title;
            this.Alignment = alignment;
            this.Importance = importance;

            return this;
        }

        /// <summary>
        /// Gets the column title
        /// </summary>
        public string Title { get; protected set; }

        /// <summary>
        /// Gets the column alignment
        /// </summary>
        public ColumnAlignment Alignment { get; protected set; }

        /// <summary>
        /// Gets the importance of the column
        /// </summary>
        public ColumnImportance Importance { get; private set; }

        /// <summary>
        /// Adds the cell action to the column
        /// </summary>
        /// <param name="action">The report action</param>
        /// <returns>The updated column</returns>
        public TableColumnDefinition WithAction
            (
                ReportActionDefinition action
            )
        {
            Validate.IsNotNull(action);

            this.CellAction = action;

            return this;
        }

        /// <summary>
        /// Gets the action associated with each cell in the column
        /// </summary>
        /// <remarks>
        /// The cell action can be used to make an entire table cell
        /// click-able instead of just text.
        /// 
        /// The cell action can be null and only works when the row
        /// action has not been set.
        /// </remarks>
        public ReportActionDefinition CellAction { get; protected set; }

        /// <summary>
        /// Provides a custom string description
        /// </summary>
        /// <returns>The column name</returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
