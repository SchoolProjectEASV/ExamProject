using Domain.PostgressEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserApplication.DTO;

namespace UserApplication
{
    public interface IUserService
    {

        /// <summary>
        /// Gets all the users (customers)
        /// </summary>
        /// <returns>Returns a colection of users</returns>
        public Task<IEnumerable<User>> GetAllUsers();
        /// <summary>
        /// Gets a user by an ID
        /// </summary>
        /// <param name="id">ID of the specific user</param>
        /// <returns>returns a DTO with all the required </returns>
        public Task<GetUserDTO> GetUserById(int id);

        /// <summary>
        /// Adds a user to the user table
        /// </summary>
        /// <param name="userDTO">DTO with the required parameters for adding an user</param>
        /// <returns>Returns the ID of the user created</returns>

        public Task<int> AddUser(AddUserDTO userDTO);

        /// <summary>
        /// Updates an user in the user table
        /// </summary>
        /// <param name="user">the user being updated</param>
        /// <returns>Returns a simple bool, and sends back a "user has been succesfully created" if it has been created</returns>

        public Task<bool> UpdateUser(User user);

        /// <summary>
        /// Deletes an user in the user table
        /// </summary>
        /// <param name="id">ID of the user being deleted</param>
        /// <returns>Returns a simple bool, and sends back a message if a user has been succesfully deleted or not</returns>

        public Task<bool> DeleteUser(int id);


    }
}
