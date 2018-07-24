﻿namespace Reportr.Filtering
{
    using Reportr.Common.Reflection;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a single report parameter value
    /// </summary>
    public class ParameterValue
    {
        private KeyValuePair<object, string>[] _lookupItems = null;

        /// <summary>
        /// Constructs the parameter value with the details
        /// </summary>
        /// <param name="parameterInfo">The parameter info</param>
        /// <param name="value">The value</param>
        /// <param name="lookupParameterValues">The lookup parameter values</param>
        public ParameterValue
            (
                ParameterInfo parameterInfo,
                object value,
                params ParameterValue[] lookupParameterValues
            )
        {
            Validate.IsNotNull(parameterInfo);

            this.Parameter = parameterInfo;
            this.Name = parameterInfo.Name;

            SetValue(value, lookupParameterValues);
        }

        /// <summary>
        /// Gets the parameter information
        /// </summary>
        public ParameterInfo Parameter { get; private set; }

        /// <summary>
        /// Gets an array of lookup parameter values
        /// </summary>
        /// <remarks>
        /// The lookup parameter values are used to filter the lookup 
        /// items when executing the lookup query.
        /// </remarks>
        public ParameterValue[] LookupParameterValues { get; private set; }

        /// <summary>
        /// Gets the lookup items available for the parameter value
        /// </summary>
        public KeyValuePair<object, string>[] LookupItems
        {
            get
            {
                if (_lookupItems == null)
                {
                    InitializeLookupItems();
                }

                return _lookupItems;
            }
        }

        /// <summary>
        /// Initializes the lookup items for the parameter value
        /// </summary>
        private void InitializeLookupItems()
        {
            var parameterInfo = this.Parameter;

            if (parameterInfo.HasLookup)
            {
                var items = default
                (
                    List<KeyValuePair<object, string>>
                );

                var sourceType = parameterInfo.LookupSourceType.Value;

                switch (sourceType)
                {
                    case ParameterLookupSourceType.Query:
                    {
                        items = ExecuteLookupQuery
                        (
                            parameterInfo
                        );
                        
                        break;
                    }
                    case ParameterLookupSourceType.Enum:
                    {
                        items = GetLookupEnumValues
                        (
                            parameterInfo
                        );
                        
                        break;
                    }
                    default:
                    {
                        var message = "The lookup source type {0} is not supported.";

                        throw new NotSupportedException
                        (
                            String.Format
                            (
                                message,
                                sourceType
                            )
                        );
                    }
                }

                if (parameterInfo.InsertBlankLookupItem)
                {
                    items.Insert
                    (
                        0,
                        new KeyValuePair<object, string>
                        (
                            null,
                            null
                        )
                    );
                }

                _lookupItems = items.ToArray();
            }
        }

        /// <summary>
        /// Executes a lookup query and returns the results as a key-value list
        /// </summary>
        /// <param name="parameterInfo">The parameter information</param>
        /// <returns>A list of key-value pairs representing the query results</returns>
        private List<KeyValuePair<object, string>> ExecuteLookupQuery
            (
                ParameterInfo parameterInfo
            )
        {
            var results = parameterInfo.LookupQuery.Execute
            (
                this.LookupParameterValues
            );

            var valueBinding = parameterInfo.LookupValueBinding;
            var textBinding = parameterInfo.LookupDisplayTextBinding;
            var items = new List<KeyValuePair<object, string>>();

            if (parameterInfo.InsertBlankLookupItem)
            {
                items.Add
                (
                    new KeyValuePair<object, string>
                    (
                        null,
                        null
                    )
                );
            }

            foreach (var row in results.AllRows)
            {
                var lookupValue = valueBinding.Resolve
                (
                    row
                );

                var lookupText = textBinding.Resolve<string>
                (
                    row
                );

                items.Add
                (
                    new KeyValuePair<object, string>
                    (
                        lookupValue,
                        lookupText
                    )
                );
            }

            return items;
        }

        /// <summary>
        /// Gets the enum values for a parameter lookup
        /// </summary>
        /// <param name="parameterInfo">The parameter information</param>
        /// <returns>A list of key-value pairs representing the enum</returns>
        private List<KeyValuePair<object, string>> GetLookupEnumValues
            (
                ParameterInfo parameterInfo
            )
        {
            var inspector = new EnumInspector();

            var enumInfo = inspector.GetEnumInfo
            (
                parameterInfo.LookupEnumType
            );

            var pairs = new List<KeyValuePair<object, string>>();

            foreach (var item in enumInfo)
            {
                pairs.Add
                (
                    new KeyValuePair<object, string>
                    (
                        item.Name,
                        item.Description
                    )
                );
            }

            return pairs;
        }

        /// <summary>
        /// Gets the parameter name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the parameter value
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Sets the parameter value
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="lookupParameterValues">The lookup parameter values</param>
        protected internal void SetValue
            (
                object value,
                params ParameterValue[] lookupParameterValues
            )
        {
            Validate.IsNotNull(lookupParameterValues);

            var parameter = this.Parameter;
            var valueChanged = (value != this.Value);

            if (value == null)
            {
                if (parameter.ValueRequired)
                {
                    var message = "A value for the parameter '{0}' is required.";

                    throw new ArgumentException
                    (
                        String.Format
                        (
                            message,
                            parameter.Name
                        )
                    );
                }
                else
                {
                    this.Value = null;
                }
            }
            else
            {
                var valueType = value.GetType();
                var expectedType = parameter.ExpectedType;

                if (valueType != expectedType)
                {
                    var message = "The type is {0} but the type {1} was expected.";

                    throw new ArgumentException
                    (
                        String.Format
                        (
                            message,
                            valueType,
                            expectedType
                        )
                    );
                }
                else
                {
                    this.Value = value;
                }
            }

            if (valueChanged || lookupParameterValues.Any())
            {
                _lookupItems = null;
            }
        }
    }
}
