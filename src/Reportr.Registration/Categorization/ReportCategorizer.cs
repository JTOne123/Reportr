﻿namespace Reportr.Registration.Categorization
{
    using System;
    using System.Collections.Generic;
    
    /// <summary>
    /// Represents the default report categorizer implementation
    /// </summary>
    public sealed class ReportCategorizer : IReportCategorizer
    {
        private readonly IReportCategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructs the report categorizer with required dependencies
        /// </summary>
        /// <param name="categoryRepository">The category repository</param>
        /// <param name="unitOfWork">The unit of work</param>
        public ReportCategorizer
            (
                IReportCategoryRepository categoryRepository,
                IUnitOfWork unitOfWork
            )
        {
            Validate.IsNotNull(categoryRepository);
            Validate.IsNotNull(unitOfWork);

            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Creates a single root level report category
        /// </summary>
        /// <param name="name">The category name</param>
        /// <param name="title">The category title</param>
        /// <param name="description">A description of the category</param>
        /// <returns>The category created</returns>
        public ReportCategory CreateCategory
            (
                string name,
                string title,
                string description
            )
        {
            var nameAvailable = _categoryRepository.IsNameAvailable
            (
                name
            );

            if (false == nameAvailable)
            {
                var message = "The category name '{0}' is not available.";

                throw new InvalidOperationException
                (
                    String.Format
                    (
                        message,
                        name
                    )
                );
            }

            var category = new ReportCategory
            (
                name,
                description
            );

            _categoryRepository.AddCategory(category);
            _unitOfWork.SaveChanges();

            return category;
        }

        /// <summary>
        /// Creates a single report sub category
        /// </summary>
        /// <param name="parentCategoryName">The parent category name</param>
        /// <param name="name">The category name</param>
        /// <param name="title">The category title</param>
        /// <param name="description">A description of the category</param>
        /// <returns>The category created</returns>
        public ReportCategory CreateSubCategory
            (
                string parentCategoryName,
                string name,
                string title,
                string description
            )
        {
            var nameAvailable = _categoryRepository.IsNameAvailable
            (
                name
            );

            if (false == nameAvailable)
            {
                var message = "The category name '{0}' is not available.";

                throw new InvalidOperationException
                (
                    String.Format
                    (
                        message,
                        name
                    )
                );
            }

            var parentCategory = _categoryRepository.GetCategory
            (
                parentCategoryName
            );

            var subCategory = parentCategory.CreateSubCategory
            (
                name,
                description
            );

            _categoryRepository.AddCategory(subCategory);
            _unitOfWork.SaveChanges();

            return subCategory;
        }

        /// <summary>
        /// Determines if a category name is available
        /// </summary>
        /// <param name="categoryName">The category name</param>
        /// <returns>True, if the name is available; otherwise false</returns>
        public bool IsNameAvailable
            (
                string categoryName
            )
        {
            return _categoryRepository.IsNameAvailable
            (
                categoryName
            );
        }

        /// <summary>
        /// Gets a single report category
        /// </summary>
        /// <param name="name">The category name</param>
        /// <returns>The matching category</returns>
        public ReportCategory GetCategory
            (
                string name
            )
        {
            return _categoryRepository.GetCategory
            (
                name
            );
        }

        /// <summary>
        /// Gets all root level report categories
        /// </summary>
        /// <returns>A collection of report categories</returns>
        public IEnumerable<ReportCategory> GetAllCategories()
        {
            return _categoryRepository.GetAllCategories();
        }

        /// <summary>
        /// Gets sub report categories from a parent category
        /// </summary>
        /// <param name="parentCategoryName">The parent category name</param>
        /// <returns>A collection of report categories</returns>
        public IEnumerable<ReportCategory> GetSubCategories
            (
                string parentCategoryName
            )
        {
            return _categoryRepository.GetSubCategories
            (
                parentCategoryName
            );
        }

        /// <summary>
        /// Assigns a single report to a category
        /// </summary>
        /// <param name="categoryName">The category name</param>
        /// <param name="reportName">The report name</param>
        public void AssignToCategory
            (
                string categoryName,
                string reportName
            )
        {
            var category = _categoryRepository.GetCategory
            (
                categoryName
            );

            category.AssignReport(reportName);

            _categoryRepository.UpdateCategory(category);
            _unitOfWork.SaveChanges();
        }

        /// <summary>
        /// Unassigns a single report from a category
        /// </summary>
        /// <param name="categoryName">The category name</param>
        /// <param name="reportName">The report name</param>
        public void UnassignFromCategory
            (
                string categoryName,
                string reportName
            )
        {
            var category = _categoryRepository.GetCategory
            (
                categoryName
            );

            category.UnassignReport(reportName);

            _categoryRepository.UpdateCategory(category);
            _unitOfWork.SaveChanges();
        }

        /// <summary>
        /// Determines if a report has been assigned to a category
        /// </summary>
        /// <param name="categoryName">The category name</param>
        /// <param name="reportName">The report name</param>
        /// <returns>True, if the report has been assigned; otherwise false</returns>
        public bool IsAssignedToCategory
            (
                string categoryName,
                string reportName
            )
        {
            var category = _categoryRepository.GetCategory
            (
                categoryName
            );

            return category.IsReportAssigned
            (
                reportName
            );
        }

        /// <summary>
        /// Deletes a single report category
        /// </summary>
        /// <param name="name">The category name</param>
        public void DeleteCategory
            (
                string name
            )
        {
            _categoryRepository.RemoveCategory(name);
            _unitOfWork.SaveChanges();
        }
    }
}
