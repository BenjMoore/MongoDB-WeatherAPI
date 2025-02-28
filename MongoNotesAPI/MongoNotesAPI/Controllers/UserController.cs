using ICTPRG553.Models.DTOs;
using ICTPRG553.Models.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoNotesAPI.Middleware;
using MongoNotesAPI.Models;
using MongoNotesAPI.Models.DTOs;
using MongoNotesAPI.Models.Filters;
using MongoNotesAPI.Repositories;
using System;
using System.Security.Cryptography.X509Certificates;

namespace MongoNotesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class UserController : ControllerBase
    {
        //Stores a reference to our repository so we can request database actions
        private readonly IUserRepository _userRepository;
        public UserRoleUpdateDTO update;
        //Requests our repository from the dependency injection and stores it in our 
        //readonly field.
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        /// <summary>
        /// Creates a new user in the system.
        /// This endpoint is intended to create a new user account with specific details 
        /// such as name, email, role, and the account's creation date.
        /// </summary>
        /// <param name="apiKey">The API key provided by the client to authenticate the request.
        /// This must meet the required access level to perform the user creation action (admin-level).</param>
        /// <param name="userDTO">The user creation data transfer object that contains information 
        /// like the name, email, role, and creation date of the new user.</param>
        /// <returns>An Ok result if the user is successfully created, or an error if the creation fails.</returns>
        [HttpPost]
        [ApiKey("TEACHER")]
        public ActionResult CreateUser(UserCreateDTO userDTO)
        {
            // Check if the user's API key meets the required lvel (Admin Access) to add a new user to the system.
            var user = new ApiUser
            {
                Name = userDTO.Name,
                Email = userDTO.Email,
                Role = userDTO.Role,               
                Active = true,
                Created = DateTime.Now
            };

            var result = _userRepository.CreateUser(user);
            return Ok();
        }

        /// <summary>
        /// Updates the role of multiple users based on provided criteria such as date range.
        /// This endpoint allows an admin to update the roles of users that were created 
        /// within a certain time period, assigning them a new role.
        /// </summary>
        /// <param name="update">An object containing the role update details, including 
        /// a date range to select the users whose roles will be updated and the new role value to assign.</param>
        /// <returns>An Ok result if the roles are updated successfully, or a BadRequest if the input is invalid or the update fails.</returns>
        [ApiKey("TEACHER")]
        [HttpPatch("UpdateRole")]
        public ActionResult UpdateRole(UserRoleUpdateDTO update)
        {
            // Check if a valid set of update details was provided.
            if (update == null)
            {
                return BadRequest();
            }

            // Ensure that at least one of the update fields (createdBefore or createdAfter) has been provided.
            if (update.createdBefore == null && update.createdAfter == null)
            {
                return BadRequest();
            }

            // Call the repository method to update user roles based on the provided date range and new role.
            var result = _userRepository.UpdateRole(update);

            if (result.WasSuccessful)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        /// <summary>
        /// Deletes a user from the system by their ID.
        /// This endpoint allows an admin to remove a user account from the system by specifying their unique ID.
        /// The operation is secured to require an admin-level API key.
        /// </summary>
        /// <param name="user">The ApiUser object representing the user to be deleted. 
        /// This includes their details and unique identifier in the system.</param>
        /// <param name="apiKey">The API key used to authenticate the delete request, 
        /// which must have admin-level permissions.</param>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <returns>An Ok result if the user is successfully deleted, or an error message if the deletion fails.</returns>
        [HttpDelete("DeleteUser")]
        [ApiKey("TEACHER")]
        public ActionResult DeleteUser(string id)
        {
            // Check if the user's API key meets the required level (Admin Access) to delete a user.
            var result = _userRepository.DeleteUser(id);
            return Ok();
        }

        /// <summary>
        /// Deletes users who have been inactive for more than 30 days from the system.
        /// This endpoint removes user accounts that were created over 30 days ago 
        /// and have not been active since, helping to clean up stale or inactive users.
        /// </summary>
        /// <returns>An Ok result if the deletion is successful, or a BadRequest if no users matched the criteria or an error occurred.</returns>
        [HttpDelete("DeleteOlderThan30Days")]
        [ApiKey("TEACHER")]
        public ActionResult DeleteOlderThan30Days()
        {
            int days = 30;

            // Create a filter to match users who have been inactive for more than 30 days.
            UserFilter filter = new UserFilter
            {
                CreatedBefore = DateTime.Now.AddDays(-days),
                CreatedAfter = null // Ensures all users older than 30 days are selected.
            };

            // Call the repository method to delete users matching the filter.
            var result = _userRepository.DeleteMany(filter);

            if (result.WasSuccessful)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }


    }
}
 