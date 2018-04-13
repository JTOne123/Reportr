﻿namespace Reportr
{
    /// <summary>
    /// Represents the default implementation for a report section
    /// </summary>
    public class ReportSection : IReportSection
    {
        /// <summary>
        /// Constructs the report section with the core details
        /// </summary>
        /// <param name="name">The section name</param>
        /// <param name="component">The report component</param>
        public ReportSection
            (
                string name,
                IReportComponent component
            )
        {
            Validate.IsNotEmpty(name);
            Validate.IsNotNull(component);

            this.Name = name;
            this.Component = component;
            this.NestedSections = new IReportSection[] { };
        }

        /// <summary>
        /// Gets the sections name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the component associated with the section
        /// </summary>
        public IReportComponent Component { get; private set; }

        /// <summary>
        /// Adds the descriptors to the report section
        /// </summary>
        /// <param name="title">The title</param>
        /// <param name="description">The description (optional)</param>
        /// <returns>The updated report section</returns>
        public ReportSection WithDescriptors
            (
                string title,
                string description = null
            )
        {
            Validate.IsNotEmpty(title);

            this.Title = title;
            this.Description = description;

            return this;
        }

        /// <summary>
        /// Gets the sections title
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the sections description
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Adds nested sections to the report section
        /// </summary>
        /// <param name="sections">The nested sections to add</param>
        /// <returns>The updated report section</returns>
        public ReportSection WithNestedSections
            (
                params IReportSection[] sections
            )
        {
            Validate.IsNotNull(sections);

            this.NestedSections = sections;

            return this;
        }

        /// <summary>
        /// Gets an array of nested report sections
        /// </summary>
        public IReportSection[] NestedSections { get; private set; }
    }
}
