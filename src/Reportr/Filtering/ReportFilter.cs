﻿namespace Reportr.Filtering
{
    using Newtonsoft.Json;
    using Reportr.Data;
    using Reportr.Data.Querying;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a single report filter
    /// </summary>
    /// <remarks>
    /// A report filter is used to filter the data in one or more
    /// sections within the report. This includes sorting columns
    /// within queries that have been defined in sections.
    /// </remarks>
    public class ReportFilter
    {
        private List<ReportParameterDefinition> _parameterDefinitions;
        private readonly List<ReportFilterParameterValue> _parameterValues;
        private readonly List<ReportFilterSortingRule> _sortingRules;

        /// <summary>
        /// Constructs the report filter with parameter definitions
        /// </summary>
        /// <param name="definitions">The parameter definitions</param>
        public ReportFilter
            (
                params ReportParameterDefinition[] definitions
            )
        {
            _parameterDefinitions = new List<ReportParameterDefinition>();
            _parameterValues = new List<ReportFilterParameterValue>();
            _sortingRules = new List<ReportFilterSortingRule>();

            if (definitions != null && definitions.Any())
            {
                _parameterDefinitions.AddRange
                (
                    definitions
                );

                foreach (var definition in definitions)
                {
                    var parameter = definition.Parameter;

                    SetParameterValue
                    (
                        parameter.Name,
                        parameter.DefaultValue
                    );
                }
            }
        }

        /// <summary>
        /// Gets the parameter definitions for the report
        /// </summary>
        [JsonIgnore]
        public IEnumerable<ReportParameterDefinition> ParameterDefinitions
        {
            get
            {
                return _parameterDefinitions;
            }
        }

        /// <summary>
        /// Gets report parameter definitions for a target type
        /// </summary>
        /// <param name="targetType">The target types</param>
        /// <returns>A collection of parameter definitions</returns>
        public IEnumerable<ReportParameterDefinition> GetDefinitions
            (
                ReportParameterTargetType targetType
            )
        {
            return _parameterDefinitions.Where
            (
                p => p.TargetType == targetType
            );
        }

        /// <summary>
        /// Determines if a parameter has been defined
        /// </summary>
        /// <param name="parameterName">The parameter name</param>
        /// <returns>True, if the parameter has been defined; otherwise false</returns>
        public bool IsDefined
            (
                string parameterName
            )
        {
            Validate.IsNotEmpty(parameterName);

            return _parameterDefinitions.Any
            (
                d => d.Parameter.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase)
            );
        }

        /// <summary>
        /// Gets a single report parameter definition
        /// </summary>
        /// <param name="parameterName">The parameter name</param>
        /// <returns>The parameter definition</returns>
        public ReportParameterDefinition GetDefinition
            (
                string parameterName
            )
        {
            Validate.IsNotEmpty(parameterName);

            var definition = _parameterDefinitions.FirstOrDefault
            (
                d => d.Parameter.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase)
            );

            if (definition == null)
            {
                throw new KeyNotFoundException
                (
                    $"The name '{parameterName}' did not match any parameters."
                );
            }

            return definition;
        }

        /// <summary>
        /// Gets a collection of parameter values for the filter
        /// </summary>
        public IEnumerable<ReportFilterParameterValue> ParameterValues
        {
            get
            {
                return _parameterValues;
            }
        }

        /// <summary>
        /// Gets the parameter values as a dictionary
        /// </summary>
        /// <returns>A dictionary with the parameter names and values</returns>
        private IDictionary<string, object> GetParameterValuesAsDictionary()
        {
            return this.ParameterValues.ToDictionary
            (
                parameter => parameter.Name,
                parameter => parameter.Value
            );
        }

        /// <summary>
        /// Sets a series of report filter parameter values
        /// </summary>
        /// <param name="parameterValues">The parameter values</param>
        /// <param name="parameterConstraints">The parameter constraints</param>
        public void SetParameterValues
            (
                IDictionary<string, object> parameterValues,
                IDictionary<string, object> parameterConstraints = null
            )
        {
            Validate.IsNotNull(parameterValues);

            foreach (var valueEntry in parameterValues)
            {
                var lookupParameterValues = CompileLookupParameterValues
                (
                    valueEntry.Key,
                    parameterValues,
                    parameterConstraints
                );

                var matchingConstraints = GetMatchingConstraints
                (
                    valueEntry.Key
                );

                if (matchingConstraints.Any())
                {
                SetParameterValue
                (
                        valueEntry.Key,
                        valueEntry.Value,
                        true,
                        matchingConstraints.First().Value,
                    lookupParameterValues
                );
            }
                else
                {
                    SetParameterValue
                    (
                        valueEntry.Key,
                        valueEntry.Value,
                        false,
                        null,
                        lookupParameterValues
                    );
                }
            }

            IEnumerable<KeyValuePair<string, object>> GetMatchingConstraints
                (
                    string parameterName
                )
            {
                if (parameterConstraints == null)
                {
                    return new List<KeyValuePair<string, object>>();
                }
                else
                {
                    return parameterConstraints.Where
                    (
                        c => c.Key.Equals
                        (
                            parameterName,
                            StringComparison.InvariantCultureIgnoreCase
                        )
                    );
                }
            }
        }
        
        /// <summary>
        /// Sets a single report filter parameter value
        /// </summary>
        /// <param name="parameterName">The parameter name</param>
        /// <param name="valueToSet">The value to set</param>
        /// <param name="isConstrained">True, if the parameter is constrained</param>
        /// <param name="constraintValue">The constrained value</param>
        /// <param name="lookupParameterValues">The lookup parameter values</param>
        private void SetParameterValue
            (
                string parameterName,
                object valueToSet,
                bool isConstrained = false,
                object constraintValue = null,
                params ParameterValue[] lookupParameterValues
            )
        {
            Validate.IsNotEmpty(parameterName);

            var parameterValue = _parameterValues.FirstOrDefault
            (
                p => p.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase)
            );

            if (parameterValue != null)
            {
                if (valueToSet == null)
                {
                    valueToSet = parameterValue.Parameter.DefaultValue;
                }

                parameterValue.SetValue
                (
                    valueToSet,
                    false,
                    lookupParameterValues
                );
                }
            else
            {
                var definition = GetDefinition
                (
                    parameterName
                );

                if (valueToSet == null)
                {
                    valueToSet = definition.Parameter.DefaultValue;
                }

                parameterValue = new ReportFilterParameterValue
                (
                    definition,
                    valueToSet,
                    lookupParameterValues
                );

                _parameterValues.Add
                (
                    parameterValue
                );
            }

            if (isConstrained)
            {
                var definition = parameterValue.Definition;

                if (definition.Parameter.HasLookup)
                {
                    parameterValue.AutoFilterLookupItemsForConstraint
                    (
                        constraintValue
                    );

                    var valueAllowed = false;

                    if (valueToSet != null && valueToSet.GetType().IsArray)
                    {
                        foreach (var item in valueToSet as Array)
                        {
                            valueAllowed = parameterValue.HasLookupItem
                            (
                                item
                            );

                            if (false == valueAllowed)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        valueAllowed = parameterValue.HasLookupItem
                        (
                            valueToSet
                        );
                    }

                    if (false == valueAllowed || valueToSet.IsNullOrEmpty())
                    {
                        parameterValue.SetValue
                        (
                            constraintValue,
                            true,
                            lookupParameterValues
                        );
                    }

                    // NOTE:
                    // Auto hide the parameter if one or less lookup items are available
                    if (parameterValue.LookupItems.Length <= 1)
                    {
                        definition.Hide();
                    }
                }
                else
                {
                    parameterValue.SetValue
                    (
                        constraintValue,
                        true,
                        lookupParameterValues
                    );

                    definition.Hide();
                }
            }
        }

        /// <summary>
        /// Compiles the lookup parameter values for a single parameters lookup
        /// </summary>
        /// <param name="parameterName">The parameter name</param>
        /// <param name="allParameterValues">All parameter values supplied</param>
        /// <param name="parameterConstraints">The parameter constraints</param>
        /// <returns>An array of lookup parameter values</returns>
        private ParameterValue[] CompileLookupParameterValues
            (
                string parameterName,
                IDictionary<string, object> allParameterValues,
                IDictionary<string, object> parameterConstraints = null
            )
        {
            var definition = GetDefinition
            (
                parameterName
            );

            var parameter = definition.Parameter;

            if (parameter.HasLookup
                && parameter.LookupSourceType == ParameterLookupSourceType.Query
                && parameter.LookupFilterParameters.Any())
            {
                var lookupValues = new List<ParameterValue>();

                foreach (var lookupInfo in parameter.LookupFilterParameters)
                {
                    var matchFound = HasMatchingValue(allParameterValues);
                    var matchIsConstraint = false;

                    if (false == matchFound && parameterConstraints != null)
                    {
                        matchFound = HasMatchingValue(parameterConstraints);

                        if (matchFound)
                        {
                            matchIsConstraint = true;
                        }
                    }

                    bool HasMatchingValue
                        (
                            IDictionary<string, object> parameterValues
                        )
                    {
                        return parameterValues.Any
                        (
                            pair => pair.Key.Equals(lookupInfo.Name, StringComparison.OrdinalIgnoreCase)
                                && pair.Value != null
                        );
                    }

                    var lookupValue = default(ParameterValue);

                    if (matchFound)
                    {
                        var matchingPair = matchIsConstraint 
                            ? GetMatchingPair(parameterConstraints) 
                            : GetMatchingPair(allParameterValues);

                        KeyValuePair<string, object> GetMatchingPair
                            (
                                IDictionary<string, object> parameterValues
                            )
                        {
                            return parameterValues.First
                            (
                                pair => pair.Key.Equals
                                (
                                    lookupInfo.Name,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            );
                        }

                        lookupValue = new ParameterValue
                        (
                            lookupInfo,
                            matchingPair.Value
                        );
                    }
                    else
                    {
                        lookupValue = new ParameterValue
                        (
                            lookupInfo,
                            lookupInfo.DefaultValue
                        );
                    }

                    lookupValues.Add(lookupValue);
                }

                return lookupValues.ToArray();
            }
            else
            {
                return new ParameterValue[] { };
            }
        }

        /// <summary>
        /// Gets a single report parameter value from the filter
        /// </summary>
        /// <param name="parameterName">The parameter name</param>
        /// <returns>The parameter value</returns>
        public ReportFilterParameterValue GetParameterValue
            (
                string parameterName
            )
        {
            Validate.IsNotEmpty(parameterName);

            var value = _parameterValues.FirstOrDefault
            (
                p => p.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase)
            );

            if (value == null)
            {
                var definition = GetDefinition
                (
                    parameterName
                );

                var allParameterValues = GetParameterValuesAsDictionary();

                var lookupParameterValues = CompileLookupParameterValues
                (
                    parameterName,
                    allParameterValues
                );

                value = new ReportFilterParameterValue
                (
                    definition,
                    definition.Parameter.DefaultValue,
                    lookupParameterValues
                );
            }

            return value;
        }

        /// <summary>
        /// Gets parameter values for a query
        /// </summary>
        /// <param name="query">The query</param>
        /// <param name="defaultValues">The default values</param>
        /// <returns>A collection of parameter values</returns>
        public IEnumerable<ParameterValue> GetQueryParameters
            (
                IQuery query,
                params ParameterValue[] defaultValues
            )
        {
            Validate.IsNotNull(query);

            var filterValues = _parameterValues.Where
            (
                p => p.Definition.TargetType == ReportParameterTargetType.Filter
            );

            var queryParameters = new List<ParameterValue>();

            foreach (var parameter in query.Parameters)
            {
                var matchingValue = default(ParameterValue);

                matchingValue = filterValues.FirstOrDefault
                (
                    pv => pv.Parameter.Name.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase)
                );

                if (matchingValue != null)
                {
                    queryParameters.Add(matchingValue);
                }
                else if (defaultValues != null)
                {
                    // Try and find a matching parameter from the default values
                    matchingValue = defaultValues.FirstOrDefault
                    (
                        pv => pv.Parameter == parameter
                    );

                    if (matchingValue != null)
                    {
                        queryParameters.Add(matchingValue);
                    }
                }
            }

            return queryParameters;
        }

        /// <summary>
        /// Gets parameter values for the report fields
        /// </summary>
        /// <param name="defaultValues">The default values</param>
        /// <returns>A collection of parameter values</returns>
        public IEnumerable<ParameterValue> GetFieldParameters
            (
                params ParameterValue[] defaultValues
            )
        {
            return GetParameterValues
            (
                ReportParameterTargetType.SetField,
                defaultValues
            );
        }

        /// <summary>
        /// Gets parameter values for the component exclusions
        /// </summary>
        /// <param name="defaultValues">The default values</param>
        /// <returns>A collection of parameter values</returns>
        public IEnumerable<ParameterValue> GetComponentExclusionParameters
            (
                params ParameterValue[] defaultValues
            )
        {
            return GetParameterValues
            (
                ReportParameterTargetType.ExcludeComponent,
                defaultValues
            );
        }

        /// <summary>
        /// Determines if a report component has been excluded
        /// </summary>
        /// <param name="componentName">The component name</param>
        /// <returns>True, if the component has been excluded; otherwise false</returns>
        public bool IsExcluded
            (
                string componentName
            )
        {
            Validate.IsNotEmpty(componentName);

            var exclusionParameters = GetComponentExclusionParameters();
            var excluded = false;

            foreach (var parameter in exclusionParameters)
            {
                var defined = IsDefined
                (
                    parameter.Name
                );

                if (defined)
                {
                    var definition = GetDefinition
                    (
                        parameter.Name
                    );

                    excluded =
                    (
                        definition.TargetName.Equals(componentName, StringComparison.OrdinalIgnoreCase)
                            && definition.TargetValue == parameter.Value
                    );

                    if (excluded)
                    {
                        break;
                    }
                }
            }

            return excluded;
        }

        /// <summary>
        /// Gets parameter values for the a specific target type
        /// </summary>
        /// <param name="targetType">The target type</param>
        /// <param name="defaultValues">The default values</param>
        /// <returns>A collection of parameter values</returns>
        protected IEnumerable<ParameterValue> GetParameterValues
            (
                ReportParameterTargetType targetType,
                params ParameterValue[] defaultValues
            )
        {
            var targetValues = new List<ParameterValue>();

            var fieldDefinitions = GetDefinitions
            (
                targetType
            );

            var matchingValues = _parameterValues.Where
            (
                p => p.Definition.TargetType == targetType
            );

            if (matchingValues.Any())
            {
                targetValues.AddRange(matchingValues);
            }

            if (defaultValues != null)
            {
                foreach (var value in defaultValues)
                {
                    var valueFound = targetValues.Any
                    (
                        v => v.Name.Equals(value.Name, StringComparison.OrdinalIgnoreCase)
                    );

                    if (false == valueFound)
                    {
                        targetValues.Add(value);
                    }
                }
            }

            return targetValues;
        }

        /// <summary>
        /// Gets a collection of sorting rules by component and column
        /// </summary>
        public IEnumerable<ReportFilterSortingRule> SortingRules
        {
            get
            {
                return _sortingRules;
            }
        }

        /// <summary>
        /// Sets a single report filter parameter value
        /// </summary>
        /// <param name="sectionType">The section type</param>
        /// <param name="componentName">The component name</param>
        /// <param name="columnName">The column name</param>
        /// <param name="direction">The sorting direction</param>
        public void SetSortingRule
            (
                ReportSectionType sectionType,
                string componentName,
                string columnName,
                SortDirection direction
            )
        {
            Validate.IsNotEmpty(componentName);
            Validate.IsNotEmpty(columnName);

            var matchingRule = _sortingRules.FirstOrDefault
            (
                p => p.SectionType == sectionType
                    && p.ComponentName.Equals(componentName, StringComparison.OrdinalIgnoreCase)
                    && p.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
            );

            if (matchingRule != null)
            {
                matchingRule.Direction = direction;
            }
            else
            {
                matchingRule = new ReportFilterSortingRule
                (
                    sectionType,
                    componentName,
                    columnName,
                    direction
                );

                _sortingRules.Add
                (
                    matchingRule
                );
            }
        }

        /// <summary>
        /// Sets a series of report sorting rules
        /// </summary>
        /// <param name="sortingRules">The sorting rules</param>
        /// <remarks>
        public void SetSortingRules
            (
                params ReportFilterSortingRule[] sortingRules
            )
        {
            Validate.IsNotNull(sortingRules);

            foreach (var rule in sortingRules)
            {
                SetSortingRule
                (
                    rule.SectionType,
                    rule.ComponentName,
                    rule.ColumnName,
                    rule.Direction
                );
            }
        }

        /// <summary>
        /// Gets sorting rules for a section and component
        /// </summary>
        /// <param name="sectionType">The report section type</param>
        /// <param name="componentName">The component name</param>
        /// <returns>A collection of sorting rules</returns>
        public IEnumerable<ReportFilterSortingRule> GetSortingRules
            (
                ReportSectionType sectionType,
                string componentName
            )
        {
            Validate.IsNotEmpty(componentName);

            return _sortingRules.Where
            (
                rule => rule.SectionType == sectionType
                    && rule.ComponentName.Equals(componentName, StringComparison.OrdinalIgnoreCase)
            );
        }

        /// <summary>
        /// Finds a sort direction for a section, component and column
        /// </summary>
        /// <param name="sectionType">The section type</param>
        /// <param name="componentName">The component name</param>
        /// <param name="columnName">The column name</param>
        /// <returns>The sort direction, if found; otherwise null</returns>
        public SortDirection? FindSortDirection
            (
                ReportSectionType sectionType,
                string componentName,
                string columnName
            )
        {
            Validate.IsNotEmpty(componentName);
            Validate.IsNotEmpty(columnName);

            var rule = _sortingRules.FirstOrDefault
            (
                r => r.SectionType == sectionType
                    && r.ComponentName.Equals(componentName, StringComparison.OrdinalIgnoreCase)
                    && r.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
            );

            if (rule == null)
            {
                return null;
            }
            else
            {
                return rule.Direction;
            }
        }

        /// <summary>
        /// Generates a new report filter which is a clone of the current
        /// </summary>
        /// <returns>The report filter that was generated</returns>
        public ReportFilter Clone()
        {
            var filter = new ReportFilter()
            {
                _parameterDefinitions = _parameterDefinitions
            };

            // Clone and add the parameter values
            foreach (var value in _parameterValues)
            {
                var valueClone = value.Clone();

                filter._parameterValues.Add
                (
                    valueClone
                );
            }

            // Clone and add the sorting rules
            foreach (var rule in _sortingRules)
            {
                var ruleClone = rule.Clone();

                filter._sortingRules.Add
                (
                    ruleClone
                );
            }

            return filter;
        }
    }
}
