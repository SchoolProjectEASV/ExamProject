

namespace CategoryApplication.DTO
{
    /// <summary>
    /// The DTO used to create a category
    /// </summary>
    public class CreateCategoryDTO
    {
        /// <summary>
        /// The category name
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// The category Description
        /// </summary>
        public string? Description { get; set; }
    }
}
