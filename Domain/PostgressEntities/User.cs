
namespace Domain.PostgressEntities
{
    /// <summary>
    /// The user entity used with postgreSQL
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The name of the user
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The email of the user
        /// </summary>
        public string Email { get; set; }
    }
}
